namespace AliceScript.Tests.Losetta.Unit;

using NUnit.Framework;

[TestFixture]
public class Variables
{
    [TestCase(42, Description = "int型で作成した変数をint型に変換できる")]
    [TestCase(4.2, Description = "double型で作成した変数をdouble型に変換できる")]
    public static void ConvertTo<T>(T val)
    {
        Variable v = new Variable(val);
        Assert.That(v.ConvertTo(typeof(T)), Is.EqualTo(val));
    }
}
