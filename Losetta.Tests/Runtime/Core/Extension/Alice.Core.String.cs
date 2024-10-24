namespace AliceScript.Tests.Running.Core.Extension;

using AliceScript.NameSpaces.Core;
using NUnit.Framework;

[TestFixture]
public class StringFunctions
{
    [TestCase(Description = "全角と半角、かなとカナ混在の文字列を置換できている")]
    public void String_Replace()
    {
        string source = "こんにちは〜コンニチハ〜ｺﾝﾆﾁﾊ〜";
        string oldValue = "こんにちは";
        string newValue = "こんばんは";

        string replaced = CoreFunctions.Replace(source, oldValue, newValue, null, false, false, false, true, true);
        
        Assert.That(replaced, Is.EqualTo("こんばんは〜こんばんは〜こんばんは〜"));
    }
    [TestCase("abc", 5, false, ExpectedResult = " abc ", Description = "文字列を中央揃えできる")]
    [TestCase("ab", 5, false, ExpectedResult = "  ab ", Description = "偶数の場合、右に寄せる")]
    [TestCase("ab", 5, true, ExpectedResult = " ab  ", Description = "偶数の場合、左に寄せる")]
    public string String_PadCenter(string str, int totalWidth, bool padLeft)
    {
        return CoreFunctions.PadCenter(str, totalWidth, padLeft);
    }
    [TestCase("abc", 2, ExpectedResult = "abcabc", Description = "文字列を繰り返せる")]
    [TestCase("abc", 0, ExpectedResult = "", Description = "0回の時、空文字列を返す")]
    public string String_Repeat(string str, int count)
    {
        return CoreFunctions.Repeat(str, count);
    }
}
