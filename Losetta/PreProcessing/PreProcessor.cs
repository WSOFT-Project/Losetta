using AliceScript.Extra;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.PreProcessing
{
    /// <summary>
    /// ユーザーが入力したソースコードを内部で使用できるスクリプトに変換します
    /// </summary>
    public static class PreProcessor
    {
        /// <summary>
        /// ユーザーが入力したソースコードを内部で使用できるスクリプトに変換します
        /// </summary>
        /// <param name="source">ユーザーによるソースコード</param>
        /// <param name="char2Line">行データ</param>
        /// <param name="defines">ソースコードで定義されたシンボル</param>
        /// <param name="settings">ソースコードで指定された設定</param>
        /// <param name="filename">ソースコードのファイル名</param>
        /// <returns>内部で使用できるスクリプト</returns>
        public static string ConvertToScript(string source, out Dictionary<int, int> char2Line, out HashSet<string> defines, out ParsingScript.ScriptSettings settings, string filename = "")
        {
            const string curlyErrorMsg = "波括弧が不均等です";
            const string bracketErrorMsg = "角括弧が不均等です";
            const string parenthErrorMsg = "括弧が不均等です";
            const string quoteErrorMsg = "クオーテーションが不均等です";

            settings = new ParsingScript.ScriptSettings();

            // Unicodeコード表現を文字列に置き換える
            source = ConvertUnicodeLiteral(source);

            // 複合代入[x op= y;]を[x = x op y]に置き換える
            source = Constants.COMPOUND_ASSIGN_PATTERN.Replace(source, "$1=$1$2$3");

            StringBuilder sb = new StringBuilder(source.Length);

            var pragmaCommand = new StringBuilder();
            var pragmaArgs = new StringBuilder();

            defines = new HashSet<string>();

            char2Line = new Dictionary<int, int>();

            bool inPragma = false;
            bool inAnnotation = false;
            bool inPragmaCommand = false;
            bool inPragmaArgs = false;
            bool inIf = false;
            bool If = false;

            //文字列リテラルなど、クオーテーションの内部
            bool inQuotes = false;
            //シングルクオーテーション
            bool inQuotes1 = false;
            //ダブルクオーテーション
            bool inQuotes2 = false;
            bool spaceOK = false;
            bool inComments = false;
            bool simpleComments = false;
            bool hasDoller = false;
            char prev = Constants.EMPTY;

            int levelCurly = 0;
            int levelBrackets = 0;
            int levelParentheses = 0;
            int lineNumber = 0;
            int lineNumberCurly = 0;
            int lineNumberBrack = 0;
            int lineNumberPar = 0;
            int lineNumberQuote = 0;
            int skipFor = 0;

            int lastScriptLength = 0;

            StringBuilder lastToken = new StringBuilder();

            for (int i = 0; i < source.Length; i++)
            {
                char ch = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : Constants.EMPTY;
                char nextnext = i + 2 < source.Length ? source[i + 2] : Constants.EMPTY;
                char last = sb.Length > 0 ? sb[sb.Length - 1] : Constants.EMPTY;

                if (skipFor > 0)
                {
                    skipFor--;
                    continue;
                }

                if (ch == Constants.EMPTY)
                {
                    lastToken.Clear();
                }


                if (ch == '\n')
                {
                    if (inPragma || inAnnotation)
                    {
                        inPragmaCommand = false;
                        inPragmaArgs = false;
                    }
                    else
                    {
                        if (sb.Length > lastScriptLength)
                        {
                            char2Line[sb.Length - 1] = lineNumber;
                            lastScriptLength = sb.Length;
                        }
                        lineNumber++;
                        if (simpleComments)
                        {
                            inComments = simpleComments = false;
                        }
                        lastToken.Clear();
                        continue;
                    }
                }

                if (inComments && ((simpleComments && ch != '\n') ||
                                  (!simpleComments && ch != '*')))
                {
                    continue;
                }

                if (IsIgnoreChar(ch))
                {
                    if (inQuotes)
                    {
                        sb.Append(ch);
                    }
                    continue;
                }
                if (IsIgnoreCharEvenIfString(ch))
                {
                    continue;
                }

                switch (ch)
                {
                    case '/':
                        if (!inQuotes && (!inIf || If) && (inComments || next == '/' || next == '*'))
                        {
                            inComments = true;
                            simpleComments = simpleComments || next == '/';
                            continue;
                        }
                        break;
                    case '#':
                        if (!inPragma && !inComments && !inQuotes)
                        {
                            inPragma = true;
                            inPragmaCommand = true;
                            continue;
                        }
                        break;
                    case '@':
                        if (!inAnnotation && !inComments && !inQuotes)
                        {
                            inAnnotation = true;
                            inPragmaCommand = true;
                            continue;
                        }
                        break;
                    case '*':
                        if (!inQuotes && inComments && next == '/')
                        {
                            i++;
                            inComments = false;
                            continue;
                        }
                        break;
                    case '\'':
                        if (!inComments && (!inIf || If) && !inQuotes2)
                        {
                            ch = '"';
                            inQuotes = inQuotes1 = !inQuotes1;
                            if (inQuotes && prev == '"' && lineNumberQuote == 0)
                            {
                                lineNumberQuote = lineNumber;
                            }
                        }
                        break;
                    case '“':
                    case '”':
                    case '„':
                    case '"':
                        ch = Constants.QUOTE;
                        if (!inComments && (!inIf || If) && !inQuotes1)
                        {
                            // 文字列リテラルの事前結合が行えるか判断
                            // つまり、ダブルクオーテーションで囲われていて、リテラル間が'+'でつながっており、次のリテラルもそうである場合
                            if (inQuotes && !hasDoller && GetMeaningChar(source, i + 1, out int left) == '+' && GetMeaningChar(source, left + 1, out int right) == Constants.QUOTE)
                            {
                                skipFor = left - i + (right - left);
                                continue;
                            }
                            inQuotes = inQuotes2 = !inQuotes2;
                            hasDoller = inQuotes && prev == Constants.DOLLER;
                            if (inQuotes && prev == '"' && lineNumberQuote == 0)
                            {
                                lineNumberQuote = lineNumber;
                            }
                        }
                        else if (inQuotes1)
                        {
                            sb.Append('\\');
                        }
                        break;
                    case Constants.START_ARG:
                        if (inAnnotation && inPragmaCommand)
                        {
                            inPragmaCommand = false;
                            inPragmaArgs = true;
                        }
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            if (levelParentheses == 0)
                            {
                                lineNumberPar = lineNumber;
                            }
                            levelParentheses++;
                        }
                        break;
                    case Constants.END_ARG:
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            levelParentheses--;
                            spaceOK = false;
                            if (levelParentheses < 0)
                            {
                                ThrowSyntaxError(parenthErrorMsg, source, Exceptions.UNBALANCED_QUOTES, levelParentheses, lineNumberPar, lineNumber, filename);
                            }
                        }
                        break;
                    case Constants.START_GROUP:
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            if (levelCurly == 0)
                            {
                                lineNumberCurly = lineNumber;
                            }
                            levelCurly++;
                        }
                        break;
                    case Constants.END_GROUP:
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            levelCurly--;
                            spaceOK = false;
                            if (levelCurly < 0)
                            {
                                ThrowSyntaxError(curlyErrorMsg, source, Exceptions.UNBALANCED_CURLY_BRACES, levelCurly, lineNumberCurly, lineNumber, filename);
                            }
                        }
                        break;
                    case Constants.START_ARRAY:
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            if (levelBrackets == 0)
                            {
                                lineNumberBrack = lineNumber;
                            }
                            levelBrackets++;
                        }
                        break;
                    case Constants.END_ARRAY:
                        if (!inQuotes && !inComments && (!inIf || If))
                        {
                            levelBrackets--;
                            if (levelBrackets < 0)
                            {
                                ThrowSyntaxError(bracketErrorMsg, source, Exceptions.UNBALANCED_SQUARE_BLACKETS, levelBrackets, lineNumberBrack, lineNumber, filename);
                            }
                        }
                        break;
                    case Constants.END_STATEMENT:
                        if (!inQuotes)
                        {
                            spaceOK = false;
                        }
                        if (inPragma || inAnnotation)
                        {
                            inPragmaCommand = false;
                            inPragmaArgs = false;
                        }
                        break;
                    case '\\':
                        {
                            if (inQuotes2)
                            {
                                i++;
                                switch (next)
                                {
                                    case '0':
                                        {
                                            sb.Append('\u0000');
                                            continue;
                                        }
                                    case 'a':
                                        {
                                            sb.Append('\u0007');
                                            continue;
                                        }
                                    case 'b':
                                        {
                                            sb.Append('\u0008');
                                            continue;
                                        }
                                    case 'e':
                                        {
                                            sb.Append('\u001B');
                                            continue;
                                        }
                                    case 'f':
                                        {
                                            sb.Append('\u000C');
                                            continue;
                                        }
                                    case 'n':
                                        {
                                            sb.Append('\u000A');
                                            continue;
                                        }
                                    case 'r':
                                        {
                                            sb.Append('\u000D');
                                            continue;
                                        }
                                    case 't':
                                        {
                                            sb.Append('\u0009');
                                            continue;
                                        }
                                    case 'v':
                                        {
                                            sb.Append('\u000B');
                                            continue;
                                        }
                                    case '\\':
                                        {
                                            sb.Append('\\');
                                            continue;
                                        }
                                    case Constants.QUOTE:
                                        {
                                            sb.Append(Constants.QUOTE_IN_LITERAL);
                                            continue;
                                        }
                                    case Constants.QUOTE1:
                                        {
                                            sb.Append(Constants.QUOTE1_IN_LITERAL);
                                            continue;
                                        }
                                    default:
                                        {
                                            ThrowSyntaxError($"`\\{next}`は認識できないエスケープ文字です", source, Exceptions.UNKNOWN_ESCAPE_CHAR, 0, 0, lineNumber, filename);
                                            break;
                                        }
                                }
                            }
                            break;
                        }
                    default:
                        break;
                }

                // 文字がスペースの場合
                if (char.IsWhiteSpace(ch))
                {
                    if (inQuotes)
                    {
                        sb.Append(ch);
                    }
                    else if (inPragmaCommand)
                    {
                        inPragmaCommand = false;
                        inPragmaArgs = true;
                    }
                    else
                    {
                        bool keepSpace = KeepSpace(sb, next);
                        bool usedSpace = spaceOK;
                        spaceOK = keepSpace ||
                             (prev != Constants.EMPTY && prev != Constants.NEXT_ARG && spaceOK);
                        if (spaceOK || KeepSpaceOnce(sb, next))
                        {
                            sb.Append(Constants.SPACE);
                        }
                        spaceOK = spaceOK || (usedSpace && prev == Constants.NEXT_ARG);
                    }
                }
                else if (!inComments && (!inIf || If) && !(inPragma || inAnnotation))
                {
                    sb.Append(ch);
                    lastToken.Append(ch);
                }
                else if (inPragmaCommand)
                {
                    pragmaCommand.Append(ch);
                }
                else if (inPragmaArgs)
                {
                    pragmaArgs.Append(ch);
                }
                else if (inAnnotation)
                {
                    inAnnotation = false;
                    sb.Append(Constants.ANNOTATION_FUNCTION_REFIX);
                    sb.Append(pragmaCommand);
                    if (pragmaArgs.Length > 0)
                    {
                        sb.Append(pragmaArgs);
                    }
                    else
                    {
                        //引数がなかった場合は空の引数リストで関数呼び出しをつける
                        sb.Append(Constants.START_ARG + Constants.END_ARG);
                    }
                    sb.Append(Constants.END_STATEMENT);
                    pragmaCommand.Clear();
                    pragmaArgs.Clear();
                    i--;
                }
                else if (inPragma)
                {
                    inPragma = false;

                    string command = pragmaCommand.ToString().ToLowerInvariant();
                    string arg = pragmaArgs.ToString().ToLowerInvariant();


                    switch (command)
                    {
                        case Constants.UNNEED_VAR:
                            {
                                settings.UnneedVarKeyword = ConvertBool(arg);
                                break;
                            }
                        case Constants.TYPE_INFERENCE:
                            {
                                settings.TypeInference = ConvertBool(arg);
                                break;
                            }
                        case Constants.FALL_THROUGH:
                            {
                                settings.FallThrough = ConvertBool(arg);
                                break;
                            }
                        case Constants.CHECK_BREAK_WHEN_CASE:
                            {
                                settings.CheckBreakWhenEndCaseBlock = ConvertBool(arg);
                                break;
                            }
                        case Constants.ENABLE_USING:
                            {
                                settings.EnableUsing = ConvertBool(arg);
                                break;
                            }
                        case Constants.ENABLE_IMPORT:
                            {
                                settings.EnableImport = ConvertBool(arg);
                                break;
                            }
                        case Constants.ENABLE_INCLUDE:
                            {
                                settings.EnableInclude = ConvertBool(arg);
                                break;
                            }
                        case Constants.DENY_TO_TOPLEVEL_SCRIPT:
                            {
                                settings.DenyAccessToTopLevelScript = ConvertBool(arg);
                                break;
                            }
                        case Constants.NULLABLE:
                            {
                                settings.Nullable = ConvertBool(arg);
                                break;
                            }
                        case Constants.INCLUDE:
                            {
                                string str = SafeReader.ReadAllText(arg, out _, out _);
                                str = ConvertToScript(str, out _, out var def, out var setting, Path.GetFileName(arg));
                                foreach (var d in def)
                                {
                                    defines.Add(d);
                                }
                                settings.Union(setting);
                                sb.Append(str);
                                break;
                            }
                        case "define":
                            {
                                defines.Add(arg);
                                break;
                            }
                        case "undef":
                            {
                                defines.Remove(arg);
                                break;
                            }
                        case "if":
                            {
                                inIf = true;
                                if (arg.StartsWith('!'))
                                {
                                    If = !defines.Contains(arg.TrimStart('!'));
                                }
                                If = defines.Contains(arg);
                                break;
                            }
                        case "else":
                            {
                                inIf = true;
                                If = !If;
                                break;
                            }
                        case "endif":
                            {
                                inIf = false;
                                If = false;
                                break;
                            }
                        case "error":
                            {
                                throw new ScriptException(arg, Exceptions.USER_DEFINED);
                            }
                        case "print":
                            {
                                Interpreter.Instance.AppendOutput(arg, true);
                                break;
                            }
                        case Constants.OBSOLETE:
                        case Constants.NET_IMPORT:
                        case Constants.LIBRARY_IMPORT:
                            {
                                // 関数呼び出しに変換する
                                sb.Append(Constants.USER_CANT_USE_FUNCTION_PREFIX);
                                sb.Append(command);
                                sb.Append(Constants.START_ARG);
                                sb.Append(pragmaArgs);
                                sb.Append(Constants.END_ARG + ";");
                                break;
                            }
                    }

                    pragmaCommand.Clear();
                    pragmaArgs.Clear();
                    i--;
                }

                prev = ch;
            }

            if (sb.Length > lastScriptLength)
            {
                char2Line[sb.Length - 1] = lineNumber;
                lastScriptLength = sb.Length;
            }

            bool error = levelCurly != 0 || levelBrackets != 0 || levelParentheses != 0 || inQuotes;
            if (error)
            {
                if (inQuotes)
                {
                    ThrowSyntaxError(quoteErrorMsg, source, Exceptions.UNBALANCED_QUOTES, 1, lineNumberQuote, lineNumber, filename);
                }
                else if (levelBrackets != 0)
                {
                    ThrowSyntaxError(bracketErrorMsg, source, Exceptions.UNBALANCED_SQUARE_BLACKETS, levelBrackets, lineNumberBrack, lineNumber, filename);
                }
                else if (levelParentheses != 0)
                {
                    ThrowSyntaxError(parenthErrorMsg, source, Exceptions.UNBALANCED_PARENTHESES, levelParentheses, lineNumberPar, lineNumber, filename);
                }
                else if (levelCurly != 0)
                {
                    ThrowSyntaxError(curlyErrorMsg, source, Exceptions.UNBALANCED_CURLY_BRACES, levelCurly, lineNumberCurly, lineNumber, filename);
                }
            }
            return sb.ToString();
        }
        private static char GetMeaningChar(string text, int startIndex, out int foundIndex)
        {
            for (int i = startIndex; i < text.Length; i++)
            {
                char c = text[i];
                if (!(IsIgnoreChar(c) || IsIgnoreCharEvenIfString(c) || char.IsWhiteSpace(c)))
                {
                    foundIndex = i;
                    return c;
                }
            }
            foundIndex = -1;
            return Constants.EMPTY;
        }
        private static void ThrowSyntaxError(string msg, string code, Exceptions ecode, int level, int lineStart, int lineEnd, string filename)
        {
            var lineNumber = level > 0 ? lineStart : lineEnd;

            string[] lines = code.Split('\n');
            lineNumber = lines.Length <= lineNumber ? -1 : lineNumber;
            string line = string.Empty;
            if (lineNumber > 0)
            {
                line = lines[lineNumber].Trim();
            }

            var currentLineNumber = lineNumber;
            StringBuilder stack = new StringBuilder();
            stack.AppendLine();
            if (filename.Length > 0)
            {
                stack.AppendFormat("ファイル : {0} ,", filename);
            }
            stack.AppendFormat("行 : {0}\r\n", currentLineNumber.ToString());
            stack.AppendFormat(" {0} <--", line);
            throw new ScriptException(msg + stack.ToString(), ecode);
        }
        /// <summary>
        /// Unicode文字リテラルが含まれた文字列をUnicode文字に直します
        /// </summary>
        /// <param name="input">Unicode文字リテラルが含まれた文字列</param>
        /// <returns>Unicode文字を含む文字列</returns>
        private static string ConvertUnicodeLiteral(string input)
        {
            if (input.Contains("\\u", StringComparison.OrdinalIgnoreCase) || input.Contains("\\x", StringComparison.Ordinal))
            {
                //UTF-16文字コードの置き換え
                foreach (Match match in Constants.UTF16_LITERAL.Matches(input))
                {
                    string result = match.Value;
                    input = input.Replace(result, (result[0] == '\\' ? null : result[0]) + ConvertUnicodeToChar(result.Substring(result.IndexOf('u') + 1), false));
                }
                //可変長UTF-16文字コードの置き換え
                foreach (Match match in Constants.UTF16_VARIABLE_LITERAL.Matches(input))
                {
                    string result = match.Value;
                    input = input.Replace(result, (result[0] == '\\' ? null : result[0]) + ConvertUnicodeToChar(result.Substring(result.IndexOf('x') + 1), true));
                }
                //UTF-32文字コードの置き換え
                foreach (Match match in Constants.UTF32_LITERAL.Matches(input))
                {
                    string result = match.Value;
                    input = input.Replace(result, (result[0] == '\\' ? null : result[0]) + ConvertUnicodeToChar(result.Substring(result.IndexOf('U') + 1), false));
                }
            }
            return input;
        }
        /// <summary>
        /// Unicode文字列リテラルをUnicode文字に変換します
        /// </summary>
        /// <param name="charCode">Unicode文字列リテラルを表す文字列</param>
        /// <param name="mode">UTF-32の文字を変換する場合はtrue、それ以外の場合はfalse</param>
        /// <returns>Unicode文字を含む文字列</returns>
        private static string ConvertUnicodeToChar(string charCode, bool mode = true)
        {
            if (mode)
            {
                int charCode16 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                char c = Convert.ToChar(charCode16);  // 数値(文字コード) -> 文字
                return c.ToString();
            }
            else
            {
                //UTF-32モード
                int charCode32 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                return char.ConvertFromUtf32(charCode32);
            }

        }
        /// <summary>
        /// プラグマに含まれる肯定/否定表現をboolに変換します
        /// </summary>
        /// <param name="str">肯定/否定表現を含む文字列</param>
        /// <returns>肯定表現の場合はtrue、否定表現の場合はfalse、それ以外の場合はnull</returns>
        private static bool? ConvertBool(string str)
        {
            str = str.ToLowerInvariant().Trim();
            switch (str)
            {
                case Constants.TRUE:
                    return true;
                case Constants.FALSE:
                    return false;
                case "enable":
                    return true;
                case "disable":
                    return false;
                default:
                    return null;
            }
        }

        private static bool SpaceNotNeeded(char next)
        {
            return next == Constants.SPACE || next == Constants.START_ARG ||
                    next == Constants.START_GROUP || /*next == Constants.START_ARRAY ||*/
                    next == Constants.EMPTY;
        }

        private static bool KeepSpace(StringBuilder sb, char next)
        {
            return !SpaceNotNeeded(next) && EndsWithFunction(sb.ToString(), Constants.FUNCT_WITH_SPACE);
        }
        private static bool KeepSpaceOnce(StringBuilder sb, char next)
        {
            if (SpaceNotNeeded(next))
            {
                return false;
            }

            string str = sb.ToString();
            char last = str.Length < 1 ? Constants.EMPTY : str.Last();
            return ((char.IsLetterOrDigit(last) || Constants.TOKEN_END.Contains(last)) && (char.IsLetterOrDigit(next) || Constants.TOKEN_START.Contains(next))) || EndsWithFunction(str, Constants.FUNCT_WITH_SPACE_ONCE);
        }

        /// <summary>
        /// 文字列リテラル中でも無視する文字を判定します
        /// </summary>
        /// <param name="ch">判定する文字</param>
        /// <returns>無視する文字の場合はtrue、それ以外の場合はfalse</returns>
        private static bool IsIgnoreCharEvenIfString(char ch)
        {
            return '\ufdd0' <= ch && ch <= '\ufddf';
        }
        /// <summary>
        /// 文字列リテラル以外で無視する文字を判定します
        /// </summary>
        /// <param name="ch">判定する文字</param>
        /// <returns>無視する文字の場合はtrue、それ以外の場合はfalse</returns>
        private static bool IsIgnoreChar(char ch)
        {
            return Constants.IGNORE_CHARS.Contains(ch) || char.IsControl(ch) || char.GetUnicodeCategory(ch).HasFlag(System.Globalization.UnicodeCategory.Format);
        }

        private static bool EndsWithFunction(string buffer, HashSet<string> functions)
        {
            foreach (string key in functions)
            {
                if (buffer.EndsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    char prev = key.Length >= buffer.Length ?
                        Constants.END_STATEMENT :
                        buffer[buffer.Length - key.Length - 1];
                    if (Constants.TOKEN_SEPARATION.Contains(prev))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
