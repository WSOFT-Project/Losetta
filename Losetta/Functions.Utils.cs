using System.Collections.Generic;
using System.Threading;

namespace AliceScript
{
    internal class wsverFunc : FunctionBase
    {
        public wsverFunc()
        {
            this.Name = "wsver";
            this.MinimumArgCounts = 0;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += WsverFunc_Run;
        }

        private void WsverFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.Return = new Variable(Alice.Version.ToString());
        }
    }

    internal class DelayFunc : FunctionBase
    {
        public DelayFunc()
        {
            this.Name = "delay";
            this.MinimumArgCounts = 0;
            this.Run += DelayFunc_Run;
        }

        private void DelayFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0 && e.Args[0].Type == Variable.VarType.NUMBER)
            {
                Thread.Sleep((int)e.Args[0].Value);
            }
            else
            {
                Thread.Sleep(-1);
            }
        }
    }

    internal class LabelFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // Just skip this label. m_name is equal to the lable name.
            return Variable.EmptyInstance;
        }
    }

    //デリゲートを作成する関数クラスです
    internal class DelegateCreator : FunctionBase
    {
        public DelegateCreator()
        {
            this.Name = "delegate";
        }
        protected override Variable Evaluate(ParsingScript script)
        {


            string[] args = Utils.GetFunctionSignature(script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = new string[0];
            }

            script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            script.GetOriginalLine(out _);

            int parentOffset = script.Pointer;

            if (script.CurrentClass != null)
            {
                parentOffset += script.CurrentClass.ParentOffset;
            }

            string body = Utils.GetBodyArrowBetween(script, Constants.START_GROUP, Constants.END_GROUP);
            //AliceScript926から、Delegateの宣言に=>演算子は必要なくなりました。下の式は将来使用するために残されています。
            //string body = Utils.GetBodyBetween(script,Constants.START_GROUP,Constants.END_GROUP);

            script.MoveForwardIf(Constants.END_GROUP);
            CustomFunction customFunc = new CustomFunction("", body, args, script,"DELEGATE");
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            return new Variable(customFunc);
        }
    }

    internal class PointerFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<string> args = Utils.GetTokens(script);
            Utils.CheckArgs(args.Count, 1, m_name);

            var result = new Variable(Variable.VarType.POINTER);
            result.Pointer = args[0];
            ParserFunction.AddGlobalOrLocalVariable(m_name,
                                        new GetVarFunction(result), script);
            return result;
        }
    }

    internal class PointerReferenceFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            var pointer = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            var result = GetRefValue(pointer, script);
            return result;
        }

        public Variable GetRefValue(string pointer, ParsingScript script)
        {
            if (string.IsNullOrWhiteSpace(pointer))
            {
                return Variable.Undefined;
            }
            var refPointer = ParserFunction.GetVariable(pointer, null, true) as GetVarFunction;
            if (refPointer == null || string.IsNullOrWhiteSpace(refPointer.Value.Pointer))
            {
                return Variable.Undefined;
            }

            var result = ParserFunction.GetVariable(refPointer.Value.Pointer, null, true);
            if (result is GetVarFunction)
            {
                return ((GetVarFunction)result).Value;
            }

            if (result is CustomFunction)
            {
                script.Forward();
                List<Variable> args = script.GetFunctionArgs();
                return ((CustomFunction)result).Run(args, script);
            }
            return Variable.Undefined;
        }
    }

    internal class GotoGosubFunction : FunctionBase
    {
        private bool m_isGoto = true;

        public GotoGosubFunction(bool gotoMode = true)
        {
            m_isGoto = gotoMode;
            if (m_isGoto)
            {
                this.Name = Constants.GOTO;
            }
            else
            {
                this.Name = Constants.GOSUB;
            }
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            Dictionary<string, int> labels;
            if (script.AllLabels == null || script.LabelToFile == null |
               !script.AllLabels.TryGetValue(script.FunctionName, out labels))
            {
                Utils.ThrowErrorMsg("次のラベルは関数内に存在しません [" + script.FunctionName + "]", Exceptions.COULDNT_FIND_LABEL_IN_FUNCTION,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            int gotoPointer;
            if (!labels.TryGetValue(labelName, out gotoPointer))
            {
                Utils.ThrowErrorMsg("ラベル:[" + labelName + "]は定義されていません", Exceptions.COULDNT_FIND_LABEL,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            string filename;
            if (script.LabelToFile.TryGetValue(labelName, out filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename);
                script.Filename = filename;
                script.String = newScript.String;
            }

            if (!m_isGoto)
            {
                script.PointersBack.Add(script.Pointer);
            }

            script.Pointer = gotoPointer;
            if (string.IsNullOrWhiteSpace(script.FunctionName))
            {
                script.Backward();
            }

            return Variable.EmptyInstance;
        }
    }



}
