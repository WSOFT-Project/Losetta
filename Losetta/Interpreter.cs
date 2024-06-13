using AliceScript.Functions;
using AliceScript.Interop;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;
using AliceScript.PreProcessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                instance ??= new Interpreter();
                return instance;
            }
        }
        /// <summary>
        /// グローバルの名前空間
        /// </summary>
        public NameSpace GlobalNameSpace => NameSpaceManager.Get(Constants.TOP_NAMESPACE);

        /// <summary>
        /// このインタプリタの名前
        /// </summary>
        public string Name => Assembly.GetExecutingAssembly().GetName().Name;

        /// <summary>
        /// デバッグモードで実行する場合はtrue、それ以外の場合はfalse
        /// </summary>
        public bool DebugMode
        {
            get => m_debugMode;
            set
            {
                m_debugMode = value;
                var script = ParsingScript.GetTopLevelScript();
                if (m_debugMode)
                {
                    script.Defines.Add(Constants.DEBUG);
                }
                else if (script.Defines.Contains(Constants.DEBUG))
                {
                    script.Defines.Remove(Constants.DEBUG);
                }
            }
        }
        private bool m_debugMode = false;

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
            if (handler is not null)
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
            if (handler is not null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }
        public void AppendDebug(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnDebug;
            if (handler is not null)
            {
                OutputAvailableEventArgs args = new OutputAvailableEventArgs(text +
                                     (newLine ? Environment.NewLine : string.Empty));
                handler(this, args);
            }
        }

        public bool AppendData(string text, bool newLine = false)
        {
            EventHandler<OutputAvailableEventArgs> handler = OnData;
            if (handler is not null)
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

#if NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
            RegisterFunctions();
            RegisterActions();

            Alice_Interpreter_Initer.Init();
        }

        public void RegisterFunctions()
        {
            NameSpace space = new NameSpace(Constants.TOP_API_NAMESPACE);
            space.Add(new ClassCreator());
            space.Add(new EnumFunction());
            space.Add(new ArrayTypeFunction());
            space.Add(new ExternFunctionCreator());
            space.Add(new LibImportFunction());
            space.Add(new NetImportFunction());

            NameSpace experimental = new NameSpace("WSOFT.Interpreter.Services");
            experimental.Add(new ObsoleteFunction());
            experimental.Add(new AnnotationFunction());
            experimental.Add(new TestCallFunction());

            NameSpaceManager.Add(space);
            NameSpaceManager.Add(experimental);

            ParserFunction.AddAction(Constants.LABEL_OPERATOR, new LabelFunction());
        }

        public void RegisterActions()
        {
            ParserFunction.AddAction(Constants.ASSIGNMENT, new AssignFunction());
            ParserFunction.AddAction(Constants.INCREMENT, new IncrementDecrementFunction());
            ParserFunction.AddAction(Constants.DECREMENT, new IncrementDecrementFunction());
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
            return magic.SequenceEqual(magicnumber);
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
            string data = PreProcessor.ConvertToScript(script, out Dictionary<int, int> char2Line, out var def, out var setting, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                data = ";";
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.Defines = def;
            toParse.Settings = setting;
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
            string data = PreProcessor.ConvertToScript(script, out Dictionary<int, int> char2Line, out var def, out var setting, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line);
            toParse.TopInFile = true;
            toParse.Defines = def;
            toParse.Settings = setting;
            toParse.OriginalScript = script;
            toParse.Filename = filename;
            toParse.Tag = tag;
            toParse.Package = package;

            if (mainFile)
            {
                toParse.MainFilename = toParse.Filename;
            }

            return toParse.Process();
        }
        public async Task<Variable> ProcessAsync(string script, string filename = "", bool mainFile = false)
        {
            string data = PreProcessor.ConvertToScript(script, out Dictionary<int, int> char2Line, out var def, out var setting, filename);
            if (string.IsNullOrWhiteSpace(data))
            {
                return null;
            }

            ParsingScript toParse = new ParsingScript(data, 0, char2Line)
            {
                TopInFile = true,
                Settings = setting,
                Defines = def,
                OriginalScript = script,
                Filename = filename
            };

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
    }
}

