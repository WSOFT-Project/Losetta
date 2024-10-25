using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Tests;

public static class TestUtils
{
    public static TestScript Script => new TestScript();
    private static TResult ExecExpression<TLeft, TRight, TResult>(TLeft x, string op, TRight y)
    {
        string code = $"{x} {op} {y};";
        return Script.Execute<TResult>(code);
    }
    private static TResult ExecExpression<TRight, TResult>(string op, TRight x)
    {
        string code = $"{op}{x};";
        return Script.Execute<TResult>(code);
    }
    private static TResult ExecExpression<TLeft, TResult>(TLeft x, string op)
    {
        string code = $"{x}{op};";
        return Script.Execute<TResult>(code);
    }
    /// <summary>
    /// 二項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TLeft">左辺の型</typeparam>
    /// <typeparam name="TRight">右辺の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">左辺の値</param>
    /// <param name="op">演算子</param>
    /// <param name="y">右辺の値</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TLeft, TRight, TResult>(TLeft x, string op, TRight y, Func<TLeft, TRight, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TLeft, TRight, TResult>(x, op, y), Is.EqualTo(testPredicate(x, y)));
    }
    /// <summary>
    /// 二項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TLeft">左辺の型</typeparam>
    /// <typeparam name="TRight">右辺の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">左辺の値</param>
    /// <param name="op">演算子</param>
    /// <param name="y">右辺の値</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TLeft, TRight, TResult>(TLeft x, char op, TRight y, Func<TLeft, TRight, TResult> testPredicate) => TestExpression(x, op.ToString(), y, testPredicate);

    /// <summary>
    /// 後置単項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TLeft">演算対象の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">演算対象の値</param>
    /// <param name="op">演算子</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TLeft, TResult>(TLeft x, string op, Func<TLeft, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TLeft, TResult>(x, op), Is.EqualTo(testPredicate(x)));
    }
    /// <summary>
    /// 後置単項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TLeft">演算対象の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">演算対象の値</param>
    /// <param name="op">演算子</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TLeft, TResult>(TLeft x, char op, Func<TLeft, TResult> testPredicate) => TestExpression(x, op.ToString(), testPredicate);

    /// <summary>
    /// 前置単項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TRight">演算対象の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">演算対象の値</param>
    /// <param name="op">演算子</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TRight, TResult>(string op, TRight x, Func<TRight, TResult> testPredicate)
    {
        Assert.That(ExecExpression<TRight, TResult>(op, x), Is.EqualTo(testPredicate(x)));
    }
    /// <summary>
    /// 前置単項演算のテストを行います。
    /// </summary>
    /// <typeparam name="TRight">演算対象の型</typeparam>
    /// <typeparam name="TResult">結果の型</typeparam>
    /// <param name="x">演算対象の値</param>
    /// <param name="op">演算子</param>
    /// <param name="testPredicate">対応するC#の演算</param>
    public static void TestExpression<TRight, TResult>(char op, TRight x, Func<TRight, TResult> testPredicate) => TestExpression(op.ToString(), x, testPredicate);

}