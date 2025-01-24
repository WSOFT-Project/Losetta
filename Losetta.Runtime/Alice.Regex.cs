using AliceScript.Binding;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Regex
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(RegexFunctions));
        }
    }
    [AliceNameSpace(Name = "Alice.Regex")]
    internal static class RegexFunctions
    {
        public static string Regex_Escape(string text)
        {
            return Regex.Escape(text);
        }
        public static bool Regex_IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
        public static string Regex_Match(string input, string pattern)
        {
            return Regex.Match(input, pattern).Value;
        }
        public static string[] Regex_Matches(string input, string pattern)
        {
            return Regex.Matches(input, pattern).Select(m => m.Value).ToArray();
        }
        public static string Regex_Replace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
        public static string[] Regex_Split(string input, string pattern)
        {
            return Regex.Split(input, pattern);
        }
        public static bool IsMatch(this string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
        public static string[] Matches(this string input, string pattern)
        {
            return Regex.Matches(input, pattern).Select(m => m.Value).ToArray();
        }
        public static bool Like(this string str, string pattern)
        {
            return Regex.IsMatch(str, WildCardToRegex(pattern));
        }
        public static string ReplaceAll(this string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
        public static string ReplaceFirst(this string input, string pattern, string replacement)
        {
            var reg = new Regex(pattern);
            return reg.Replace(input, replacement, 1);
        }
        /// <summary>
        /// ワイルドカード文字列から正規表現文字列を作る
        /// </summary>
        /// <param name="wildCard"></param>
        /// <returns></returns>
        private static string WildCardToRegex(string wildCard)
        {
            wildCard = Regex.Escape(wildCard);

            // 置換条件的にもこの後文字列が伸びることはない
            var sb = new StringBuilder(wildCard.Length);

            // 現在の文字はエスケープされた結果であるか
            bool escaped = false;

            char prev = '\0';

            foreach (char ch in wildCard)
            {
                if (escaped)
                {
                    switch (ch)
                    {
                        case '*':
                            if (escaped)
                            {
                                sb.Append("\\*");
                            }
                            else
                            {
                                sb.Append(".*");
                            }
                            escaped = false;
                            break;
                        case '?':
                            if (escaped)
                            {
                                sb.Append("\\?");
                            }
                            else
                            {
                                sb.Append(".");
                            }
                            escaped = false;
                            break;
                        case '[':
                            if (escaped)
                            {
                                sb.Append("\\[");
                            }
                            else
                            {
                                sb.Append("[");
                            }
                            escaped = false;
                            break;
                        case '!':
                            if (escaped)
                            {
                                sb.Append("\\!");
                            }
                            else
                            {
                                sb.Append("[^");
                            }
                            escaped = false;
                            break;
                        case '#':
                            if (escaped)
                            {
                                sb.Append("\\d");
                            }
                            else
                            {
                                sb.Append("#");
                            }
                            escaped = false;
                            break;
                        case '\\':
                            if (escaped)
                            {
                                sb.Append("\\\\");
                            }
                            else
                            {
                                escaped = true;
                            }
                            break;
                        default:
                            if (escaped)
                            {
                                sb.Append('\\');
                                escaped = false;
                            }
                            sb.Append(ch);
                            break;
                    }
                }
                else
                {
                    if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (prev == '[' && ch == '!')
                {
                    sb.Append('^');
                }
                prev = ch;
            }

            return $"^({sb})$";
        }

    }
}
