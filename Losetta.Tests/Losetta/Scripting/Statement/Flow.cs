namespace AliceScript.Tests.Losetta.Scripting;

using NUnit.Framework;

[TestFixture]
public class Flow
{
    [TestCase(true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行できる")]
    [TestCase(false, ExpectedResult = 0, Description = "条件が偽の場合にif文の本文がスキップできる")]
    public int If(bool condition)
    {
        string code = $$"""
        if({{nameof(condition)}})
        {
            return 1;
        }
        return 0;
        """;

        return TestUtils.Script.WithVariables([(nameof(condition), condition)]).Execute<int>(code);
    }
    [TestCase(true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行でき、else文がスキップできる")]
    [TestCase(false, ExpectedResult = 2, Description = "条件が偽の場合にif文の本文がスキップでき、else文が実行できる")]
    public int If_Else(bool condition)
    {
        string code = $$"""
        if({{nameof(condition)}})
        {
            return 1;
        }
        else
        {
            return 2;
        }
        return 0;
        """;

        return TestUtils.Script.WithVariables([(nameof(condition), condition)]).Execute<int>(code);
    }
    [TestCase(true, true, ExpectedResult = 1, Description = "条件が真の場合にif文の本文が実行でき、else if文がスキップできる")]
    [TestCase(true, false, ExpectedResult = 1, Description = "条件が偽の場合にif文の本文がスキップでき、else if文がスキップできる")]
    [TestCase(false, true, ExpectedResult = 2, Description = "条件が真の場合にif文の本文が実行でき、else if文がスキップできる")]
    [TestCase(false, false, ExpectedResult = 0, Description = "条件が偽の場合にif文の本文がスキップでき、else if文がスキップできる")]
    public int If_ElseIf(bool condA, bool condB)
    {
        string code = $$"""
        if({{nameof(condA)}})
        {
            return 1;
        }
        else if({{nameof(condB)}})
        {
            return 2;
        }
        return 0;
        """;

        return TestUtils.Script.WithVariables([(nameof(condA), condA), (nameof(condB), condB)]).Execute<int>(code);
    }
    [TestCase(15, ExpectedResult = 15, Description = "for文が指定回数ループしている")]
    [TestCase(150, ExpectedResult = 150, Description = "for文が指定回数ループしている(数が大きくても平気)")]
    [TestCase(0, ExpectedResult = 0, Description = "0回ループのときに本文がスキップできる")]
    [TestCase(-5, ExpectedResult = 0, Description = "負の回数ループのときに本文がスキップできる")]
    public int For_Common(int count)
    {
        string code = $$"""
        number sum = 0;
        for(number i = 0; i < {{nameof(count)}}; i++)
        {
            sum += 1;
        }
        return sum;
        """;


        return TestUtils.Script.WithVariables([(nameof(count), count)]).Execute<int>(code);
    }
    [TestCase(15, ExpectedResult = 15, Description = "変数定義をせずにfor文が指定回数ループしている")]
    [TestCase(150, ExpectedResult = 150, Description = "変数定義をせずにfor文が指定回数ループしている(数が大きくても平気)")]
    [TestCase(0, ExpectedResult = 0, Description = "変数定義をせずに0回ループのときに本文がスキップできる")]
    [TestCase(-5, ExpectedResult = 0, Description = "変数定義をせずに負の回数ループのときに本文がスキップできる")]
    public int For_WithOutDefine(int count)
    {
        string code = $$"""
        number sum = 0;
        number i = 0;
        for(; i < {{nameof(count)}}; i++)
        {
            sum += 1;
        }
        return sum;
        """;

        return TestUtils.Script.WithVariables([(nameof(count), count)]).Execute<int>(code);
    }
}
