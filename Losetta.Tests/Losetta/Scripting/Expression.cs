namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Expression
{
    [TestCase(Description = "数値の足し算ができる")]
    public void Number_Add()
    {
        string code = "1 + 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(3));
    }
    [TestCase(Description = "数値の引き算ができる")]
    public void Number_Sub()
    {
        string code = "5 - 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(3));
    }
    [TestCase(Description = "数値の掛け算ができる")]
    public void Number_Time()
    {
        string code = "3 * 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(6));
    }
    [TestCase(Description = "数値の割り算ができる")]
    public void Number_Div()
    {
        string code = "4 / 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(2));
    }
    [TestCase(Description = "数値の剰余演算ができる")]
    public void Number_Mod()
    {
        string code = "5 % 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(1));
    }
    [TestCase(Description = "数値の冪乗演算ができる")]
    public void Number_Pow()
    {
        string code = "3 ** 2;";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(9));
    }
}
