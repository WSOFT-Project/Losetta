using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static void Delay()
        {
            Thread.Sleep(-1);
        }
        public static void Delay(int milliSeconds)
        {
            Thread.Sleep(milliSeconds);
        }
        public static void SpinWait(int iterations)
        {
            Thread.SpinWait(iterations);
        }
        public static void Exit()
        {
            Alice.OnExiting();
        }

        public static string Read()
        {
            return Interpreter.Instance.ReadInput();
        }

        private static void AddOutput(string text, ParsingScript script = null,
                                    bool addLine = true, bool addSpace = true, string start = "")
        {

            string output = text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendOutput(output);

        }

        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(ParsingScript script)
        {
            AddOutput(string.Empty, script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(ParsingScript script, string text)
        {
            AddOutput(text, script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(ParsingScript script, Variable item)
        {
            AddOutput(item.AsString(), script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(ParsingScript script, string format, params Variable[] args)
        {
            AddOutput(StringFormatFunction.Format(format, args), script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(ParsingScript script)
        {
            AddOutput(string.Empty, script, false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(ParsingScript script, string text)
        {
            AddOutput(text, script, false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(ParsingScript script, Variable item)
        {
            AddOutput(item.AsString(), script, false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(ParsingScript script, string format, params Variable[] args)
        {
            AddOutput(StringFormatFunction.Format(format, args), script, false);
        }

        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Throw(ParsingScript script, int errorCode)
        {
            throw new ScriptException(string.Empty, (Exceptions)errorCode, script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Throw(ParsingScript script, string message)
        {
            throw new ScriptException(message, Exceptions.USER_DEFINED, script);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Throw(ParsingScript script, ExceptionObject exception)
        {
            var s = exception.MainScript ?? script;
            throw new ScriptException(exception.Message, exception.ErrorCode, s);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Throw(ParsingScript script, int errorCode, string message)
        {
            throw new ScriptException(message, (Exceptions)errorCode, script);
        }

        private static Dictionary<string, Variable> m_singletons =
           new Dictionary<string, Variable>();

        public static Variable Singleton(ParsingScript script)
        {
            string expr = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

            if (m_singletons.TryGetValue(expr, out Variable result))
            {
                return result;
            }

            ParsingScript tempScript = script.GetTempScript(expr);
            result = tempScript.Execute();

            m_singletons[expr] = result;
            return result;
        }

        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static void Import(ParsingScript script, string filePath)
        {
            if (script.EnableImport)
            {
                var data = Utils.GetFileFromPackageOrLocal(filePath);
                AlicePackage.LoadData(data, filePath, true);
            }
            else
            {
                throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, script);
            }
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static void Import(ParsingScript script, string filePath, bool fromPackage)
        {
            if (script.EnableImport)
            {
                var data = Utils.GetFileFromPackageOrLocal(filePath, fromPackage, script);
                if (fromPackage)
                {
                    Interop.NetLibraryLoader.LoadLibrary(data);
                }
                else
                {
                    AlicePackage.LoadData(data, filePath, true);
                }
            }
            else
            {
                throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, script);
            }
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static Variable Include(ParsingScript script, BindFunction func, string fileName)
        {
            ParsingScript tempScript = script.GetIncludeFileScript(fileName, func);

            Variable result = null;
            while (tempScript.StillValid())
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
            if (result == null) { result = Variable.EmptyInstance; }
            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static Variable Return(ParsingScript script, Variable result = null)
        {
            // Returnに到達したら終了
            script.SetDone();
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            result.IsReturn = true;

            return result;
        }
    }
}
