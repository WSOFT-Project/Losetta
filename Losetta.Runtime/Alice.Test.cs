using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using System;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Test
    {
        public static void Init()
        {
            Alice.RegisterFunctions<TestFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.Test")]
    internal sealed class TestFunctions
    {
        public static void Ok(bool condition, string testName)
        {
            if (!condition)
            {
                throw new Exception($"Test failed: {testName}");
            }
        }
        public static void Ok(Variable got, Variable excepted, DelegateObject comparer, string testName)
        {
            if (!comparer.Invoke([got, excepted]).As<bool>())
            {
                throw new Exception($"Test failed: {testName}. Expected: {excepted}, Actual: {got}");
            }
        }
        public static void NotOk(bool condition, string testName)
        {
            if (condition)
            {
                throw new Exception($"Test failed: {testName}");
            }
        }
        public static void Is(Variable expected, Variable actual, string testName)
        {
            if (!Variable.Equals(expected, actual))
            {
                throw new Exception($"Test failed: {testName}. Expected: {expected}, Actual: {actual}");
            }
        }
        public static void IsNot(Variable notExpected, Variable actual, string testName)
        {
            if (Variable.Equals(notExpected, actual))
            {
                throw new Exception($"Test failed: {testName}. Not Expected: {notExpected}, Actual: {actual}");
            }
        }

    }
}