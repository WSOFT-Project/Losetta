namespace AliceScript.Tests.Losetta.Scripting.Expression.Operator;

using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class BooleanBinary
{
    public static object[][] BooleanCombinations = new object[][]
    {
        new object[] { true, true },
        new object[] { true, false },
        new object[] { false, true },
        new object[] { false, false },
    };
    [TestCaseSource(nameof(BooleanCombinations))]
    public void And(bool a, bool b)
    {
        Utils.TestExpression(a, "&", b, (x, y) => x & y);
    }
    [TestCaseSource(nameof(BooleanCombinations))]
    public void And_Short(bool a, bool b)
    {
        Utils.TestExpression(a, "&&", b, (x, y) => x && y);
    }
    [TestCaseSource(nameof(BooleanCombinations))]
    public void Or(bool a, bool b)
    {
        Utils.TestExpression(a, "|", b, (x, y) => x | y);
    }
    [TestCaseSource(nameof(BooleanCombinations))]
    public void Or_Short(bool a, bool b)
    {
        Utils.TestExpression(a, "||", b, (x, y) => x || y);
    }
    [TestCaseSource(nameof(BooleanCombinations))]
    public void Xor(bool a, bool b)
    {
        Utils.TestExpression(a, "^", b, (x, y) => x ^ y);
    }
}
