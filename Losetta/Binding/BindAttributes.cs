using AliceScript.Functions;

namespace AliceScript.Binding
{

    /// <summary>
    /// AliceScriptで使用できる関数として公開するメソッド
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AliceFunctionAttribute : Attribute
    {
        public string Name { get; set; }
        public FunctionAttribute Attribute { get; set; }
    }

    /// <summary>
    /// AliceScriptの名前空間として公開するクラス
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AliceNameSpaceAttribute : Attribute
    {
        public string Name { get; set; }
        public bool NeedBindAttribute { get; set; }

    }
}
