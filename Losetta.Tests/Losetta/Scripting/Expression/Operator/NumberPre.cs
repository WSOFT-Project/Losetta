namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class NumberPre
{
    [TestCase(Description = "プラス演算子を使っても変わらない")]
    public void Number_Plus()
    {
        int val = 123;
        string code = $"+{val};";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(val));
    }
    [TestCase(Description = "マイナス演算子を使うと符号が反転する")]
    public void Number_Minus()
    {
        int val = 123;
        string code = $"-{val};";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(val * -1));
    }
    [TestCase(Description = "論理反転演算子を使うと論理が反転する")]
    public void Number_BitwiseNot()
    {
        int val = 123;
        string code = $"~{val};";

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(~ val));
    }
    [TestCase(Description = "前置インクリメント演算子を使うと値が増加する")]
    public void Number_Increment()
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
    public void Number_Decrement()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return --val + val;
        """;

        int result = Alice.Execute<int>(code);
        Assert.That(result, Is.EqualTo(--val + val));
    }
    [TestCase(Description = "前置単項Range演算子が動作する")]
    public void Number_Range()
    {
        int val = 123;
        string code = $"""
        number val = {val};
        
        return ..val;
        """;

        RangeStruct result = Alice.Execute<RangeStruct>(code);
        Assert.That(result, Is.EqualTo(new RangeStruct(0, val)));
    }
}
