using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
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
        private static readonly string TEST_ID_VARNAME = Constants.USER_CANT_USE_VARIABLE_PREFIX + "Alice.Test.TestID";
        /// <summary>
        /// 可能であればテスト結果をテストコンテキストに報告します
        /// </summary>
        /// <param name="condition">okかどうか</param>
        /// <param name="testName">テスト名</param>
        /// <param name="script"></param>
        /// <exception cref="ScriptException"></exception>
        private static void ReportTest(bool condition, string testName, ParsingScript script)
        {
            if (script.TryGetVariable(TEST_ID_VARNAME, out ParserFunction func)) // テスト中であれば例外として出す
            {
                throw new ScriptException($"Alice.Test:{func.GetValue(script).As<string>()}:{(condition ? "OK" : "Not OK")}:{testName}", Exceptions.USER_DEFINED);
            }
        }
        public static void Ok(bool condition, string testName, [BindInfo] ParsingScript script)
        {
            ReportTest(condition, testName, script);
        }
        public static void Ok(Variable got, Variable excepted, DelegateObject comparer, string testName, [BindInfo] ParsingScript script)
        {
            bool condition = comparer.Invoke([got, excepted]).As<bool>();
            ReportTest(condition, testName, script);
        }
        public static void NotOk(bool condition, string testName, [BindInfo] ParsingScript script)
        {
            ReportTest(!condition, testName, script);
        }
        public static void Is(Variable expected, Variable actual, string testName, [BindInfo] ParsingScript script)
        {
            bool condition = Equals(expected, actual);
            ReportTest(condition, testName, script);
        }
        public static void IsNot(Variable notExpected, Variable actual, string testName, [BindInfo] ParsingScript script)
        {
            bool condition = !Equals(notExpected, actual);
            ReportTest(condition, testName, script);
        }
        public static void Test(int tests, [BindInfo] ParsingScript script)
        {
            string test_id = Guid.NewGuid().ToString();
            script.Variables.Add(TEST_ID_VARNAME, new ValueFunction(Variable.From(test_id))); // 秘密変数test_idを作成
            int testCount = 1;
            string testName = $"Alice.Test:{test_id}:";
            ThrowErrorEventhandler handler = (s, e) =>
            {
                if (e.ErrorCode == Exceptions.USER_DEFINED && e.Message.StartsWith(testName, StringComparison.Ordinal))
                {
                    string[] args = e.Message.Split(':');
                    if (args.Length == 4)
                    {
                        if (args[2] == "OK")
                        {
                            Console.WriteLine($"ok {testCount++} - {args[3]}");
                        }
                        else
                        {
                            Console.WriteLine($"not ok {testCount++} - {args[3]}");
                        }
                        e.Handled = true;
                    }
                }
            };
            ThrowErrorManager.Subscribe(handler);
            Console.WriteLine($"1..{tests}");
            script.ProcessBlock();
            ThrowErrorManager.ThrowError -= handler;
        }
    }
}