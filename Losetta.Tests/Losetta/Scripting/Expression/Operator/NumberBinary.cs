namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class NumberBinary
{
    private static T Evaluate<T>(double x, string op, double y)
    {
        string code = $"{x} {op} {y};";
        return Alice.Execute<T>(code);
    }
    private static void TestEvaluate<T>(double x, string op, double y, Func<double, double, T> testPredicate)
    {
        Assert.That(Evaluate<T>(x, op, y),
            Is.EqualTo(testPredicate(x, y)));
    }
    private static void TestEvaluate<T>(double x, char op, double y, Func<double, double, T> testPredicate)
    {
        TestEvaluate(x, op.ToString(), y, testPredicate);
    }
    [TestCase(Description = "数値の足し算ができる")]
    public void Number_Add()
    {
        TestEvaluate(5, Constants.PLUS, 2, (x, y) => x + y);
    }
    [TestCase(Description = "数値の引き算ができる")]
    public void Number_Sub()
    {
        TestEvaluate(5, Constants.MINUS, 2, (x, y) => x - y);
    }
    [TestCase(Description = "数値の掛け算ができる")]
    public void Number_Time()
    {
        TestEvaluate(3, Constants.TIMES, 2, (x, y) => x * y);
    }
    [TestCase(Description = "数値の割り算ができる")]
    public void Number_Div()
    {
        TestEvaluate(4, Constants.DIV, 2, (x, y) => x / y);
    }
    [TestCase(Description = "数値の剰余演算ができる")]
    public void Number_Mod()
    {
        TestEvaluate(5, Constants.MOD, 2, (x, y) => x % y);
    }
    [TestCase(Description = "数値の冪乗演算ができる")]
    public void Number_Pow()
    {
        TestEvaluate(3, Constants.POW, 2, Math.Pow);
    }
    [TestCase(Description = "xがyより大きいか判定できる")]
    public void Number_GreaterThan()
    {
        TestEvaluate(5, Constants.GREATER, 2, (x, y) => x > y);
    }
    [TestCase(Description = "xがyより大きいか同値か判定できる")]
    public void Number_GreaterEquals()
    {
        TestEvaluate(5, Constants.GREATER_EQ, 2, (x, y) => x >= y);
    }
    [TestCase(Description = "xがyより小さいか判定できる")]
    public void Number_LessThan()
    {
        TestEvaluate(3, Constants.LESS, 5, (x, y) => x < y);
    }
    [TestCase(Description = "xがyより小さいか同値か判定できる")]
    public void Number_LessEquals()
    {
        TestEvaluate(3, Constants.LESS_EQ, 5, (x, y) => x <= y);
    }
    [TestCase(Description = "xをyだけ左シフトできる")]
    public void Number_LeftShift()
    {
        TestEvaluate(5, Constants.LEFT_SHIFT, 2, (x, y) => (long)x << (int)y);
    }
    [TestCase(Description = "xをyだけ右シフトできる")]
    public void Number_RightShift()
    {
        TestEvaluate(5, Constants.RIGHT_SHIFT, 2, (x, y) => (long)x >> (int)y);
    }
    [TestCase]
    public void Number_And()
    {
        TestEvaluate(5, '&', 2, (x, y) => (long)x & (long)y);
    }
    [TestCase]
    public void Number_Xor()
    {
        TestEvaluate(5, '^', 2, (x, y) => (long)x ^ (long)y);
    }
    [TestCase]
    public void Number_Or()
    {
        TestEvaluate(5, '|', 2, (x, y) => (long)x | (long)y);
    }
    [TestCase]
    public void Number_Ranges()
    {
        TestEvaluate(2, Constants.RANGE, 5, (x, y) => new RangeStruct((int)x, (int)y));
    }
}
