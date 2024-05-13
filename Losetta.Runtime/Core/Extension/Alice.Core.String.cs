using AliceScript.Binding;
using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
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
            return str.Replace(item, null);
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
        public static bool IsNormalized(this string str)
        {
            return str.IsNormalized();
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
#if NETCOREAPP2_1_OR_GREATER
            return str.Contains(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
#else
            if (ignoreCase)
            {
                str = str.ToUpper();
                value = value.ToUpper();
            }
            return str.Contains(value);
#endif
        }
        public static bool Contains(this string str, string value, bool ignoreCase, bool considerCulture)
        {
#if NETCOREAPP2_1_OR_GREATER
            return str.Contains(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static string ToUpper(this string str)
        {
            return str.ToUpper();
        }
        public static string ToUpper(this string str, string? cultureName)
        {
            if (cultureName == null)
            {
                return str.ToUpperInvariant();
            }
            var info = CultureInfo.GetCultureInfo(cultureName).TextInfo;
            return info.ToUpper(str);
        }
        public static string ToUpperInvariant(this string str)
        {
            return str.ToUpperInvariant();
        }
        public static string ToLower(this string str)
        {
            return str.ToLower();
        }
        public static string ToLower(this string str, string? cultureName)
        {
            if (cultureName == null)
            {
                return str.ToLowerInvariant();
            }
            var info = CultureInfo.GetCultureInfo(cultureName).TextInfo;
            return info.ToLower(str);
        }
        public static string ToLowerInvariant(this string str)
        {
            return str.ToLowerInvariant();
        }
        public static string ToTitleCase(this string str)
        {
            var info = CultureInfo.CurrentCulture.TextInfo;
            return info.ToTitleCase(str);
        }
        public static string ToTitleCase(this string str, string? cultureName)
        {
            if (cultureName == null)
            {
                return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);
            }
            var info = CultureInfo.GetCultureInfo(cultureName).TextInfo;
            return info.ToTitleCase(str);
        }
        public static string ToTitleInvariant(this string str)
        {
            var info = CultureInfo.InvariantCulture.TextInfo;
            return info.ToTitleCase(str);
        }
        public static string PadLeft(this string str, int totalWidth)
        {
            return str.PadLeft(totalWidth);
        }
        public static string PadLeft(this string str, int totalWidth, char paddingChar)
        {
            return str.PadLeft(totalWidth, paddingChar);
        }
        public static string PadRight(this string str, int totalWidth)
        {
            return str.PadRight(totalWidth);
        }
        public static string PadRight(this string str, int totalWidth, char paddingChar)
        {
            return str.PadRight(totalWidth, paddingChar);
        }
        public static string PadCenter(this string str, int totalWidth, bool padRight = false, bool truncate = false)
        {
            return PadCenter(str, totalWidth, Constants.SPACE, padRight, truncate);
        }
        public static string PadCenter(this string str, int totalWidth, char paddingChar, bool padRight = false, bool truncate = false)
        {
            int length = (totalWidth - str.Length) / 2;
            int surplus = totalWidth - str.Length - length;
            if (padRight)
            {
#if NET47_OR_GREATER || NETCOREAPP2_0_OR_GREATER
                // 割り切れないとき右寄せ指定の場合はスワップ
                (length, surplus) = (surplus, length);
#else
                // 割り切れないとき右寄せ指定の場合はスワップ(タプルが使えない時は昔ながらのやり方で)
                {
                    var tmp = length;
                    length = surplus;
                    surplus = tmp;
                }
#endif
            }
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(paddingChar);
            }
            if (truncate && str.Length > totalWidth)
            {
                // 文字列がtotalWidthより長い場合は切り詰め
#if NETCOREAPP2_1_OR_GREATER
                sb.Append(str.AsSpan().Slice(0, totalWidth));
#else
                // Spanが使えない時は無理しない
                sb.Append(str.Substring(0,totalWidth));
#endif
            }
            else
            {
                sb.Append(str);
            }
            for (int i = 0; i < surplus; i++)
            {
                sb.Append(paddingChar);
            }
            return sb.ToString();
        }
        public static string Format(this string str, params Variable[] args)
        {
            return StringFormatFunction.Format(str, args);
        }
        public static string String_Format(string str, params Variable[] args)
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
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        public static bool IsEmptyOrWhiteSpace(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        public static string Replace(this string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }
        public static string Replace(this string str, string oldvalue, string newValue, bool ignoreCase)
        {
#if NETCOREAPP2_0_OR_GREATER
            return str.Replace(oldvalue, newValue, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static string Replace(this string str, string oldvalue, string newValue, bool ignoreCase, bool considerCulture)
        {
#if NETCOREAPP2_0_OR_GREATER
            return str.Replace(oldvalue, newValue, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
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

            foreach (char c in str)
            {
                result.Add(c.ToString());
            }

            return result.ToArray();
        }
        public static string[] Split(this string str, string separator)
        {
            return str.Split(separator);
        }
        public static string[] SplitLines(this string str)
        {
            var result = str.Split('\n');
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = result[i].Trim('\r');
            }
            return result;
        }
        public static string Substring(this string str, int startIndex)
        {
            return str.Substring(startIndex);
        }
        public static string Substring(this string str, int startIndex, int length)
        {
            return str.Substring(startIndex, length);
        }
        public static bool Equals(this string str, string value)
        {
            return str.Equals(value);
        }
        public static bool Equals(this string str, string value, bool ignoreCase)
        {
            return str.Equals(value, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture);
        }
        public static bool Equals(this string str, string value, bool ignoreCase, bool considerCulture)
        {
            return str.Equals(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static string Repeat(this string str, int repeatCount)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < repeatCount; i++)
            {
                sb.Append(str);
            }

            return sb.ToString();
        }
        public static string Indent(this string str, int indentLevel, string indentChar = " ")
        {
            return Repeat(indentChar, indentLevel) + str;
        }
        public static byte[] GetBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        public static byte[] GetBytes(this string str, string charCode)
        {
            return Encoding.GetEncoding(charCode).GetBytes(str);
        }
        public static byte[] GetBytes(this string str, int codepage)
        {
            return Encoding.GetEncoding(codepage).GetBytes(str);
        }

        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this string str)
        {
            return str.Length;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int LengthInTextElements(this string str)
        {
            var info = new StringInfo(str);
            return info.LengthInTextElements;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Size(this string str)
        {
            return str.Length;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Count(this string str)
        {
            return str.Length;
        }
        #endregion
    }
}
