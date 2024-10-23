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
}
