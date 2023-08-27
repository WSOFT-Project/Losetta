using System.Globalization;
using System.Text;
using System.Text.Json;

namespace AliceScript
{
    public class Parser
    {
        public static Variable AliceScript(ParsingScript script)
        {
            return AliceScript(script, Constants.END_PARSE_ARRAY);
        }
        public static async Task<Variable> AliceScriptAsync(ParsingScript script)
        {
            return await AliceScriptAsync(script, Constants.END_PARSE_ARRAY);
        }

        public static Variable AliceScript(ParsingScript script, char[] to)
        {
            // First step: process passed expression by splitting it into a list of cells.
            List<Variable> listToMerge = Split(script, to);

            if (listToMerge.Count == 0)
            {
                throw new ScriptException(script.Rest + "を解析できません", Exceptions.COULDNT_PARSE, script);
            }

            // Second step: merge list of cells to get the result of an expression.
            Variable result = MergeList(listToMerge, script);
            return result;
        }

        public static async Task<Variable> AliceScriptAsync(ParsingScript script, char[] to)
        {
            // First step: process passed expression by splitting it into a list of cells.
            List<Variable> listToMerge = await SplitAsync(script, to);

            if (listToMerge.Count == 0)
            {
                throw new ScriptException(script.Rest + "を解析できません", Exceptions.COULDNT_PARSE, script);
            }

            // Second step: merge list of cells to get the result of an expression.
            Variable result = MergeList(listToMerge, script);
            return result;
        }

        private static List<Variable> Split(ParsingScript script, char[] to)
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
                if (func.m_impl is FunctionBase fb && (script.ProcessingFunction == null || !(fb is StringOrNumberFunction)))
                {
                    script.ProcessingFunction = fb;
                }
                Variable current = func.GetValue(script);
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
                if (func.m_impl is FunctionBase fb && (script.ProcessingFunction == null || !(fb is StringOrNumberFunction)))
                {
                    script.ProcessingFunction = fb;
                }
                Variable current =await func.GetValueAsync(script);
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
                    // The char still belongs to the previous operand.
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

            switch (preop)
            {
                case PreOperetors.Minus:
                    {
                        // -マークがついている場合は数値がマイナス
                        current = new Variable(-1 * current.Value);
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
                        ((action == Constants.NULL_ACTION) && (current != null && current.Type != Variable.VarType.BOOLEAN)) ||
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

            if (token.Length > 2 && token.StartsWith(Constants.INCREMENT,StringComparison.Ordinal) && token[2] != Constants.QUOTE)
            {
                token = token.Substring(2);
                return PreOperetors.Increment;
            }else if (token.Length > 2 && token.StartsWith(Constants.DECREMENT, StringComparison.Ordinal) && token[2] != Constants.QUOTE)
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
            else if (token.Length > 1 && token[0]=='-' && token[1] != Constants.QUOTE)
            {
                token = token.Substring(1);
                return PreOperetors.Minus;
            }
            return PreOperetors.None;
        }

        private enum PreOperetors
        {
            Increment,Decrement,Minus,None
        }
        private static void CheckQuotesIndices(ParsingScript script,
                            char ch, ref bool inQuotes, ref int arrayIndexDepth)
        {
            switch (ch)
            {
                case Constants.QUOTE:
                    {
                        char prev = script.TryPrev(2);
                        char prevprev = script.TryPrev(2);
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
                                   ch == Constants.START_GROUP ||
                                 next == Constants.EMPTY)
            {
                return false;
            }

            //角かっこまたは波かっこまたはポインタ
            if(item.Length==0 && (ch == Constants.END_ARRAY || ch == Constants.END_ARG) || ch == '&')
            {
                return true;
            }
            // プラスまたはマイナスはトークン区切りの直後またはプラスマイナスの直後のときのみトークンとしてあつかう
            if (item.Length < 2 && (ch == '-' || ch=='+') && (prev == '+' || prev == '-' || Constants.TOKEN_SEPARATION.Contains(prev)) )
            {
                return true;
            }

            // eを用いた数値記法の場合
            if (Char.ToUpper(prev) == 'E' &&
               (ch == '-' || ch == '+' || Char.IsDigit(ch)) &&
               item.Length > 1 && Char.IsDigit(item[item.Length - 2]))
            {
                return true;
            }

            //それ以外の場合完了
            if ((action = Utils.ValidAction(script.FromPrev())) != null ||
                (item.Length > 0 && ch == Constants.SPACE))
            {
                return false;
            }

            if (ch == Constants.TERNARY_OPERATOR)
            {
                script.Backward();
                return false;
            }
            return true;
        }

        private static bool UpdateIfTernary(ParsingScript script, string token, char ch, List<Variable> listInput, Action<List<Variable>> listToMerge)
        {
            if (listInput.Count < 1 || ch != Constants.TERNARY_OPERATOR || token.Length > 0)
            {
                return false;
            }

            Variable result;
            Variable arg1 = MergeList(listInput, script);
            script.MoveForwardIf(Constants.TERNARY_OPERATOR);
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

            return true;
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
            // We search a valid action till we get to the End of Argument ')'
            // or pass the end of string.
            if (!script.StillValid() || script.Current == Constants.END_ARG ||
                to.Contains(script.Current))
            {
                return Constants.NULL_ACTION;
            }

            string action = Utils.ValidAction(script.Rest);

            // We need to advance forward not only the action length but also all
            // the characters we skipped before getting the action.
            int advance = action == null ? 0 : action.Length;
            script.Forward(advance);
            return action == null ? Constants.NULL_ACTION : action;
        }

        private static Variable MergeList(List<Variable> listToMerge, ParsingScript script)
        {
            if (listToMerge.Count == 0)
            {
                return Variable.EmptyInstance;
            }
            // If there is just one resulting cell there is no need
            // to perform the second step to merge tokens.
            if (listToMerge.Count == 1)
            {
                return listToMerge[0];
            }

            Variable baseCell = listToMerge[0];
            int index = 1;

            // Second step: merge list of cells to get the result of an expression.
            Variable result = Merge(baseCell, ref index, listToMerge, script);
            return result;
        }

        // From outside this function is called with mergeOneOnly = false.
        // It also calls itself recursively with mergeOneOnly = true, meaning
        // that it will return after only one merge.
        private static Variable Merge(Variable current, ref int index, List<Variable> listToMerge,
                                      ParsingScript script, bool mergeOneOnly = false)
        {

            while (index < listToMerge.Count)
            {
                Variable next = listToMerge[index++];

                while (!CanMergeCells(current, next))
                {
                    // If we cannot merge cells yet, go to the next cell and merge
                    // next cells first. E.g. if we have 1+2*3, we first merge next
                    // cells, i.e. 2*3, getting 6, and then we can merge 1+6.
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
                //処理は不要
                return Variable.EmptyInstance;
            }
            //[is]演算子、型テスト演算子ですべての型に適応できます
            if (leftCell.Action == Constants.IS && rightCell.Object != null && rightCell.Object is TypeObject to)
            {
                leftCell = new Variable(to.Match(leftCell));
            }
            //[is not]演算子、型テスト否定演算子ですべての型に適応できます
            if (leftCell.Action == Constants.IS_NOT && rightCell.Object != null && rightCell.Object is TypeObject t)
            {
                leftCell = new Variable(!t.Match(leftCell));
            }
            //[as]演算子、キャスト演算子で右辺がType型の時すべての型に適応できます
            else if (leftCell.Action == Constants.AS && rightCell.Object is TypeObject type)
            {
                leftCell = leftCell.Convert(type.Type);
            }
            //[??]演算子、Null合体演算子ですべての型に適応できます
            else if (leftCell.Action == "??")
            {
                if (leftCell.IsNull())
                {
                    leftCell = rightCell;
                }
            }
            // [==]または[===]つまり、等値演算子の場合はそれぞれのEqualsメソッドを呼び出す
            else if (leftCell.Action == "==" || leftCell.Action == "===")
            {
                leftCell = new Variable(leftCell.Equals(rightCell));
            }
            // [!=]または[!==]つまり、非等値演算子の場合はそれぞれのEqualsメソッドを呼び出しそれを反転する
            else if (leftCell.Action == "!=" || leftCell.Action == "!==")
            {
                leftCell = new Variable(!leftCell.Equals(rightCell));
            }
            else if (leftCell.Type == Variable.VarType.NUMBER &&
                rightCell.Type == Variable.VarType.NUMBER)
            {
                leftCell = MergeNumbers(leftCell, rightCell, script);
            }
            else if (leftCell.Type == Variable.VarType.BOOLEAN &&
                    rightCell.Type == Variable.VarType.BOOLEAN)
            {
                leftCell = MergeBooleans(leftCell, rightCell, script);
            }
            else if (leftCell.Type == Variable.VarType.STRING || rightCell.Type == Variable.VarType.STRING)
            {
                leftCell = MergeStrings(leftCell, rightCell, script);
            }
            else if (leftCell.Type == Variable.VarType.ARRAY)
            {
                leftCell = MergeArray(leftCell, rightCell, script);
            }
            else if (leftCell.Type == Variable.VarType.DELEGATE && rightCell.Type == Variable.VarType.DELEGATE)
            {
                leftCell = MergeDelegate(leftCell, rightCell, script);
            }
            else if (leftCell.Type == Variable.VarType.OBJECT && leftCell.Object is ObjectBase obj && obj.HandleOperator)
            {
                leftCell = obj.Operator(leftCell, rightCell, leftCell.Action, script);
            }
            else
            {
                leftCell = MergeObjects(leftCell, rightCell, script);
            }
            leftCell.Action = rightCell.Action;
            return leftCell;
        }
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
                    if (rightCell.Type != Variable.VarType.NUMBER)
                    {
                        return new Variable(leftCell.AsString() + rightCell.String);
                    }
                    else
                    {
                        return new Variable(leftCell.Value + rightCell.Value);
                    }
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
