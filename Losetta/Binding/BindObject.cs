using AliceScript.Objects;
using System;

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

        public override int CompareTo(object other)
        {
            if(Instance is IComparable var1 && other is IComparable var2)
            {
                return var1.CompareTo(var2);
            }
            return 0;
        }
    }
}
