using AliceScript.Binding;

namespace AliceScript.Functions
{
    internal class ExternFunction : FunctionBase
    {
        public ExternFunction()
        {
            Name = Constants.EXTERNAL;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ExternFunction_Run;
        }

        private void ExternFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string returnType = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            string funcName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);

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


            var prevfunc = e.Script.PrevProcessingFunction;
            if (prevfunc is not PInvokeFlagFunction info)
            {
                throw new ScriptException("外部定義関数は、#libimportと併用することでのみ使用できます", Exceptions.NONE);
            }

            BindFunction func = BindFunction.CreateExternBindFunction(funcName, info.LibraryName, returnType, args.ToArray(), info.EntryPoint, info.UseUnicode);

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
    internal class PInvokeFlagFunction : FunctionBase
    {
        public PInvokeFlagFunction()
        {
            Name = "." + Constants.EXTERNAL;
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
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
