using AliceScript.Functions;
using AliceScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        internal static HashSet<FunctionBase> m_attributeFuncs = null;
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

                Stack<PreOperetors> negSign = CheckConsistencyAndSign(script, listToMerge, action, ref token);//前置演算子を取得

                // このトークンに対応する関数を取得する
                ParserFunction func = new ParserFunction(script, token, ch, ref action, keywords);
                if (func.m_impl is FunctionBase fb && (script.ProcessingFunction is null || (fb is not LiteralFunction && fb is not ValueFunction)))
                {
                    script.ProcessingFunction = fb;//現在処理中としてマーク
                    if (fb.Name.StartsWith(Constants.ANNOTATION_FUNCTION_REFIX))
                    {
                        m_attributeFuncs ??= new HashSet<FunctionBase>();
                        m_attributeFuncs.Add(fb);
                    }
                    else if (m_attributeFuncs is not null && m_attributeFuncs.Count > 0)
                    {
                        fb.AttributeFunctions = m_attributeFuncs;
                        m_attributeFuncs = null;
                    }
                }
                Variable current;
                if(NeedReferenceNext)
                {
                    // 参照が必要な場合
                    current = new Variable(func.m_impl);
                    NeedReferenceNext = false;
                }
                else
                {
                    current = func.GetValue(script);
                }
                if (UpdateResult(script, to, listToMerge, token, negSign, ref current, ref negated, ref action))
                {
                    return listToMerge;
                }
            } while (script.StillValid() &&
                    (inQuotes || arrayIndexDepth > 0 || !to.Contains(script.Current)));

            script.MoveForwardIf(Constants.END_ARG);

            return listToMerge;
        }

        public static bool NeedReferenceNext {get;set;}
        public static string ExtractNextToken(ParsingScript script, char[] to, ref bool inQuotes,
            ref int arrayIndexDepth, ref int negated, out char ch, out string action, bool throwExc = true)
        {
            StringBuilder item = new StringBuilder();
            ch = Constants.EMPTY;
            action = null;
            do
            {
                string negateSymbol = Utils.IsNotSign(script.Rest);
                if (negateSymbol is not null && !inQuotes)
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

        private static bool UpdateResult(ParsingScript script, char[] to, List<Variable> listToMerge, string token, Stack<PreOperetors> preops,
                                 ref Variable current, ref int negated, ref string action)
        {
            if (current is null)
            {
                current = Variable.EmptyInstance;
            }
            current.ParsingToken = token;

            if (current.Type == Variable.VarType.NUMBER && current.m_value.HasValue)
            {
                while (preops.Count > 0)
                {
                    PreOperetors op = preops.Pop();
                    current = ProcessUnaryPreOperation(current, op);
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

            if (action is null)
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
                        ((action == Constants.NULL_ACTION) && current is not null && current.Type != Variable.VarType.BOOLEAN) ||
                         (current is not null && current.IsReturn));
            if (done)
            {
                // 数値結果がない場合は、2項演算ではない
                current.Action = action;
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

        private static Stack<PreOperetors> CheckConsistencyAndSign(ParsingScript script, List<Variable> listToMerge, string action, ref string token)
        {
            if (Constants.CONTROL_FLOW.Contains(token) && listToMerge.Count > 0)
            {
                listToMerge.Clear();
            }
            var result = new Stack<PreOperetors>();

            script.MoveForwardIf(Constants.SPACE);

            if (action is not null && action.Length > 1)
            {
                script.Forward(action.Length - 1);
            }

            while (true)
            {
                if (MatchPreOperator(token, Constants.INCREMENT))
                {
                    token = token.Substring(2);
                    result.Push(PreOperetors.Increment);
                }
                else if (MatchPreOperator(token, Constants.DECREMENT))
                {
                    token = token.Substring(2);
                    result.Push(PreOperetors.Decrement);
                }
                else if (MatchPreOperator(token, Constants.RANGE))
                {
                    token = token.Substring(2);
                    result.Push(PreOperetors.Range);
                }
                else if (MatchPreOperator(token, Constants.PLUS))
                {
                    token = token.Substring(1);
                    //単項プラス演算子は何もする必要がない
                }
                else if (MatchPreOperator(token, Constants.MINUS))
                {
                    token = token.Substring(1);
                    result.Push(PreOperetors.Minus);
                }
                else if (MatchPreOperator(token, Constants.BITWISE_NOT))
                {
                    token = token.Substring(1);
                    result.Push(PreOperetors.BitwiseNot);
                }
                else
                {
                    return result;
                }
            }
        }
        private static bool MatchPreOperator(string token, string op)
        {
            return token.Length > op.Length && token.StartsWith(op, StringComparison.Ordinal) && token[op.Length] != Constants.QUOTE && token[op.Length] != Constants.QUOTE1;
        }
        private static bool MatchPreOperator(string token, char op)
        {
            return token.Length > 1 && token.StartsWith(op) && token[1] != Constants.QUOTE && token[1] != Constants.QUOTE1;
        }

        /// <summary>
        /// 前置演算子の種類
        /// </summary>
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
            Minus,
            /// <summary>
            /// 前置Range
            /// </summary>
            Range,
            /// <summary>
            /// ビット補数
            /// </summary>
            BitwiseNot,
            None
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

            // 単項前置演算子向けの対応(前置演算子はその前の文字がトークン区切りである)
            if (Constants.TOKEN_SEPARATION_ANDEND_STR.Contains(prev))
            {
                // 2文字分の単項演算子なら、トークン区切り、PRE_DOUBLE_SIZE_ACTIONSになっている
                if (Constants.PRE_DOUBLE_SIZE_ACTIONS.Contains($"{ch}{next}"))
                {
                    // 前置演算子の場合
                    return (action = Utils.ValidAction(script.FromPrev())) is not null;
                }
                // 1文字分の単項前置演算子なら、トークン区切り、PRE_SINGLE_SIZE_ACTIONS、文字の形になっている
                if (Constants.PRE_SINGLE_SIZE_ACTIONS.Contains(ch) && !Constants.TOKEN_SEPARATION_ANDEND_STR.Contains(next))
                {
                    return true;
                }
            }

            // eを用いた数値記法の場合
            if (char.ToUpperInvariant(prev) == 'E' &&
               (ch == '-' || ch == '+' || char.IsDigit(ch)) &&
               item.Length > 1 && char.IsDigit(item[item.Length - 2]))
            {
                return true;
            }

            //それ以外の場合完了
            if ((action = Utils.ValidAction(script.FromPrev())) is not null ||
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

            // 短絡論理演算で、評価をスキップできる時にトークンをスキップする
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
            int advance = action is null ? 0 : action.Length;
            script.Forward(advance);
            return action ?? Constants.NULL_ACTION;
        }

        private static Variable MergeList(List<Variable> listToMerge, ParsingScript script)
        {
            if (listToMerge.Count == 0)
            {
                return Variable.EmptyInstance;
            }
            // セルが1つだけなら単項演算を処理してReturn
            if (listToMerge.Count == 1)
            {
                listToMerge[0] = ProcessUnaryPostOperation(listToMerge[0], listToMerge[0].Action);
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
                    next = Merge(next, ref index, listToMerge, script, true /* mergeOneOnly */);
                }

                current = ProcessBinaryOperation(current, next, script);
                if (mergeOneOnly)
                {
                    break;
                }
            }

            return current;
        }
        /// <summary>
        /// 単項前置演算子を処理します
        /// </summary>
        /// <param name="current">演算対象の値</param>
        /// <param name="action">前置演算子</param>
        /// <returns>演算結果の値</returns>
        /// <exception cref="ScriptException">不明な演算子の場合にスローされる例外</exception>
        private static Variable ProcessUnaryPreOperation(Variable current, PreOperetors action)
        {
            switch (action)
            {
                case PreOperetors.Increment:
                    current.Value++;
                    return current;
                case PreOperetors.Decrement:
                    current.Value--;
                    return current;
                case PreOperetors.Minus:
                    return new Variable(current.Value * -1);
                case PreOperetors.BitwiseNot:
                    return new Variable(~(long)current.Value);
                case PreOperetors.Range:
                    return new Variable(new RangeStruct((int)current.Value));
                default:
                    throw new ScriptException($"演算子`{action}`は`{current.GetTypeString()}`型のオペランドに適用できません。", Exceptions.INVALID_OPERAND);
            }
        }
        /// <summary>
        /// 単項後置演算子を処理します。
        /// 単項後置演算子を追加したいときは、Constants.POST_UNARY_OPERATORSに追加してください。
        /// </summary>
        /// <param name="current">演算対象の値</param>
        /// <param name="action">後置演算子</param>
        /// <returns>演算結果の値</returns>
        /// <exception cref="ScriptException">不明な演算子の場合にスローされる例外</exception>
        private static Variable ProcessUnaryPostOperation(Variable current, string action)
        {
            // 演算子は処理できたとしておく
            current.Action = string.Empty;
            if(action == ")" || action == "\0" || string.IsNullOrEmpty(action))
            {
                return current;
            }
            if(current.Type == Variable.VarType.NUMBER && current.m_value.HasValue)
            {
                switch (action)
                {
                    case Constants.RANGE:
                        return new Variable(new RangeStruct((int)current.Value));
                    case Constants.INCREMENT:
                        current.Value++;
                        return current;
                    case Constants.DECREMENT:
                        current.Value--;
                        return current;
                    default:
                        break;
                }
            }
            throw new ScriptException($"演算子`{action}`は`{current.GetTypeString()}`型のオペランドに適用できません。", Exceptions.INVALID_OPERAND);
        }
        /// <summary>
        /// 2項演算子を処理します
        /// </summary>
        /// <param name="leftCell">演算対象の左辺の値</param>
        /// <param name="rightCell">演算対象の右辺の値</param>
        /// <param name="script">処理中のスクリプト</param>
        /// <returns>演算結果の値</returns>
        private static Variable ProcessBinaryOperation(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            if (leftCell.IsReturn ||
     leftCell.Type == Variable.VarType.BREAK ||
     leftCell.Type == Variable.VarType.CONTINUE)
            {
                return Variable.EmptyInstance;
            }

            if(leftCell.Action == ":")
            {
                return new Variable(new KeyValuePair<Variable, Variable>(leftCell, rightCell));
            }
            if (leftCell.Action == Constants.IS && rightCell.Object is not null && rightCell.Object is TypeObject to)
            {
                leftCell = new Variable(to.Match(leftCell));
            }
            else if (leftCell.Action == Constants.IS_NOT && rightCell.Object is not null && rightCell.Object is TypeObject t)
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
                if (leftCell.Action == Constants.EQUAL || leftCell.Action == "===")
                {
                    leftCell = new Variable(leftCell.Equals(rightCell));
                }
                else
                if (leftCell.Action == Constants.NOT_EQUAL || leftCell.Action == "!==")
                {
                    leftCell = new Variable(!leftCell.Equals(rightCell));
                }
                else
                if (leftCell.Type == Variable.VarType.NUMBER && rightCell.Type == Variable.VarType.NUMBER)
                {
                    leftCell = MergeNumbers(leftCell, rightCell, script);
                }
                else
                if (leftCell.Type == Variable.VarType.BOOLEAN && rightCell.Type == Variable.VarType.BOOLEAN)
                {
                    leftCell = MergeBooleans(leftCell, rightCell, script);
                }
                else
                if (leftCell.Type == Variable.VarType.STRING || rightCell.Type == Variable.VarType.STRING)
                {
                    leftCell = MergeStrings(leftCell, rightCell, script);
                }
                else
                if (leftCell.Type == Variable.VarType.ARRAY)
                {
                    leftCell = MergeArray(leftCell, rightCell, script);
                }
                else
                if (leftCell.Type == Variable.VarType.DELEGATE && rightCell.Type == Variable.VarType.DELEGATE)
                {
                    leftCell = MergeDelegate(leftCell, rightCell, script);
                }
                else
                if (leftCell.Type == Variable.VarType.OBJECT && leftCell.Object is ObjectBase obj && obj.HandleOperator)
                {
                    leftCell = obj.Operator(leftCell, rightCell, leftCell.Action, script);
                }
                else
                {
                    leftCell = MergeObjects(leftCell, rightCell, script);
                }

            }
            leftCell.Action = rightCell.Action;
            return leftCell;
        }
        //Bool同士の演算
        private static Variable MergeBooleans(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            switch (leftCell.Action)
            {
                case "&":
                    return new Variable(leftCell.Bool & rightCell.Bool);
                case "&&":
                    return new Variable(leftCell.Bool && rightCell.Bool);
                case "|":
                    return new Variable(leftCell.Bool | rightCell.Bool);
                case "||":
                    return new Variable(leftCell.Bool || rightCell.Bool);
                case "^":
                    return new Variable(leftCell.Bool ^ rightCell.Bool);
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
            }
        }
        //数値同士の演算
        private static Variable MergeNumbers(Variable leftCell, Variable rightCell, ParsingScript script)
        {
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
                case Constants.LEFT_SHIFT:
                    return new Variable(leftCell.As<long>() << rightCell.As<int>());
                case Constants.RIGHT_SHIFT:
                    return new Variable(leftCell.As<long>() >> rightCell.As<int>());
                case "&":
                    return new Variable((int)leftCell.Value & (int)rightCell.Value);
                case "^":
                    return new Variable((int)leftCell.Value ^ (int)rightCell.Value);
                case "|":
                    return new Variable((int)leftCell.Value | (int)rightCell.Value);
                case "**":
                    return new Variable(Math.Pow(leftCell.Value, rightCell.Value));
                case Constants.RANGE:
                    return new Variable(new RangeStruct((int)leftCell.Value, (int)rightCell.Value));
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
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
                case "*":
                    uint repeat = rightCell.As<uint>();
                    if (repeat == 0)
                    {
                        return new Variable(Variable.VarType.STRING);
                    }
                    string text = leftCell.AsString();
#if NET6_0_OR_GREATER
                    DefaultInterpolatedStringHandler sh = new DefaultInterpolatedStringHandler(text.Length * (int)repeat, 0);
                    for (int i = 0; i < repeat; i++)
                    {
                        sh.AppendLiteral(text);
                    }
                    return Variable.FromText(sh.ToStringAndClear());
#else
                    StringBuilder sb = new StringBuilder(text.Length * (int)repeat);
                    for(int i = 0;i < repeat; i++)
                    {
                        sb.Append(text);
                    }
                    return Variable.FromText(sb.ToString());
#endif
                case null:
                case "\0":
                case ")":
                    break;
                default:
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
            }
            return leftCell;
        }
        //配列演算
        private static Variable MergeArray(Variable leftCell, Variable rightCell, ParsingScript script)
        {
            // 左辺が配列なのは確定しているので、右辺が配列かどうかを確認
            if (rightCell.Type != Variable.VarType.ARRAY)
            {
                if (leftCell.Action == "*")
                {
                    uint repeat = rightCell.As<uint>();
                    Variable v = new Variable(Variable.VarType.ARRAY);
                    for (int i = 0; i < repeat; i++)
                    {
                        v.Tuple.AddRange(leftCell.Tuple);
                    }
                    return v;
                }
                throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
            }
            switch (leftCell.Action)
            {
                case "+":
                    {
                        return new Variable(leftCell.Tuple.Concat(rightCell.Tuple));
                    }
                case "-":
                    {
                        return new Variable(leftCell.Tuple.Except(rightCell.Tuple));
                    }
                case "|":
                    {
                        return new Variable(leftCell.Tuple.Union(rightCell.Tuple));
                    }
                case "&":
                    {
                        return new Variable(leftCell.Tuple.Intersect(rightCell.Tuple));
                    }
                case "^":
                    {
                        var intersect = leftCell.Tuple.Intersect(rightCell.Tuple);
                        var union = leftCell.Tuple.Union(rightCell.Tuple);
                        return new Variable(union.Except(intersect));
                    }
                case null:
                case "\0":
                case ")":
                    return leftCell;
                default:
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
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
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
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
                    throw new ScriptException($"演算子`{leftCell.Action}`を`{leftCell.GetTypeString()}`と`{rightCell.GetTypeString()}`型のオペランド間に適用できません。", Exceptions.INVALID_OPERAND);
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
                case "++":
                case "--": return 14;
                case "**": return 13;
                case "%":
                case "*":
                case "/": return 12;
                case "+":
                case "-": return 11;
                case Constants.LEFT_SHIFT:
                case Constants.RIGHT_SHIFT: return 10;
                case "<":
                case ">":
                case ">=":
                case "<=": return 9;
                case "==":
                case "!=": return 8;
                case "&": return 7;
                case "^": return 6;
                case "|": return 5;
                case "&&": return 4;
                case "||": return 3;
                case "??": return 2;
                case "=": return 1;
                default: return 0;// NULL action has priority 0.
            }
        }
    }
}
