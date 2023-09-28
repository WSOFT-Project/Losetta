using AliceScript.Binding;
using System.Diagnostics;
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
            var result = new List<string>();
            foreach (Match m in Regex.Matches(input, pattern))
            {
                result.Add(m.Value);
            }
            return result.ToArray();
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
            var mc = Regex.Matches(input, pattern);
            List<string> result = new List<string>();
            foreach (Match m in mc)
            {
                result.Add(m.Value);
            }
            return result.ToArray();
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
        public static string Regex_FromWildCard(string wildCard)
        {
            wildCard = Regex.Escape(wildCard);
            wildCard = wildCard.Replace("\\*", ".*");
            wildCard = wildCard.Replace("\\?", ".");
            wildCard = wildCard.Replace("\\[", "[");
            wildCard = wildCard.Replace("[!", "[^");
            wildCard = wildCard.Replace("#", "\\d");

            return $"^({wildCard})$";
        }
    }
}
