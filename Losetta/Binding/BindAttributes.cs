using AliceScript.Functions;
using System;

namespace AliceScript.Binding
{

    /// <summary>
    /// AliceScriptで使用できる関数として公開するメソッド
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
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
        /// この関数をAliceScriptにバインドするか指定する値
        /// </summary>
        public AliceBindState State { get; set; }

        /// <summary>
        /// この関数が拡張メソッドとして使用可能なとき、この関数は拡張メソッドとしてのみ呼び出すことができます
        /// </summary>
        public bool MethodOnly { get; set; } = true;
    }

    /// <summary>
    /// メソッドまたはプロパティをAliceScriptで使用できるようにバインドするかどうかを表す値
    /// </summary>
    public enum AliceBindState
    {
        /// <summary>
        /// メソッドまたはプロパティをバインドします
        /// </summary>
        Enabled = 0,
        /// <summary>
        /// メソッドまたはプロパティをバインドしません
        /// </summary>
        Disabled = 1
    }

    /// <summary>
    /// AliceScriptで使用できるプロパティとして公開するプロパティ
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AlicePropertyAttribute : Attribute
    {
        /// <summary>
        /// プロパティの名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// このプロパティをAliceScriptにバインドするか指定する値
        /// </summary>
        public AliceBindState State { get; set; }
    }
    /// <summary>
    /// AliceScriptで使用できるオブジェクトとして公開するクラス
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct)]
    public class AliceObjectAttribute : Attribute
    {
        /// <summary>
        /// オブジェクトの名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// オブジェクトの所属する名前空間
        /// </summary>
        public string NameSpace { get; set; }

        /// <summary>
        /// この関数を規定でAliceScriptにバインドするか指定する値
        /// </summary>
        public AliceBindState DefaultState { get; set; }
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
        /// この関数を規定でAliceScriptにバインドするか指定する値
        /// </summary>
        public AliceBindState DefaultState { get; set; }

    }
}
