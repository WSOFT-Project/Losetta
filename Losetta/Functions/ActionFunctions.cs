using AliceScript.Parsing;
using System;
using System.Collections.Generic;

namespace AliceScript.Functions
{
    internal sealed class IncrementDecrementFunction : ActionFunction, INumericFunction
    {
        public IncrementDecrementFunction()
        {
            Name = "IncrementDecrement";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += IncrementDecrementFunction_Run;
        }

        private void IncrementDecrementFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool prefix = string.IsNullOrWhiteSpace(Name);
            if (prefix)
            {// If it is a prefix we do not have the variable name yet.
                Name = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            }

            Utils.CheckLegalName(Name);

            // Value to be added to the variable:
            int valueDelta = m_action == Constants.INCREMENT ? 1 : -1;
            int returnDelta = prefix ? valueDelta : 0;

            // Check if the variable to be set has the form of x[a][b],
            // meaning that this is an array element.
            double newValue = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(e.Script, m_name, (name) => { m_name = name; }, this);

            ParserFunction func = GetVariable(m_name, e.Script);
            Utils.CheckNotNull(m_name, func, e.Script);

            Variable currentValue = func.GetValue(e.Script);
            currentValue = currentValue.DeepClone();

            if (arrayIndices.Count > 0 || e.Script.TryCurrent() == Constants.START_ARRAY)
            {
                if (prefix)
                {
                    string tmpName = m_name + e.Script.Rest;
                    int delta = 0;
                    arrayIndices = Utils.GetArrayIndices(e.Script, tmpName, delta, (t, d) => { tmpName = t; delta = d; }, this);
                    e.Script.Forward(Math.Max(0, delta - tmpName.Length));
                }

                Variable element = Utils.ExtractArrayElement(currentValue, arrayIndices, e.Script);
                e.Script.MoveForwardIf(Constants.END_ARRAY);

                newValue = element.Value + returnDelta;
                element.Value += valueDelta;
            }
            else
            { // A normal variable.
                newValue = currentValue.Value + returnDelta;
                currentValue.Value += valueDelta;
            }

            ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                    new ValueFunction(currentValue), e.Script);
            e.Return = new Variable(newValue);
        }


        public override ParserFunction NewInstance()
        {
            return new IncrementDecrementFunction();
        }
    }

    internal sealed class LabelFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // ラベル名のため何もしない
            return Variable.EmptyInstance;
        }
    }
}
