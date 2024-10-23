namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Statement
{
    [TestCase(Description = "Hello,Worldのコードを実行できる")]
    public void Hello_World()
    {
        string code = "print(\"Hello,World!\");";
        Assert.That(() =>
        {
            Alice.Execute(code);
        }, Throws.Nothing);
    }
}
