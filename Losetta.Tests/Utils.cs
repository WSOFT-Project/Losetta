namespace AliceScript.Tests;

public static class Utils
{
    private static TResult ExecExpression<TLeft, TRight, TResult>(TLeft x, string op, TRight y)
    {
        string code = $"{x} {op} {y};";
        return Alice.Execute<TResult>(code);
    }
    public static void TestExpression<TLeft, TRight, TResult>(TLeft x, string op, TRight y, Func<TLeft, TRight, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TLeft, TRight, TResult>(x, op, y), Is.EqualTo(testPredicate(x, y)));
    }
    public static void TestExpression<TLeft, TRight, TResult>(TLeft x, char op, TRight y, Func<TLeft, TRight, TResult> testPredicate) => TestExpression(x, op.ToString(), y, testPredicate);

    private static TResult ExecExpression<TLeft,TResult>(TLeft x, string op)
    {
        string code = $"{x}{op};";
        return Alice.Execute<TResult>(code);
    }
    public static void TestExpression<TLeft, TResult>(TLeft x, string op, Func<TLeft, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TLeft, TResult>(x, op), Is.EqualTo(testPredicate(x)));
    }
    public static void TestExpression<TLeft, TResult>(TLeft x, char op, Func<TLeft, TResult> testPredicate) => TestExpression(x, op.ToString(), testPredicate);

    private static TResult ExecExpression<TRight, TResult>(string op, TRight x)
    {
        string code = $"{op}{x};";
        return Alice.Execute<TResult>(code);
    }
    public static void TestExpression<TRight, TResult>(string op, TRight x, Func<TRight, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TRight, TResult>(x, op), Is.EqualTo(testPredicate(x)));
    }
    public static void TestExpression<TRight, TResult>(char op, TRight x, Func<TRight, TResult> testPredicate) => TestExpression(op.ToString(), x, testPredicate);
}