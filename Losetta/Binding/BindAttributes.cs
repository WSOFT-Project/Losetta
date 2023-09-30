using AliceScript.Functions;

namespace AliceScript.Binding
{

    /// <summary>
    /// AliceScriptで使用できる関数として公開するメソッド
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AliceFunctionAttribute : Attribute
    {
        /// <summary>
        /// この関数の名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// この関数に付与する属性
        /// </summary>
        public FunctionAttribute Attribute { get; set; }

        /// <summary>
        /// この関数が拡張メソッドとして使用可能なとき、この関数は拡張メソッドとしてのみ呼び出すことができる
        /// </summary>
        public bool MethodOnly { get; set; } = true;
    }

    /// <summary>
    /// AliceScriptで使用できるプロパティとして公開するプロパティ
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AlicePropertyAttribute : Attribute
    {
        /// <summary>
        /// プロパティの名前
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// AliceScriptの名前空間として公開するクラス
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AliceNameSpaceAttribute : Attribute
    {
        /// <summary>
        /// この名前空間の名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// このクラスの公開するメソッドにはすべてAliceFunction属性を付けることを求める場合はTrue
        /// </summary>
        public bool NeedBindAttribute { get; set; }

    }
}
