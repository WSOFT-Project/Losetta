using AliceScript.Objects;

namespace AliceScript.Binding
{
    /// <summary>
    /// .NETのオブジェクトと対応するAliceScriptのオブジェクト
    /// </summary>
    public class BindObject : ObjectBase
    {
        /// <summary>
        /// .NETのオブジェクトインスタンス
        /// </summary>
        public object Instance { get; set; }
    }
}
