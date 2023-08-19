using System.IO;
using System.Text;

namespace AliceScript
{
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
        private List<string> m_defines = new List<string>();// 現在のスクリプトで宣言されたシンボル
        private static ParsingScript m_toplevel_script = new ParsingScript("", 0, null);// 最上位のスクリプト
        private ParsingScript m_parentScript = m_toplevel_script;// このスクリプトの親
        private Dictionary<int, int> m_char2Line = null; // 元の行へのポインタ
        private Dictionary<string, ParserFunction> m_variables = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された変数
        private Dictionary<string, ParserFunction> m_consts = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された定数
        private Dictionary<string, ParserFunction> m_functions = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された関数
        private List<NameSpace> m_namespace = new List<NameSpace>();
        internal List<StackInfo> m_stacktrace = new List<StackInfo>();

        /// <summary>
        /// 最上位のスクリプトを取得します
        /// </summary>
        /// <param name="script">呼び出し元のスクリプト</param>
        /// <returns>最上位のスクリプト</returns>
        public static ParsingScript GetTopLevelScript(ParsingScript script)
        {
            if (m_toplevel_script.ParentScript != null)
            {
                //トップレベルスクリプトは親を持たない
                m_toplevel_script.ParentScript = null;
            }
            if (script != null && script.ContainsSymbol(Constants.DENY_TO_TOPLEVEL_SCRIPT))
            {
                throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, script);
            }
            return m_toplevel_script;
        }

        /// <summary>
        /// このスクリプトで宣言されたシンボル
        /// </summary>
        public List<string> Defines
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
                    s.Add(new StackInfo(ProcessingFunction, this.OriginalLine, this.OriginalLineNumber, this.Filename));
                }
                return s;
            }
        }

        /// <summary>
        /// StackTraceで使用するひとつ当たりのスタック
        /// </summary>
        public class StackInfo
        {
            public FunctionBase Function { get; set; }
            public string Line { get; set; }
            public int LineNumber { get; set; }
            public string FileName { get; set; }

            public StackInfo()
            {

            }
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
                    sb.Append(k + " ");
                }
                if (Function.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE) || Function.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE_ONC))
                {
                    sb.Append(Constants.COMMAND + " ");
                }
                if ((Function is CustomFunction))
                {
                    sb.Append("custom ");
                }
                else if (Function.Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE))
                {
                    sb.Append("keyword ");
                }
                sb.Append(Constants.FUNCTION + " ");
                if (!string.IsNullOrEmpty(Function.RelatedNameSpace))
                {
                    sb.Append(Function.RelatedNameSpace+".") ;
                }
                sb.Append((string.IsNullOrWhiteSpace(Function.Name) ? "Anonymous" : Function.Name) + "(");
                int args_count = 0;
                if (Function is CustomFunction cf && cf.RealArgs != null && cf.RealArgs.Length > 0)
                {
                    foreach (string a in Function.RealArgs)
                    {
                        sb.Append(a + (++args_count == Function.RealArgs.Length ? string.Empty : ","));
                    }
                }
                sb.Append(");");
                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    sb.Append(" 場所 ");
                    sb.Append(FileName);
                    sb.Append(":行 " + LineNumber);
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
        public List<NameSpace> UsingNamespaces
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
        /// 他のスクリプトに対してこのスクリプトの例外処理情報を引き継ぎます
        /// </summary>
        /// <param name="other">引き継ぐ対象のスクリプト</param>
        internal void CloneThrowTryInfo(ParsingScript other)
        {
            other.InTryBlock = this.InTryBlock;
            other.ThrowError = this.ThrowError;
        }

        public string Rest => Substr(m_from, Constants.MAX_CHARS_TO_SHOW);
        public char Current => m_from < m_data.Length ? m_data[m_from] : Constants.EMPTY;
        public char Prev => m_from >= 1 ? m_data[m_from - 1] : Constants.EMPTY;
        public char PrevPrev => m_from >= 2 ? m_data[m_from - 2] : Constants.EMPTY;
        public char Next => m_from + 1 < m_data.Length ? m_data[m_from + 1] : Constants.EMPTY;
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

        public Dictionary<string, Dictionary<string, int>> AllLabels
        {
            get;
            set;
        }
        public Dictionary<string, string> LabelToFile
        {
            get;
            set;
        }

        public List<int> PointersBack { get; set; } = new List<int>();

        private string m_functionName = "";
        public string FunctionName
        {
            get => m_functionName;
            set => m_functionName = value.ToLower();
        }

        public ParserFunction.StackLevel StackLevel { get; set; }
        public bool ProcessingList { get; set; }

        public bool DisableBreakpoints;
        public bool InTryBlock;
        public string MainFilename;

        public ParsingScript ParentScript
        {
            get
            {
                return m_parentScript;
            }
            set
            {
                m_parentScript = value;
            }
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
                Using(Constants.TOP_NAMESPACE);
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
            InTryBlock = other.InTryBlock;
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
        public void Using(string name)
        {
            if (NameSpaceManerger.Contains(name))
            {
                this.UsingNamespaces.Add(NameSpaceManerger.NameSpaces[name]);
            }
            else
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
            foreach (var s in this.StackTrace)
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
        public bool TryGetLocal(string name,out ParserFunction function)
        {
            return (TryGetVariable(name, out function) || TryGetConst(name, out function) || TryGetFunction(name, out function));
        }
        public bool StartsWith(string str, bool caseSensitive = true)
        {
            if (String.IsNullOrEmpty(str) || str.Length > m_data.Length - m_from)
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
        { return m_data.IndexOf(ch, from < 0 ? m_from : from); }

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
            if (lineNumber < lines.Length)
            {
                return lines[lineNumber];
            }

            return "";
        }

        public int OriginalLineNumber => GetOriginalLineNumber();
        public string OriginalLine
        {
            get
            {
                int lineNumber;
                return GetOriginalLine(out lineNumber);
            }
        }

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
        public char TryPrev()
        {
            return m_from >= 1 ? m_data[m_from - 1] : Constants.EMPTY;
        }
        public char TryPrevPrev()
        {
            return m_from >= 2 ? m_data[m_from - 2] : Constants.EMPTY;
        }
        public char TryPrevPrevPrev()
        {
            return m_from >= 3 ? m_data[m_from - 3] : Constants.EMPTY;
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

        /// <summary>
        /// シンボルがこのスクリプトまたは親スクリプトで定義されているかどうかを判定します
        /// </summary>
        /// <param name="symbol">シンボル</param>
        /// <returns>定義されている場合はTrue、それ以外の場合はFalse。</returns>
        public bool ContainsSymbol(string symbol)
        {
            bool b = m_defines.Contains(symbol);
            if (!b && !m_defines.Contains(Constants.RESET_DEFINES) && ParentScript!=null)
            {
                return ParentScript.ContainsSymbol(symbol);
            }
            return b;
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
            toArray = toArray == null ? Constants.END_PARSE_ARRAY : toArray;
            Pointer = from < 0 ? Pointer : from;

            if (!m_data.EndsWith(Constants.END_STATEMENT.ToString()))
            {
                m_data += Constants.END_STATEMENT;
            }

            Variable result = null;


            if (InTryBlock)
            {
                result = Parser.AliceScript(this, toArray);
            }
            else
            {
#if !DEBUG_THROW
                try
#endif
                {
                    result = Parser.AliceScript(this, toArray);
                }
#if !DEBUG_THROW
                catch (ScriptException scriptExc)
                {
                    OnThrowError(scriptExc.Message,scriptExc.ErrorCode, scriptExc.Source, scriptExc.HelpLink,scriptExc.Script,scriptExc.Exception);
                }
                catch(ParsingException parseExc)
                {
                    OnThrowError(parseExc.Message, Exceptions.COULDNT_PARSE, parseExc.Source, parseExc.HelpLink, this,parseExc);
                }
                catch(FileNotFoundException fileNotFoundExc)
                {
                    OnThrowError("ファイル"+ (string.IsNullOrEmpty(fileNotFoundExc.FileName) ? string.Empty : " '" + fileNotFoundExc.FileName + "' ")+"が見つかりませんでした。", Exceptions.FILE_NOT_FOUND, fileNotFoundExc.Source,fileNotFoundExc.HelpLink);
                }
                catch (IndexOutOfRangeException indexOutOfRangeExc)
                {
                    OnThrowError("インデックスが配列の境界外です。",Exceptions.INDEX_OUT_OF_RANGE,indexOutOfRangeExc.Source);
                }
                catch(Exception otherExc)
                {
                    OnThrowError(otherExc.Message,Exceptions.NONE,otherExc.Source,otherExc.HelpLink);
                }
               
#endif
            }
            return result;

        }
        private void OnThrowError(string message,Exceptions errorCode,string source, string helpLink=null, ParsingScript script=null,ParsingException parsingException=null)
        {
            var ex = new ThrowErrorEventArgs();
            ex.Message=message;
            ex.ErrorCode=errorCode;
            ex.HelpLink=helpLink;
            ex.Source=source;
            ex.Script=script;
            ex.Exception=parsingException;
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
                ThrowErrorManerger.OnThrowError(ex.Script, ex);
            }
        }

        public async Task<Variable> ExecuteAsync(char[] toArray = null, int from = -1)
        {
            toArray = toArray == null ? Constants.END_PARSE_ARRAY : toArray;
            Pointer = from < 0 ? Pointer : from;

            if (!m_data.EndsWith(Constants.END_STATEMENT.ToString()))
            {
                m_data += Constants.END_STATEMENT;
            }

            Variable result = null;


            if (InTryBlock)
            {
                result = await Parser.AliceScriptAsync(this, toArray);
            }
            else
            {
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
                    if (this.InTryBlock) { return result; }
                    ThrowError?.Invoke(ex.Script, ex);
                }
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
            return result;
        }
        public Variable Process()
        {
            Variable result = null;
            while (this.Pointer < m_data.Length)
            {
                result = this.Execute();
                this.GoToNextStatement();
            }
            return result;
        }
        public Variable ProcessForWhile()
        {
            Variable result = null;
            while (this.Pointer < m_data.Length)
            {
                result = this.Execute();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    return result;
                }
                this.GoToNextStatement();
            }
            return result;
        }
        public async Task<Variable> ProcessAsync()
        {
            Variable result = null;
            while (this.Pointer < m_data.Length)
            {
                result = await this.ExecuteAsync();
                this.GoToNextStatement();
            }
            return result;
        }
        public void SkipBlock()
        {
            int blockStart = this.Pointer;
            int startCount = 0;
            int endCount = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            char previous = Constants.EMPTY;
            char prevprev = Constants.EMPTY;
            while (startCount == 0 || startCount > endCount)
            {
                if (!this.StillValid())
                {
                    throw new ScriptException("次のブロックを実行できませんでした [" +
                    this.Substr(blockStart, Constants.MAX_CHARS_TO_SHOW) + "]", Exceptions.COULDNT_EXECUTE_BLOCK, this);
                }
                char currentChar = this.CurrentAndForward();
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
            while (StillValid())
            {
                int endOfToken = this.Pointer;
                ParsingScript nextData = new ParsingScript(this);
                string nextToken = Utils.GetNextToken(nextData);
                if (Constants.ELSE_IF != nextToken &&
                    Constants.ELSE != nextToken)
                {
                    return;
                }
                this.Pointer = nextData.Pointer;
                SkipBlock();
            }
        }
        /// <summary>
        /// 波かっこで始まって終わるブロックを子スクリプトとして実行します
        /// </summary>
        /// <returns>ブロックの値</returns>
        public Variable ProcessBlock(bool blockmode=true)
        {
            string body = Utils.GetBodyBetween(this, Constants.START_GROUP, Constants.END_GROUP, "\0", blockmode);
            ParsingScript mainScript = this.GetTempScript(body);
            return mainScript.Process();
        }
        public ParsingScript GetTempScript(string str, FunctionBase callFrom = null, int startIndex = 0)
        {
            str = Utils.ConvertToScript(str, out _, out var def);
            ParsingScript tempScript = new ParsingScript(str, startIndex);
            tempScript.Defines = def;
            tempScript.Filename = this.Filename;
            tempScript.ParentScript = this;
            tempScript.Char2Line = this.Char2Line;
            tempScript.OriginalScript = this.OriginalScript;
            tempScript.InTryBlock = this.InTryBlock;
            tempScript.StackLevel = this.StackLevel;
            tempScript.AllLabels = this.AllLabels;
            tempScript.LabelToFile = this.LabelToFile;
            tempScript.FunctionName = this.FunctionName;
            tempScript.Tag = this.Tag;
            tempScript.Package = this.Package;
            tempScript.Generation = this.Generation + 1;
            tempScript.ThrowError = this.ThrowError;
            tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(m_stacktrace);
            if (callFrom != null)
            {
                tempScript.m_stacktrace.Add(new StackInfo(callFrom, this.OriginalLine, this.OriginalLineNumber, this.Filename));
            }
            else
            {
          //      tempScript.m_stacktrace.Add(new StackInfo(ProcessingFunction, this.OriginalLine, this.OriginalLineNumber, this.Filename));
            }

            return tempScript;
        }
        public ParsingScript GetIncludeFileScript(string filename, FunctionBase callFrom)
        {
            string pathname;
            if (!this.ContainsSymbol(Constants.DISABLE_INCLUDE))
            {
                bool isPackageFile;
                string includeFile = GetIncludeFileLine(filename, out pathname, out isPackageFile);
                Dictionary<int, int> char2Line;
                var includeScript = Utils.ConvertToScript(includeFile, out char2Line, out var def, pathname);
                ParsingScript tempScript = new ParsingScript(includeScript, 0, char2Line);
                tempScript.Filename = pathname;
                tempScript.OriginalScript = includeFile.Replace(Environment.NewLine, Constants.END_LINE.ToString());
                tempScript.ParentScript = this;
                tempScript.InTryBlock = InTryBlock;
                tempScript.Tag = this.Tag;
                tempScript.Generation = this.Generation + 1;
                tempScript.ThrowError = this.ThrowError;
                tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(this.m_stacktrace);

                if (!this.ContainsSymbol(Constants.FOLLOW_INCLUDE))
                {
                    tempScript.Defines.Add(Constants.RESET_DEFINES);
                }

                if (callFrom != null)
                {
                    tempScript.m_stacktrace.Add(new StackInfo(callFrom, this.OriginalLine, this.OriginalLineNumber, this.Filename));
                }
                if (isPackageFile)
                {
                    tempScript.Package = this.Package;
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
