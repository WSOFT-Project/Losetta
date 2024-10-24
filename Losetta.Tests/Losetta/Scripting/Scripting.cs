namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[SetUpFixture]
public class Scripting
{
    [OneTimeSetUp]
    public void Init()
    {
        Interpreter.Instance.OnOutput += (sender, e) =>
        {
            TestContext.WriteLine(e.Output);
        };
        ThrowErrorManager.ThrowError += (sender, e) =>
        {
            throw new ScriptException(e.Message, e.ErrorCode, e.Script, e.Exception);
        };
        Runtime.Init();
    }
}
