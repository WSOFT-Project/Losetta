namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class NumberBinary
{
    public static object[][] NumberBinaryData = new object[][]
    {
        new object[] { 10, 0 },
        new object[] { 1, 9 },
        new object[] { 2, 8 },
        new object[] { 3, 7 },
        new object[] { 4, 6 },
        new object[] { 5, 5 },
        new object[] { 6, 4 },
        new object[] { 7, 3 },
        new object[] { 8, 2 },
        new object[] { 9, 1 },
        new object[] { 10, 0 },
    };
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Add(double x, double y)
    {
        Utils.TestExpression(x, Constants.PLUS, y, (x,y) => x + y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Sub(double x, double y)
    {
        Utils.TestExpression(x, Constants.MINUS, y, (x, y) => x - y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Times(double x, double y)
    {
        Utils.TestExpression(x, Constants.TIMES, y, (x, y) => x * y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Div(double x, double y)
    {
        Utils.TestExpression(x, Constants.DIV, y, (x, y) => x / y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Mod(double x, double y)
    {
        Utils.TestExpression(x, Constants.MOD, y, (x, y) => x % y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Pow(double x, double  y)
    {
        Utils.TestExpression(x, Constants.POW, y, (x, y) => Math.Pow(x, y));
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_GreaterThan(double x, double y)
    {
        Utils.TestExpression(x, Constants.GREATER, y, (x, y) => x > y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_GreaterEquals(double x, double y)
    {
        Utils.TestExpression(x, Constants.GREATER_EQ, y, (x, y) => x >= y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LessThan(double x, double y)
    {
        Utils.TestExpression(x, Constants.LESS, y, (x, y) => x < y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LessEquals(double x, double y)
    {
        Utils.TestExpression(x, Constants.LESS_EQ, y, (x, y) => x <= y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LeftShift(double x, double y)
    {
        Utils.TestExpression(x, Constants.LEFT_SHIFT, y, (x, y) => (long)x << (int)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_RightShift(double x, double y)
    {
        Utils.TestExpression(x, Constants.RIGHT_SHIFT, y, (x, y) => (long)x >> (int)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_And(double x, double y)
    {
        Utils.TestExpression(x, '&', y, (x, y) => (long)x & (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Xor(double x, double y)
    {
        Utils.TestExpression(x, '^', y, (x, y) => (long)x ^ (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Or(double x, double y)
    {
        Utils.TestExpression(x, '|', y, (x, y) => (long)x | (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Ranges(double x, double y)
    {
        Utils.TestExpression(x, Constants.RANGE, y, (x, y) => new RangeStruct((int)x, (int)y));
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Equals(double x, double y)
    {
        Utils.TestExpression(x, Constants.EQUAL, y, (x, y) => x == y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_NotEquals(double x, double y)
    {
        Utils.TestExpression(x, Constants.NOT_EQUAL, y, (x, y) => x != y);
    }
}
