using AliceScript.Functions;
using AliceScript.Objects;
using System.Text;

namespace AliceScript.Parsing
{
    /// <summary>
    /// スクリプトを解析します
    /// </summary>
    public class Parser
    {
        /// <summary>
        /// スクリプトを解析・実行し、実行結果を返します
        /// </summary>
        /// <param name="script">解析・実行するスクリプト</param>
        /// <param name="to">式を終了させる文字</param>
        /// <returns>スクリプトの実行結果</returns>
        /// <exception cref="ScriptException">スクリプトを解析できない場合に発生する例外</exception>
        public static Variable AliceScript(ParsingScript script, char[] to)
        {
            // まず、式をトークンごとに分割してVariableにする
            List<Variable> listToMerge = Split(script, to);

            if (listToMerge.Count == 0)
            {
                throw new ScriptException(script.Rest + "を解析できません", Exceptions.COULDNT_PARSE, script);
            }

            // 得られたVariable同士を演算する
            Variable result = MergeList(listToMerge, script);
            return result;
        }

        /// <summary>
        /// 非同期的にスクリプトを解析・実行し、実行結果を返します
        /// </summary>
        /// <param name="script">解析・実行するスクリプト</param>
        /// <param name="to">式を終了させる文字</param>
        /// <returns>スクリプトの実行結果</returns>
        /// <exception cref="ScriptException">スクリプトを解析できない場合に発生する例外</exception>
        public static async Task<Variable> AliceScriptAsync(ParsingScript script, char[] to)
        {
            // まず、式をトークンごとに分割してVariableにする
            List<Variable> listToMerge = await SplitAsync(script, to);

            if (listToMerge.Count == 0)
            {
                throw new ScriptException(script.Rest + "を解析できません", Exceptions.COULDNT_PARSE, script);
            }

            // 得られたVariable同士を演算する
            Variable result = MergeList(listToMerge, script);
            return result;
        }

        private static List<Variable> Split(ParsingScript script, char[] to)
        {
            List<Variable> listToMerge = new List<Variable>();

            if (!script.StillValid() || to.Contains(script.Current))
            {
                listToMerge.Add(Variable.EmptyInstance);
                script.Forward();
                return listToMerge;
            }

            int arrayIndexDepth = 0;
            bool inQuotes = false;
            int negated = 0;
            char ch;
            string action;

            do
            {
                HashSet<string> keywords = new HashSet<string>();
            ExtractNextToken:
                string token = ExtractNextToken(script, to, ref inQuotes, ref arrayIndexDepth, ref negated, out ch, out action);

                if (string.IsNullOrEmpty(token) && script.Prev != Constants.START_ARG && script.Prev != Constants.START_GROUP && script.StillValid())
                {
                    //トークンが空で、無意味だった場合
                    goto ExtractNextToken;
                }
                if (!(script.Current == ';' || Constants.TOKEN_SEPARATION_ANDEND_STR.Contains(script.Next)) && Constants.KEYWORD.Contains(token))
                {
                    //null許容型修飾子の場合(bool?とか)
                    if (script.Current == '?')
                    {
                        token += '?';
                        //本来の位置に進めておく
                        script.Forward();
                    }
                    keywords.Add(token.ToLowerInvariant());//キーワード一覧に格納
                    goto ExtractNextToken;
                }

                bool ternary = UpdateIfTernary(script, token, ch, listToMerge, (List<Variable> newList) => { listToMerge = newList; });
                if (ternary)
                {
                    return listToMerge;
                }

                PreOperetors negSign = CheckConsistencyAndSign(script, listToMerge, action, ref token);//前置演算子を取得

                // このトークンに対応する関数を取得する
                ParserFunction func = new ParserFunction(script, token, ch, ref action, keywords);
                if (func.m_impl is FunctionBase fb && (script.ProcessingFunction == null || !(fb is LiteralFunction)))
                {
                    script.ProcessingFunction = fb;//現在処理中としてマーク
                    if(fb is AttributeFunction af)
                    {
                        script.AttributeFunction = af;
                    }
                }
                Variable current = func.GetValue(script);//関数を呼び出し
                if (UpdateResult(script, to, listToMerge, token, negSign, ref current, ref negated, ref action))
                {
                    return listToMerge;
                }
            } while (script.StillValid() &&
                    (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current)));

            script.MoveForwardIf(Constants.END_ARG);

            return listToMerge;
        }

        private static async Task<List<Variable>> SplitAsync(ParsingScript script, char[] to)
        {
            List<Variable> listToMerge = new List<Variable>(16);

            if (!script.StillValid() || to.Contains(script.Current))
            {
                listToMerge.Add(Variable.EmptyInstance);
                script.Forward();
                return listToMerge;
            }

            int arrayIndexDepth = 0;
            bool inQuotes = false;
            int negated = 0;
            char ch;
            string action;

            do
            { // Main processing cycle of the first part.
                HashSet<string> keywords = new HashSet<string>();
            ExtractNextToken:
                string token = ExtractNextToken(script, to, ref inQuotes, ref arrayIndexDepth, ref negated, out ch, out action);

                if (!(script.Current == ';' || Constants.TOKEN_SEPARATION_ANDEND_STR.Contains(script.Next)) && Constants.KEYWORD.Contains(token))
                {
                    keywords.Add(token);
                    goto ExtractNextToken;
                }
                if (string.IsNullOrEmpty(token) && script.StillValid())
                {
                    goto ExtractNextToken;
                }

                bool ternary = UpdateIfTernary(script, token, ch, listToMerge, (List<Variable> newList) => { listToMerge = newList; });
                if (ternary)
                {
                    return listToMerge;
                }

                PreOperetors negSign = CheckConsistencyAndSign(script, listToMerge, action, ref token);

                // We are done getting the next token. The GetValue() call below may
                // recursively call AliceScript(). This will happen if extracted
                // item is a function or if the next item is starting with a START_ARG '('.
                ParserFunction func = new ParserFunction(script, token, ch, ref action, keywords);
                if (func.m_impl is FunctionBase fb && (script.ProcessingFunction == null || !(fb is LiteralFunction)))
                {
                    script.ProcessingFunction = fb;
                }
                Variable current = await func.GetValueAsync(script);
                if (UpdateResult(script, to, listToMerge, token, negSign, ref current, ref negated, ref action))
                {
                    return listToMerge;
                }
            } while (script.StillValid() &&
                    (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current)));

            // This happens when called recursively inside of the math expression:
            script.MoveForwardIf(Constants.END_ARG);

            return listToMerge;
        }

        public static string ExtractNextToken(ParsingScript script, char[] to, ref bool inQuotes,
            ref int arrayIndexDepth, ref int negated, out char ch, out string action, bool throwExc = true)
        {
            StringBuilder item = new StringBuilder();
            ch = Constants.EMPTY;
            action = null;
            do
            {
                string negateSymbol = Utils.IsNotSign(script.Rest);
                if (negateSymbol != null && !inQuotes)
                {
                    negated++;
                    script.Forward(negateSymbol.Length);
                    continue;
                }

                ch = script.CurrentAndForward();
                CheckQuotesIndices(script, ch, ref inQuotes, ref arrayIndexDepth);

                bool keepCollecting = inQuotes || arrayIndexDepth > 0 ||
                     StillCollecting(item.ToString(), to, script, ref action);
                if (keepCollecting)
                {
                    // 文字をオペランドに追加
                    item.Append(ch);

                    bool goForMore = script.StillValid() &&
                        (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current));
                    if (goForMore)
                    {
                        continue;
                    }
                }

                if (SkipOrAppendIfNecessary(item, ch, to))
                {
                    continue;
                }
                break;
            }
            while (true);

            if (to.Contains(Constants.END_ARRAY) && ch == Constants.END_ARRAY &&
                item[item.Length - 1] != Constants.END_ARRAY &&
                item.ToString().Contains(Constants.START_ARRAY))
            {
                item.Append(ch);
            }

            string result = item.ToString();

            if (throwExc && string.IsNullOrWhiteSpace(result) && action != "++" && action != "--" &&
                Utils.IsAction(script.Prev) && Utils.IsAction(script.PrevPrev))
            {
                Utils.ThrowErrorMsg("次のトークンを実行できませんでした [" + script.PrevPrev + script.Prev + script.Current +
                                    "].", Exceptions.INVALID_TOKEN, script, script.Current.ToString());
            }

            return result;
        }

        private static bool UpdateResult(ParsingScript script, char[] to, List<Variable> listToMerge, string token, PreOperetors preop,
                                 ref Variable current, ref int negated, ref string action)
        {
            if (current == null)
            {
                current = Variable.EmptyInstance;
            }
            current.ParsingToken = token;

            if(current.Type == Variable.VarType.NUMBER && current.m_value.HasValue)
            {
                switch (preop)
                {
                    case PreOperetors.Minus:
                        {
                            // -マークがついている場合は数値がマイナス
                            current.Value = current.Value * -1;
                            break;
                        }
                    case PreOperetors.Increment:
                        {
                            current.Value++;
                            break;
                        }
                    case PreOperetors.Decrement:
                        {
                            current.Value--;
                            break;
                        }
                }
            }

            if (negated > 0 && current.Type == Variable.VarType.BOOLEAN)
            {
                // !マークがついている場合は真理値否定
                // 排他的論理和を使用して評価
                bool neg = !((negated % 2 == 0) ^ current.AsBool());
                current = new Variable(neg);
                negated = 0;
            }
            if (script.Current == '.')
            {
                bool inQuotes = false;
                int arrayIndexDepth = 0;
                script.Forward();
                string property = ExtractNextToken(script, to, ref inQuotes, ref arrayIndexDepth, ref negated, out _, out action);

                Variable propValue = current.Type == Variable.VarType.ENUM ?
                     current.GetEnumProperty(property, script) :
                     current.GetProperty(property, script);
                current = propValue;
            }

            if (action == null)
            {
                action = UpdateAction(script, to);
            }
            else
            {
                script.MoveForwardIf(action[0]);
            }


            char next = script.TryCurrent(); // 前進済み
            bool done = listToMerge.Count == 0 &&
                        (next == Constants.END_STATEMENT ||
                        ((action == Constants.NULL_ACTION) && current != null && current.Type != Variable.VarType.BOOLEAN) ||
                         (current != null && current.IsReturn));
            if (done)
            {

                // 数値結果がない場合は、数式ではありません
                listToMerge.Add(current);
                return true;
            }

            Variable cell = current.Clone();
            cell.Action = action;
            bool addIt = UpdateIfBool(script, cell, (Variable newCell) => { cell = newCell; }, listToMerge, (List<Variable> var) => { listToMerge = var; });
            if (addIt)
            {
                listToMerge.Add(cell);
            }
            return false;
        }

        private static PreOperetors CheckConsistencyAndSign(ParsingScript script, List<Variable> listToMerge, string action, ref string token)
        {
            if (Constants.CONTROL_FLOW.Contains(token) && listToMerge.Count > 0)
            {
                listToMerge.Clear();
            }

            script.MoveForwardIf(Constants.SPACE);

            if (action != null && action.Length > 1)
            {
                script.Forward(action.Length - 1);
            }

            if (token.Length > 2 && token.StartsWith(Constants.INCREMENT, StringComparison.Ordinal) && token[2] != Constants.QUOTE)
            {
                token = token.Substring(2);
                return PreOperetors.Increment;
            }
            else if (token.Length > 2 && token.StartsWith(Constants.DECREMENT, StringComparison.Ordinal) && token[2] != Constants.QUOTE)
            {
                token = token.Substring(2);
                return PreOperetors.Decrement;
            }
            else if (token.Length > 1 && token[0] == '+' && token[1] != Constants.QUOTE)
            {
                token = token.Substring(1);
                //単項プラス演算子は何もする必要がない
                return PreOperetors.None;
            }
            else if (token.Length > 1 && token[0] == '-' && token[1] != Constants.QUOTE)
            {
                token = token.Substring(1);
                return PreOperetors.Minus;
            }
            return PreOperetors.None;
        }

        private enum PreOperetors
        {
            /// <summary>
            /// 前置インクリメント
            /// </summary>
            Increment,
            /// <summary>
            /// 前置デクリメント
            /// </summary>
            Decrement,
            /// <summary>
            /// 単項マイナス
            /// </summary>
            Minus, None
        }
        private static void CheckQuotesIndices(ParsingScript script,
                            char ch, ref bool inQuotes, ref int arrayIndexDepth)
        {
            switch (ch)
            {
                case Constants.QUOTE:
                    {
                        char prev = script.TryPrev(2);
                        char prevprev = script.TryPrev(3);
                        inQuotes = (prev != '\\' || prevprev == '\\') ? !inQuotes : inQuotes;
                        return;
                    }
                case Constants.START_ARRAY:
                    {
                        if (!inQuotes)
                        {
                            arrayIndexDepth++;
                        }
                        return;
                    }
                case Constants.END_ARRAY:
                    {
                        if (!inQuotes)
                        {
                            arrayIndexDepth--;
                        }
                        return;
                    }
            }
        }

        private static bool SkipOrAppendIfNecessary(StringBuilder item, char ch, char[] to)
        {
            if (to.Length == 1 && to[0] == Constants.END_ARRAY && item.Length > 0)
            {
                if (ch == Constants.END_ARRAY && item[item.Length - 1] != Constants.END_ARRAY)
                {
                    item.Append(ch);
                }
                else if (item.Length == 1 && item[0] == Constants.END_ARRAY)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool StillCollecting(string item, char[] to, ParsingScript script,
                                    ref string action)
        {
            char prev = script.TryPrev(2);
            char ch = script.TryPrev();
            char next = script.TryCurrent();

            if (to.Contains(ch) || ch == Constants.START_ARG ||
                                   ch == Constants.START_GROUP || ch == '?' ||
                                 next == Constants.EMPTY)
            {
                return false;
            }

            //次の項の丸括弧が来た時
            if (item.Length != 0 && ch == Constants.END_ARG)
            {
                return false;
            }
            //角かっこまたは波かっこ
            if (item.Length == 0 && (ch == Constants.END_ARRAY || ch == Constants.END_ARG))
            {
                return true;
            }
            // プラスまたはマイナスはトークン区切りの直後またはプラスマイナスの直後のときのみトークンとしてあつかう
            if (item.Length < 2 && (ch == '-' || ch == '+') && (prev == '+' || prev == '-' || Constants.TOKEN_SEPARATION.Contains(prev)))
            {
                return true;
            }

            // eを用いた数値記法の場合
            if (char.ToUpperInvariant(prev) == 'E' &&
               (ch == '-' || ch == '+' || char.IsDigit(ch)) &&
               item.Length > 1 && char.IsDigit(item[item.Length - 2]))
            {
                return true;
            }


            //それ以外の場合完了
            if ((action = Utils.ValidAction(script.FromPrev())) != null ||
                (item.Length > 0 && ch == Constants.SPACE))
            {
                return false;
            }

            //TODO:3項条件演算の実装
            /*
            if (ch == Constants.TERNARY_OPERATOR)
            {
                //script.Backward();
                //今のところ?が途中に入るトークンはない（許可されない）
                return false;
            }*/
            return true;
        }

        private static bool UpdateIfTernary(ParsingScript script, string token, char ch, List<Variable> listInput, Action<List<Variable>> listToMerge)
        {
            //TODO:3項条件演算の実装
            /*
            if (listInput.Count < 1 || ch != Constants.TERNARY_OPERATOR || token.Length > 0)
            {
                return false;
            }

            Variable result;
            Variable arg1 = MergeList(listInput, script);
            //script.MoveForwardIf(Constants.TERNARY_OPERATOR);
            bool condition = arg1.AsBool();
            if (condition)
            {
                result = script.Execute(Constants.TERNARY_SEPARATOR);
                script.MoveForwardIf(Constants.TERNARY_SEPARATOR);
                Utils.SkipRestExpr(script, Constants.END_STATEMENT);
            }
            else
            {
                Utils.SkipRestExpr(script, Constants.TERNARY_SEPARATOR[0]);
                script.MoveForwardIf(Constants.TERNARY_SEPARATOR);
                result = script.Execute(Constants.NEXT_OR_END_ARRAY);
            }

            listInput.Clear();
            listInput.Add(result);
            listToMerge(listInput);
            */
            return false;
        }

        private static bool UpdateIfBool(ParsingScript script, Variable current, Action<Variable> updateCurrent, List<Variable> listInput, Action<List<Variable>> listToMerge)
        {
            // 演算のショートカット:これ以上演算する必要がないかどうかを判定
            bool needToAdd = true;
            if ((current.Action == "&&" || current.Action == "||") &&
                    listInput.Count > 0)
            {
                if (CanMergeCells(listInput.Last(), current))
                {
                    listInput.Add(current);
                    current = MergeList(listInput, script);
                    updateCurrent(current);
                    listInput.Clear();
                    needToAdd = false;
                }
            }

            //[&&]'かつ'演算なのに左辺がすでにFalseであったり[||]'または'演算なのに左辺がすでにTrueのとき、これ以上演算の必要がない。
            //これは[&]演算子および[|]演算子には適用されません
            if ((current.Action == "&&" && !current.Bool) ||
                (current.Action == "||" && current.Bool))
            {
                Utils.SkipRestExpr(script);
                current.Action = Constants.NULL_ACTION;
                needToAdd = true;
                updateCurrent(current);
            }
            listToMerge(listInput);
            return needToAdd;
        }

        private static string UpdateAction(ParsingScript script, char[] to)
        {
            // かっこが閉じられるまで検索
            if (!script.StillValid() || script.Current == Constants.END_ARG ||
                to.Contains(script.Current))
            {
                return Constants.NULL_ACTION;
            }

            string action = Utils.ValidAction(script.Rest);

            // ポインタを合わせる
            int advance = action == null ? 0 : action.Length;
            script.Forward(advance);
            return action ?? Constants.NULL_ACTION;
        }

        private static Variable MergeList(List<Variable> listToMerge, ParsingScript script)
        {
            if (listToMerge.Count == 0)
            {
                return Variable.EmptyInstance;
            }
            // セルが1つだけなら早期リターン
            if (listToMerge.Count == 1)
            {
                return listToMerge[0];
            }

            Variable baseCell = listToMerge[0];
            int index = 1;

            // 変数同士の演算
            Variable result = Merge(baseCell, ref index, listToMerge, script);
            return result;
        }


        private static Variable Merge(Variable current, ref int index, List<Variable> listToMerge,
                                      ParsingScript script, bool mergeOneOnly = false)
        {

            while (index < listToMerge.Count)
            {
                Variable next = listToMerge[index++];

                while (!CanMergeCells(current, next))
                {
                    // 優先順位が前後する場合、先に高いほうを演算する
                    Merge(next, ref index, listToMerge, script, true /* mergeOneOnly */);
                }

                current = MergeCells(current, next, script);
                if (mergeOneOnly)
                {
                    break;
                }
            }

            return current;
        }

        private static Variable MergeCells(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            if (leftCell.IsReturn ||
     leftCell.Type == Variable.VarType.BREAK ||
     leftCell.Type == Variable.VarType.CONTINUE)
            {
                return Variable.EmptyInstance;
            }

            if (leftCell.Action == Constants.IS && rightCell.Object != null && rightCell.Object is TypeObject to)
            {
                leftCell = new Variable(to.Match(leftCell));
            }
            else if (leftCell.Action == Constants.IS_NOT && rightCell.Object != null && rightCell.Object is TypeObject t)
            {
                leftCell = new Variable(!t.Match(leftCell));
            }
            else if (leftCell.Action == Constants.AS && rightCell.Object is TypeObject type)
            {
                leftCell = leftCell.Convert(type.Type);
                leftCell.Nullable = true;
            }
            else if (leftCell.Action == Constants.NULL_OP)
            {
                if (leftCell.IsNull())
                {
                    leftCell = rightCell;
                }
            }
            else
            {
                leftCell = leftCell.Action == Constants.EQUAL || leftCell.Action == "==="
                    ? new Variable(leftCell.Equals(rightCell))
                    : leftCell.Action == Constants.NOT_EQUAL || leftCell.Action == "!=="
                                    ? new Variable(!leftCell.Equals(rightCell))
                                    : leftCell.Type == Variable.VarType.NUMBER && rightCell.Type == Variable.VarType.NUMBER
                                                    ? MergeNumbers(leftCell, rightCell, script)
                                                    : leftCell.Type == Variable.VarType.BOOLEAN && rightCell.Type == Variable.VarType.BOOLEAN
                                                    ? MergeBooleans(leftCell, rightCell, script)
                                                    : leftCell.Type == Variable.VarType.STRING || rightCell.Type == Variable.VarType.STRING
                                                                                    ? MergeStrings(leftCell, rightCell, script)
                                                                                    : leftCell.Type == Variable.VarType.ARRAY
                                                                                                    ? MergeArray(leftCell, rightCell, script)
                                                                                                    : leftCell.Type == Variable.VarType.DELEGATE && rightCell.Type == Variable.VarType.DELEGATE
                                                                                                                    ? MergeDelegate(leftCell, rightCell, script)
                                                                                                                    : leftCell.Type == Variable.VarType.OBJECT && leftCell.Object is ObjectBase obj && obj.HandleOperator
                                                                                                                                    ? obj.Operator(leftCell, rightCell, leftCell.Action, script)
                                                                                                                                    : MergeObjects(leftCell, rightCell, script);
            }

            leftCell.Action = rightCell.Action;
            return leftCell;
        }
        //Bool同士の演算
        private static Variable MergeBooleans(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            if (rightCell.Type != Variable.VarType.BOOLEAN)
            {
                rightCell = new Variable(rightCell.AsBool());
            }
            switch (leftCell.Action)
            {
                case "&&":
                    return new Variable(
                        leftCell.Bool && rightCell.Bool);
                case "||":
                    return new Variable(
                         leftCell.Bool || rightCell.Bool);
                case "^":
                    return new Variable(
                        leftCell.Bool ^ rightCell.Bool);
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND,
                         script, leftCell.Action);
                    return leftCell;
            }
        }
        //数値同士の演算
        private static Variable MergeNumbers(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            if (rightCell.Type != Variable.VarType.NUMBER)
            {
                rightCell.Value = rightCell.AsDouble();
            }
            switch (leftCell.Action)
            {
                case "%":
                    return new Variable(leftCell.Value % rightCell.Value);
                case "*":
                    return new Variable(leftCell.Value * rightCell.Value);
                case "/":
                    return new Variable(leftCell.Value / rightCell.Value);
                case "+":
                    return rightCell.Type != Variable.VarType.NUMBER
                        ? new Variable(leftCell.AsString() + rightCell.String)
                        : new Variable(leftCell.Value + rightCell.Value);
                case "-":
                    return new Variable(leftCell.Value - rightCell.Value);
                case "<":
                    return new Variable(leftCell.Value < rightCell.Value);
                case ">":
                    return new Variable(leftCell.Value > rightCell.Value);
                case "<=":
                    return new Variable(leftCell.Value <= rightCell.Value);
                case ">=":
                    return new Variable(leftCell.Value >= rightCell.Value);
                case "&":
                    return new Variable((int)leftCell.Value & (int)rightCell.Value);
                case "^":
                    return new Variable((int)leftCell.Value ^ (int)rightCell.Value);
                case "|":
                    return new Variable((int)leftCell.Value | (int)rightCell.Value);
                case "**":
                    return new Variable(Math.Pow(leftCell.Value, rightCell.Value));
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND,
                         script, leftCell.Action);
                    return leftCell;
            }
        }
        //文字列演算
        private static Variable MergeStrings(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            switch (leftCell.Action)
            {
                case "+":
                    return new Variable(leftCell.AsString() + rightCell.AsString());
                case "<":
                    string arg1 = leftCell.AsString();
                    string arg2 = rightCell.AsString();
                    return new Variable(string.Compare(arg1, arg2, StringComparison.Ordinal) < 0);
                case ">":
                    return new Variable(
                     string.Compare(leftCell.AsString(), rightCell.AsString(), StringComparison.Ordinal) > 0);
                case "<=":
                    return new Variable(
                      string.Compare(leftCell.AsString(), rightCell.AsString(), StringComparison.Ordinal) <= 0);
                case ">=":
                    return new Variable(
                      string.Compare(leftCell.AsString(), rightCell.AsString(), StringComparison.Ordinal) >= 0);
                case ":":
                    leftCell.SetHashVariable(leftCell.AsString(), rightCell);
                    break;
                case null:
                case "\0":
                case ")":
                    break;
                default:
                    Utils.ThrowErrorMsg("String型演算で次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND
                         , script, leftCell.Action);
                    break;
            }
            return leftCell;
        }
        //配列演算
        private static Variable MergeArray(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            switch (leftCell.Action)
            {
                case "+=":
                    {
                        if (rightCell.Type == Variable.VarType.ARRAY)
                        {
                            leftCell.Tuple.AddRange(rightCell.Tuple);
                        }
                        else
                        {
                            leftCell.Tuple.Add(rightCell);
                        }
                        return leftCell;
                    }
                case "+":
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);
                        if (rightCell.Type == Variable.VarType.ARRAY)
                        {
                            v.Tuple.AddRange(leftCell.Tuple);
                            v.Tuple.AddRange(rightCell.Tuple);
                        }
                        else
                        {
                            v.Tuple.AddRange(leftCell.Tuple);
                            v.Tuple.Add(rightCell);
                        }
                        return v;
                    }
                case "-=":
                    {
                        if (leftCell.Tuple.Remove(rightCell))
                        {
                            return leftCell;
                        }
                        else
                        {
                            Utils.ThrowErrorMsg("配列に対象の変数が見つかりませんでした", Exceptions.COULDNT_FIND_ITEM,
                         script, leftCell.Action);
                            return leftCell;
                        }
                    }
                case "-":
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);

                        v.Tuple.AddRange(leftCell.Tuple);
                        v.Tuple.Remove(rightCell);

                        return v;
                    }
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND,
                         script, leftCell.Action);
                    return leftCell;
            }

        }
        //デリゲート演算
        private static Variable MergeDelegate(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            switch (leftCell.Action)
            {
                case "+=":
                    {
                        leftCell.Delegate.Add(rightCell.Delegate);
                        return leftCell;
                    }
                case "+":
                    {
                        Variable v = new Variable(Variable.VarType.DELEGATE);
                        v.Delegate = new DelegateObject(leftCell.Delegate);
                        v.Delegate.Add(rightCell.Delegate);
                        return v;
                    }
                case "-=":
                    {
                        if (leftCell.Delegate.Remove(rightCell.Delegate))
                        {
                            return leftCell;
                        }
                        else
                        {
                            Utils.ThrowErrorMsg("デリゲートにに対象の変数が見つかりませんでした", Exceptions.COULDNT_FIND_ITEM,
                         script, leftCell.Action);
                            return leftCell;
                        }
                    }
                case "-":
                    {
                        Variable v = new Variable(Variable.VarType.DELEGATE);
                        v.Delegate = new DelegateObject(leftCell.Delegate);
                        v.Delegate.Remove(rightCell.Delegate);

                        return v;
                    }
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND,
                         script, leftCell.Action);
                    return leftCell;
            }

        }
        //オブジェクトか、それ以外の演算
        private static Variable MergeObjects(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            switch (leftCell.Action)
            {
                case ">":
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + leftCell.Action + "]", Exceptions.INVALID_OPERAND,
                         script, leftCell.Action);
                    return leftCell;
            }
        }

        private static bool CanMergeCells(Variable leftCell, Variable rightCell)
        {
            return GetPriority(leftCell.Action) >= GetPriority(rightCell.Action);
        }

        private static int GetPriority(string action)
        {
            switch (action)
            {
                case "**":
                case "++":
                case "--": return 11;
                case "%":
                case "*":
                case "/": return 10;
                case "+":
                case "-": return 9;
                case "<":
                case ">":
                case ">=":
                case "<=": return 8;
                case "==":
                case "!=": return 7;
                case "&": return 6;
                case "|": return 5;
                case "^": return 4;
                case "&&": return 2;
                case "||": return 2;
                case "+=":
                case "-=":
                case "*=":
                case "/=":
                case "%=":
                case "=": return 1;
            }
            return 0; // NULL action has priority 0.
        }
    }
}
