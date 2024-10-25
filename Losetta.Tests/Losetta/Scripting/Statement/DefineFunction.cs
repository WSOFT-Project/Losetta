namespace AliceScript.Tests.Losetta.Scripting;

using AliceScript.Functions;
using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class DefineFunction
{
    [TestCase(Description = "引数・戻り値なしの関数を定義できる")]
    public void Procedure()
    {
        string funcName = "test";
        string code = $$"""
        void {{funcName}}()
        {
            return;
        }
        __makeref(test());
        """;
        var result = Alice.Execute<CustomFunction>(code);
        Assert.Multiple(()=>
        {
            Assert.That(result.ReturnType.Type, Is.EqualTo(Variable.VarType.VOID));
            Assert.That(result.AccessModifier, Is.EqualTo(AccessModifier.PRIVATE));
            Assert.That(result.ArgumentCount, Is.EqualTo(0));
            Assert.That(result.Name, Is.EqualTo(funcName));
        });
    }
    [TestCase(new Variable.VarType[] { }, new string[] { }, Variable.VarType.VOID, Description = "引数・戻り値なしの関数を定義できる")]
    [TestCase(new Variable.VarType[] { Variable.VarType.STRING, Variable.VarType.NUMBER }, new string[] { "argStr", "argNum" }, Variable.VarType.VOID, Description = "引数あり・戻り値なしの関数を定義できる")]
    [TestCase(new Variable.VarType[]{Variable.VarType.STRING, Variable.VarType.NUMBER}, new string[] { "argStr", "argNum" }, Variable.VarType.NUMBER, Description = "引数・戻り値ありの関数を定義できる")]
    public void Action(Variable.VarType[] argTypes, string[] argNames, Variable.VarType returnVarType)
    {
        string funcName = "test";

        TypeObject returnType = new TypeObject(returnVarType);

        // 引数リストを組み立てる
        string args ="";
        for(int i = 0;i < argTypes.Length;i++)
        {
            args += $"{Constants.TypeToString(argTypes[i])} {argNames[i]},";
        }
        args = args.TrimEnd(',');

        string code = $$"""
        {{Constants.TypeToString(returnType.Type)}} {{funcName}}({{args}})
        {
            return;
        }
        __makeref(test());
        """;

        var result = Alice.Execute<CustomFunction>(code);
        Assert.Multiple(() =>
        {
            Assert.That(result.ReturnType, Is.EqualTo(returnType));
            Assert.That(result.AccessModifier, Is.EqualTo(AccessModifier.PRIVATE));
            Assert.That(result.ArgumentCount, Is.EqualTo(argTypes.Length));
            Assert.That(result.Name, Is.EqualTo(funcName));

            for(int i = 0; i < argTypes.Length; i++)
            {
                Assert.That(result.RealArgs[i], Is.EqualTo(argNames[i]));
                Assert.That(result.ArgTypes[i].Type, Is.EqualTo(argTypes[i]));
            }
        });
    }
}
