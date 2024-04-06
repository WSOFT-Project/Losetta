using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AliceScript
{
    public static partial class Utils
    {
        public static Variable GetItem(ParsingScript script, bool eatLast = true)
        {
            script.MoveForwardIf(Constants.NEXT_ARG, Constants.SPACE);
            Utils.CheckNotEnd(script);

            bool inQuotes = script.Current == Constants.QUOTE;
            bool inQuotes1 = script.Current == Constants.QUOTE1;

            Variable var;

            bool isList = script.Current == Constants.START_GROUP || script.Current == Constants.START_ARRAY;
            if (isList)
            {
                var = ProcessArrayMap(script);
            }
            else
            {
                var sep = script.ProcessingList ? Constants.NEXT_OR_END_ARRAY_EXT : Constants.NEXT_OR_END_ARRAY;
                // A variable, a function, or a number.
                var = script.Execute(sep);
                //value = var.Clone();
                if (script.ProcessingFunction is not null && script.ProcessingFunction.Keywords is not null && var is not null)
                {
                    var.Keywords = script.ProcessingFunction.Keywords;
                }
                if (inQuotes)
                {
                    script.MoveForwardIf(Constants.QUOTE);
                }
                else if (inQuotes1)
                {
                    script.MoveForwardIf(Constants.QUOTE1);
                }
                if (eatLast)
                {
                    script.MoveForwardIf(Constants.END_ARG, Constants.SPACE);
                }
            }


            if (var is not null && var.Type == Variable.VarType.VOID)
            {
                throw new ScriptException("`文`を式中に含めることはできません", Exceptions.INVALID_SYNTAX);
            }
            return var;
        }

        public static Variable ProcessArrayMap(ParsingScript script)
        {
            bool isList = true;
            char start = script.Current;
            char end = script.Current == Constants.START_GROUP ?
                Constants.END_GROUP : Constants.END_ARRAY;
            script.Forward(); // Skip the first brace.
            Variable value = new Variable();
            var processingListBefore = script.ProcessingList;
            script.ProcessingList = true;// script.ProcessingList || script.Current == Constants.START_ARRAY;
            try
            {
                value.Tuple = new VariableCollection();
                value.Tuple.AddRange(GetArgs(script, start, end, (outList) => { isList = outList; }, null, true));
            }
            finally
            {
                script.ProcessingList = processingListBefore;
            }
            return value;
        }

        public static string GetToken(ParsingScript script, char[] to, bool eatLast = false)
        {
            char curr = script.TryCurrent();
            char prev = script.TryPrev();

            if (!to.Contains(Constants.SPACE))
            {
                // Skip a leading space unless we are inside of quotes
                while (curr == Constants.SPACE && prev != Constants.QUOTE && prev != Constants.QUOTE1)
                {
                    script.Forward();
                    curr = script.TryCurrent();
                    prev = script.TryPrev();
                }
            }

            script.MoveForwardIf(Constants.SPACE);

            // String in quotes
            bool inQuotes = curr == Constants.QUOTE || curr == Constants.QUOTE1;
            if (inQuotes)
            {
                int qend = script.Find(curr, script.Pointer + 1);
                if (qend == -1)
                {
                    throw new ScriptException("`" + script.FromPrev() + "` で、クオーテーションが不均等です。", Exceptions.UNBALANCED_QUOTES, script);
                }
                string result = script.Substr(script.Pointer + 1, qend - script.Pointer - 1);
                script.Pointer = qend + 1;
                return result;
            }

            script.MoveForwardIf(Constants.QUOTE, Constants.QUOTE1);

            int end = script.FindFirstOf(to);
            end = end < 0 ? script.Size() : end;


            // Skip found characters that have a backslash before.
            while (end > 0 && end + 1 < script.Size() &&
                   script.String[end - 1] == '\\')
            {
                end = script.FindFirstOf(to, end + 1);
            }

            end = end < 0 ? script.Size() : end;

            if (script.At(end - 1) == Constants.QUOTE)
            {
                end--;
            }

            string var = script.Substr(script.Pointer, end - script.Pointer);
            // \"yes\" --> "yes"
            var = var.Replace("\\\"", "\"");
            script.Pointer = end;

            script.MoveForwardIf(Constants.QUOTE, Constants.SPACE);

            if (eatLast && !script.MoveForwardIf(Constants.END_ARG, Constants.SPACE))
            {
                script.MoveForwardIf(Constants.NEXT_ARG);
            }

            return var;
        }

        public static string GetNextToken(ParsingScript script, bool eatLast = false, bool ignoreWhileSpace = false)
        {
            if (ignoreWhileSpace)
            {
                // 空白文字でなくなるまで前に進む
                script.MoveForwardWhile(Constants.EMPTY_AND_WHITE);
            }

            if (!script.StillValid())
            {
                return "";
            }
            int end = script.FindFirstOf(Constants.TOKEN_SEPARATION);

            if (end < 0)
            {
                return "";
            }

            string var = script.Substr(script.Pointer, end - script.Pointer);
            script.Pointer = end;

            if (eatLast)
            {
                script.Forward(1);
            }
            return var;
        }
        public static CustomFunction GetFunction(ParsingScript script, string funcName, string token)
        {
            CustomFunction customFunc = null;
            if (token == Constants.FUNCTION && script.Prev == '(')
            {
                string[] args = Utils.GetFunctionSignature(script);
                script.MoveForwardIf('{');
                string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP, "\0", true);
                script.MoveForwardIf('}');

                int parentOffset = script.Pointer +
                    (script.CurrentClass is not null ? script.CurrentClass.ParentOffset : 0);
                customFunc = new CustomFunction(funcName, body, args, script)
                {
                    ParentScript = script,
                    ParentOffset = parentOffset
                };

            }
            return customFunc;
        }

        private static void SetPropertyFromStr(string token, Variable result, ParsingScript script,
            string funcName, CustomFunction customFunc)
        {
            if (string.IsNullOrWhiteSpace(token) || (token[0] == '"' && token[^1] != '"'))
            {
                Utils.ThrowErrorMsg("値を混合して取得/設定することはできません", Exceptions.CANT_MIX_VALUE_AND_SET_GET, script, funcName);
            }
            if (customFunc is not null)
            {
                if (funcName == "set")
                {
                    result.CustomFunctionSet = customFunc;
                }
                else
                {
                    result.CustomFunctionGet = customFunc;
                }
                return;
            }
            if (token[0] == '"')
            {
                result.String = token.Substring(1, token.Length - 2);
            }
            else if (CanConvertToDouble(token.ToLowerInvariant(), out double num))
            {
                result.Value = num;
            }
            else
            {
                if (funcName == "set")
                {
                    result.CustomSet = token;
                }
                else
                {
                    result.CustomGet = token;
                }
            }
        }
        public static Variable GetProperties(ParsingScript script)
        {
            Variable result = new Variable();
            bool valueProvided = false;
            bool setgetProvided = false;
            bool _readonly = false;
            bool configurable = false;
            bool enumerable = false;
            script.MoveForwardIf('{');
            while (script.StillValid() && script.Current != '}')
            {
                var funcName = Utils.GetNextToken(script, true);
                if (string.IsNullOrWhiteSpace(funcName))
                {
                    break;
                }
                var token = Utils.GetNextToken(script, true);

                CustomFunction customFunc = GetFunction(script, funcName, token);
                script.MoveForwardIf(',');
                string lower = funcName.ToLowerInvariant();
                switch (lower)
                {
                    case "value":
                        if (setgetProvided)
                        {
                            Utils.ThrowErrorMsg("値を混合して取得/設定することはできません", Exceptions.CANT_MIX_VALUE_AND_SET_GET, script, funcName);
                        }
                        valueProvided = true;
                        SetPropertyFromStr(token, result, script, lower, customFunc);
                        break;
                    case "set":
                        if (valueProvided)
                        {
                            Utils.ThrowErrorMsg("値を混合して取得/設定することはできません", Exceptions.CANT_MIX_VALUE_AND_SET_GET, script, funcName);
                        }
                        setgetProvided = true;
                        SetPropertyFromStr(token, result, script, lower, customFunc);
                        break;
                    case "get":
                        if (valueProvided)
                        {
                            Utils.ThrowErrorMsg("値を混合して取得/設定することはできません", Exceptions.CANT_MIX_VALUE_AND_SET_GET, script, funcName);
                        }
                        setgetProvided = true;
                        SetPropertyFromStr(token, result, script, lower, customFunc);
                        break;
                    case "readonly":
                        _readonly = ConvertToDouble(token.ToLowerInvariant(), script) < 1;
                        break;
                    case "enumerable":
                        enumerable = ConvertToDouble(token.ToLowerInvariant(), script) > 0;
                        break;
                    case "configurable":
                        configurable = ConvertToDouble(token.ToLowerInvariant(), script) > 0;
                        break;
                }
            }
            if (result.Type == Variable.VarType.VOID)
            {
                result.Type = Variable.VarType.CUSTOM;
            }
            result.Readonly = _readonly;
            result.Enumerable = enumerable;
            result.Configurable = configurable;

            return result;
        }

        public static void SkipRestExpr(ParsingScript script, char toChar = Constants.END_STATEMENT)
        {
            int argRead = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            char prev = Constants.EMPTY;
            char prevprev = Constants.EMPTY;

            while (script.StillValid())
            {
                char currentChar = script.Current;
                if (inQuotes && currentChar != Constants.QUOTE)
                {
                    script.Forward();
                    continue;
                }
                if (currentChar == toChar)
                {
                    return;
                }

                switch (currentChar)
                {
                    case Constants.QUOTE1:
                        if (!inQuotes2)
                        {
                            inQuotes = inQuotes1 = !inQuotes1;
                        }
                        break;
                    case Constants.QUOTE:
                        if (!inQuotes1)
                        {
                            inQuotes = inQuotes2 = !inQuotes2;
                        }
                        break;
                    case Constants.START_ARG:
                        argRead++;
                        break;
                    case Constants.END_ARG:
                        argRead--;
                        if (argRead < 0)
                        {
                            return;
                        }
                        break;
                    case Constants.END_STATEMENT:
                        return;
                    /*case Constants.TERNARY_OPERATOR:*/
                    case Constants.NEXT_ARG:
                        if (argRead <= 0)
                        {
                            return;
                        }
                        break;
                    default:
                        break;
                }

                script.Forward();
                prevprev = prev;
                prev = currentChar;
            }
        }
        public static List<Variable> GetArgs(ParsingScript script,
            char start, char end, Action<bool> outList, FunctionBase callFrom, bool arrayMode)
        {
            List<Variable> args = new List<Variable>();
            bool isList = script.StillValid() && script.Current == Constants.START_GROUP;

            if (!script.StillValid() || script.Current == Constants.END_STATEMENT)
            {
                return args;
            }

            ParsingScript tempScript = script.GetTempScript(script.String, callFrom, script.Pointer);

            if (script.Current != start && script.TryPrev() != start &&
               (script.Current == ' ' || script.TryPrev() == ' '))
            { // 引数がスペースで区切られている場合
                start = ' ';
                end = Constants.END_STATEMENT;
            }

#pragma warning disable 219
            string body = Utils.GetBodyBetween(tempScript, start, end, "\0", false);
#pragma warning restore 219
            // 本文の最後の文字まで移動

            while (script.Pointer < tempScript.Pointer)
            {
                bool spread = false;
                // スプレッド構文かどうかを判定
                if (arrayMode && script.Current == Constants.SPREAD[0] && script.Next == Constants.SPREAD[1] && script.NextNext == Constants.SPREAD[2] && script.TryNext(3) != Constants.SPREAD[2])
                {
                    spread = true;
                    script.Forward(3);
                }
                Variable item = Utils.GetItem(script, false);
                if (spread)
                {
                    // スプレッド構文なら展開
                    args.AddRange(item.Tuple);
                }
                else
                {
                    args.Add(item);
                }
                if (script.Pointer < tempScript.Pointer)
                {
                    script.MoveForwardIf(Constants.END_GROUP);
                    script.MoveForwardIf(Constants.NEXT_ARG);
                }
                if (script.Pointer == tempScript.Pointer - 1)
                {
                    script.MoveForwardIf(Constants.END_ARG, Constants.END_GROUP);
                }
            }

            if (script.Pointer <= tempScript.Pointer)
            {
                // 閉じかっこがあるなら、そのかっこ分前に進める
                script.MoveForwardIf(Constants.END_ARG, end);
            }

            script.MoveForwardIf(Constants.SPACE);
            outList(isList);
            return args;
        }

        public static List<Variable> GetFunctionArgsAsStrings(ParsingScript script)
        {
            string[] signature = GetFunctionSignature(script);
            List<Variable> args = new List<Variable>(signature.Length);
            for (int i = 0; i < signature.Length; i++)
            {
                args.Add(new Variable(signature[i]));
            }

            return args;
        }

        public static string[] GetFunctionSignature(ParsingScript script, bool isLambda = false)
        {
            script.MoveForwardIf(Constants.START_ARG, Constants.SPACE);

            int endArgs = script.FindFirstOf(Constants.END_ARG.ToString());
            if (endArgs < 0)
            {
                endArgs = script.FindFirstOf(Constants.END_STATEMENT.ToString());
            }

            if (endArgs < 0 && !isLambda)
            {
                throw new ScriptException("関数のシグネチャを解析できませんでした。", Exceptions.INVALID_FUNCTION_SIGNATURE);
            }

            string argStr = script.Substr(script.Pointer, endArgs - script.Pointer);
            StringBuilder collect = new StringBuilder(argStr.Length);
            List<string> args = new List<string>();
            int curlyLevel = 0;
            for (int i = 0; i < argStr.Length; i++)
            {
                if (argStr[i] == '{')
                {
                    curlyLevel++;
                }
                else if (argStr[i] == '}')
                {
                    curlyLevel--;
                    if (curlyLevel < 0)
                    {
                        break;
                    }
                }
                else if (argStr[i] == Constants.NEXT_ARG && curlyLevel == 0)
                {
                    string item = collect.ToString().Trim();
                    if (item.Length == 0)
                    {
                        throw new ScriptException("関数のシグネチャが空でした。", Exceptions.INVALID_FUNCTION_SIGNATURE, script);
                    }
                    args.Add(item);
                    collect.Clear();
                    continue;
                }
                collect.Append(argStr[i]);
            }

            if (curlyLevel != 0)
            {
                throw new ScriptException("関数のシグネチャ `" + argStr + "` 内の中括弧が不均等です。", Exceptions.UNBALANCED_CURLY_BRACES);
            }
            if (collect.Length > 0)
            {
                args.Add(collect.ToString().Trim());
            }

            script.Pointer = endArgs + 1;

            return args.ToArray();
        }

        public static Variable GetVariableFromString(string str, ParsingScript script, FunctionBase callFrom, int startIndex = 0)
        {
            ParsingScript tempScript = script.GetTempScript(str, callFrom, startIndex);
            Variable result = Utils.GetItem(tempScript);
            return result;
        }

        public static string[] GetBaseClasses(ParsingScript script)
        {
            if (script.Current != ':')
            {
                return Array.Empty<string>();
            }
            script.Forward();

            int endArgs = script.FindFirstOf(Constants.START_GROUP.ToString());
            if (endArgs < 0)
            {
                throw new ScriptException("基底クラスを取得できませんでした。", Exceptions.COULDNT_EXTRACT_BASE_CLASSES, script);
            }

            string argStr = script.Substr(script.Pointer, endArgs - script.Pointer);
            string[] args = argStr.Split(Constants.NEXT_ARG_ARRAY, StringSplitOptions.RemoveEmptyEntries);

            args = args.Select(element => Constants.ConvertName(element.Trim())).ToArray();
            script.Pointer = endArgs + 1;

            return args;
        }
        public static string GetBodySize(ParsingScript script, string endToken1, string endToken2 = null)
        {
            int start = script.Pointer;
            int length = 0;
            int braces = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            bool checkBraces = true;
            char prev = Constants.EMPTY;
            char prevprev = Constants.EMPTY;

            for (; script.StillValid(); script.Forward())
            {
                if (script.StartsWith(endToken1))
                {
                    script.Forward(endToken1.Length + 1);
                    return endToken1;
                }
                if (script.StartsWith(endToken2))
                {
                    script.Forward(endToken2.Length + 1);
                    return endToken2;
                }

                char ch = script.Current;
                checkBraces = !inQuotes;
                if (ch == Constants.QUOTE && !inQuotes1)
                {
                    inQuotes = inQuotes2 = !inQuotes2;
                }
                if (ch == Constants.QUOTE1 && !inQuotes2)
                {
                    inQuotes = inQuotes1 = !inQuotes1;
                }
                if (string.IsNullOrWhiteSpace(ch.ToString()) && length == 0)
                {
                    continue;
                }
                else if (checkBraces && ch == '{')
                {
                    braces++;
                }
                else if (checkBraces && ch == '}')
                {
                    braces--;
                }

                length++;
                prevprev = prev;
                prev = ch;
                if (braces < 0)
                {
                    if (ch == '}')
                    {
                        length--;
                    }
                    break;
                }
            }

            return "";
        }

        public static string GetBodyBetween(ParsingScript script, char open = Constants.START_ARG,
                                            char close = Constants.END_ARG, string end = "\0", bool stepIn = true)
        {
            // We are supposed to be one char after the beginning of the string, i.e.
            // we must not have the opening char as the first one.
            StringBuilder sb = new StringBuilder(script.Size());
            int braces = 0;
            bool inQuotes = false;
            bool inQuotes1 = false;
            bool inQuotes2 = false;
            bool checkBraces = true;
            char prev = Constants.EMPTY;
            char prevprev = Constants.EMPTY;

            if (stepIn)
            {
                script.MoveForwardIf(open);
            }

            for (; script.StillValid(); script.Forward())
            {
                char ch = script.Current;

                if (end.Contains(ch) && !inQuotes)
                {
                    break;
                }

                if (close != Constants.QUOTE)
                {
                    checkBraces = !inQuotes;
                    if (ch == Constants.QUOTE)
                    {
                        inQuotes = inQuotes2 = !inQuotes2;
                    }
                    if (ch == Constants.QUOTE1 && !inQuotes2)
                    {
                        inQuotes = inQuotes1 = !inQuotes1;
                    }
                }

                if (string.IsNullOrWhiteSpace(ch.ToString()) && sb.Length == 0)
                {
                    continue;
                }
                else if (checkBraces && ch == open)
                {
                    braces++;
                }
                else if (checkBraces && ch == close)
                {
                    braces--;
                }

                sb.Append(ch);
                prevprev = prev;
                prev = ch;
                if (braces < 0)
                {
                    if (ch == close)
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    break;
                }
            }
            return sb.ToString();
        }

        public static string IsNotSign(string data)
        {
            //return data.StartsWith(Constants.NOT) ? Constants.NOT : null;
            return data.StartsWith(Constants.NOT, StringComparison.Ordinal) && !data.StartsWith(Constants.NOT_EQUAL, StringComparison.Ordinal) ? Constants.NOT : null;
        }

        public static string ValidAction(string rest)
        {
            string action = Utils.StartsWith(rest, Constants.ACTIONS);
            return action;
        }

        public static bool IsAction(char ch)
        {
            return ch == '+' || ch == '-' || ch == '*' || ch == '/' || ch == '%' || ch == '&' || ch == '|' || ch == '^';
        }

        public static string StartsWith(string data, string[] items)
        {
            foreach (string item in items)
            {
                if (data.StartsWith(item, StringComparison.Ordinal))
                {
                    return item;
                }
            }
            return null;
        }

        public static List<Variable> GetArrayIndices(ParsingScript script, string varName, Action<string> updateVarName, FunctionBase callFrom, int max = 0)
        {
            int end = 0;
            return GetArrayIndices(script, varName, end, (string str, int i) => { updateVarName(str); end = i; }, callFrom, max);
        }
        public static List<Variable> GetArrayIndices(ParsingScript script, string varName, int end, Action<string, int> updateVals, FunctionBase callFrom, int max = 0)
        {
            List<Variable> indices = new List<Variable>();

            if (max != 0)
            {
                //逆からインデックス
                varName = Constants.REVERSE_INDEXER.Replace(varName, "$1[" + max + "-$2]");
            }

            int argStart = varName.IndexOf(Constants.START_ARRAY, StringComparison.Ordinal);
            if (argStart < 0)
            {
                return indices;
            }
            int firstIndexStart = argStart;

            while (argStart < varName.Length &&
                   varName[argStart] == Constants.START_ARRAY)
            {
                int argEnd = varName.IndexOf(Constants.END_ARRAY, argStart + 1);
                if (argEnd == -1 || argEnd <= argStart + 1)
                {
                    break;
                }

                ParsingScript tempScript = script.GetTempScript(varName, callFrom, argStart);
                tempScript.MoveForwardIf(Constants.START_ARG, Constants.START_ARRAY);

                Variable index = tempScript.Execute(Constants.END_ARRAY_ARRAY);

                indices.Add(index);
                argStart = argEnd + 1;
            }

            if (indices.Count > 0)
            {
                varName = varName.Substring(0, firstIndexStart);
                end = argStart - 1;
            }

            updateVals(varName, end);
            return indices;
        }
        public static List<string> ExtractTokens(ParsingScript script)
        {
            List<string> tokens = new List<string>();
            script.MoveForwardIf(Constants.START_ARG);
            while (script.TryCurrent() != Constants.END_GROUP)
            {
                string propName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
                script.MoveForwardIf(Constants.NEXT_ARG);
                tokens.Add(propName);
            }
            return tokens;
        }

        public static Variable ExtractArrayElement(Variable array,
                                                   List<Variable> indices,
                                                   ParsingScript script)
        {
            Variable currLevel = array;

            for (int i = 0; i < indices.Count; i++)
            {
                Variable index = indices[i];
                int arrayIndex = currLevel.GetArrayIndex(index);

                int tupleSize = currLevel.GetSize();

                if (arrayIndex < 0 || arrayIndex >= tupleSize)
                {
                    throw new IndexOutOfRangeException("インデックス `" + index.AsString() + "`は配列の境界 `" + tupleSize + "` 外です。");
                }
                switch (currLevel.Type)
                {
                    case Variable.VarType.ARRAY:
                        {
                            currLevel = currLevel.Tuple[arrayIndex];
                            break;
                        }
                    case Variable.VarType.DELEGATE:
                        {
                            currLevel = new Variable(currLevel.Delegate.Functions[arrayIndex]);
                            break;
                        }
                    case Variable.VarType.STRING:
                        {
                            currLevel = new Variable(currLevel.String[arrayIndex].ToString());
                            break;
                        }
                }
            }
            return currLevel;
        }
    }
}
