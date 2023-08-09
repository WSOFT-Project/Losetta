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
            if (e.Script.Prev == Constants.START_ARG)
            {
                ///本来の関数のように使用されている
                List<Variable> args = e.Script.GetFunctionArgs();
                if (args.Count > 0 && args[0].Object is TypeObject type)
                {
                    var arg = new List<Variable>();
                    if (args.Count > 1)
                    {
                        arg = args.Skip(1).ToList();
                    }
                    e.Return = new Variable(type.Activate(arg, e.Script));
                }
            }
            else
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
    }

    internal class IfStatement : FunctionBase
    {
        public IfStatement()
        {
            this.Name = Constants.IF;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += IfStatement_Run;
        }

        private void IfStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessIf(e.Script);
        }
    }

    internal class ForStatement : FunctionBase
    {
        public ForStatement()
        {
            this.Name = Constants.FOR;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += ForStatement_Run;
        }

        private void ForStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessFor(e.Script);
        }
    }

    internal class ForeachStatement : FunctionBase
    {
        public ForeachStatement()
        {
            this.Name = Constants.FOREACH;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += ForeachStatement_Run;
        }

        private void ForeachStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessForeach(e.Script);
        }

    }

    internal class WhileStatement : FunctionBase
    {
        public WhileStatement()
        {
            this.Name = Constants.WHILE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += WhileStatement_Run;
        }

        private void WhileStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessWhile(e.Script);
        }
    }

    internal class DoWhileStatement : FunctionBase
    {
        public DoWhileStatement()
        {
            this.Name = Constants.DO;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += DoWhileStatement_Run;
        }

        private void DoWhileStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessDoWhile(e.Script);
        }
    }

    internal class SwitchStatement : FunctionBase
    {
        public SwitchStatement()
        {
            this.Name = Constants.SWITCH;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += SwitchStatement_Run;
        }

        private void SwitchStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessSwitch(e.Script);
        }
    }

    internal class CaseStatement : FunctionBase
    {
        public CaseStatement()
        {
            this.Name = Constants.CASE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += CaseStatement_Run;
        }

        private void CaseStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessCase(e.Script, Name);
        }

    }
    //デリゲートを作成する関数クラスです
    internal class DelegateCreator : FunctionBase
    {
        public DelegateCreator()
        {
            this.Name = "delegate";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += DelegateCreator_Run;
        }

        private void DelegateCreator_Run(object sender, FunctionBaseEventArgs e)
        {
            string[] args = Utils.GetFunctionSignature(e.Script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = new string[0];
            }

            e.Script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            e.Script.GetOriginalLine(out _);

            int parentOffset = e.Script.Pointer;

            if (e.Script.CurrentClass != null)
            {
                parentOffset += e.Script.CurrentClass.ParentOffset;
            }

            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP);

            e.Script.MoveForwardIf(Constants.END_GROUP);
            CustomFunction customFunc = new CustomFunction("", body, args, e.Script);
            customFunc.ParentScript = e.Script;
            customFunc.ParentOffset = parentOffset;
            e.Return = new Variable(customFunc);
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
