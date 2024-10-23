using AliceScript.Binding;
using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static int CompareTo(this string str, string item)
        {
            return string.Compare(str, item, StringComparison.Ordinal);
        }
        public static int CompareTo(this string str, string item, bool ignoreCase)
        {
            return string.Compare(str, item, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public static int CompareTo(this string str, string item, bool ignoreCase, bool considerCulture)
        {
            return string.Compare(str, item, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }
        public static int CompareTo(this string str, string item, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
            var options = GetCompareOptions(ignoreCase, ignoreNonSpace, ignoreWidth, ignoreKanaType);
            return (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo.Compare(str, item, options);
        }
        private static CompareOptions GetCompareOptions(bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
            CompareOptions options = CompareOptions.None;
            options |= ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            options |= ignoreNonSpace ? CompareOptions.IgnoreNonSpace : CompareOptions.None;
            options |= ignoreSymbols ? CompareOptions.IgnoreSymbols : CompareOptions.None;
            options |= ignoreWidth ? CompareOptions.IgnoreWidth : CompareOptions.None;
            options |= ignoreKanaType ? CompareOptions.IgnoreKanaType : CompareOptions.None;
            return options;
        }
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
            return str.StartsWith(value, considerCulture ? ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture : ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture);
        }
        public static bool StartsWith(this string str, string item, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreKanaType = false)
        {
            CompareOptions options = CompareOptions.None;
            options |= ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
            options |= ignoreNonSpace ? CompareOptions.IgnoreNonSpace : CompareOptions.None;
            options |= ignoreSymbols ? CompareOptions.IgnoreSymbols : CompareOptions.None;
            options |= ignoreKanaType ? CompareOptions.IgnoreKanaType : CompareOptions.None;

            return (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo.IsPrefix(str, item, options);
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
        public static bool EndsWith(this string str, string item, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
            CompareOptions options = GetCompareOptions(ignoreCase, ignoreNonSpace, ignoreSymbols, ignoreWidth, ignoreKanaType);

            return (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo.IsSuffix(str, item, options);
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
        public static bool Contains(this string str, string item, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
            CompareOptions options = GetCompareOptions(ignoreCase, ignoreNonSpace, ignoreSymbols, ignoreWidth, ignoreKanaType);
            return (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo.IndexOf(str, item, options) > 0;
        }
        public static string ToUpper(this string str)
        {
            return str.ToUpper();
        }
        public static string ToUpper(this string str, string cultureName)
        {
            var culture = cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);
            return culture.TextInfo.ToUpper(str);
        }
        public static string ToUpperInvariant(this string str)
        {
            return str.ToUpperInvariant();
        }
        public static string ToLower(this string str)
        {
            return str.ToLower();
        }
        public static string ToLower(this string str, string cultureName)
        {
            var culture = cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);
            return culture.TextInfo.ToLower(str);
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
        public static string ToTitleCase(this string str, string cultureName)
        {
            var culture = cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName);
            return culture.TextInfo.ToTitleCase(str);
        }
        public static string ToTitleCaseInvariant(this string str)
        {
            var info = CultureInfo.InvariantCulture.TextInfo;
            return info.ToTitleCase(str);
        }
        public static string PadLeft(this string str, int totalWidth, bool truncate = false)
        {
            if (truncate && str.Length > totalWidth)
            {
                //切り詰めが必要
                return str.Substring(0, totalWidth);
            }
            return str.PadLeft(totalWidth);
        }
        public static string PadLeft(this string str, int totalWidth, char paddingChar, bool truncate = false)
        {
            if (truncate && str.Length > totalWidth)
            {
                //切り詰めが必要
                return str.Substring(0, totalWidth);
            }
            return str.PadLeft(totalWidth, paddingChar);
        }
        public static string PadRight(this string str, int totalWidth, bool truncate = false)
        {
            if (truncate && str.Length > totalWidth)
            {
                //切り詰めが必要
                return str.Substring(0, totalWidth);
            }
            return str.PadRight(totalWidth);
        }
        public static string PadRight(this string str, int totalWidth, char paddingChar, bool truncate = false)
        {
            if (truncate && str.Length > totalWidth)
            {
                //切り詰めが必要
                return str.Substring(0, totalWidth);
            }
            return str.PadRight(totalWidth, paddingChar);
        }
        public static string PadCenter(this string str, int totalWidth, bool padLeft = false, bool truncate = false)
        {
            return PadCenter(str, totalWidth, Constants.SPACE, padLeft, truncate);
        }
        public static string PadCenter(this string str, int totalWidth, char paddingChar, bool padLeft = false, bool truncate = false)
        {
            if (str.Length > totalWidth)
            {
                //切り詰めが必要なら行う
                if (truncate)
                {
                    return str.Substring(0, totalWidth);
                }
                else
                {
                    return str;
                }
            }
            int length = (totalWidth - str.Length) / 2;
            int surplus = totalWidth - str.Length - length;
            if (!padLeft)
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
            sb.Append(str);
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
        public static string Replace(this string str, char oldChar, char newChar)
        {
            return str.Replace(oldChar, newChar);
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
        public static string Replace(this string str, string oldValue, string newValue, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
#if NET5_0_OR_GREATER
            CompareOptions options = GetCompareOptions(ignoreCase, ignoreNonSpace, ignoreSymbols, ignoreWidth, ignoreKanaType);

            var comp = (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo;

            ReadOnlySpan<char> source = str.AsSpan();
            string result = str;
            int diff = 0;
            int pointer;
            while((pointer = comp.IndexOf(source, oldValue, options, out int matchLength)) >= 0)
            {
                result = ReplaceAt(result, diff + pointer, matchLength, newValue);
                source = source.Slice(pointer + matchLength);
                diff += matchLength + pointer;
            }
            return result;
#else
            throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static string ReplaceAt(this string str, int index, char newChar)
        {
            char[] chars = str.ToCharArray();
            chars[index] = newChar;
            return new string(chars);
        }
        public static string ReplaceAt(this string str, int index, string replacement)
        {
            return ReplaceAt(str, index, replacement.Length, replacement);
        }
        public static string ReplaceAt(this string str, int index, int length, string replacement)
        {
            if (index < str.Length)
            {
                return str.Remove(index, Math.Min(length, str.Length - index)).Insert(index, replacement);
            }
            throw new ArgumentOutOfRangeException("index", "指定されたインデックスが文字列の範囲外です");
        }
        public static string ReplaceLineEndings(this string str)
        {
#if NET6_0_OR_GREATER
            return str.ReplaceLineEndings();
#else
            return ReplaceLineEndings(str, Environment.NewLine);
#endif
        }
        public static string ReplaceLineEndings(this string str, string replacementText)
        {
#if NET6_0_OR_GREATER
            return str.ReplaceLineEndings(replacementText);
#else
            if (str == null || replacementText == null)
            {
                return null;
            }

            StringBuilder result = new StringBuilder(str.Length);
            int length = str.Length;
            for (int i = 0; i < length; i++)
            {
                char currentChar = str[i];

                if (currentChar == '\u000D')
                {
                    if (i + 1 < length && str[i + 1] == '\u000A')
                    {
                        // CRLF
                        result.Append(replacementText);
                        i++; // \nを飛ばす
                    }
                    else
                    {
                        // CR
                        result.Append(replacementText);
                    }
                }
                else if (currentChar == '\u000A' || currentChar == '\u0085' || currentChar == '\u2028' || currentChar == '\u000C' || currentChar == '\u2029')
                {
                    result.Append(replacementText);
                }
                else
                {
                    result.Append(currentChar);
                }
            }

            return result.ToString();
#endif
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
            return str.Split(separator, StringSplitOptions.None);
        }
        public static string[] Split(this string str, char sep)
        {
            return str.Split(sep);
        }
        public static string[] SplitLines(this string str)
        {
            return ReplaceLineEndings(str, "\n").Split('\n');
        }
        public static List<string> Chunk(this string str, int size)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < str.Length; i += size)
            {
                if (i + size > str.Length)
                {
                    result.Add(str.Substring(i));
                }
                else
                {
                    result.Add(str.Substring(i, size));
                }
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
        public static bool Equals(this string str, string item, string cultureName, bool ignoreCase = false, bool ignoreNonSpace = false, bool ignoreSymbols = false, bool ignoreWidth = false, bool ignoreKanaType = false)
        {
            CompareOptions options = GetCompareOptions(ignoreCase, ignoreNonSpace, ignoreSymbols, ignoreWidth, ignoreKanaType);

            return (cultureName == null ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfo(cultureName)).CompareInfo.Compare(str, item, options) == 0;
        }
        public static string Repeat(this string str, int repeatCount)
        {
            if (repeatCount <= 0)
            {
                return string.Empty;
            }
#if NET6_0_OR_GREATER
            DefaultInterpolatedStringHandler sh = new DefaultInterpolatedStringHandler(str.Length * repeatCount, 0);
            for (int i = 0; i < repeatCount; i++)
            {
                sh.AppendLiteral(str);
            }
            return sh.ToStringAndClear();
#else
            StringBuilder sb = new StringBuilder(str.Length * repeatCount);
                    for(int i = 0;i < repeatCount; i++)
                    {
                        sb.Append(str);
                    }
            return sb.ToString();
#endif
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
        public static int CodePointAt(this string str, int index)
        {
            return str[index];
        }
        public static int CodePointAt(this string str, int index, bool utf32)
        {
            if (utf32)
            {
                return char.ConvertToUtf32(str, index);
            }
            else
            {
                return str[index];
            }
        }
        public static IEnumerator<object> GetEnumerator(this string str)
        {
            IEnumerator<object> CastEnumerator()
            {
                var source = str.GetEnumerator();
                while (source.MoveNext())
                {
                    yield return source.Current;
                }
            }
            return CastEnumerator();
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
