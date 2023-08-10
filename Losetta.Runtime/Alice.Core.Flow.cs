﻿namespace AliceScript.NameSpaces
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
                List<Variable> args = e.Script.GetFunctionArgs(this);
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
                List<Variable> args = e.Script.GetFunctionArgs(this);

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
            string forString = Utils.GetBodyBetween(e.Script, Constants.START_ARG, Constants.END_ARG);
            e.Script.Forward();
            //foreach(var in ary)の形式です
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。
            var tokens = forString.Split(' ');

            bool registVar = false;
            if (tokens[0].ToLower() == Constants.VAR)
            {
                tokens = tokens.Skip(1).ToArray();
                forString = forString.Substring(3);
                registVar = true;
            }
            var sep = tokens.Length > 2 ? tokens[1] : "";
            string varName = tokens[0];
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。

            if (sep != Constants.FOR_IN)
            {
                int index = forString.IndexOf(Constants.FOR_EACH);
                if (index <= 0 || index == forString.Length - 1)
                {
                    Utils.ThrowErrorMsg("foreach文はforeach(variable in array)の形をとるべきです", Exceptions.INVALID_SYNTAX
                                     , e.Script, Constants.FOREACH);
                }
                varName = forString.Substring(0, index);
            }

            ParsingScript forScript = e.Script.GetTempScript(forString,this, varName.Length + sep.Length + 1);

            Variable arrayValue = Utils.GetItem(forScript);

            if (arrayValue.Type == Variable.VarType.STRING)
            {
                arrayValue = new Variable(new List<string>(arrayValue.ToString().ToCharArray().Select(c => c.ToString())));
            }

            int cycles = arrayValue.Count;
            if (cycles == 0)
            {
                e.Script.SkipBlock();
                return;
            }
            int startForCondition = e.Script.Pointer;

            for (int i = 0; i < cycles; i++)
            {
                e.Script.Pointer = startForCondition;
                Variable current = arrayValue.GetValue(i);

                string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP,
                                                       Constants.END_GROUP);
                ParsingScript mainScript = e.Script.GetTempScript(body);
                ParserFunction.AddGlobalOrLocalVariable(varName,
                               new GetVarFunction(current), mainScript, false, registVar, false);
                Variable result = mainScript.Process();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    //script.Pointer = startForCondition;
                    //SkipBlock(script);
                    //return;
                    break;
                }
            }
            e.Script.Pointer = startForCondition;
            e.Script.SkipBlock();
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
