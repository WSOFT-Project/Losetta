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
    public void Number_Add(int x, int y)
    {
        Utils.TestExpression(x, Constants.PLUS, y, (x,y) => x + y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Sub(int x, int y)
    {
        Utils.TestExpression(x, Constants.MINUS, y, (x, y) => x - y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Times(int x, int y)
    {
        Utils.TestExpression(x, Constants.TIMES, y, (x, y) => x * y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Div(int x, int y)
    {
        Utils.TestExpression(x, Constants.DIV, y, (x, y) => x / y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Mod(int x, int y)
    {
        Utils.TestExpression(x, Constants.MOD, y, (x, y) => x % y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Pow(int x, int  y)
    {
        Utils.TestExpression(x, Constants.POW, y, (x, y) => Math.Pow(x, y));
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_GreaterThan(int x, int y)
    {
        Utils.TestExpression(x, Constants.GREATER, y, (x, y) => x > y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_GreaterEquals(int x, int y)
    {
        Utils.TestExpression(x, Constants.GREATER_EQ, y, (x, y) => x >= y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LessThan(int x, int y)
    {
        Utils.TestExpression(x, Constants.LESS, y, (x, y) => x < y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LessEquals(int x, int y)
    {
        Utils.TestExpression(x, Constants.LESS_EQ, y, (x, y) => x <= y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_LeftShift(int x, int y)
    {
        Utils.TestExpression(x, Constants.LEFT_SHIFT, y, (x, y) => (long)x << (int)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_RightShift(int x, int y)
    {
        Utils.TestExpression(x, Constants.RIGHT_SHIFT, y, (x, y) => (long)x >> (int)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_And(int x, int y)
    {
        Utils.TestExpression(x, '&', y, (x, y) => (long)x & (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Xor(int x, int y)
    {
        Utils.TestExpression(x, '^', y, (x, y) => (long)x ^ (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Or(int x, int y)
    {
        Utils.TestExpression(x, '|', y, (x, y) => (long)x | (long)y);
    }
    [TestCaseSource(nameof(NumberBinaryData))]
    public void Number_Ranges(int x, int y)
    {
        Utils.TestExpression(x, Constants.RANGE, y, (x, y) => new RangeStruct((int)x, (int)y));
    }
}
