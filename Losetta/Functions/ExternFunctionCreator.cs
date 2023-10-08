using System.Reflection;

namespace AliceScript.Functions
{
    internal class ExternFunctionCreator : FunctionBase
    {
        public ExternFunctionCreator()
        {
            Name = Constants.EXTERNAL;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ExternFunction_Run;
        }

        private void ExternFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string returnType = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            string funcName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);

            Utils.CheckLegalName(funcName);

            bool? mode = null;
            bool isGlobal = e.Keywords.Contains(Constants.PUBLIC);
            bool isCommand = e.Keywords.Contains(Constants.COMMAND);
            bool isExtension = e.Keywords.Contains(Constants.EXTENSION);
            if (e.Keywords.Contains(Constants.OVERRIDE))
            {
                mode = true;
            }
            else if (e.Keywords.Contains(Constants.VIRTUAL))
            {
                mode = false;
            }

            Span<string> args = Utils.GetFunctionSignature(e.Script).AsSpan();

            for (int i = 0; i < args.Length; i++)
            {
                string target = args[i];
                int index = target.IndexOf(Constants.SPACE);
                if (index > 0)
                {
                    // HWND hWndのような場合、本当に欲しいのはHWNDのみ
                    args[i] = target.Substring(0, index);
                }
            }

            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = Array.Empty<string>();
            }

            var prevfunc = e.Script.AttributeFunction;

            FunctionBase func;

            if (prevfunc is LibImportFunction info)
            {
                func = Utils.CreateExternBindFunction(funcName, info.LibraryName, returnType, args.ToArray(), info.EntryPoint, info.UseUnicode);
            }
            else if (prevfunc is NetImportFunction libInfo)
            {
                if (libInfo.Class == null)
                {
                    throw new ScriptException("メソッドが存在する適切なクラスが見つかりませんでした", Exceptions.OBJECT_DOESNT_EXIST);
                }
                MethodInfo method = libInfo.Class.GetMethod(funcName, Constants.InvokeStringToType(args.ToArray()));
                if (libInfo.Class == null)
                {
                    throw new ScriptException("外部に適切に定義された関数が見つかりませんでした", Exceptions.COULDNT_FIND_VARIABLE);
                }
                func = Utils.CreateBindFunction(method);
            }
            else
            {
                throw new ScriptException("外部定義関数は、#libimportか#libimportと併用することでのみ使用できます", Exceptions.NONE);
            }

            funcName = Constants.ConvertName(funcName);

            if (mode != null)
            {
                func.IsVirtual = true;
            }
            if (!FunctionExists(funcName, e.Script, out _) || (mode == true && FunctionIsVirtual(funcName, e.Script)))
            {
                FunctionBaseManager.Add(func, funcName, e.Script, isGlobal);
            }
            else
            {
                throw new ScriptException("指定された関数はすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED, e.Script);
            }
        }
    }
    internal class NetImportFunction : AttributeFunction
    {
        public NetImportFunction()
        {
            Name = "." + Constants.NET_IMPORT;
            MinimumArgCounts = 1;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string asmName = Utils.GetSafeString(e.Args, 1, null);
            string asmLocate = Utils.GetSafeString(e.Args, 2, null);
            string typeName = e.Args[0].AsString();

            if (!string.IsNullOrEmpty(asmName))
            {
                typeName += $",{asmName}";
            }
            if (string.IsNullOrEmpty(asmLocate))
            {
                Class = Type.GetType(typeName, false, true);
            }
            else
            {
                var asm = Assembly.LoadFrom(asmLocate);
                Class = asm.GetType(typeName);
            }
        }
        public Type Class { get; set; }
    }
    internal class LibImportFunction : AttributeFunction
    {
        public LibImportFunction()
        {
            Name = "." + Constants.LIBRARY_IMPORT;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            LibraryName = Utils.GetSafeString(e.Args, 0);
            EntryPoint = Utils.GetSafeString(e.Args, 1, null);
            if (e.Args.Count > 3)
            {
                UseUnicode = e.Args[2].m_bool;
            }
        }
        public string LibraryName { get; set; }
        public string EntryPoint { get; set; }
        public bool? UseUnicode { get; set; }
    }
}
