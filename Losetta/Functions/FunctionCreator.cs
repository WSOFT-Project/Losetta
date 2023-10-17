using AliceScript.Parsing;

namespace AliceScript.Functions
{
    internal sealed class FunctionCreator : FunctionBase
    {
        public FunctionCreator()
        {
            Name = Constants.FUNCTION;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += FunctionCreator_Run;
        }
        internal static bool DefineFunction(string funcName, ParsingScript script, HashSet<string> keywords)
        {
            bool? mode = null;
            bool isGlobal = keywords.Contains(Constants.PUBLIC);
            bool isCommand = keywords.Contains(Constants.COMMAND);
            bool isExtension = keywords.Contains(Constants.EXTENSION);
            if (keywords.Contains(Constants.OVERRIDE))
            {
                mode = true;
            }
            else if (keywords.Contains(Constants.VIRTUAL))
            {
                mode = false;
            }

            Variable.VarType type_modifer = Variable.VarType.VARIABLE;
            bool nullable = false;
            foreach (string str in keywords)
            {
                string type_str = str;
                if (type_str.EndsWith("?", StringComparison.Ordinal))
                {
                    nullable = true;
                    type_str = type_str.Substring(0, type_str.Length - 1);
                }
                if (Constants.TYPE_MODIFER.Contains(type_str))
                {
                    type_modifer = Constants.StringToType(type_str);
                    break;
                }
            }

            funcName = Constants.ConvertName(funcName);

            string[] args = Utils.GetFunctionSignature(script);
            string body = string.Empty;
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = Array.Empty<string>();
            }

            //script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            script.GetOriginalLine(out _);

            int parentOffset = script.Pointer;

            if (script.Current != Constants.START_GROUP)
            {
                if (mode == false)
                {
                    body = $"throw(\"このメソッドは実装されていません\",{(int)Exceptions.NOT_IMPLEMENTED});";
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (script.CurrentClass is not null)
                {
                    parentOffset += script.CurrentClass.ParentOffset;
                }
                body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);
                script.MoveForwardIf(Constants.END_GROUP);
            }

            CustomFunction customFunc = new CustomFunction(funcName, body, args, script, false, type_modifer, nullable);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            customFunc.MethodOnly = isExtension;
            if (isCommand)
            {
                customFunc.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            }
            if (mode is not null)
            {
                customFunc.IsVirtual = true;
            }
            if (script.CurrentClass is not null)
            {
                script.CurrentClass.AddMethod(funcName, args, customFunc);
            }
            else
            {
                if (!FunctionExists(funcName, script, out _) || (mode == true && FunctionIsVirtual(funcName, script)))
                {
                    FunctionBaseManager.Add(customFunc, funcName, script, isGlobal);
                }
                else
                {
                    throw new ScriptException("指定された関数はすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED, script);
                }
            }
            return true;
        }

        private void FunctionCreator_Run(object sender, FunctionBaseEventArgs e)
        {
            string funcName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            DefineFunction(funcName, e.Script, Keywords);
        }


    }
}
