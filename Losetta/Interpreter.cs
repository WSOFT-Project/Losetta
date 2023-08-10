using AliceScript.Interop;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace AliceScript
{
    public class OutputAvailableEventArgs : EventArgs
    {
        public OutputAvailableEventArgs(string output)
        {
            Output = output;
        }
        public string Output { get; set; }
    }
    public class ReadInputEventArgs : EventArgs
    {
        public string Input { get; set; }
    }

    public partial class Interpreter
    {

        private static Interpreter instance;
        private bool m_bHasBeenInitialized = false;

        private Interpreter()
        {
            Init();
        }

        public static Interpreter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Interpreter();
                }
                return instance;
            }
        }

        public string Name => Assembly.GetExecutingAssembly().GetName().Name;


        private StringBuilder m_output = new StringBuilder();
        public string Output
        {
            get
            {
                string output = m_output.ToString().Trim();
                m_output.Clear();
                return output;
            }
        }

        public event EventHandler<OutputAvailableEventArgs> OnOutput;
        public event EventHandler<OutputAvailableEventArgs> OnData;
        public event EventHandler<OutputAvailableEventArgs> OnDebug;
        public event EventHandler<ReadInputEventArgs> OnInput;

        public string ReadInput()
        {
            var handler = OnInput;
            if (handler != null)
            {
                var args = new ReadInputEventArgs();
                handler(this, args);
                return args.Input;
            }
            else
            {
                return Console.ReadLine();
            }

        }
        public void AppendOutput(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnOutput;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }
        public void AppendDebug(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnDebug;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }

        public bool AppendData(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnData;
            if (handler != null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
                return true;
            }
            return false;
        }

        public void Init()
        {
            if (m_bHasBeenInitialized)
            {
                return;
            }

            m_bHasBeenInitialized = true; // このメソッドは一度のみ呼び出すことができます


            RegisterFunctions();
            RegisterActions();

            Alice_Interpreter_Initer.Init();
        }

        public void RegisterFunctions()
        {
            NameSpaceManerger.Add(new NameSpace(Constants.TOP_NAMESPACE));

            FunctionBaseManerger.Add(new ClassCreator());
            FunctionBaseManerger.Add(new FunctionCreator());
            FunctionBaseManerger.Add(new EnumFunction());
            FunctionBaseManerger.Add(new NamespaceFunction());
            FunctionBaseManerger.Add(new ArrayTypeFunction());

            ParserFunction.AddAction(Constants.LABEL_OPERATOR, new LabelFunction());
            ParserFunction.AddAction(Constants.POINTER, new PointerFunction());
            ParserFunction.AddAction(Constants.POINTER_REF, new PointerReferenceFunction());
        }


        public void RegisterActions()
        {
            ParserFunction.AddAction(Constants.ASSIGNMENT, new AssignFunction());
            ParserFunction.AddAction(Constants.INCREMENT, new IncrementDecrementFunction());
            ParserFunction.AddAction(Constants.DECREMENT, new IncrementDecrementFunction());


            for (int i = 0; i < Constants.OPER_ACTIONS.Length; i++)
            {
                ParserFunction.AddAction(Constants.OPER_ACTIONS[i], new OperatorAssignFunction());
            }
        }

        public Variable ProcessFile(string filename, bool mainFile = false)
        {
            if (!File.Exists(filename))
            {
                throw new ScriptException("ファイルが存在しません", Exceptions.FILE_NOT_FOUND);
            }
            byte[] data = File.ReadAllBytes(filename);
            return ProcessData(data, filename, mainFile);
        }
        public Variable ProcessData(byte[] data, string filename = "", bool mainFile = false)
        {
            if (IsEqualMagicnumber(data, Constants.PACKAGE_MAGIC_NUMBER))
            {
                AlicePackage.LoadEncodingPackage(data, filename);
                return Variable.EmptyInstance;
            }
            else if (IsEqualMagicnumber(data, Constants.DLL_MAGIC_NUMBER))
            {
                NetLibraryLoader.LoadLibrary(data);
                return Variable.EmptyInstance;
            }
            else if (IsEqualMagicnumber(data, Constants.ZIP_MAGIC_NUMBER))
            {
                AlicePackage.LoadArchive(new ZipArchive(new MemoryStream(data)), filename);
                return Variable.EmptyInstance;
            }
            string script = Utils.GetFileContents(data);
            return Process(script, filename, mainFile, null, null);
        }
        public async Task<Variable> ProcessDataAsync(byte[] data, string filename = "", bool mainFile = false)
        {
            if (IsEqualMagicnumber(data, Constants.PACKAGE_MAGIC_NUMBER))
            {
                AlicePackage.LoadEncodingPackage(data, filename);
                return Variable.EmptyInstance;
            }
            else if (IsEqualMagicnumber(data, Constants.DLL_MAGIC_NUMBER))
            {
                NetLibraryLoader.LoadLibrary(data);
                return Variable.EmptyInstance;
            }
            else if (IsEqualMagicnumber(data, Constants.ZIP_MAGIC_NUMBER))
            {
                AlicePackage.LoadArchive(new ZipArchive(new MemoryStream(data)), filename);
                return Variable.EmptyInstance;
            }
            string script = Utils.GetFileContents(data);
            Variable result = await ProcessAsync(script, filename, mainFile);
            return result;
        }
        private bool IsEqualMagicnumber(byte[] data, byte[] magicnumber)
        {
            byte[] magic = data.Take(magicnumber.Length).ToArray();
            return (magic.SequenceEqual(magicnumber));
        }
        public async Task<Variable> ProcessFileAsync(string filename, bool mainFile = false)
        {
            if (!File.Exists(filename))
            {
                throw new ScriptException("ファイルが存在しません", Exceptions.FILE_NOT_FOUND);
            }
            byte[] data = File.ReadAllBytes(filename);
            return await ProcessDataAsync(data, filename, mainFile);
        }
        public ParsingScript GetScript(string script, string filename = "", bool mainFile = false, object tag = null, AlicePackage package = null)
        {
            Dictionary<int, int> char2Line;
            string data = Utils.ConvertToScript(script, out char2Line, out var def, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                data = ";";
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.Defines = def;
            toParse.OriginalScript = script;
            toParse.Filename = filename;
            toParse.Tag = tag;
            toParse.Package = package;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }
            return toParse;
        }
        public Variable Process(string script, string filename = "", bool mainFile = false, object tag = null, AlicePackage package = null)
        {
            Dictionary<int, int> char2Line;
            string data = Utils.ConvertToScript(script, out char2Line, out var def, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.OriginalScript = script;
            toParse.Filename = filename;
            toParse.Tag = tag;
            toParse.Package = package;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }

            Variable result = toParse.Process();

            /*
            while (toParse.Pointer < data.Length)
            {
                result = toParse.Execute();
                toParse.GoToNextStatement();
            }
            //これでこのスクリプトの処理は終わり
            if (Interop.GCManerger.CollectAfterExecute)
            {
                GC.Collect();
            }
            */
            return result;
        }
        public async Task<Variable> ProcessAsync(string script, string filename = "", bool mainFile = false)
        {
            Dictionary<int, int> char2Line;
            string data = Utils.ConvertToScript(script, out char2Line, out var def, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.Defines = def;
            toParse.OriginalScript = script;
            toParse.Filename = filename;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }

            Variable result = null;

            while (toParse.Pointer < data.Length)
            {
                result = await toParse.ExecuteAsync();
                toParse.GoToNextStatement();
            }

            return result;
        }

        //AliceScript925からNWhileは実装されなくなりました。否定条件のループはwhile(!bool)を使用するべきです


        internal static void SkipBlock(ParsingScript script)
        {
            int blockStart = script.Pointer;
            int startCount = 0;
            int endCount = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            char previous = Constants.EMPTY;
            char prevprev = Constants.EMPTY;
            while (startCount == 0 || startCount > endCount)
            {
                if (!script.StillValid())
                {
                    throw new ScriptException("次のブロックを実行できませんでした [" +
                    script.Substr(blockStart, Constants.MAX_CHARS_TO_SHOW) + "]", Exceptions.COULDNT_EXECUTE_BLOCK, script);
                }
                char currentChar = script.CurrentAndForward();
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
                throw new ScriptException("波括弧が不足しています", Exceptions.NEED_BRACKETS, script);
            }
            else if (startCount < endCount)
            {
                throw new ScriptException("終端の波括弧は不要です", Exceptions.UNNEED_TO_BRACKETS, script);
            }
        }

        internal static void SkipRestBlocks(ParsingScript script)
        {
            while (script.StillValid())
            {
                int endOfToken = script.Pointer;
                ParsingScript nextData = new ParsingScript(script);
                string nextToken = Utils.GetNextToken(nextData);
                if (Constants.ELSE_IF != nextToken &&
                    Constants.ELSE != nextToken)
                {
                    return;
                }
                script.Pointer = nextData.Pointer;
                SkipBlock(script);
            }
        }
    }
}

