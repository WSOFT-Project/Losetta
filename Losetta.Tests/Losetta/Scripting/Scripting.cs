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
        Runtime.Init();
    }
}
