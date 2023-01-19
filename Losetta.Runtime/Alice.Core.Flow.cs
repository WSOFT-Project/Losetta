using System.Collections.Generic;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces
{
   

    internal class NewObjectFunction : FunctionBase
    {
        public NewObjectFunction()
        {
            this.Name = Constants.NEW;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += NewObjectFunction_Run;
        }

        private void NewObjectFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string className = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            className = Constants.ConvertName(className);
            e.Script.MoveForwardIf(Constants.START_ARG);
            List<Variable> args = e.Script.GetFunctionArgs();

            ObjectBase csClass = AliceScriptClass.GetClass(className, e.Script) as ObjectBase;
            if (csClass != null)
            {
                Variable obj = csClass.GetImplementation(args, e.Script);
                e.Return = obj;
                return;
            }

            AliceScriptClass.ClassInstance instance = new
                AliceScriptClass.ClassInstance(e.Script.CurrentAssign, className, args, e.Script);

            e.Return = new Variable(instance);
        }
    }

    internal class IfStatement : FunctionBase
    {
        public IfStatement()
        {
            this.Name = Constants.IF;

        }
        protected override Variable Evaluate(ParsingScript script)
        {
            Variable result = Interpreter.Instance.ProcessIf(script);
            return result;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            Variable result = await Interpreter.Instance.ProcessIfAsync(script);
            return result;
        }
    }

    internal class ForStatement : FunctionBase
    {
        public ForStatement()
        {
            this.Name = Constants.FOR;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessFor(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessForAsync(script);
        }
    }

    internal class ForeachStatement : FunctionBase
    {
        public ForeachStatement()
        {
            this.Name = Constants.FOREACH;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessForeach(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessForeachAsync(script);
        }
    }

    internal class WhileStatement : FunctionBase
    {
        public WhileStatement()
        {
            this.Name = Constants.WHILE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessWhile(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessWhileAsync(script);
        }
    }

    internal class DoWhileStatement : FunctionBase
    {
        public DoWhileStatement()
        {
            this.Name = Constants.DO;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessDoWhile(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessDoWhile(script);
        }
    }

    internal class SwitchStatement : FunctionBase
    {
        public SwitchStatement()
        {
            this.Name = Constants.SWITCH;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessSwitch(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessSwitch(script);
        }
    }

    internal class CaseStatement : FunctionBase
    {
        public CaseStatement()
        {
            this.Name = Constants.CASE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessCase(script, Name);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessCase(script, Name);
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

            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);

            script.MoveForwardIf(Constants.END_GROUP);
            CustomFunction customFunc = new CustomFunction("", body, args, script);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            return new Variable(customFunc);
        }
    }
    internal class TryBlock : FunctionBase
    {
        public TryBlock()
        {
            this.Name = Constants.TRY;
            this.Attribute = FunctionAttribute.CONTROL_FLOW | FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += TryBlock_Run;
        }

        private void TryBlock_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessTry(e.Script);
        }
    }
}
