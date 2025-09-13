namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using NUnit.Framework;

[TestFixture]
public class TypeOperator
{
    [TestCase("123", "number", true, Description = "numberリテラルはnumber型")]
    [TestCase("\"abc\"", "string", true, Description = "stringリテラルはstring型")]
    [TestCase("true", "bool", true, Description = "boolリテラルはbool型")]
    [TestCase("123", "string", false, Description = "numberはstring型ではない")]
    [TestCase("\"abc\"", "number", false, Description = "stringはnumber型ではない")]
    public void Is_Primitive(string literal, string typeName, bool expected)
    {
        string code = $"{literal} is {typeName};";
        bool result = TestUtils.Script.Execute<bool>(code);
        Assert.That(result, Is.EqualTo(expected));
    }

    [TestCase("123", "number", false, Description = "numberをnumberに変換")]
    [TestCase("\"abc\"", "number", true, Description = "数値ではなさそうな文字列はnumberにならない")]
    [TestCase("123", "number", false, Description = "数値っぽい文字列はnumberになる")]
    [TestCase("123", "string", false, Description = "数値は文字列になる")]
    [TestCase("\"abc\"", "string", false, Description = "stringをstringに変換")]
    public void As_Primitive(string literal, string typeName, bool expectedNull)
    {
        string code = $"{literal} as {typeName};";
        var result = TestUtils.Script.Execute<Variable>(code);
        Assert.That(result.IsNull(), Is.EqualTo(expectedNull));
    }

    [TestCase("\"abc\"", "number", Description = "型キャストに失敗した場合はnull")]
    public void As_Failed_ReturnsNull(string literal, string typeName)
    {
        string code = $"{literal} as {typeName};";
        var result = TestUtils.Script.Execute<Variable>(code);
        Assert.That(result.IsNull(), Is.True);
    }

    [TestCase(Description = "is not 演算子は否定判定")]
    public void IsNot_Primitive()
    {
        string code = "123 is not string;";
        bool result = TestUtils.Script.Execute<bool>(code);
        Assert.That(result, Is.True);
    }
}
