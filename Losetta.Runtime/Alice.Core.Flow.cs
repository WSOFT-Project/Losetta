﻿using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces
{
    internal sealed class NewObjectFunction : FunctionBase
    {
        public NewObjectFunction()
        {
            Name = Constants.NEW;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += NewObjectFunction_Run;
        }

        private void NewObjectFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script.Prev == Constants.START_ARG)
            {
                //new関数として動作
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
                //new式として動作
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

    internal sealed class ForStatement : FunctionBase
    {
        public ForStatement()
        {
            Name = Constants.FOR;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ForStatement_Run;
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
                Variable result = mainScript.Process(true);
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

    internal sealed class ForeachStatement : FunctionBase
    {
        public ForeachStatement()
        {
            Name = Constants.FOREACH;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ForeachStatement_Run;
        }

        private void ForeachStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            string forString = Utils.GetBodyBetween(e.Script, Constants.START_ARG, Constants.END_ARG);
            e.Script.Forward();
            //foreach(var in ary)の形式です
            //AliceScript925からforeach(var : ary)またはforeach(var of ary)の形は使用できなくなりました。同じ方法をとるとき、複数の方法が存在するのは好ましくありません。
            var tokens = forString.Split(' ');

            bool registVar = false;
            if (tokens[0].Equals(Constants.VAR, StringComparison.OrdinalIgnoreCase))
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

            ParsingScript forScript = e.Script.GetTempScript(forString, this, varName.Length + sep.Length + 1);

            Variable arrayValue = Utils.GetItem(forScript);

            if (arrayValue == null)
            {
                return;
            }

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
                Variable result = mainScript.Process(true);
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

    internal sealed class WhileStatement : FunctionBase
    {
        public WhileStatement()
        {
            Name = Constants.WHILE;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += WhileStatement_Run;
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
                result = e.Script.ProcessBlock(true);
                if (result.IsReturn || result.Type == Variable.VarType.BREAK)
                {
                    e.Script.Pointer = startWhileCondition;
                    break;
                }
            }

            // 条件はもうtrueではないので、ブロックをスキップ
            e.Script.SkipBlock();
            e.Return = result.IsReturn ? result : Variable.EmptyInstance;
        }
    }

    internal sealed class DoWhileStatement : FunctionBase
    {
        public DoWhileStatement()
        {
            Name = Constants.DO;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += DoWhileStatement_Run;
        }

        private void DoWhileStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            int startDoCondition = e.Script.Pointer;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                e.Script.Pointer = startDoCondition;

                result = e.Script.ProcessBlock(true);
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

            e.Return = result.IsReturn ? result : Variable.EmptyInstance;
        }
    }

    internal sealed class SwitchStatement : FunctionBase
    {
        public SwitchStatement()
        {
            Name = Constants.SWITCH;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += SwitchStatement_Run;
        }

        private void SwitchStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable switchValue = Utils.GetItem(e.Script);
            e.Script.Forward();

            Variable result = Variable.EmptyInstance;
            var caseSep = ":".ToCharArray();

            bool fallThrough = e.Script.FallThrough;
            bool needBreak = e.Script.CheckBreakWhenEndCaseBlock;
            bool caseDone = false;
            bool nextTrue = false;

            int startPointer = e.Script.Pointer - 1;


            while (e.Script.StillValid())
            {
                var nextToken = Utils.GetBodySize(e.Script, Constants.CASE, Constants.DEFAULT);
                if (string.IsNullOrEmpty(nextToken))
                {
                    break;
                }
                if (nextToken == Constants.DEFAULT && (!caseDone || fallThrough))
                {
                    result = e.Script.ProcessBlock();
                    //スタート地点に帰ってブロックを終わらせる
                    e.Script.Pointer = startPointer;
                    e.Script.SkipBlock();
                    if (needBreak && result.Type != Variable.VarType.BREAK && !result.IsReturn)
                    {
                        throw new ScriptException("defaultブロックはbreakまたはreturnで抜ける必要があります", Exceptions.CASE_BLOCK_MISSING_BREAK);
                    }
                }
                if (!caseDone || fallThrough)
                {
                    Variable caseValue = e.Script.Execute(caseSep);
                    bool equal = switchValue.Equals(caseValue);
                    e.Script.Forward();
                    if (e.Script.Current != Constants.START_GROUP)
                    {
                        //ほかの条件がある場合
                        nextTrue = equal;
                        continue;
                    }
                    e.Script.Forward();
                    if (caseDone || equal || nextTrue)
                    {
                        nextTrue = false;
                        caseDone = true;
                        result = e.Script.ProcessBlock();
                        if (!fallThrough)
                        {
                            //スタート地点に帰ってブロックを終わらせる
                            e.Script.Pointer = startPointer;
                            e.Script.SkipBlock();
                            if (needBreak && result.Type != Variable.VarType.BREAK && !result.IsReturn)
                            {
                                throw new ScriptException("caseブロックはbreakまたはreturnで抜ける必要があります", Exceptions.CASE_BLOCK_MISSING_BREAK);
                            }
                            break;
                        }
                        e.Script.SkipBlock();
                        e.Script.Forward();
                    }
                    else
                    {
                        e.Script.Backward();
                        e.Script.SkipBlock();
                    }
                }
            }
            e.Script.GoToNextStatement();
            e.Return = result;
        }
    }

    internal sealed class CaseStatement : FunctionBase
    {
        public CaseStatement()
        {
            Name = Constants.CASE;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += CaseStatement_Run;
        }

        private void CaseStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Name == Constants.CASE)
            {
                /*var token = */
                Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            }
            e.Script.MoveForwardIf(':');

            e.Return = e.Script.ProcessBlock();
            e.Script.MoveBackIfPrevious('}');
        }

    }
    //デリゲートを作成する関数クラスです
    internal sealed class DelegateCreator : FunctionBase
    {
        public DelegateCreator()
        {
            Name = "delegate";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += DelegateCreator_Run;
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
    internal sealed class TryBlock : FunctionBase
    {
        public TryBlock()
        {
            Name = Constants.TRY;
            Attribute = FunctionAttribute.CONTROL_FLOW | FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += TryBlock_Run;
        }
        private class CatchData
        {
            /// <summary>
            /// このオブジェクトがCatchで、かつ例外オブジェクトを受取るときその変数名
            /// </summary>
            public string ExceptionName { get; set; }
            /// <summary>
            /// このオブジェクトがCatchで、かつWhenで条件フィルターがある場合条件式
            /// </summary>
            public string Filter { get; set; }

            /// <summary>
            /// このブロックの本文
            /// </summary>
            public string Body { get; set; }
        }
        private void TryBlock_Run(object sender, FunctionBaseEventArgs e)
        {
            int startTryCondition = e.Script.Pointer - 1;
            int currentStackLevel = ParserFunction.GetCurrentStackLevel();

            Variable result = null;

            // tryブロック内のスクリプト
            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript mainScript = e.Script.GetTempScript(body);
            // catchブロックのリスト
            List<CatchData> catches = new List<CatchData>();
            // finallyブロック
            string final_body = null;

            ParsingScript nextData = new ParsingScript(e.Script);
            nextData.ParentScript = e.Script;
            while (true)
            {
                string nextToken = Utils.GetNextToken(nextData);
                nextData.Forward();

                if (Constants.CATCH == nextToken)
                {
                    var data = new CatchData();
                    if (nextData.Prev == '(')
                    {
                        data.ExceptionName = Utils.GetNextToken(nextData);
                        nextData.Forward(); // skip closing parenthesis
                    }
                    if (nextData.Prev != '{' && Utils.GetNextToken(nextData) == Constants.WHEN)
                    {
                        data.Filter = Utils.GetBodyBetween(nextData, Constants.START_ARG, Constants.END_ARG, Constants.START_GROUP.ToString() + '\0', true);
                        nextData.Forward();
                    }
                    data.Body = Utils.GetBodyBetween(nextData, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                    catches.Add(data);

                    e.Script.Pointer = nextData.Pointer + 1;
                }
                else if (Constants.FINALLY == nextToken)
                {
                    final_body = Utils.GetBodyBetween(nextData, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

                    e.Script.Pointer = nextData.Pointer + 1;
                    break;
                }
                else if (!string.IsNullOrEmpty(nextToken) || !nextData.StillValid())
                {
                    break;
                }
            }

            bool handled = catches.Count > 0 || final_body != null;
            if (!handled)
            {
                throw new ScriptException("tryブロックには1つ以上catchまたはfinallyが必要です。", Exceptions.TRY_BLOCK_MISSING_HANDLERS, e.Script);
            }

            mainScript.ThrowError += delegate (object sender, ThrowErrorEventArgs e)
            {
                foreach (var data in catches)
                {
                    GetVarFunction excMsgFunc = new GetVarFunction(new Variable(new ExceptionObject(e.Message, e.ErrorCode, e.Script, e.Source, e.HelpLink)));
                    if (data.Filter != null)
                    {
                        ParsingScript filterScript = e.Script.GetTempScript(data.Filter, this);
                        if (data.ExceptionName != null)
                        {
                            filterScript.Variables.Add(data.ExceptionName, excMsgFunc);
                        }
                        Variable condition = filterScript.Process();
                        bool? isTrue = condition?.AsBool();
                        if (isTrue != true)
                        {
                            continue;
                        }
                    }

                    ParsingScript catchScript = e.Script.GetTempScript(data.Body);
                    if (data.ExceptionName != null)
                    {
                        catchScript.Variables.Add(data.ExceptionName, excMsgFunc);
                    }
                    result = catchScript.Process();
                    e.Handled = true;
                    break;
                }
                if (final_body != null)
                {
                    ParsingScript finallyScript = e.Script.GetTempScript(final_body);
                    finallyScript.Process();
                    e.Handled = true;
                }
            };

            result = mainScript.Process();

            if (final_body != null)
            {
                ParsingScript finallyScript = e.Script.GetTempScript(final_body);
                finallyScript.Process();
            }

            e.Script.SkipRestBlocks();
            e.Return = result;
        }
    }
    internal sealed class BlockStatement : FunctionBase
    {
        public BlockStatement()
        {
            Name = "block";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += DoWhileStatement_Run;
        }

        private void DoWhileStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.Script.ProcessBlock();
        }
    }
}
