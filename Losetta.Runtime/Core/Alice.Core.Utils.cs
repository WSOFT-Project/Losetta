using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Packaging;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Threading;

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
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Exit()
        {
            Alice.OnExiting();
        }

        public static string Read()
        {
            return Interpreter.Instance.ReadInput();
        }

        private static void AddOutput(string text, bool addLine = true)
        {

            string output = text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendOutput(output);

        }

        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print()
        {
            AddOutput(string.Empty);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(string text)
        {
            AddOutput(text);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(Variable item)
        {
            AddOutput(item.AsString());
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Print(string format, params Variable[] args)
        {
            AddOutput(StringFormatFunction.Format(format, args));
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write()
        {
            AddOutput(string.Empty, false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(string text)
        {
            AddOutput(text, false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(Variable item)
        {
            AddOutput(item.AsString(), false);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Write(string format, params Variable[] args)
        {
            AddOutput(StringFormatFunction.Format(format, args), false);
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
            throw new ScriptException(exception.Message, exception.Error, s);
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static void Throw(ParsingScript script, string message, int errorCode)
        {
            throw new ScriptException(message, (Exceptions)errorCode, script);
        }

        private static readonly Dictionary<string, Variable> m_singletons =
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
        public static void Import(ParsingScript script, string filePath, bool fromPackage = false)
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
            if (result is null) { result = Variable.EmptyInstance; }
            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE)]
        public static Variable Return(ParsingScript script, Variable result = null)
        {
            // Returnに到達したら終了
            script.SetDone();
            if (result is null)
            {
                result = new Variable(Variable.VarType.VOID);
            }
            result.IsReturn = true;

            return result;
        }
    }
}
