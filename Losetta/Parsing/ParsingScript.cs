using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Packaging;
using System.Text;

namespace AliceScript.Parsing
{
    /// <summary>
    /// パース中のスクリプトを表します
    /// </summary>
    public class ParsingScript
    {
        private string m_data;          // スクリプト全体が含まれます
        private int m_from;             // スクリプトへのポインタ
        private string m_filename;      // スクリプトのファイル名
        private string m_originalScript;// 生のスクリプト
        private int m_scriptOffset = 0; // 大きなスクリプトで定義された関数で使用されます
        private int m_generation = 1;   // スクリプトの世代
        private object m_tag;           // 現在のスクリプトに関連付けられたオブジェクト。これは多用途で使用されます
        private AlicePackage m_package = null;//現在のスクリプトが実行されているパッケージ
        private HashSet<string> m_defines = new HashSet<string>();// 現在のスクリプトで宣言されたシンボル
        private static ParsingScript m_toplevel_script = new ParsingScript("", 0, null);// 最上位のスクリプト
        private ParsingScript m_parentScript = m_toplevel_script;// このスクリプトの親
        private ScriptSettings m_settings = new ScriptSettings();//このスクリプトの設定
        private Dictionary<int, int> m_char2Line = null; // 元の行へのポインタ
        private Dictionary<string, ParserFunction> m_variables = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された変数
        private Dictionary<string, ParserFunction> m_consts = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された定数
        private Dictionary<string, ParserFunction> m_functions = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された関数
        private HashSet<NameSpace> m_namespace = new HashSet<NameSpace>();
        internal List<StackInfo> m_stacktrace = new List<StackInfo>();


        /// <summary>
        /// 最上位のスクリプトを取得します
        /// </summary>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <returns>最上位のスクリプト</returns>
        public static ParsingScript GetTopLevelScript(ParsingScript script = null)
        {
            if (m_toplevel_script.ParentScript != null)
            {
                //トップレベルスクリプトは親を持たない
                m_toplevel_script.ParentScript = null;
            }
            return script != null && script.DenyAccessToTopLevelScript
                ? throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, script)
                : m_toplevel_script;
        }

        /// <summary>
        /// このスクリプトで宣言されたシンボル
        /// </summary>
        public HashSet<string> Defines
        {
            get => m_defines;
            set => m_defines = value;
        }
        /// <summary>
        /// このスクリプトのCustomFunctionの呼び出し履歴
        /// </summary>
        public List<StackInfo> StackTrace
        {
            get
            {
                var s = new List<StackInfo>(m_stacktrace);
                if (ProcessingFunction != null)
                {
                    s.Add(new StackInfo(ProcessingFunction, OriginalLine, OriginalLineNumber, Filename));
                }
                return s;
            }
        }

        /// <summary>
        /// StackTraceで使用するひとつ当たりのスタック
        /// </summary>
        public class StackInfo
        {
            /// <summary>
            /// 現在実行中の関数
            /// </summary>
            public FunctionBase Function { get; set; }
            /// <summary>
            /// 現在の行の文字列
            /// </summary>
            public string Line { get; set; }
            /// <summary>
            /// 現在の行番号
            /// </summary>
            public int LineNumber { get; set; }
            /// <summary>
            /// ファイル名
            /// </summary>
            public string FileName { get; set; }

            public StackInfo(FunctionBase function, string line, int lineNumber, string fileName)
            {
                Function = function;
                Line = line;
                LineNumber = lineNumber;
                FileName = fileName;
            }
            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("場所 ");
                foreach (string k in Function.Keywords)
                {
                    sb.Append(k);
                    sb.Append(Constants.SPACE);
                }
                if (Function.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE) || Function.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
                {
                    sb.Append(Constants.COMMAND);
                    sb.Append(Constants.SPACE);
                }
                if (Function is BindFunction)
                {
                    sb.Append(".bind ");
                }
                if (Function is CustomFunction)
                {
                    sb.Append(".custom ");
                }
                else if (Function.Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE))
                {
                    sb.Append(".structure ");
                }
                sb.Append(Constants.FUNCTION);
                sb.Append(Constants.SPACE);
                if (!string.IsNullOrEmpty(Function.RelatedNameSpace))
                {
                    sb.Append(Function.RelatedNameSpace);
                    sb.Append(".");
                }
                sb.Append(string.IsNullOrWhiteSpace(Function.Name) ? "Anonymous" : Function.Name);
                sb.Append(Constants.START_ARG);
                int args_count = 0;
                if (Function is CustomFunction cf && cf.RealArgs != null && cf.RealArgs.Length > 0)
                {
                    foreach (string a in Function.RealArgs)
                    {
                        sb.Append(a);
                        sb.Append(++args_count == Function.RealArgs.Length ? string.Empty : ",");
                    }
                }
                sb.Append(");");
                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    sb.Append(" 場所 ");
                    sb.Append(FileName);
                    sb.Append(":行 ");
                    sb.Append(LineNumber);
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 現在実行中あるいは最後に実行された関数
        /// </summary>
        public FunctionBase ProcessingFunction { get; set; }

        /// <summary>
        /// このスクリプトの現在の名前空間
        /// </summary>
        public NameSpace CurrentNamespace
        {
            get; set;
        }
        /// <summary>
        /// このスクリプトでusingされた名前空間の一覧
        /// </summary>
        public HashSet<NameSpace> UsingNamespaces
        {
            get => m_namespace;
            set => m_namespace = value;
        }
        /// <summary>
        /// このスクリプトに関連付けられたオブジェクトです
        /// </summary>
        public object Tag
        {
            get => m_tag;
            set => m_tag = value;
        }
        /// <summary>
        /// これが実行されているパッケージを表します
        /// </summary>
        public AlicePackage Package
        {
            get => m_package;
            set => m_package = value;
        }
        /// <summary>
        /// 現在のスクリプトの世代数を取得または設定します
        /// </summary>
        public int Generation
        {
            get => m_generation;
            set => m_generation = value;
        }
        /// <summary>
        /// 現在のスクリプトのポインタを取得または設定します
        /// </summary>
        public int Pointer
        {
            get => m_from;
            set => m_from = value;
        }
        /// <summary>
        /// 現在のスクリプト全体を取得または設定します
        /// </summary>
        public string String
        {
            get => m_data;
            set => m_data = value;
        }
        /// <summary>
        /// 現在のスクリプト内で定義された変数
        /// </summary>
        public Dictionary<string, ParserFunction> Variables
        {
            get => m_variables;
            set => m_variables = value;
        }
        /// <summary>
        /// 現在のスクリプト内で定義された関数
        /// </summary>
        public Dictionary<string, ParserFunction> Functions
        {
            get => m_functions;
            set => m_functions = value;
        }
        /// <summary>
        /// 現在のスクリプト内で定義された定数
        /// </summary>
        public Dictionary<string, ParserFunction> Consts
        {
            get => m_consts;
            set => m_consts = value;
        }
        /// <summary>
        /// このスクリプトで例外が発生したときに通知するイベントです
        /// </summary>
        public event ThrowErrorEventhandler ThrowError;

        /// <summary>
        /// これから実行されるコード
        /// </summary>
        public string Rest => Substr(m_from, Constants.MAX_CHARS_TO_SHOW);
        /// <summary>
        /// 現在解析中の文字
        /// </summary>
        public char Current => m_from < m_data.Length ? m_data[m_from] : Constants.EMPTY;
        /// <summary>
        /// ひとつ前に解析された文字
        /// </summary>
        public char Prev => m_from >= 1 ? m_data[m_from - 1] : Constants.EMPTY;
        /// <summary>
        /// ふたつ前に解析された文字
        /// </summary>
        public char PrevPrev => m_from >= 2 ? m_data[m_from - 2] : Constants.EMPTY;
        /// <summary>
        /// ひとつ後に解析される文字
        /// </summary>
        public char Next => m_from + 1 < m_data.Length ? m_data[m_from + 1] : Constants.EMPTY;
        /// <summary>
        /// 二つ後に解析される文字
        /// </summary>
        public char NextNext => m_from + 2 < m_data.Length ? m_data[m_from + 2] : Constants.EMPTY;
        public Dictionary<int, int> Char2Line
        {
            get => m_char2Line;
            set => m_char2Line = value;
        }
        public int ScriptOffset
        {
            get => m_scriptOffset;
            set => m_scriptOffset = value;
        }
        public string Filename
        {
            get => m_filename;
            set => m_filename = Utils.GetFullPath(value);
        }
        public string PWD => Utils.GetDirectoryName(m_filename);
        public string OriginalScript
        {
            get => m_originalScript;
            set => m_originalScript = value;
        }

        public string CurrentAssign { get; set; }

        internal void OnThrowError(object sender, ThrowErrorEventArgs e)
        {
            ThrowError?.Invoke(sender, e);
        }

        public Dictionary<string, Dictionary<string, int>> AllLabels { get; set; }
        public Dictionary<string, string> LabelToFile { get; set; }
        /// <summary>
        /// このスクリプトの設定
        /// </summary>
        public ScriptSettings Settings
        {
            get => m_settings;
            set => m_settings = value;
        }
        /// <summary>
        /// スクリプトの設定
        /// </summary>
        public class ScriptSettings
        {
            /// <summary>
            /// このスクリプトでは変数が暗黙的に定義される
            /// </summary>
            public bool? UnneedVarKeyword { get; set; }
            /// <summary>
            /// このスクリプトでは型推論が有効
            /// </summary>
            public bool? TypeInference { get; set; }
            /// <summary>
            /// このスクリプトではswitch文のフォールスルーが有効
            /// </summary>
            public bool? FallThrough { get; set; }
            /// <summary>
            /// このスクリプトではbreakのないcaseをエラーとして扱う
            /// </summary>
            public bool? CheckBreakWhenEndCaseBlock { get; set; }
            /// <summary>
            /// このスクリプトは名前空間の参照を追加できる
            /// </summary>
            public bool? EnableUsing { get; set; }
            /// <summary>
            /// このスクリプトでは、パッケージやライブラリを読み込める
            /// </summary>
            public bool? EnableImport { get; set; }
            /// <summary>
            /// このスクリプトではスクリプトを読み込める
            /// </summary>
            public bool? EnableInclude { get; set; }
            /// <summary>
            /// このスクリプトはグローバルにアクセスできない
            /// </summary>
            public bool? DenyAccessToTopLevelScript { get; set; }
            /// <summary>
            /// このスクリプトで定義された変数はnullを許容する
            /// </summary>
            public bool? Nullable { get; set; }

            /// <summary>
            /// この設定ともう一方の設定を結合します。設定値がどちらにもある場合はotherを優先します。
            /// </summary>
            /// <param name="other">結合するもう一方の設定</param>
            public void Union(ScriptSettings other)
            {
                if (!other.UnneedVarKeyword.HasValue)
                {
                    UnneedVarKeyword = other.UnneedVarKeyword;
                }

                if (!other.TypeInference.HasValue)
                {
                    TypeInference = other.TypeInference;
                }

                if (!other.FallThrough.HasValue)
                {
                    FallThrough = other.FallThrough;
                }

                if (!other.CheckBreakWhenEndCaseBlock.HasValue)
                {
                    CheckBreakWhenEndCaseBlock = other.CheckBreakWhenEndCaseBlock;
                }

                if (!other.EnableUsing.HasValue)
                {
                    EnableUsing = other.EnableUsing;
                }

                if (!other.EnableImport.HasValue)
                {
                    EnableImport = other.EnableImport;
                }

                if (!other.EnableInclude.HasValue)
                {
                    EnableInclude = other.EnableInclude;
                }

                if (!other.DenyAccessToTopLevelScript.HasValue)
                {
                    DenyAccessToTopLevelScript = other.DenyAccessToTopLevelScript;
                }
            }
        }

        /// <summary>
        /// このスクリプトで変数宣言が不要の場合はTrue
        /// </summary>
        public bool UnneedVarKeyword
        {
            get
            {
                bool result = false;//規定値
                if (Settings.UnneedVarKeyword.HasValue)
                {
                    result = Settings.UnneedVarKeyword.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.UnneedVarKeyword;
                }
                return result;
            }
            set => Settings.UnneedVarKeyword = value;
        }
        /// <summary>
        /// 定義時に型推論を行う場合はTrue
        /// </summary>
        public bool TypeInference
        {
            get
            {
                bool result = true;//規定値
                if (Settings.TypeInference.HasValue)
                {
                    result = Settings.TypeInference.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.TypeInference;
                }
                return result;
            }
            set => Settings.TypeInference = value;

        }
        /// <summary>
        /// switch文でフォールスルーを認める場合True
        /// </summary>
        public bool FallThrough
        {
            get
            {
                bool result = false;//規定値
                if (Settings.FallThrough.HasValue)
                {
                    result = Settings.FallThrough.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.FallThrough;
                }
                return result;
            }
            set => Settings.FallThrough = value;
        }
        /// <summary>
        /// caseまたはdefaultを抜けるのにbreakまたはreturnが必要
        /// </summary>
        public bool CheckBreakWhenEndCaseBlock
        {
            get
            {
                bool result = true;//規定値
                if (Settings.CheckBreakWhenEndCaseBlock.HasValue)
                {
                    result = Settings.CheckBreakWhenEndCaseBlock.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.CheckBreakWhenEndCaseBlock;
                }
                return result;
            }
            set => Settings.CheckBreakWhenEndCaseBlock = value;
        }
        /// <summary>
        /// このスクリプトでUsingステートメントを許可するか
        /// </summary>
        public bool EnableUsing
        {
            get
            {
                bool result = true;//規定値
                if (Settings.EnableUsing.HasValue)
                {
                    result = Settings.EnableUsing.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.EnableUsing;
                }
                return result;
            }
            set => Settings.EnableUsing = value;
        }
        /// <summary>
        /// このスクリプトでimportを許可するか
        /// </summary>
        public bool EnableImport
        {
            get
            {
                bool result = true;//規定値
                if (Settings.EnableImport.HasValue)
                {
                    result = Settings.EnableImport.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.EnableImport;
                }
                return result;
            }
            set => Settings.EnableImport = value;
        }
        /// <summary>
        /// このスクリプトでincludeを許可するか
        /// </summary>
        public bool EnableInclude
        {
            get
            {
                bool result = true;//規定値
                if (Settings.EnableInclude.HasValue)
                {
                    result = Settings.EnableInclude.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.EnableInclude;
                }
                return result;
            }
            set => Settings.EnableInclude = value;
        }

        public bool DenyAccessToTopLevelScript
        {
            get
            {
                bool result = false;//規定値
                if (Settings.DenyAccessToTopLevelScript.HasValue)
                {
                    result = Settings.DenyAccessToTopLevelScript.Value;
                }
                else if (ParentScript != null && !ParentScript.TopInFile)
                {
                    result = ParentScript.DenyAccessToTopLevelScript;
                }
                return result;
            }
            set => Settings.DenyAccessToTopLevelScript = value;
        }

        /// <summary>
        /// このスクリプトがファイル内の一番外の場合True、それ以外の場合はFalse
        /// </summary>
        public bool TopInFile { get; set; }
        public List<int> PointersBack { get; set; } = new List<int>();

        private string m_functionName = "";
        public string FunctionName
        {
            get => m_functionName;
            set => m_functionName = value.ToLowerInvariant();
        }

        public ParserFunction.StackLevel StackLevel { get; set; }
        public bool ProcessingList { get; set; }

        public bool DisableBreakpoints;
        public string MainFilename;

        /// <summary>
        /// このスクリプトの親
        /// </summary>
        public ParsingScript ParentScript
        {
            get => m_parentScript;
            set => m_parentScript = value;
        }
        public AliceScriptClass CurrentClass { get; set; }
        public AliceScriptClass.ClassInstance ClassInstance { get; set; }

        public ParsingScript(string data, int from = 0,
                             Dictionary<int, int> char2Line = null, bool usingAlice = true)
        {
            m_data = data;
            m_from = from;
            m_char2Line = char2Line;
            if (usingAlice)
            {
                Using(Constants.TOP_NAMESPACE, true);
            }
        }

        public ParsingScript(ParsingScript other, bool usingAlice = true)
        {
            m_data = other.String;
            m_from = other.Pointer;
            m_char2Line = other.Char2Line;
            m_filename = other.Filename;
            m_originalScript = other.OriginalScript;
            m_namespace = other.m_namespace;
            StackLevel = other.StackLevel;
            CurrentClass = other.CurrentClass;
            ClassInstance = other.ClassInstance;
            ScriptOffset = other.ScriptOffset;
            AllLabels = other.AllLabels;
            LabelToFile = other.LabelToFile;
            FunctionName = other.FunctionName;
            Tag = other.Tag;
            Package = other.Package;
            Generation = other.Generation + 1;
            CurrentNamespace = other.CurrentNamespace;
            ThrowError = other.ThrowError;
            if (usingAlice)
            {
                Using(Constants.TOP_NAMESPACE);
            }
        }

        /// <summary>
        /// 現在のスクリプトで名前空間を参照します
        /// </summary>
        /// <param name="name"></param>
        /// <param name="whenPossible">名前空間が存在しない場合に例外を発生させない場合にtrue</param>
        public void Using(string name, bool whenPossible = false)
        {
            name = name.ToLowerInvariant();
            if (NameSpaceManager.Contains(name))
            {
                UsingNamespaces.Add(NameSpaceManager.NameSpaces[name]);
            }
            else if (!whenPossible)
            {
                throw new ScriptException("該当する名前空間がありません", Exceptions.NAMESPACE_NOT_FOUND, this);
            }

        }
        public int Size() { return m_data.Length; }
        public bool StillValid() { return m_from < m_data.Length; }

        public void SetDone() { m_from = m_data.Length; }

        /// <summary>
        /// 現在のスクリプトの呼び出し履歴を変数で返します。
        /// </summary>
        /// <returns>Delegate配列</returns>
        public Variable GetStackTrace()
        {
            var trace = new Variable(Variable.VarType.ARRAY);
            trace.Tuple.Type = new TypeObject(Variable.VarType.STRING);
            foreach (var s in Utils.GetSpan(StackTrace))
            {
                trace.Tuple.Add(new Variable(s.ToString()));
            }
            return trace;
        }
        public string GetFilePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                string pathname = Path.Combine(PWD, path);
                if (File.Exists(pathname))
                {
                    return pathname;
                }
            }
            return path;
        }
        /// <summary>
        /// このスクリプトから変数を取得します。取得できない場合は親スクリプトも試みます。
        /// </summary>
        /// <param name="name">変数名</param>
        /// <param name="function">取得した変数</param>
        /// <returns>取得できた場合はtrue、それ以外の場合はfalse</returns>
        public bool TryGetVariable(string name, out ParserFunction function)
        {
            if (Variables.TryGetValue(name, out function))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.TryGetVariable(name, out function))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// このスクリプトから定数を取得します。取得できない場合は親スクリプトも試みます。
        /// </summary>
        /// <param name="name">変数名</param>
        /// <param name="function">取得した定数</param>
        /// <returns>取得できた場合はtrue、それ以外の場合はfalse</returns>
        public bool TryGetConst(string name, out ParserFunction function)
        {
            if (Consts.TryGetValue(name, out function))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.TryGetConst(name, out function))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// このスクリプトから関数を取得します。取得できない場合は親スクリプトも試みます。
        /// </summary>
        /// <param name="name">関数名</param>
        /// <param name="function">取得した関数</param>
        /// <returns>取得できた場合はtrue、それ以外の場合はfalse</returns>
        public bool TryGetFunction(string name, out ParserFunction function)
        {
            if (Functions.TryGetValue(name, out function))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.TryGetFunction(name, out function))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// このスクリプトから任意の識別子をもつ関数を取得します。取得できない場合は親スクリプトも試みます。
        /// </summary>
        /// <param name="name">識別子</param>
        /// <param name="function">取得した関数</param>
        /// <returns>取得できた場合はtrue、それ以外の場合はfalse</returns>
        public bool TryGetLocal(string name, out ParserFunction function)
        {
            return TryGetVariable(name, out function) || TryGetConst(name, out function) || TryGetFunction(name, out function);
        }
        public bool StartsWith(string str, bool caseSensitive = true)
        {
            if (string.IsNullOrEmpty(str) || str.Length > m_data.Length - m_from)
            {
                return false;
            }
            for (int i = m_from; i < m_data.Length && i < str.Length + m_from; i++)
            {
                var ch1 = str[i - m_from];
                var ch2 = m_data[i];

                if ((caseSensitive && ch1 != ch2) ||
                   (!caseSensitive && char.ToUpperInvariant(ch1) != char.ToUpperInvariant(ch2)))
                {
                    return false;
                }
            }

            return true;
        }
        public int Find(char ch, int from = -1)
        { return m_data.IndexOf(ch.ToString(), from < 0 ? m_from : from, StringComparison.Ordinal); }

        public int FindFirstOf(string str, int from = -1)
        { return FindFirstOf(str.ToCharArray(), from); }

        public int FindFirstOf(char[] arr, int from = -1)
        { return m_data.IndexOfAny(arr, from < 0 ? m_from : from); }

        public string Substr(int fr = -2, int len = -1)
        {
            int from = Math.Min(Pointer, m_data.Length - 1);
            fr = fr == -2 ? from : fr == -1 ? 0 : fr;
            return len < 0 || len >= m_data.Length - fr ? m_data.Substring(fr) : m_data.Substring(fr, len);
        }

        public string GetStack(int firstOffset = 0)
        {
            StringBuilder result = new StringBuilder();
            ParsingScript script = this;

            while (script != null)
            {
                int pointer = script == this ? script.Pointer + firstOffset : script.Pointer;
                int lineNumber = script.GetOriginalLineNumber(pointer);
                string filename = string.IsNullOrWhiteSpace(script.Filename) ? "" :
                                  Utils.GetFullPath(script.Filename);
                string line = string.IsNullOrWhiteSpace(filename) || !File.Exists(filename) ? "" :
                              File.ReadLines(filename).Skip(lineNumber).Take(1).First();

                result.AppendLine("" + lineNumber);
                result.AppendLine(filename);
                result.AppendLine(line.Trim());

                script = script.ParentScript;
            }

            return result.ToString().Trim();
        }

        public string GetOriginalLine(out int lineNumber)
        {
            lineNumber = GetOriginalLineNumber();
            if (lineNumber < 0 || m_originalScript == null)
            {
                return "";
            }

            string[] lines = m_originalScript.Split(Constants.END_LINE);
            return lineNumber < lines.Length ? lines[lineNumber] : "";
        }

        public int OriginalLineNumber => GetOriginalLineNumber();
        public string OriginalLine => GetOriginalLine(out int lineNumber);

        public int GetOriginalLineNumber()
        {
            return GetOriginalLineNumber(m_from);
        }
        public int GetOriginalLineNumber(int charNumber)
        {
            if (m_char2Line == null || m_char2Line.Count == 0)
            {
                return -1;
            }

            int pos = m_scriptOffset + charNumber;
            List<int> lineStart = m_char2Line.Keys.ToList();
            int lower = 0;
            int index = lower;

            if (pos <= lineStart[lower])
            { // First line.
                return m_char2Line[lineStart[lower]];
            }
            int upper = lineStart.Count - 1;
            if (pos >= lineStart[upper])
            { // Last line.
                return m_char2Line[lineStart[upper]];
            }

            while (lower <= upper)
            {
                index = (lower + upper) / 2;
                int guessPos = lineStart[index];
                if (pos == guessPos)
                {
                    break;
                }
                if (pos < guessPos)
                {
                    if (index == 0 || pos > lineStart[index - 1])
                    {
                        break;
                    }
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }

            int charIndex = lineStart[index];
            return m_char2Line[charIndex];
        }

        public char At(int i) { return m_data[i]; }
        public char CurrentAndForward() { return m_data[m_from++]; }

        public char TryCurrent()
        {
            return m_from < m_data.Length ? m_data[m_from] : Constants.EMPTY;
        }
        public char TryNext()
        {
            return m_from + 1 < m_data.Length ? m_data[m_from + 1] : Constants.EMPTY;
        }
        public char TryPrev(int count = 1)
        {
            return m_from >= count ? m_data[m_from - count] : Constants.EMPTY;
        }
        public string FromPrev(int backChars = 1, int maxChars = Constants.MAX_CHARS_TO_SHOW)
        {
            int from = Math.Max(0, m_from - backChars);
            int max = Math.Min(m_data.Length - from, maxChars);
            string result = m_data.Substring(from, max);
            return result;
        }
        public void Forward(int delta = 1) { m_from += delta; }
        public void Backward(int delta = 1)
        {
            if (m_from >= delta)
            {
                m_from -= delta;
            }
        }

        public void MoveForwardIf(char[] arr)
        {
            foreach (char ch in arr)
            {
                if (MoveForwardIf(ch))
                {
                    return;
                }
            }
        }
        public bool MoveForwardIf(char expected, char expected2 = Constants.EMPTY)
        {
            if (StillValid() && (Current == expected || Current == expected2))
            {
                Forward();
                return true;
            }
            return false;
        }
        public void MoveBackIf(char notExpected)
        {
            if (StillValid() && Pointer > 0 && Current == notExpected)
            {
                Backward();
            }
        }
        public void MoveBackIfPrevious(char ch)
        {
            if (Prev == ch)
            {
                Backward();
            }
        }
        public void MoveForwardIfNotPrevious(char ch)
        {
            if (Prev != ch)
            {
                Forward();
            }
        }
        public void SkipAllIfNotIn(char toSkip, char[] to)
        {
            if (to.Contains(toSkip))
            {
                return;
            }
            while (StillValid() && Current == toSkip)
            {
                Forward();
            }
        }

        public List<Variable> GetFunctionArgs(FunctionBase callFrom, char start = Constants.START_ARG,
                                      char end = Constants.END_ARG)
        {
            bool isList;
            List<Variable> args = Utils.GetArgs(this,
                                                start, end, (outList) => { isList = outList; }, callFrom);
            return args;
        }
        public int GoToNextStatement()
        {
            int endGroupRead = 0;
            while (StillValid())
            {
                char currentChar = Current;
                switch (currentChar)
                {
                    case Constants.END_GROUP:
                        endGroupRead++;
                        Forward();                  // '}'
                        return endGroupRead;
                    case Constants.START_GROUP:     // '{'
                    case Constants.QUOTE:           // '"'
                    case Constants.SPACE:           // ' '
                    case Constants.END_STATEMENT:   // ';'
                    case Constants.END_ARG:         // ')'
                        Forward();
                        break;
                    default: return endGroupRead;
                }
            }
            return endGroupRead;
        }

        public static Variable RunString(string str, ParsingScript script)
        {
            ParsingScript tempScript = script.GetTempScript(str);
            Variable result = tempScript.Execute();
            return result;
        }

        public Variable Execute(char[] toArray = null, int from = -1)
        {
            toArray = toArray ?? Constants.END_PARSE_ARRAY;
            Pointer = from < 0 ? Pointer : from;

            if (!m_data.EndsWith(Constants.END_STATEMENT.ToString(), StringComparison.Ordinal))
            {
                m_data += Constants.END_STATEMENT;
            }

            Variable result = null;


#if !DEBUG_THROW
            try
#endif
            {
                result = Parser.AliceScript(this, toArray);
            }
#if !DEBUG_THROW
            catch (ScriptException scriptExc)
            {
                OnThrowError(scriptExc, scriptExc.Message, scriptExc.ErrorCode, scriptExc.Source, scriptExc.HelpLink, scriptExc.Script ?? this, scriptExc.Exception);
            }
            catch (ParsingException parseExc)
            {
                OnThrowError(parseExc, parseExc.Message, Exceptions.COULDNT_PARSE, parseExc.Source, parseExc.HelpLink, this, parseExc);
            }
            catch (FileNotFoundException fileNotFoundExc)
            {
                OnThrowError(fileNotFoundExc, "ファイル" + (string.IsNullOrEmpty(fileNotFoundExc.FileName) ? string.Empty : " '" + fileNotFoundExc.FileName + "' ") + "が見つかりませんでした。", Exceptions.FILE_NOT_FOUND, fileNotFoundExc.Source, fileNotFoundExc.HelpLink);
            }
            catch (IndexOutOfRangeException indexOutOfRangeExc)
            {
                OnThrowError(indexOutOfRangeExc, "インデックスが配列の境界外です。", Exceptions.INDEX_OUT_OF_RANGE, indexOutOfRangeExc.Source);
            }
            catch (Exception otherExc)
            {
                OnThrowError(otherExc, otherExc.Message, Exceptions.NONE, otherExc.Source, otherExc.HelpLink);
            }

#endif
            return result;

        }
        private void OnThrowError(Exception exc, string message, Exceptions errorCode, string source, string helpLink = null, ParsingScript script = null, ParsingException parsingException = null)
        {
            var ex = new ThrowErrorEventArgs();
            ex.Message = message;
            ex.ErrorCode = errorCode;
            ex.HelpLink = helpLink;
            ex.Source = source;
            ex.Script = script;
            ex.Exception = parsingException;
            if (string.IsNullOrWhiteSpace(helpLink))
            {
                ex.HelpLink = Constants.HELP_LINK + ((int)ex.ErrorCode).ToString("x3");
            }
            if (script == null)
            {
                ex.Script = this;
            }
            ex.Script.OnThrowError(ex.Script, ex);
            if (!ex.Handled)
            {
                //続行されなかった場合は再スロー
                if (ParentScript != null)
                {
                    //throw exc;
                    ParentScript.OnThrowError(exc, message, errorCode, source, helpLink, script, parsingException);
                }
                else
                {
                    ThrowErrorManager.OnThrowError(ex.Script, ex);
                }
            }
        }

        public async Task<Variable> ExecuteAsync(char[] toArray = null, int from = -1)
        {
            toArray = toArray ?? Constants.END_PARSE_ARRAY;
            Pointer = from < 0 ? Pointer : from;

            if (!m_data.EndsWith(Constants.END_STATEMENT.ToString(), StringComparison.Ordinal))
            {
                m_data += Constants.END_STATEMENT;
            }

            Variable result = null;


            result = await Parser.AliceScriptAsync(this, toArray);
            try
            {
                result = await Parser.AliceScriptAsync(this, toArray);
            }
            catch (Exception e)
            {
                ThrowErrorEventArgs ex = new ThrowErrorEventArgs();
                ex.Message = e.Message;
                ex.Script = this;

                if (e is ScriptException scriptExc)
                {
                    ex.ErrorCode = scriptExc.ErrorCode;
                    ex.Exception = scriptExc.Exception;
                    if (scriptExc.Script != null)
                    {
                        ex.Script = scriptExc.Script;
                    }
                }
                else if (e is ParsingException parseExc)
                {
                    ex.ErrorCode = Exceptions.COULDNT_PARSE;
                    ex.Exception = parseExc;
                }
                ex.Script.OnThrowError(ex.Script, ex);
                if (ex.Handled)
                {
                    return result;
                }
                ThrowError?.Invoke(ex.Script, ex);
            }
            return result;
        }

        public Variable ExecuteAll()
        {
            Variable result = null;
            while (StillValid())
            {
                result = Execute(Constants.END_LINE_ARRAY);
                GoToNextStatement();
            }
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            return result;
        }
        public Variable Process(bool checkBreak = false)
        {
            Variable result = null;
            while (Pointer < m_data.Length)
            {
                result = Execute();
                if (result == null)
                {
                    result = Variable.EmptyInstance;
                }
                if (checkBreak && (result.IsReturn || result.Type == Variable.VarType.BREAK))
                {
                    return result;
                }
                GoToNextStatement();
            }
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            return result;
        }
        public async Task<Variable> ProcessAsync()
        {
            Variable result = null;
            while (Pointer < m_data.Length)
            {
                result = await ExecuteAsync();
                GoToNextStatement();
            }
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            return result;
        }
        public void SkipBlock()
        {
            int blockStart = Pointer;
            int startCount = 0;
            int endCount = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            char previous = Constants.EMPTY;
            char prevprev = Constants.EMPTY;

            while (startCount == 0 || startCount > endCount)
            {
                if (!StillValid())
                {
                    throw new ScriptException("次のブロックを実行できませんでした [" +
                    Substr(blockStart, Constants.MAX_CHARS_TO_SHOW) + "]", Exceptions.COULDNT_EXECUTE_BLOCK, this);
                }
                char currentChar = CurrentAndForward();
                switch (currentChar)
                {
                    case Constants.QUOTE1:
                        if (!inQuotes2 && (previous != '\\' || prevprev == '\\'))
                        {
                            inQuotes = inQuotes1 = !inQuotes1;
                        }
                        break;
                    case Constants.QUOTE:
                        if (!inQuotes1 && (previous != '\\' || prevprev == '\\'))
                        {
                            inQuotes = inQuotes2 = !inQuotes2;
                        }
                        break;
                    case Constants.START_GROUP:
                        if (!inQuotes)
                        {
                            startCount++;
                        }
                        break;
                    case Constants.END_GROUP:
                        if (!inQuotes)
                        {
                            endCount++;
                        }
                        break;
                }
                prevprev = previous;
                previous = currentChar;
            }
            if (startCount > endCount)
            {
                throw new ScriptException("波括弧が不足しています", Exceptions.NEED_BRACKETS, this);
            }
            else if (startCount < endCount)
            {
                throw new ScriptException("終端の波括弧は不要です", Exceptions.UNNEED_TO_BRACKETS, this);
            }
        }
        public void SkipRestBlocks()
        {
            ParsingScript nextData = new ParsingScript(this);
            nextData.Forward();
            while (StillValid())
            {
                int endOfToken = nextData.Pointer;
                string nextToken = Utils.GetNextToken(nextData);
                if (Constants.ELSE_IF != nextToken &&
                    Constants.ELSE != nextToken)
                {
                    Pointer = endOfToken;
                    return;
                }
                nextData.SkipBlock();
            }
            Pointer = nextData.Pointer;
        }
        /// <summary>
        /// 波かっこで始まって終わるブロックを子スクリプトとして実行します
        /// </summary>
        /// <param name="inForOrWhile">forブロックやwhileブロックなど、breakなどで抜けるブロック</param>
        /// <returns>ブロックの値</returns>
        public Variable ProcessBlock(bool inForOrWhile = false)
        {
            string body = Utils.GetBodyBetween(this, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript mainScript = GetTempScript(body);
            return mainScript.Process(inForOrWhile);
        }

        public ParsingScript GetTempScript(string str, FunctionBase callFrom = null, int startIndex = 0)
        {
            str = Utils.ConvertToScript(str, out _, out var def, out var settings);
            ParsingScript tempScript = new ParsingScript(str, startIndex);
            tempScript.Settings = settings;
            tempScript.Defines = def;
            tempScript.Filename = Filename;
            tempScript.ParentScript = this;
            tempScript.Char2Line = Char2Line;
            tempScript.OriginalScript = OriginalScript;
            tempScript.StackLevel = StackLevel;
            tempScript.AllLabels = AllLabels;
            tempScript.LabelToFile = LabelToFile;
            tempScript.FunctionName = FunctionName;
            tempScript.Tag = Tag;
            tempScript.Package = Package;
            tempScript.Generation = Generation + 1;
            tempScript.ThrowError = ThrowError;
            tempScript.m_stacktrace = new List<StackInfo>(m_stacktrace);
            if (callFrom != null)
            {
                tempScript.m_stacktrace.Add(new StackInfo(callFrom, OriginalLine, OriginalLineNumber, Filename));
            }

            return tempScript;
        }
        public ParsingScript GetIncludeFileScript(string filename, FunctionBase callFrom)
        {
            if (EnableInclude)
            {
                string includeFile = GetIncludeFileLine(filename, out string pathname, out bool isPackageFile);
                var includeScript = Utils.ConvertToScript(includeFile, out Dictionary<int, int> char2Line, out var def, out var setting, pathname);
                ParsingScript tempScript = new ParsingScript(includeScript, 0, char2Line);
                tempScript.TopInFile = true;
                tempScript.Settings = setting;
                tempScript.Filename = pathname;
                tempScript.OriginalScript = includeFile.Replace(Environment.NewLine, Constants.END_LINE.ToString());
                tempScript.ParentScript = this;
                tempScript.Tag = Tag;
                tempScript.Generation = Generation + 1;
                tempScript.ThrowError = ThrowError;
                tempScript.m_stacktrace = new List<StackInfo>(m_stacktrace);


                if (callFrom != null)
                {
                    tempScript.m_stacktrace.Add(new StackInfo(callFrom, OriginalLine, OriginalLineNumber, Filename));
                }
                if (isPackageFile)
                {
                    tempScript.Package = Package;
                }

                return tempScript;
            }
            else
            {
                throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, this);
            }
        }
        private string GetIncludeFileLine(string filename, out string pathname, out bool isPackageFile)
        {
            pathname = filename;
            if (Package != null && Package.ExistsEntry(pathname))
            {
                isPackageFile = true;
                return AlicePackage.GetEntryScript(Package.archive.GetEntry(pathname), pathname);
            }
            isPackageFile = false;
            pathname = GetFilePath(filename);
            return Utils.GetFileLines(pathname);
        }
    }

    public class ParsingException : Exception
    {
        public ParsingScript ExceptionScript { get; private set; }
        public string ExceptionStack { get; private set; } = "";

        public ParsingException(string message, string excStack = "")
            : base(message)
        {
            ExceptionStack = excStack.Trim();
        }
        public ParsingException(string message, ParsingScript script)
            : base(message)
        {
            ExceptionScript = script;
            ExceptionStack = script.GetStack(-1);
        }
        public ParsingException(string message, ParsingScript script, Exception inner)
            : base(message, inner)
        {
            ExceptionScript = script;
            ExceptionStack = script.GetStack(-1);
        }
    }
}
