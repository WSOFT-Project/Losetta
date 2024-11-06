namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Others
{
    [TestCase(Description = "Hello,Worldのコードを実行できる")]
    public void Hello_World()
    {
        string code = "print(\"Hello,World!\");";
        Assert.That(() =>
        {
            TestUtils.Script.Execute<Variable>(code);
        }, Throws.Nothing);
    }
    [TestCase(Description = "よくあるAliceコードが一通り実行できる")]
    public void CommonScript()
    {
        string code = @"
        number a = 1;
        number b = 2;
        
        number add(number x, number y)
        {
            return x + y;
        }

        var c = add(a, b);
        print(c);

        return c + 2;
        ";
        
        Assert.That(TestUtils.Script.Execute<int>(code), Is.EqualTo(5));
    }
    [TestCase]
    public void Dictionary()
    {
        string code = @"
        dictionary dic = {""key1"": 1, ""key2"": 2};
        return dic;
        ";
        Assert.That(TestUtils.Script.Execute<Dictionary<string,int>>(code)["key1"], Is.EqualTo(1));
    }
}
