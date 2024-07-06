using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        /// <summary>
        /// このブロック内で、指定された変数をすべて読み取り専用にします
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="items">読み取り専用にしたい変数</param>
        /// <returns>本文の実行結果</returns>
        public static Variable Readonly(ParsingScript script, params Variable[] items)
        {
            BitArray beforeStates = new BitArray(items.Length);

            for (int i = 0; i < items.Length; i++)//もともとの状態を覚えておく
            {
                beforeStates[i] = items[i].Readonly;
                items[i].Readonly = true;
            }

            Variable result = script.ProcessBlock();

            for (int i = 0; i < items.Length; i++)//実行後に元に戻す
            {
                items[i].Readonly = beforeStates[i];
            }

            return result;
        }
        /// <summary>
        /// このブロック内で、指定された変数への排他的なアクセスを保証します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="item">排他的ロックを行いたい変数</param>
        /// <returns>本文の実行結果</returns>
        public static Variable Lock(ParsingScript script, Variable item)
        {
            Variable result;

            lock (item)
            {
                result = script.ProcessBlock();
            }

            return result;
        }

        /// <summary>
        /// ブロックの中の子スクリプトを実行します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <returns>本文の実行結果</returns>
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Block(ParsingScript script)
        {
            return script.ProcessBlock();
        }
        /// <summary>
        /// 指定した名前空間への参照を現在のスクリプトに追加します
        /// </summary>
        /// <param name="e">この関数の実行時情報</param>
        /// <exception cref="ScriptException">usingを許可しない設定である場合にスローされる例外</exception>

        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static void Using(FunctionBaseEventArgs e)
        {
            bool isGlobal = e.Keywords.Contains(Constants.PUBLIC);
            string name = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            var script = e.Script;
            if (isGlobal)
            {
                script = ParsingScript.GetTopLevelScript(script);
            }
            if (e.Script.EnableUsing)
            {

                script.Using(name);
            }
            else
            {
                throw new ScriptException("その操作は禁止されています", Exceptions.FORBIDDEN_OPERATION, e.Script);
            }
        }

        /// <summary>
        /// 指定された式が真と評価されたときに、本文を実行します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="func">この関数がバインドされるFunctionBase</param>
        /// <param name="condition">本文を実行するかどうかを決める条件</param>
        /// <returns>本文の実行結果</returns>
        public static Variable If(ParsingScript script, BindFunction func, bool condition)
        {
            Variable result = Variable.EmptyInstance;

            if (condition)
            {
                result = script.ProcessBlock();

                if (result is not null && (result.IsReturn ||
                    result.Type == Variable.VarType.BREAK ||
                    result.Type == Variable.VarType.CONTINUE))
                {
                    // if文中で早期リターンしたからブロックごと飛ばす
                    script.SkipBlock();
                }
                script.SkipRestBlocks();

                //return result;
            }
            else
            {
                // elseブロックのためifを飛ばす
                script.SkipBlock();
            }

            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;

            string nextToken = Utils.GetNextToken(nextData, false, true);

            if (Constants.ELSE_IF == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;

                if (condition)
                {
                    script.SkipBlock();
                }
                else
                {
                    result = func.Execute(script);
                }
            }
            else if (Constants.ELSE == nextToken)
            {
                script.Pointer = nextData.Pointer + 1;

                // 一応その次のトークンも調べる
                nextToken = Utils.GetNextToken(nextData, false, true);

                if (Constants.IF == nextToken)
                {
                    // もしelseの次がifなら、else ifのため続きで実行
                    script.Pointer = nextData.Pointer + 1;
                    if (condition)
                    {
                        script.SkipBlock();
                    }
                    else
                    {
                        result = func.Execute(script);// この関数を再帰的に呼び出す
                    }
                }
                else
                {
                    if (condition)
                    {
                        script.SkipBlock();
                    }
                    else
                    {
                        result = script.ProcessBlock();
                    }
                }

            }
            if (result is null)
            {
                result = Variable.EmptyInstance;
            }

            return result;
        }
        /// <summary>
        /// 指定した値に一致する文を実行します
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="item">比較する値</param>
        /// <returns>本文の実行結果</returns>
        /// <exception cref="ScriptException">内包する文がbreakまたはreturnで抜けることができない場合に発生する例外</exception>
        public static Variable Switch(ParsingScript script, Variable item)
        {

            Variable result = Variable.EmptyInstance;
            var caseSep = ":".ToCharArray();

            bool fallThrough = script.FallThrough;
            bool needBreak = script.CheckBreakWhenEndCaseBlock;
            bool caseDone = false;
            bool nextTrue = false;

            int startPointer = script.Pointer - 1;


            while (script.StillValid())
            {
                var nextToken = Utils.GetBodySize(script, Constants.CASE, Constants.DEFAULT);
                if (string.IsNullOrEmpty(nextToken))
                {
                    break;
                }
                if (nextToken == Constants.DEFAULT && (!caseDone || fallThrough))
                {
                    result = script.ProcessBlock();
                    //スタート地点に帰ってブロックを終わらせる
                    script.Pointer = startPointer;
                    script.SkipBlock();
                    if (needBreak && result.Type != Variable.VarType.BREAK && !result.IsReturn && result.Type != Variable.VarType.CONTINUE)
                    {
                        throw new ScriptException("defaultブロックはbreakまたはreturnで抜ける必要があります", Exceptions.CASE_BLOCK_MISSING_BREAK);
                    }
                }
                if (!caseDone || fallThrough)
                {
                    Variable caseValue = script.Execute(caseSep);
                    bool equal = item.Equals(caseValue);
                    script.Forward();
                    if (script.Current != Constants.START_GROUP)
                    {
                        //ほかの条件がある場合
                        nextTrue = equal;
                        continue;
                    }
                    script.Forward();
                    if (caseDone || equal || nextTrue)
                    {
                        nextTrue = false;
                        caseDone = true;
                        result = script.ProcessBlock();
                        if (result.Type == Variable.VarType.BREAK || result.IsReturn || result.Type == Variable.VarType.CONTINUE)
                        {
                            script.SkipBlock();
                            script.Forward();
                            break;
                        }
                        if (!fallThrough)
                        {
                            //スタート地点に帰ってブロックを終わらせる
                            script.Pointer = startPointer;
                            script.SkipBlock();
                            if (needBreak && result.Type != Variable.VarType.BREAK && !result.IsReturn && result.Type != Variable.VarType.CONTINUE)
                            {
                                throw new ScriptException("caseブロックはbreakまたはreturnで抜ける必要があります", Exceptions.CASE_BLOCK_MISSING_BREAK);
                            }
                            break;
                        }
                        script.SkipBlock();
                        script.Forward();
                    }
                    else
                    {
                        script.Backward();
                        script.SkipBlock();
                    }
                }
            }
            script.GoToNextStatement();
            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable While(ParsingScript script)
        {
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;
            string condExp = Utils.GetBodyBetween(script);
            script.Forward();
            string loopBody = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);

            while (stillValid)
            {
                stillValid = script.GetTempScript(condExp).Process().Bool;
                if (!stillValid)
                {
                    break;
                }
                result = script.GetTempScript(loopBody).Process();
                if (result.IsReturn)
                {
                    return result;
                }

                if (result.Type == Variable.VarType.BREAK)
                {
                    return Variable.EmptyInstance;
                }
                if (result.Type == Variable.VarType.CONTINUE)
                {
                    continue;
                }
            }

            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Do(ParsingScript script)
        {
            int startDoCondition = script.Pointer;
            bool stillValid = true;
            Variable result = Variable.EmptyInstance;

            while (stillValid)
            {
                script.Pointer = startDoCondition;

                result = script.ProcessBlock();
                if (result.IsReturn)
                {
                    script.Pointer = startDoCondition;
                    break;
                }

                if (result.Type == Variable.VarType.BREAK)
                {
                    return Variable.EmptyInstance;
                }
                if (result.Type == Variable.VarType.CONTINUE)
                {
                    continue;
                }
                script.Forward(Constants.WHILE.Length + 1);
                Variable condResult = script.Execute(Constants.END_ARG_ARRAY);
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }
            }
            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable For(ParsingScript script)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();
            //for(init; condition; loopStatemen;)の形式です
            string[] forTokens = forString.Split(Constants.END_STATEMENT);
            if (forTokens.Length < 3)
            {
                Utils.ThrowErrorMsg("for文はfor(init; condition; loopStatement;)の形である必要があります", Exceptions.INVALID_SYNTAX,
                                     script, Constants.FOR);
            }

            int startForCondition = script.Pointer;

            ParsingScript initScript = script.GetTempScript(forTokens[0] + Constants.END_STATEMENT);
            ParsingScript condScript = initScript.GetTempScript(forTokens[1] + Constants.END_STATEMENT);
            ParsingScript loopScript = initScript.GetTempScript(forTokens[2] + Constants.END_STATEMENT);

            condScript.Variables = loopScript.Variables = initScript.Variables;


            script.Pointer = startForCondition;
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

            initScript.Execute(null, 0);
            bool stillValid = true;

            while (stillValid)
            {
                Variable condResult = condScript.Execute(null, 0); condScript.Tag = "COND";
                if (condResult is null)
                {
                    condResult = Variable.EmptyInstance;
                }
                stillValid = condResult.AsBool();
                if (!stillValid)
                {
                    break;
                }
                ParsingScript mainScript = initScript.GetTempScript(body);
                Variable result = mainScript.Process();
                if (result.IsReturn)
                {
                    return result;
                }

                if (result.Type == Variable.VarType.BREAK)
                {
                    return Variable.EmptyInstance;
                }
                if (result.Type == Variable.VarType.CONTINUE)
                {
                    continue;
                }
                loopScript.Execute(null, 0);
            }

            script.Pointer = startForCondition;
            script.SkipBlock();
            return Variable.EmptyInstance;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Foreach(ParsingScript script, BindFunction func)
        {
            string forString = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);
            script.Forward();
            string bodyString = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            string[] tokens = forString.Split(Constants.FOR_IN);

            if (tokens.Length != 2)
            {
                Utils.ThrowErrorMsg("foreach文はforeach(variable in array)の形をとるべきです", Exceptions.INVALID_SYNTAX, script, "foreach");
            }

            ParsingScript forScript = script.GetTempScript(tokens[1], func);

            Variable arrayValue = Utils.GetItem(forScript);

            if (arrayValue is null)
            {
                return Variable.EmptyInstance;
            }

            Variable ProcessEach(Variable current)
            {
                // 代入式を逐次実行できるように隠し変数を作成
                current.Readonly = true;
                string forConstName = Constants.USER_CANT_USE_VARIABLE_PREFIX + "eachconst";

                var body = new StringBuilder();
                body.Append(tokens[0].Trim());
                body.Append(Constants.ASSIGNMENT);
                body.Append(forConstName);
                body.Append(Constants.END_STATEMENT);
                body.Append(bodyString);

                ParsingScript mainScript = forScript.GetTempScript(body.ToString());
                mainScript.Variables[forConstName] = new ValueFunction(current);

                return mainScript.Process(true);
            }

            switch (arrayValue.Type)
            {
                case Variable.VarType.ARRAY:
                    {
                        int cycles = arrayValue.Count;
                        for (int i = 0; i < cycles; i++)
                        {
                            Variable result = ProcessEach(arrayValue.GetValue(i));
                            if (result.IsReturn)
                            {
                                return result;
                            }

                            if (result.Type == Variable.VarType.BREAK)
                            {
                                return Variable.EmptyInstance;
                            }
                            if (result.Type == Variable.VarType.CONTINUE)
                            {
                                continue;
                            }
                        }
                        break;
                    }
                case Variable.VarType.STRING:
                    {
                        foreach (char c in arrayValue.String)
                        {
                            Variable result = ProcessEach(new Variable(c.ToString()));
                            if (result.IsReturn)
                            {
                                return result;
                            }

                            if (result.Type == Variable.VarType.BREAK)
                            {
                                return Variable.EmptyInstance;
                            }
                            if (result.Type == Variable.VarType.CONTINUE)
                            {
                                continue;
                            }
                        }
                        break;
                    }
                case Variable.VarType.OBJECT:
                    {
                        EnumeratorObject enumerator = forScript.GetTempScript(tokens[1] + ".GetEnumerator()").Process().Object as EnumeratorObject;
                        if (enumerator is null)
                        {
                            Utils.ThrowErrorMsg("foreachでループできるオブジェクトはGetEnumeratorメソッドを実装する必要があります", Exceptions.INVALID_SYNTAX, script, "foreach");
                        }
                        IDisposable disposable = enumerator.Enumerator;
                        while (enumerator.MoveNext())
                        {
                            Variable result = ProcessEach(new Variable(enumerator.Current));
                            if (result.IsReturn)
                            {
                                disposable?.Dispose();
                                return result;
                            }

                            if (result.Type == Variable.VarType.BREAK)
                            {
                                return Variable.EmptyInstance;
                            }
                            if (result.Type == Variable.VarType.CONTINUE)
                            {
                                continue;
                            }
                        }
                        disposable?.Dispose();
                        break;
                    }
            }

            if (arrayValue.Type == Variable.VarType.STRING)
            {
                arrayValue = new Variable(new List<string>(arrayValue.ToString().ToCharArray().Select(c => c.ToString())));
            }
            return Variable.EmptyInstance;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Case(ParsingScript script)
        {
            Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            script.MoveForwardIf(':');

            var result = script.ProcessBlock();
            script.MoveBackIfPrevious('}');
            return result;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static void Goto(ParsingScript script, BindFunction func)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            if (script.AllLabels is null || script.LabelToFile is null |
               !script.AllLabels.TryGetValue(script.FunctionName, out Dictionary<string, int> labels))
            {
                Utils.ThrowErrorMsg("次のラベルは関数内に存在しません [" + script.FunctionName + "]", Exceptions.COULDNT_FIND_LABEL_IN_FUNCTION,
                                    script, func.Name);
                return;
            }

            if (!labels.TryGetValue(labelName, out int gotoPointer))
            {
                Utils.ThrowErrorMsg("ラベル:[" + labelName + "]は定義されていません", Exceptions.COULDNT_FIND_LABEL,
                                    script, func.Name);
                return;
            }

            if (script.LabelToFile.TryGetValue(labelName, out string filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename, func);
                script.Filename = filename;
                script.String = newScript.String;
            }

            if (func.Name == "GoSub")
            {
                script.PointersBack.Add(script.Pointer);
            }

            script.Pointer = gotoPointer;
            if (string.IsNullOrWhiteSpace(script.FunctionName))
            {
                script.Backward();
            }

            return;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static void NameSpace(ParsingScript script)
        {
            string spaceName = Utils.GetToken(script, Constants.TOKEN_SEPARATION).ToLowerInvariant();

            if (script.Current == Constants.START_GROUP)
            {
                // 名前空間ブロック
                ParsingScript block = script.GetBlock();

                if (NameSpaceManager.NameSpaces.TryGetValue(spaceName, out var space))
                {
                    block.NameSpace = space;
                }
                else
                {
                    space = new NameSpace();
                    space.Name = spaceName;
                    block.NameSpace = space;
                    NameSpaceManager.Add(space);
                }

                block.Process();
            }
            else
            {
                // スコープ名前空間
                if (NameSpaceManager.NameSpaces.TryGetValue(spaceName, out var space))
                {
                    script.NameSpace = space;
                }
                else
                {
                    space = new NameSpace();
                    space.Name = spaceName;
                    script.NameSpace = space;
                    NameSpaceManager.Add(space);
                }
            }

        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static void GoSub(ParsingScript script, BindFunction func)
        {
            Goto(script, func);
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Default(ParsingScript script)
        {
            script.MoveForwardIf(':');

            var result = script.ProcessBlock();
            script.MoveBackIfPrevious('}');
            return result;
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
        /// <summary>
        /// 例外が発生する可能性があるコードを検証するためのブロック
        /// </summary>
        /// <param name="script">このブロックがあるスクリプト</param>
        /// <param name="func">呼び出し時の関数インスタンス</param>
        /// <returns>本文の実行結果</returns>
        /// <exception cref="ScriptException">catchブロックおよびfinallyブロックがありません</exception>
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Try(ParsingScript script, BindFunction func)
        {
            int startTryCondition = script.Pointer - 1;

            Variable result = null;

            // tryブロック内のスクリプト
            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
            ParsingScript mainScript = script.GetTempScript(body);
            // catchブロックのリスト
            List<CatchData> catches = new List<CatchData>();
            // finallyブロック
            string final_body = null;

            ParsingScript nextData = new ParsingScript(script);
            nextData.ParentScript = script;
            while (true)
            {
                nextData.Forward();
                string nextToken = Utils.GetNextToken(nextData, false, true);
                nextData.MoveForwardWhile(Constants.EMPTY_AND_WHITE);
                nextData.Forward();

                if (Constants.CATCH == nextToken)
                {
                    var data = new CatchData();
                    if (nextData.Prev == '(')
                    {
                        data.ExceptionName = Utils.GetNextToken(nextData);
                        nextData.Forward();
                    }
                    if (nextData.Prev != '{' && Utils.GetNextToken(nextData, false, true) == Constants.WHEN)
                    {
                        data.Filter = Utils.GetBodyBetween(nextData, Constants.START_ARG, Constants.END_ARG, Constants.START_GROUP.ToString() + '\0', true);
                        nextData.Forward();
                    }
                    data.Body = Utils.GetBodyBetween(nextData, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                    catches.Add(data);

                    script.Pointer = nextData.Pointer + 1;
                    continue;
                }
                else if (Constants.FINALLY == nextToken)
                {
                    final_body = Utils.GetBodyBetween(nextData, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

                    script.Pointer = nextData.Pointer + 1;
                    break;
                }
                else
                {
                    break;
                }

            }

            bool handled = catches.Count > 0 || final_body is not null;
            if (!handled)
            {
                throw new ScriptException("tryブロックには1つ以上catchまたはfinallyブロックが必要です。", Exceptions.TRY_BLOCK_MISSING_HANDLERS, script);
            }

            mainScript.ThrowError += delegate (object sender, ThrowErrorEventArgs e)
            {
                foreach (var data in catches)
                {
                    ValueFunction excMsgFunc = new ValueFunction(new Variable(new ExceptionObject(e.Message, e.ErrorCode, e.Script, e.Source, e.HelpLink)));
                    if (data.Filter is not null)
                    {
                        ParsingScript filterScript = script.ParentScript.GetTempScript(data.Filter, func);
                        if (data.ExceptionName is not null)
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

                    ParsingScript catchScript = script.ParentScript.GetTempScript(data.Body);
                    if (data.ExceptionName is not null)
                    {
                        catchScript.Variables.Add(data.ExceptionName, excMsgFunc);
                    }
                    result = catchScript.Process();
                    e.Handled = true;
                    break;
                }
                if (final_body is not null)
                {
                    ParsingScript finallyScript = script.GetTempScript(final_body);
                    finallyScript.Process();
                    e.Handled = true;
                }
            };

            result = mainScript.Process();

            if (final_body is not null)
            {
                ParsingScript finallyScript = script.GetTempScript(final_body);
                finallyScript.Process();
            }

            script.SkipRestBlocks();
            return result;
        }
        /// <summary>
        /// 本文と引数の内容からデリゲートを生成します
        /// </summary>
        /// <param name="script">本文の実行結果</param>
        /// <returns>生成されたデリゲート</returns>
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static DelegateObject Delegate(ParsingScript script)
        {
            string[] args = Utils.GetFunctionSignature(script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = Array.Empty<string>();
            }

            script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            script.GetOriginalLine(out _);

            int parentOffset = script.Pointer;

            if (script.CurrentClass is not null)
            {
                parentOffset += script.CurrentClass.ParentOffset;
            }

            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);

            script.MoveForwardIf(Constants.END_GROUP);
            CustomFunction customFunc = new CustomFunction("", body, args, script);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            return new DelegateObject(customFunc);
        }
    }
}
