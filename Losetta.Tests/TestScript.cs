using AliceScript.Functions;
using AliceScript.Parsing;
using AliceScript.PreProcessing;

namespace AliceScript.Tests;

public class TestScript
{
    private ParsingScript Script { get; set; }
    public TestScript()
    {
        Script = ParsingScript.GetTopLevelScript();
    }
    private TestScript(ParsingScript script)
    {
        Script = script;
    }
    /// <summary>
    /// スクリプトに変数を追加します
    /// </summary>
    /// <param name="items">追加する変数の名前と値</param>
    /// <returns>変数が追加されたスクリプト</returns>
    public TestScript WithVariables(IEnumerable<(string, Variable)> items)
    {
        var script = this.Script.GetTempScript();
        foreach (var item in items)
        {
            string name = Constants.GetRealName(Constants.ConvertName(item.Item1)).ToLowerInvariant();
            script.Variables[name] = new ValueFunction(item.Item2);
        }
        return new TestScript(script);
    }
    /// <summary>
    /// スクリプトに変数を追加します
    /// </summary>
    /// <param name="items">追加する変数の名前と値</param>
    /// <returns>変数が追加されたスクリプト</returns>
    public TestScript WithVariables(IEnumerable<(string, object)> items)
    {
        return WithVariables(items.Select(x => (x.Item1, new Variable(x.Item2))));
    }
    /// <summary>
    /// スクリプトを実行します
    /// </summary>
    /// <typeparam name="T">実行結果を変換する型</typeparam>
    /// <param name="code">実行するスクリプト</param>
    /// <returns>スクリプトの実行結果</returns>
    public T Execute<T>(string code)
    {
        code = PreProcessor.ConvertToScript(code, out _, out var defines, out var settings);
        var script = Script.GetTempScript(code);
        script.Defines = defines;
        script.Settings = settings;
        var result = script.Process();
        return result.As<T>();
    }
}