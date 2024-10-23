using AliceScript.Binding;
using AliceScript.Functions;
using System;
using System.Text;

namespace AliceScript.NameSpaces.Core
{
    public partial class CoreFunctions
    {
        public static string ToString(this byte[] data, string charCode)
        {
            return Encoding.GetEncoding(charCode).GetString(data);
        }
        public static string ToString(this byte[] data, int codePage)
        {
            return Encoding.GetEncoding(codePage).GetString(data);
        }
        public static string ToBase64(this byte[] data)
        {
            return System.Convert.ToBase64String(data);
        }
        public static string ToBase64(this byte[] data, int offcset, int length)
        {
            return System.Convert.ToBase64String(data, offcset, length);
        }
        public static string ToBase64(this byte[] data, int offcset, int length, bool insertLineBrakes)
        {
            return System.Convert.ToBase64String(data, offcset, length, insertLineBrakes ? Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None);
        }
        public static string ToBase64(this byte[] data, bool insertLineBrakes)
        {
            return System.Convert.ToBase64String(data, 0, data.Length, insertLineBrakes ? Base64FormattingOptions.InsertLineBreaks : Base64FormattingOptions.None);
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this byte[] data)
        {
            return data.Length;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Size(this byte[] data)
        {
            return data.Length;
        }
    }
}
