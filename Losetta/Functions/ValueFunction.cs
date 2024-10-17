using AliceScript.Parsing;
using System;
using System.Collections.Generic;

namespace AliceScript.Functions
{
    public class ValueFunction : FunctionBase
    {
        public ValueFunction(Variable value)
        {
            m_value = value;
            Init();
        }
        public ValueFunction()
        {
            m_value = Variable.EmptyInstance;
            Init();
        }
        private void Init()
        {
            Name = "Variable";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ValueFunction_Run;
        }

        private void ValueFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable value = Value;
            Value.Keywords = Keywords;
            if (e.Script.Current == Constants.TERNARY_OPERATOR)
            {
                if (value.IsNull())
                {
                    e.Script.MoveForwardNotWhile(Constants.TOKENS_SEPARATION);
                    e.Return = Variable.EmptyInstance;
                }
                else
                {
                    e.Script.Forward();
                }
            }
            // 要素が配列の一部かを確認
            if (e.Script.TryPrev() == Constants.START_ARRAY)
            {
                switch (value.Type)
                {
                    case Variable.VarType.ARRAY:
                    case Variable.VarType.DELEGATE:
                    case Variable.VarType.STRING:
                    case Variable.VarType.DICTIONARY:
                        break;
                    default:
                        {
                            throw new ScriptException("この変数で配列添え字演算子を使用することはできません。", Exceptions.VARIABLE_CANT_USE_WITH_ARRAY_SUBSCRIPT, e.Script);
                        }
                }

                if (m_arrayIndices is null)
                {
                    string startName = e.Script.Substr(e.Script.Pointer - 1);
                    m_arrayIndices = Utils.GetArrayIndices(e.Script, startName, m_delta, (newStart, newDelta) => { startName = newStart; m_delta = newDelta; }, this);
                }

                e.Script.Forward(m_delta);
                while (e.Script.MoveForwardIf(Constants.END_ARRAY))
                {
                    ;
                }

                Variable result = Utils.ExtractArrayElement(value, m_arrayIndices, e.Script);
                if (e.Script.Prev == '.')
                {
                    e.Script.Backward();
                }

                if (e.Script.TryCurrent() != '.')
                {
                    e.Return = result;
                    return;
                }
                e.Script.Forward();

                m_propName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
                Variable propValue = result.GetProperty(m_propName, e.Script);
                Utils.CheckNotNull(propValue, m_propName, e.Script);
                e.Return = propValue;
                return;
            }

            // Now check that this is an object:
            if (!string.IsNullOrWhiteSpace(m_propName))
            {
                string temp = m_propName;
                m_propName = null; // Need this to reset for recursive calls
                Variable propValue = value.Type == Variable.VarType.ENUM ?
                                     value.GetEnumProperty(temp, e.Script) :
                                     value.GetProperty(temp, e.Script);
                Utils.CheckNotNull(propValue, temp, e.Script);
                e.Return = EvaluateFunction(propValue, e.Script, m_propName, this);
                return;
            }

            // Otherwise just return the stored value.
            e.Return = value;
        }
        public static Variable EvaluateFunction(Variable var, ParsingScript script, string m_propName, FunctionBase callFrom)
        {
            if (var is not null && var.CustomFunctionGet is not null)
            {
                List<Variable> args = script.Prev == '(' ? script.GetFunctionArgs(callFrom) : new List<Variable>();
                if (var.StackVariables is not null)
                {
                    args.AddRange(var.StackVariables);
                }
                return var.CustomFunctionGet.ARun(args, script);
            }
            return var is not null && !string.IsNullOrWhiteSpace(var.CustomGet) ? ParsingScript.RunString(var.CustomGet, script) : var;
        }
        public int Delta
        {
            set => m_delta = value;
        }

        public Variable Value
        {
            get => GetValue();
            set => SetValue(value);
        }
        public List<Variable> Indices
        {
            set => m_arrayIndices = value;
        }
        public string PropertyName
        {
            set => m_propName = value;
        }

        private Variable m_value;
        private int m_delta = 0;
        private List<Variable> m_arrayIndices = null;
        private string m_propName;

        /// <summary>
        /// TrueにするとSettingイベントおよびGettingイベントが発生します
        /// </summary>

        public bool HandleEvents { get; set; }

        /// <summary>
        /// プロパティに変数が代入されるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>
        public event ValueEventHandler Setting;
        /// <summary>
        /// プロパティから変数が読みだされるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>

        public event ValueEventHandler Getting;


        /// <summary>
        /// SetPropertyが使用可能かを表す値。デフォルトではTrueです。
        /// </summary>
        public bool CanSet
        {
            get => m_CanSet;
            set => m_CanSet = value;
        }
        private bool m_CanSet = true;
        public Variable GetValue(Variable parent = null)
        {
            if (HandleEvents)
            {
                ValueFunctionEventArgs e = new ValueFunctionEventArgs();
                e.Parent = parent;
                e.Value = m_value;
                Getting?.Invoke(this, e);
                return e.Value;
            }
            else
            {
                return m_value;
            }
        }
        public void SetValue(Variable value, Variable parent = null)
        {
            if (CanSet)
            {
                if (HandleEvents)
                {
                    ValueFunctionEventArgs e = new ValueFunctionEventArgs();
                    e.Parent = parent;
                    e.Value = value;
                    Setting?.Invoke(this, e);
                }
                else
                {
                    m_value.Assign(value);
                }
            }
            else
            {
                throw new ScriptException("このプロパティには代入できません", Exceptions.COULDNT_ASSIGN_THIS_PROPERTY);
            }
        }
    }
    public class ValueFunctionEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティの変数の内容
        /// </summary>
        public Variable Value { get; set; }

        /// <summary>
        /// 呼び出し元の変数。これはコアプロパティで使用します。
        /// </summary>
        public Variable Parent { get; set; }
    }
    public delegate void ValueEventHandler(object sender, ValueFunctionEventArgs e);
}
