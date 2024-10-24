namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Literal
{
    [TestCase("Hello,World", Description = "文字列リテラルが使える")]
    [TestCase("𩸽👨🏻‍🦱", Description = "マルチバイト文字も書ける")]
    public void StringLiteral(string str)
    {
        string code = $"\"{str}\";";
        string result = Alice.Execute<string>(code);
        Assert.That(result, Is.EqualTo(str));
    }
    public static Dictionary<char, string> EscapeChars = Constants.ESCAPE_CHARS;

    [TestCaseSource(nameof(EscapeChars))]
    public void StringLiteral_Escape(KeyValuePair<char, string> kvp)
    {
        char ch = kvp.Key;
        string expect = kvp.Value;
        if(expect[0] == Constants.QUOTE_IN_LITERAL)
        {
            expect = "\"";
        }
        if(expect[0] == Constants.QUOTE1_IN_LITERAL)
        {
            expect = "'";
        }
        string code = $"\"\\{ch}\";";
        
        string result = Alice.Execute<string>(code);
        Assert.That(result, Is.EqualTo(expect));
    }
    [TestCase(Description = "不明なエスケープ文字に対してエラーが出る")]
    public void StringLiteral_Escape_Unknown()
    {
        string str = @"\x";
        string code = $"\"{str}\";";
        
        Assert.That(() =>
        {
            Alice.Execute<string>(code);
        }, Throws.TypeOf(typeof(ScriptException)).With.Message.EqualTo($"`{str}`は認識できないエスケープ文字です"));
    }
}
