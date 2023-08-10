namespace AliceScript
{
    internal class LabelFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // Just skip this label. m_name is equal to the lable name.
            return Variable.EmptyInstance;
        }
    }

    internal class PointerFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<string> args = Utils.GetTokens(script);
            Utils.CheckArgs(args.Count, 1, Name);

            var result = new Variable(Variable.VarType.POINTER);
            result.Pointer = args[0];
            ParserFunction.AddGlobalOrLocalVariable(Name,
                                        new GetVarFunction(result), script);
            return result;
        }
    }

    internal class PointerReferenceFunction : ActionFunction
    {
        public PointerReferenceFunction()
        {
            this.Name = "PointerReference";
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            var pointer = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            var result = GetRefValue(pointer, script);
            return result;
        }

        public Variable GetRefValue(string pointer, ParsingScript script)
        {
            if (string.IsNullOrWhiteSpace(pointer))
            {
                return Variable.Undefined;
            }
            var refPointer = ParserFunction.GetVariable(pointer, null, true) as GetVarFunction;
            if (refPointer == null || string.IsNullOrWhiteSpace(refPointer.Value.Pointer))
            {
                return Variable.Undefined;
            }

            var result = ParserFunction.GetVariable(refPointer.Value.Pointer, null, true);
            if (result is GetVarFunction)
            {
                return ((GetVarFunction)result).Value;
            }

            if (result is CustomFunction)
            {
                script.Forward();
                List<Variable> args = script.GetFunctionArgs(this);
                return ((CustomFunction)result).ARun(args, script);
            }
            return Variable.Undefined;
        }
    }

    public interface INumericFunction { }

    public interface IArrayFunction { }

    public interface IStringFunction { }

}
