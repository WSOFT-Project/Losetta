using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Linq;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Test
    {
        public static void Init()
        {
            Alice.RegisterFunctions<TestFunctions>();
            NameSpace skipFuncs = new NameSpace("Alice.Test");

            skipFuncs.Add(new SkipFunction());
            NameSpaceManager.Add(skipFuncs);
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
        private static bool ShouldSkip(FunctionBaseEventArgs e, out string why)
        {
            var skipFunc = e.AttributeFunctions?.OfType<SkipFunction>().FirstOrDefault();
            why = skipFunc?.Why ?? string.Empty;
            if (skipFunc?.Comparer is not null)
            {
                if (skipFunc.Comparer.Invoke([]).As<bool>() == false)
                {
                    why = string.Empty; // 比較関数がfalseを返した場合はスキップしない
                    return false;
                }
            }
            return skipFunc is not null;
        }
        public static void Ok(bool condition, string testName, [BindInfo] ParsingScript script, [BindInfo] FunctionBaseEventArgs e)
        {
            if (ShouldSkip(e, out string why))
            {
                ReportTest(true, $"{testName} # SKIP because {why}", script);
                return;
            }
            ReportTest(condition, testName, script);
        }
        public static void Ok(Variable got, Variable excepted, DelegateObject comparer, string testName, [BindInfo] ParsingScript script, [BindInfo] FunctionBaseEventArgs e)
        {
            if (ShouldSkip(e, out string why))
            {
                ReportTest(true, $"{testName} # SKIP because {why}", script);
                return;
            }
            bool condition = comparer.Invoke([got, excepted]).As<bool>();
            ReportTest(condition, testName, script);
        }
        public static void NotOk(bool condition, string testName, [BindInfo] ParsingScript script, [BindInfo] FunctionBaseEventArgs e)
        {
            if (ShouldSkip(e, out string why))
            {
                ReportTest(true, $"{testName} # SKIP because {why}", script);
                return;
            }
            ReportTest(!condition, testName, script);
        }
        public static void Is(Variable expected, Variable actual, string testName, [BindInfo] ParsingScript script, [BindInfo] FunctionBaseEventArgs e)
        {
            if (ShouldSkip(e, out string why))
            {
                ReportTest(true, $"{testName} # SKIP because {why}", script);
                return;
            }
            bool condition = Equals(expected, actual);
            ReportTest(condition, testName, script);
        }
        public static void IsNot(Variable notExpected, Variable actual, string testName, [BindInfo] ParsingScript script, [BindInfo] FunctionBaseEventArgs e)
        {
            if (ShouldSkip(e, out string why))
            {
                ReportTest(true, $"{testName} # SKIP because {why}", script);
                return;
            }
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
    public class SkipFunction : FunctionBase
    {
        public SkipFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + "skip";
            Run += SkipFunction_Run;
        }
        private void SkipFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0 && e.Args[0].Is(out string? arg))
            {
                Why = arg ?? string.Empty;
                e.Return = Variable.From(true);
                if (e.Args[1].Is(out DelegateObject? cmp))
                {
                    Comparer = cmp;
                }
                return;
            }
            throw new ScriptException("Alice.Test.Skip: 引数の数が違います", Exceptions.INVALID_ARGUMENT);
        }
        internal string Why { get; set; } = string.Empty;
        internal DelegateObject Comparer { get; set; } = null;
    }
}