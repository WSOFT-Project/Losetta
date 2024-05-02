using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

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
        internal static bool DefineFunction(string funcName, ParsingScript script, HashSet<string> keywords,HashSet<AttributeFunction> attributes)
        {
            bool? mode = null;

            AccessModifier accessModifier = keywords.Contains(Constants.PUBLIC) ? AccessModifier.PUBLIC : AccessModifier.PRIVATE;
            accessModifier = keywords.Contains(Constants.PRIVATE) ? AccessModifier.PUBLIC : accessModifier;
            accessModifier = keywords.Contains(Constants.PROTECTED) ? AccessModifier.PROTECTED : accessModifier;

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
            string ensure = string.Empty;
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = Array.Empty<string>();
            }

            ParsingScript nextData = new ParsingScript(script);
            nextData.Pointer = script.Pointer;
            nextData.ParentScript = script;

            while (true)
            {
                string nextToken = Utils.GetNextToken(nextData, false, true);

                if (nextToken == Constants.REQUIRES)
                {
                    body = $"Alice.Diagnostics.Assert({Utils.GetBodyBetween(nextData)},\"この呼び出しは、関数が表明した事前条件を満たしませんでした\");";
                    script.Pointer = ++nextData.Pointer;
                }
                else if (nextToken == Constants.ENSURES)
                {
                    ensure = Utils.GetBodyBetween(nextData);
                    script.Pointer = ++nextData.Pointer;
                }
                else
                {
                    break;
                }
            }
            script.GetOriginalLine(out _);

            int parentOffset = script.Pointer;

            if (script.Current != Constants.START_GROUP)
            {
                if (mode == false)
                {
                    body = $"Alice.throw(\"このメソッドは実装されていません\",{(int)Exceptions.NOT_IMPLEMENTED});";
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
                body += Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);
                script.MoveForwardIf(Constants.END_GROUP);
            }
            if (ensure.Length > 0)
            {
                ensure = ensure.Replace("return", "\ufdd4return");
                body = Constants.RETURN_PATTERN.Replace(body, $"{{readonly var \ufdd4return=$1;Alice.Diagnostics.Assert({ensure},\"この関数は、関数が表明した事後条件を満たしませんでした\");return \ufdd4return;}}");
            }

            CustomFunction customFunc = new CustomFunction(funcName, body, args, script, false, type_modifer, nullable);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            customFunc.MethodOnly = isExtension;
            if (Interpreter.Instance.DebugMode)
            {
                customFunc.Obsolete = attributes?.OfType<ObsoleteFunction>().FirstOrDefault();
            }
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
                    FunctionBaseManager.Add(customFunc, funcName, script, accessModifier);
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
            DefineFunction(funcName, e.Script, Keywords,e.AttributeFunctions);
        }

    }
}
