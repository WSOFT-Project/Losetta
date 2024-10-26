namespace AliceScript.Tests.Losetta.Scripting;

using AliceScript.Functions;
using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class DefineVariable
{
    public static object[][] DefineValues = [
        // ネイティブ型
        [TypeObject.GetType(Variable.VarType.NUMBER), "num", 123],
        [TypeObject.GetType(Variable.VarType.STRING), "str", "Alice"],
        // 型推論
        [TypeObject.GetType(Variable.VarType.VARIABLE), "num", 123],
        [TypeObject.GetType(Variable.VarType.VARIABLE), "str", "Alice"],
        // .NET型
        [TypeObject.GetType<DateTime>(), "dt", DateTime.Parse("2021/01/01")],
        [TypeObject.GetType<TimeSpan>(), "ts", TimeSpan.FromDays(1)],
    ];
    [TestCaseSource(nameof(DefineValues))]
    public void Define(TypeObject varType, string varName, object value)
    {
        string typeName = "T";
        string valueName = "value";
        string code = $@"
        {typeName} {varName} = {valueName};
        ";

        Assert.That(TestUtils.Script.WithVariables([(typeName, varType), (valueName, value)])
            .Execute(code).GetValue(varName, value.GetType()), Is.EqualTo(value));
    }
}