using System.Runtime.CompilerServices;

namespace AliceScript.NameSpaces.Core
{
    internal partial class ExFunctions
    {
        public static int IndexOf(this string str, string item)
        {
            return str.IndexOf(item);
        }
        public static int IndexOf(this string str, string item, int index)
        {
            return str.IndexOf(item, index);
        }
        public static int IndexOf(this string str, string item, int index, int count)
        {
            return str.IndexOf(item, index, count);
        }
        public static int LastIndexOf(this string str, string item)
        {
            return str.LastIndexOf(item);
        }
        public static int LastIndexOf(this string str, string item, int index)
        {
            return str.LastIndexOf(item, index);
        }
        public static int LastIndexOf(this string str, string item, int index, int count)
        {
            return str.LastIndexOf(item, index, count);
        }
        public static string Insert(this string str, int index, Variable item)
        {
            return str.Insert(index, item.AsString());
        }
        public static string Remove(this string str, string item)
        {
            return str.Replace(item,null);
        }
        public static string Remove(this string str, int startIndex)
        {
            return str.Remove(startIndex);
        }
        public static string Remove(this string str, int startIndex, int count)
        {
            return str.Remove(startIndex, count);
        }
        public static string RemoveAt(this string str, int index)
        {
            return str.Remove(index);
        }
        public static string Trim(this string str)
        {
            return str.Trim();
        }
        public static string Trim(this string str, string chars)
        {
            return str.Trim(chars.ToArray());
        }
        public static string TrimStart(this string str)
        {
            return str.TrimStart();
        }
        public static string TrimStart(this string str, string chars)
        {
            return str.TrimStart(chars.ToArray());
        }
        public static string TrimEnd(this string str)
        {
            return str.TrimEnd();
        }
        public static string TrimEnd(this string str, string chars)
        {
            return str.TrimEnd(chars.ToArray());
        }
        public static bool StartsWith(this string str, string value)
        {
            return str.StartsWith(value);
        }
        public static bool StartsWith(this string str, string value, bool ignoreCase)
        {
            return str.StartsWith(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public static bool StartsWith(this string str, string value, bool ignoreCase, bool considerCulture)
        {
            return str.StartsWith(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static bool EndsWith(this string str, string value)
        {
            return str.EndsWith(value);
        }
        public static bool EndsWith(this string str, string value, bool ignoreCase)
        {
            return str.EndsWith(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public static bool EndsWith(this string str, string value, bool ignoreCase, bool considerCulture)
        {
            return str.EndsWith(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static bool Contains(this string str, string value)
        {
            return str.Contains(value);
        }
        public static bool Contains(this string str, string value, bool ignoreCase)
        {
            return str.Contains(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public static bool Contains(this string str, string value, bool ignoreCase, bool considerCulture)
        {
            return str.Contains(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static string ToUpper(this string str)
        {
            return str.ToUpper();
        }
        public static string ToUpperInvariant(this string str)
        {
            return str.ToUpperInvariant();
        }
        public static string ToLower(this string str)
        {
            return str.ToLower();
        }
        public static string ToLowerInvariant(this string str)
        {
            return str.ToLowerInvariant();
        }
        public static string PadLeft(this string str, int totalWidth)
        {
            return str.PadLeft(totalWidth);
        }
        public static string PadLeft(this string str, int totalWidth, string chars)
        {
            return str.PadLeft(totalWidth, chars.ToArray()[0]);
        }
        public static string PadRight(this string str, int totalWidth)
        {
            return str.PadRight(totalWidth);
        }
        public static string PadRight(this string str, int totalWidth, string chars)
        {
            return str.PadRight(totalWidth, chars.ToArray()[0]);
        }
        public static string Format(this string str, params Variable[] args)
        {
            return StringFormatFunction.Format(str, args);
        }
        public static string Join(this string str, params Variable[] args)
        {
            return string.Join(str, args.ToList());
        }
        public static string Normalize(this string str)
        {
            return str.Normalize();
        }
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsEmptyOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        public static string Replace(this string str,string oldValue,string newValue)
        {
            return str.Replace(oldValue,newValue);
        }
        public static string Replace(this string str, string oldvalue , string newValue , bool ignoreCase)
        {
            return str.Replace(oldvalue,newValue, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public static string Replace(this string str, string oldvalue , string newValue, bool ignoreCase, bool considerCulture)
        {
            return str.Replace(oldvalue,newValue, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static string ReplaceLineEndings(this string str)
        {
            return str.ReplaceLineEndings();
        }
        public static string ReplaceLineEndings(this string str, string replacementText)
        {
            return str.ReplaceLineEndings(replacementText);
        }
        public static string[] Split(this string str)
        {
            var result = new List<string>();

            foreach(char c in str)
            {
                result.Add(c.ToString());
            }

            return result.ToArray();
        }
        public static string[] Split(this string str,string separator)
        {
            return str.Split(separator);
        }
        public static string Substring(this string str, int startIndex)
        {
            return str.Substring(startIndex);
        }
        public static string Substring(this string str, int startIndex,int length)
        {
            return str.Substring(startIndex,length);
        }
    }
}
