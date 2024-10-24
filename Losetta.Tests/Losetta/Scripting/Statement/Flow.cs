namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Flow
{
    [TestCase(true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行できる")]
    [TestCase(true, ExpectedResult = 0, Description = "条件が偽の場合にif文の本文がスキップできる")]
    public int If(bool condition)
    {
        string code = $$"""
        if({{condition}})
        {
            return 1;
        }
        return 0;
        """;
        
        return Alice.Execute<int>(code);
    }
    [TestCase(true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行でき、else文がスキップできる")]
    [TestCase(true, ExpectedResult = 2, Description = "条件が偽の場合にif文の本文がスキップでき、else文が実行できる")]
    public int If_Else(bool condition)
    {
        string code = $$"""
        if({{condition}})
        {
            return 1;
        }
        else
        {
            return 2;
        }
        return 0;
        """;

        return Alice.Execute<int>(code);
    }
    [TestCase(true, true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行でき、else if文がスキップできる")]
    [TestCase(true, false, ExpectedResult = 1, Description = "条件が偽の場合にif文の本文がスキップでき、else if文がスキップできる")]
    [TestCase(false, true, ExpectedResult = 2, Description = "条件が真の場合にif文の本文が実行でき、else if文がスキップできる")]
    [TestCase(false, false, ExpectedResult = 0, Description = "条件が偽の場合にif文の本文がスキップでき、else if文がスキップできる")]
    public int If_ElseIf(bool condA, bool condB)
    {
        string code = $$"""
        if({{condA}})
        {
            return 1;
        }
        else if({{condB}})
        {
            return 2;
        }
        return 0;
        """;

        return Alice.Execute<int>(code);
    }
}
