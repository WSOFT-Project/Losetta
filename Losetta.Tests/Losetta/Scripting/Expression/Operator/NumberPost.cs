namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class NumberPost
{
    [TestCase(Description = "後置インクリメント演算子を使うと値が増加する")]
    public void Increment()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return((val++) + val);
        """;

        int result = TestUtils.Script.Execute<int>(code);
        Assert.That(result, Is.EqualTo(val++ + val));
    }
    [TestCase(Description = "後置デクリメント演算子を使うと値が増加する")]
    public void Decrement()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return((val--) + val);
        """;

        int result = TestUtils.Script.Execute<int>(code);
        Assert.That(result, Is.EqualTo(val-- + val));
    }
    [TestCase(Description = "後置単項Range演算子が動作する")]
    public void Range()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return val..;
        """;

        RangeStruct result = TestUtils.Script.Execute<RangeStruct>(code);
        Assert.That(result, Is.EqualTo(new RangeStruct(val)));
    }
}
