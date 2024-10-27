namespace AliceScript.Tests.Losetta.Scripting;

using AliceScript.Functions;
using AliceScript.Objects;
using NUnit.Framework;

[TestFixture]
public class DefineFunction
{
    public static object[][] FunctionSignatures =
    [
        [
            null,
            new TypeObject(Variable.VarType.NUMBER),
            "test",
            new (TypeObject, string)[] {
                (new TypeObject(Variable.VarType.STRING), "argStr"),
                (new TypeObject(Variable.VarType.NUMBER), "argNum")
            },
        ],
        [
            null,
            TypeObject.GetType(typeof(DateTime)),
            "testDateTime",
            new (TypeObject, string)[] {
                (TypeObject.GetType(typeof(DateTime)), "dateTime"),
            },
        ],
        [
            null,
            new TypeObject(Variable.VarType.VOID),
            "testAction",
            new (TypeObject, string)[] {
                (new TypeObject(Variable.VarType.STRING), "argStr"),
            },
        ],
        [
            null,
            new TypeObject(Variable.VarType.VOID),
            "procedure",
            new (TypeObject, string)[] {
            },
        ],
        [
            AccessModifier.PRIVATE,
            new TypeObject(Variable.VarType.NUMBER),
            "testPrivate",
            new (TypeObject, string)[] {
                (new TypeObject(Variable.VarType.STRING), "argStr"),
                (new TypeObject(Variable.VarType.NUMBER), "argNum")
            },
        ],
        [
            AccessModifier.PROTECTED,
            new TypeObject(Variable.VarType.NUMBER),
            "testProtected",
            new (TypeObject, string)[] {
                (new TypeObject(Variable.VarType.STRING), "argStr"),
                (new TypeObject(Variable.VarType.NUMBER), "argNum")
            },
        ],
        [
            AccessModifier.PUBLIC,
            new TypeObject(Variable.VarType.NUMBER),
            "testPublic",
            new (TypeObject, string)[] {
                (new TypeObject(Variable.VarType.STRING), "argStr"),
                (new TypeObject(Variable.VarType.NUMBER), "argNum")
            },
        ],
    ];
    [TestCaseSource(nameof(FunctionSignatures))]
    public void Define(AccessModifier? accessor, TypeObject returnType, string funcName, (TypeObject, string)[] args)
    {
        // 引数リストの組み立て
        string resultTypeName = "TResult";
        string argsTypeName = "TArgs";
        string argStr ="";
        string accessModifierStr = accessor.HasValue ? $"{accessor.Value.ToString().ToLower()} " : "";
        AccessModifier accessModifier = accessor ?? AccessModifier.PRIVATE;
        int id = 0;
        List<(string, Variable)> tArgs = new List<(string, Variable)>(){ (Constants.GetRealName(resultTypeName), Variable.From(returnType)) };
        foreach(var kvp in args)
        {
            string typeName = $"{argsTypeName}{id++}";
            argStr += $"{Constants.GetRealName(typeName)} {kvp.Item2},";
            tArgs.Add((typeName, Variable.From(kvp.Item1)));
        }
        argStr = argStr.TrimEnd(',');

        // 関数定義コードの組み立て
        // 今回のテストでは定義された関数を参照するため、__makerefを使用して関数への参照を取得する
        string code = $$"""
        {{accessModifierStr}}{{Constants.GetRealName(resultTypeName)}} {{funcName}}({{argStr}}){ };
        """;

        // テスト実行
        var result = TestUtils.Script.WithVariables(tArgs).Execute(code).GetFunction<CustomFunction>(funcName);
        
        Assert.Multiple(() =>
        {
            // 戻り値の型が正しい
            Assert.That(result.ReturnType, Is.EqualTo(returnType));
            // アクセス修飾子が正しい
            Assert.That(result.AccessModifier, Is.EqualTo(accessModifier));
            // 引数の数が正しい
            Assert.That(result.ArgumentCount, Is.EqualTo(args.Length));
            // 関数名が正しい
            Assert.That(result.Name, Is.EqualTo(funcName));

            for(int i = 0; i < args.Length; i++)
            {
                // 引数名が正しい
                Assert.That(result.RealArgs[i], Is.EqualTo(args[i].Item2));
                // 引数の型が正しい
                Assert.That(result.ArgTypes[i], Is.EqualTo(args[i].Item1));
            }
        });
    }
}
