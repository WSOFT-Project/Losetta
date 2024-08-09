using AliceScript.Objects;
using AliceScript.Parsing;
using AliceScript.Functions;
using System;
using System.Collections.Generic;

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
        public BindObject()
        {
            HandleOperator = true;
        }
        public override Variable Operator(Variable left, Variable right, string action, ParsingScript script)
        {
            if(Operators.TryGetValue(action, out var result))
            {
                return result.Evaluate(new List<Variable>{right}, script);
            }
            if (left.Object is BindObject leftObj && right.Object is BindObject rightObj && leftObj.Instance is IComparable comp1 && rightObj.Instance is IComparable comp2)
            {
                switch (action)
                {
                    case ">":
                        {
                            return new Variable(comp1.CompareTo(comp2) > 0);
                        }
                    case "<":
                        {
                            return new Variable(comp1.CompareTo(comp2) < 0);
                        }
                    case ">=":
                        {
                            return new Variable(comp1.CompareTo(comp2) >= 0);
                        }
                    case "<=":
                        {
                            return new Variable(comp1.CompareTo(comp2) <= 0);
                        }
                }
            }
            Utils.ThrowErrorMsg("次の演算子を処理できませんでした。[" + action + "]", Exceptions.INVALID_OPERAND,
                 script, action);
            return Variable.EmptyInstance;
        }
        public override bool Equals(ObjectBase other)
        {
            if (other is BindObject rightObj)
            {
                return Instance.Equals(rightObj.Instance);
            }
            return base.Equals(other);
        }
        public override int CompareTo(object other)
        {
            if (Instance is IComparable var1 && other is IComparable var2)
            {
                return var1.CompareTo(var2);
            }
            return 0;
        }
        public Dictionary<string, FunctionBase> Operators
        {
            get => m_operators;
            set => m_operators = value;
        }
        private Dictionary<string, FunctionBase> m_operators = new Dictionary<string, FunctionBase>();
    }
}
