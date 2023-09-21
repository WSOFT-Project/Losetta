using AliceScript.Parsing;

namespace AliceScript.Functions
{
    // Get a value of a variable or of an array element
    public class GetVarFunction : FunctionBase
    {
        public GetVarFunction(Variable value)
        {
            m_value = value;
            Name = "Variable";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            //this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += GetVarFunction_Run;
        }

        private void GetVarFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script.Current == Constants.TERNARY_OPERATOR)
            {
                if (m_value.IsNull())
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
                switch (m_value.Type)
                {
                    case Variable.VarType.ARRAY:
                        {
                            break;
                        }

                    case Variable.VarType.DELEGATE:
                        {
                            break;
                        }
                    case Variable.VarType.STRING:
                        {
                            break;
                        }
                    default:
                        {
                            throw new ScriptException("この変数で配列添え字演算子を使用することはできません。", Exceptions.VARIABLE_CANT_USE_WITH_ARRAY_SUBSCRIPT, e.Script);
                        }
                }

                if (m_arrayIndices == null)
                {
                    string startName = e.Script.Substr(e.Script.Pointer - 1);
                    m_arrayIndices = Utils.GetArrayIndices(e.Script, startName, m_delta, (newStart, newDelta) => { startName = newStart; m_delta = newDelta; }, this);
                }

                e.Script.Forward(m_delta);
                while (e.Script.MoveForwardIf(Constants.END_ARRAY))
                {
                    ;
                }

                Variable result = Utils.ExtractArrayElement(m_value, m_arrayIndices, e.Script);
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
                Variable propValue = m_value.Type == Variable.VarType.ENUM ?
                                     m_value.GetEnumProperty(temp, e.Script) :
                                     m_value.GetProperty(temp, e.Script);
                Utils.CheckNotNull(propValue, temp, e.Script);
                e.Return = EvaluateFunction(propValue, e.Script, m_propName, this);
                return;
            }

            // Otherwise just return the stored value.
            e.Return = m_value;
        }
        public static Variable EvaluateFunction(Variable var, ParsingScript script, string m_propName, FunctionBase callFrom)
        {
            if (var != null && var.CustomFunctionGet != null)
            {
                List<Variable> args = script.Prev == '(' ? script.GetFunctionArgs(callFrom) : new List<Variable>();
                if (var.StackVariables != null)
                {
                    args.AddRange(var.StackVariables);
                }
                return var.CustomFunctionGet.ARun(args, script);
            }
            return var != null && !string.IsNullOrWhiteSpace(var.CustomGet) ? ParsingScript.RunString(var.CustomGet, script) : var;
        }
        public int Delta
        {
            set => m_delta = value;
        }
        public Variable Value => m_value;
        public List<Variable> Indices
        {
            set => m_arrayIndices = value;
        }
        public string PropertyName
        {
            set => m_propName = value;
        }

        internal Variable m_value;
        private int m_delta = 0;
        private List<Variable> m_arrayIndices = null;
        private string m_propName;
    }
}
