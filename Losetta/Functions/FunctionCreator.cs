using AliceScript.Objects;
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
        internal static bool DefineFunction(string funcName, ParsingScript script, HashSet<string> keywords, HashSet<FunctionBase> attributes, TypeObject returnType)
        {
            bool? mode = null;

            AccessModifier accessModifier = AccessModifier.PRIVATE;
            
            if(keywords.Contains(Constants.PUBLIC))
            {
                accessModifier = AccessModifier.PUBLIC;
            }
            else if(keywords.Contains(Constants.PRIVATE))
            {
                accessModifier = AccessModifier.PRIVATE;
            }
            else if(keywords.Contains(Constants.PROTECTED))
            {
                accessModifier = AccessModifier.PROTECTED;
            }

            bool isCommand = keywords.Contains(Constants.COMMAND);
            bool isExtension = keywords.Contains(Constants.EXTENSION);
            bool forceReturn = false;

            if (keywords.Contains(Constants.OVERRIDE))
            {
                mode = true;
            }
            else if (keywords.Contains(Constants.VIRTUAL))
            {
                mode = false;
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

            if(script.Current == Constants.ARROW[0] && script.Next == Constants.ARROW[1])
            {
                // 式形式の関数の場合
                forceReturn = true;
                script.Forward();
                body += Utils.GetBodyBetween(script, Constants.ARROW[1], '\0', Constants.TOKENS_SEPARATION_STR + "\0");
            }
            else if (script.Current != Constants.START_GROUP)
            {
                // 未実装の関数の場合
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

            CustomFunction customFunc = new CustomFunction(funcName, body, args, script, forceReturn, returnType);

            customFunc.ParentScript = attributes?.OfType<IndependentFunction>().FirstOrDefault() is null ? script : ParsingScript.GetTopLevelScript();
            customFunc.ParentOffset = parentOffset;
            customFunc.MethodOnly = isExtension;

            var handles = attributes?.OfType<ICallingHandleAttribute>();
            if (handles is not null)
            {
                foreach (var handle in handles)
                {
                    customFunc.HandleAttributes.Add(handle);
                }
            }
            bool hasAnnotationFunction = attributes?.OfType<AnnotationFunction>().FirstOrDefault() != null;
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
                    FunctionBaseManager.Add(customFunc, funcName, script, accessModifier, false, hasAnnotationFunction ? Constants.ANNOTATION_FUNCTION_REFIX : '\0');
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
            // DefineFunction(funcName, e.Script, Keywords, e.AttributeFunctions, );
        }

    }
}
