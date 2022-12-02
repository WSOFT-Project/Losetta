﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private Dictionary<int, int> m_char2Line = null; // 元の行へのポインタ
        private Dictionary<string, ParserFunction> m_variables = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された変数
        private Dictionary<string, ParserFunction> m_consts = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された定数
        private Dictionary<string, ParserFunction> m_functions = new Dictionary<string, ParserFunction>();// スクリプトの内部で定義された関数
        /// <summary>
        /// このスクリプトに関連付けられたオブジェクトです
        /// </summary>
        public object Tag
        {
            get
            {
                return m_tag;
            }
            set
            {
                m_tag = value;
            }
        }
        /// <summary>
        /// これが実行されているパッケージを表します
        /// </summary>
        public AlicePackage Package
        {
            get { return m_package; }
            set { m_package = value; }
        }
        /// <summary>
        /// 現在のスクリプトの世代数を取得または設定します
        /// </summary>
        public int Generation
        {
            get { return m_generation; }
            set { m_generation = value; }
        }
        /// <summary>
        /// 現在のスクリプトのポインタを取得または設定します
        /// </summary>
        public int Pointer
        {
            get { return m_from; }
            set { m_from = value; }
        }
        /// <summary>
        /// 現在のスクリプト全体を取得または設定します
        /// </summary>
        public string String
        {
            get { return m_data; }
            set { m_data = value; }
        }
        /// <summary>
        /// 現在のスクリプト内で定義された変数
        /// </summary>
        public Dictionary<string,ParserFunction> Variables
        {
            get { return m_variables; }
            set { m_variables = value; }
        }
        /// <summary>
        /// 現在のスクリプト内で定義された関数
        /// </summary>
        public Dictionary <string, ParserFunction> Functions
        {
            get { return m_functions; }
            set { m_functions = value; }
        }
        /// <summary>
        /// 現在のスクリプト内で定義された定数
        /// </summary>
        public Dictionary<string,ParserFunction> Consts
        {
            get { return m_consts; }
            set { m_consts = value; }
        }
        public string Rest
        {
            get { return Substr(m_from, Constants.MAX_CHARS_TO_SHOW); }
        }
        public char Current
        {
            get { return m_from < m_data.Length ? m_data[m_from] : Constants.EMPTY; }
        }
        public char Prev
        {
            get { return m_from >= 1 ? m_data[m_from - 1] : Constants.EMPTY; }
        }
        public char PrevPrev
        {
            get { return m_from >= 2 ? m_data[m_from - 2] : Constants.EMPTY; }
        }
        public char Next
        {
            get { return m_from + 1 < m_data.Length ? m_data[m_from + 1] : Constants.EMPTY; }
        }
        public Dictionary<int, int> Char2Line
        {
            get { return m_char2Line; }
            set { m_char2Line = value; }
        }
        public int ScriptOffset
        {
            get { return m_scriptOffset; }
            set { m_scriptOffset = value; }
        }
        public string Filename
        {
            get { return m_filename; }
            set
            {
                m_filename = Utils.GetFullPath(value);
            }
        }
        public string PWD
        {
            get
            {
                return Utils.GetDirectoryName(m_filename);
            }
        }
        public string OriginalScript
        {
            get { return m_originalScript; }
            set { m_originalScript = value; }
        }

        public string CurrentAssign { get; set; }


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
            get { return m_functionName; }
            set { m_functionName = value.ToLower(); }
        }

        public ParserFunction.StackLevel StackLevel { get; set; }
        public bool ProcessingList { get; set; }

        public bool DisableBreakpoints;
        public bool InTryBlock;
        public string MainFilename;

        public ParsingScript ParentScript;

        public AliceScriptClass CurrentClass { get; set; }
        public AliceScriptClass.ClassInstance ClassInstance { get; set; }

        public ParsingScript(string data, int from = 0,
                             Dictionary<int, int> char2Line = null)
        {
            m_data = data;
            m_from = from;
            m_char2Line = char2Line;
        }

        public ParsingScript(ParsingScript other)
        {
            m_data = other.String;
            m_from = other.Pointer;
            m_char2Line = other.Char2Line;
            m_filename = other.Filename;
            m_originalScript = other.OriginalScript;
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
        }

        public int Size() { return m_data.Length; }
        public bool StillValid() { return m_from < m_data.Length; }

        public void SetDone() { m_from = m_data.Length; }

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
        public bool TryGetVariable(string name,out ParserFunction function)
        {
            if(Variables.TryGetValue(name,out function))
            {
                return true;
            }
            else
            {
                if (ParentScript != null&&ParentScript.TryGetVariable(name,out function))
                {
                    return true;
                }
            }
            return false;
        }
        public bool ContainsVariable(string name)
        {
            if (Variables.ContainsKey(name))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.ContainsVariable(name))
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
        public bool ContainsConst(string name)
        {
            if (Consts.ContainsKey(name))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.ContainsConst(name))
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
        public bool ContainsFunction(string name)
        {
            if (Functions.ContainsKey(name))
            {
                return true;
            }
            else
            {
                if (ParentScript != null && ParentScript.ContainsFunction(name))
                {
                    return true;
                }
            }
            return false;
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

        public int OriginalLineNumber { get { return GetOriginalLineNumber(); } }
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

        public List<Variable> GetFunctionArgs(char start = Constants.START_ARG,
                                      char end = Constants.END_ARG)
        {
            bool isList;
            List<Variable> args = Utils.GetArgs(this,
                                                start, end, (outList) => { isList = outList; });
            return args;
        }
        public async Task<List<Variable>> GetFunctionArgsAsync(char start = Constants.START_ARG,
                                      char end = Constants.END_ARG)
        {
            bool isList;
            List<Variable> args = await Utils.GetArgsAsync(this,
                                                start, end, (outList) => { isList = outList; });
            return args;
        }

        public bool IsProcessingFunctionCall()
        {
            if (TryPrev() == Constants.START_ARG || TryCurrent() == Constants.START_ARG)
            {
                return true;
            }
            return false;
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

        public static Variable RunString(string str)
        {
            ParsingScript tempScript = new ParsingScript(str);
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
                bool before = ThrowErrorManerger.InTryBlock;
                ThrowErrorManerger.InTryBlock = true;
                result = Parser.AliceScript(this, toArray);
                ThrowErrorManerger.InTryBlock = before;
            }
            else
            {
                try
                {
                    result = Parser.AliceScript(this, toArray);
                }
                catch (HandledErrorException)
                {

                }
                catch (ParsingException parseExc)
                {
                    if (!this.InTryBlock)
                    {

                        if (ThrowErrorManerger.HandleError)
                        {
                            ThrowErrorManerger.OnThrowError(parseExc.Message, Exceptions.NONE, this, parseExc);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception exc)
                {
                    if (!this.InTryBlock)
                    {
                        ParsingException parseExc = new ParsingException(exc.Message, this, exc);
                        if (ThrowErrorManerger.HandleError)
                        {
                            ThrowErrorManerger.OnThrowError(parseExc.Message, Exceptions.NONE, this, parseExc);
                        }
                        else
                        {
                            throw parseExc;
                        }
                    }
                }
            }
            return result;
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
                catch (HandledErrorException)
                {

                }
                catch (ParsingException parseExc)
                {
                    if (!this.InTryBlock)
                    {

                        if (ThrowErrorManerger.HandleError)
                        {
                            ThrowErrorManerger.OnThrowError(parseExc.Message, Exceptions.NONE, this, parseExc);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                catch (Exception exc)
                {
                    if (!this.InTryBlock)
                    {
                        ParsingException parseExc = new ParsingException(exc.Message, this, exc);
                        if (ThrowErrorManerger.HandleError)
                        {
                            ThrowErrorManerger.OnThrowError(parseExc.Message, Exceptions.NONE, this, parseExc);
                        }
                        else
                        {
                            throw parseExc;
                        }
                    }
                }
            }
            return result;
        }

        public void ExecuteAll()
        {
            while (StillValid())
            {
                Execute(Constants.END_LINE_ARRAY);
                GoToNextStatement();
            }
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
                if (result.IsReturn||result.Type == Variable.VarType.BREAK)
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
                result =await this.ExecuteAsync();
                this.GoToNextStatement();
            }
            return result;
        }
        public async Task<Variable> ProcessForWhileAsync()
        {
            Variable result = null;
            while (this.Pointer < m_data.Length)
            {
                result = await this.ExecuteAsync();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    return result;
                }
                this.GoToNextStatement();
            }
            return result;
        }

        public ParsingScript GetTempScript(string str, int startIndex = 0)
        {
            ParsingScript tempScript = new ParsingScript(str, startIndex);
            tempScript.Filename = this.Filename;
            tempScript.InTryBlock = this.InTryBlock;
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

            return tempScript;
        }

        public ParsingScript GetIncludeFileScript(string filename)
        {
            string pathname = "";bool isPackageFile;
            string includeFile = GetIncludeFileLine(filename,out pathname,out isPackageFile);
            Dictionary<int, int> char2Line;
            var includeScript = Utils.ConvertToScript(includeFile, out char2Line, pathname);
            ParsingScript tempScript = new ParsingScript(includeScript, 0, char2Line);
            tempScript.Filename = pathname;
            tempScript.OriginalScript = includeFile.Replace(Environment.NewLine, Constants.END_LINE.ToString());
            tempScript.ParentScript = this;
            tempScript.InTryBlock = InTryBlock;
            tempScript.Tag = this.Tag;
            tempScript.Generation = this.Generation + 1;
            if (isPackageFile)
            {
                tempScript.Package = this.Package;
            }

            return tempScript;
        }
        private string GetIncludeFileLine(string filename,out string pathname,out bool isPackageFile)
        {
            pathname = filename;
            if (Package != null&&Package.ExistsEntry(pathname))
            {
                isPackageFile = true;
                return AlicePackage.GetEntryScript(Package.archive.GetEntry(pathname),pathname);
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
