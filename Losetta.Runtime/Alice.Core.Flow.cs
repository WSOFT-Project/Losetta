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
            e.Return = ProcessIf(e.Script);
        }
        private Variable ProcessIf(ParsingScript script)
        {
            int startIfCondition = script.Pointer;

            Variable result = script.Execute(Constants.END_ARG_ARRAY);
            bool isTrue = false;
            if (result != null)
            {
                isTrue = result.AsBool();
            }

            if (isTrue)
            {
                result = script.ProcessBlock();

                if (result !=null && (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE))
                {
                    // We are here from the middle of the if-block. Skip it.
                    script.Pointer = startIfCondition;
                    script.SkipBlock();
                }
                script.Forward();
                script.SkipRestBlocks();
                //script.SkipBlock();

                //return result;
                return result!=null&&(result.IsReturn ||
                       result.Type == Variable.VarType.BREAK ||
                       result.Type == Variable.VarType.CONTINUE) ? result : Variable.EmptyInstance;
            }

            // We are in Else. Skip everything in the If statement.
            script.SkipBlock();
            //script.Backward();
            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;

            string nextToken = Utils.GetNextToken(nextData);

            if (Constants.ELSE_IF == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = ProcessIf(script);
            }
            else if (Constants.ELSE == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;
                result = script.ProcessBlock(false);
            }
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }

            return result.IsReturn ||
                   result.Type == Variable.VarType.BREAK ||
                   result.Type == Variable.VarType.CONTINUE ? result : Variable.EmptyInstance;
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
            string forString = Utils.GetBodyBetween(e.Script, Constants.START_ARG, Constants.END_ARG);
            e.Script.Forward();
            //for(init; condition; loopStatemen;)の形式です
            string[] forTokens = forString.Split(Constants.END_STATEMENT);
            if (forTokens.Length < 3)
            {
                Utils.ThrowErrorMsg("for文はfor(init; condition; loopStatement;)の形である必要があります", Exceptions.INVALID_SYNTAX,
                                     e.Script, Constants.FOR);
            }

            int startForCondition = e.Script.Pointer;

            ParsingScript initScript = e.Script.GetTempScript(forTokens[0] + Constants.END_STATEMENT);
            ParsingScript condScript = initScript.GetTempScript(forTokens[1] + Constants.END_STATEMENT);
            ParsingScript loopScript = initScript.GetTempScript(forTokens[2] + Constants.END_STATEMENT);

            condScript.Variables = loopScript.Variables = initScript.Variables;

            initScript.Execute(null, 0);
            bool stillValid = true;

            while (stillValid)
            {
                Variable condResult = condScript.Execute(null, 0); condScript.Tag = "COND";
                if (condResult == null)
                {
                    condResult = Variable.EmptyInstance;
                }
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                e.Script.Pointer = startForCondition;
                string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                ParsingScript mainScript = initScript.GetTempScript(body);
                //mainScript.Variables = initScript.Variables;
                Variable result = mainScript.Process();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    return;
                }
                loopScript.Execute(null, 0);
            }

            e.Script.Pointer = startForCondition;
            e.Script.SkipBlock();
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

                string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
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
            int startWhileCondition = e.Script.Pointer;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                e.Script.Pointer = startWhileCondition;

                //int startSkipOnBreakChar = from;
                Variable condResult = e.Script.Execute(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }

                result = e.Script.ProcessBlock();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    e.Script.Pointer = startWhileCondition;
                    break;
                }
            }

            // 条件はもうtrueではないので、ブロックをスキップします
            e.Script.SkipBlock();
            e.Return = result.IsReturn ? result : Variable.EmptyInstance;
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
            int startDoCondition = e.Script.Pointer;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                e.Script.Pointer = startDoCondition;

                string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                ParsingScript mainScript = e.Script.GetTempScript(body);
                result = mainScript.ProcessForWhile();
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    e.Script.Pointer = startDoCondition;
                    break;
                }
                e.Script.Forward(Constants.WHILE.Length + 1);
                Variable condResult = e.Script.Execute(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }
            }

            //SkipBlock(script);
            e.Return= result.IsReturn ? result : Variable.EmptyInstance;
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
            Variable switchValue = Utils.GetItem(e.Script);
            e.Script.Forward();

            Variable result = Variable.EmptyInstance;
            var caseSep = ":".ToCharArray();

            bool caseDone = false;

            while (e.Script.StillValid())
            {
                var nextToken = Utils.GetBodySize(e.Script, Constants.CASE, Constants.DEFAULT);
                if (string.IsNullOrEmpty(nextToken))
                {
                    break;
                }
                if (nextToken == Constants.DEFAULT && !caseDone)
                {
                    e.Script.ProcessBlock();
                    break;
                }
                if (!caseDone)
                {
                    Variable caseValue = e.Script.Execute(caseSep);
                    e.Script.Forward(2);

                    if (switchValue.Equals(caseValue))
                    {
                        caseDone = true;
                        string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                        ParsingScript mainScript = e.Script.GetTempScript(body);
                        result = mainScript.Process();
                        if (mainScript.Prev == '}')
                        {
                            break;
                        }
                        e.Script.Forward();
                    }
                    else
                    {
                        e.Script.Backward();
                        e.Script.SkipBlock();
                    }
                }
            }
            //  script.MoveForwardIfNotPrevious('}');
            e.Script.GoToNextStatement();
            e.Return=result;
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
            if (Name == Constants.CASE)
            {
                /*var token = */
                Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            }
            e.Script.MoveForwardIf(':');

            e.Return=e.Script.ProcessBlock();
            e.Script.MoveBackIfPrevious('}');
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

            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

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
            int startTryCondition = e.Script.Pointer - 1;
            int currentStackLevel = ParserFunction.GetCurrentStackLevel();

            Variable result = null;

            // tryブロック内のスクリプト
            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript mainScript = e.Script.GetTempScript(body);
            mainScript.InTryBlock = true;

        // catchブロック内のスクリプト
        getCatch:
            string catchToken = Utils.GetNextToken(e.Script);
            e.Script.Forward(); // skip opening parenthesis
                              // The next token after the try block must be a catch.
            if (Constants.CATCH != catchToken && e.Script.StillValid())
            {
                goto getCatch;
            }
            else if (Constants.CATCH != catchToken)
            {
                throw new ScriptException("Catchステートメントがありません", Exceptions.MISSING_CATCH_STATEMENT, e.Script);
            }

            string exceptionName = Utils.GetNextToken(e.Script);
            e.Script.Forward(); // skip closing parenthesis
            string body2 = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript catchScript = e.Script.GetTempScript(body2);

            mainScript.ThrowError += delegate (object sender, ThrowErrorEventArgs e)
            {
                GetVarFunction excMsgFunc = new GetVarFunction(new Variable(new ExceptionObject(e.Message, e.ErrorCode, e.Script, e.Source, e.HelpLink)));
                catchScript.Variables.Add(exceptionName, excMsgFunc);
                result = catchScript.Process();
                e.Handled = true;
            };

            result = mainScript.Process();

            e.Script.SkipRestBlocks();
            e.Return=result;
        }
    }
}
