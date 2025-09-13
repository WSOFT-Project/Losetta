namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class NumberPre
{
    [TestCase(123, Description = "プラス演算子を使っても変わらない")]
    [TestCase(-123, Description = "プラス演算子を使っても変わらない")]
    public void Plus(int val)
    {
        TestUtils.TestExpression(Constants.PLUS, val, x => +x);
    }
    [TestCase(123, Description = "マイナス演算子を使うと符号が反転する")]
    public void Minus(int val)
    {
        TestUtils.TestExpression(Constants.MINUS, val, x => -x);
    }
    [TestCase(123, Description = "論理反転演算子を使うと論理が反転する")]
    [TestCase(-123, Description = "論理反転演算子を使うと論理が反転する")]
    public void BitwiseNot(int val)
    {
        TestUtils.TestExpression(Constants.BITWISE_NOT, val, x => ~x);
    }
    [TestCase(123, Description = "前置単項Range演算子が動作する")]
    public void Range(int val)
    {
        TestUtils.TestExpression(Constants.RANGE, val, x => new RangeStruct(0, val));
    }
    [TestCase(Description = "前置インクリメント演算子を使うと値が増加する")]
    [Ignore("Temporary: fails on current implementation")]
    public void Increment()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return ++val + val;
        """;

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(++val + val));
    }
    [TestCase(Description = "前置デクリメント演算子を使うと値が増加する")]
    [Ignore("Temporary: fails on current implementation")]
    public void Decrement()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return --val + val;
        """;

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(--val + val));
    }
}
