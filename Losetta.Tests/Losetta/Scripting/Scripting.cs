namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[SetUpFixture]
public class Scripting
{
    [OneTimeSetUp]
    public void Init()
    {
        // 標準出力をテストコンソールにリダイレクトする
        Interpreter.Instance.OnOutput += (sender, e) =>
        {
            TestContext.WriteLine(e.Output);
        };
        // 例外は常にスローする
        ThrowErrorManager.ThrowError += (sender, e) =>
        {
            throw new ScriptException(e.Message, e.ErrorCode, e.Script, e.Exception);
        };
        // Alice.Runtimeを初期化する
        Runtime.Init();
    }
}
