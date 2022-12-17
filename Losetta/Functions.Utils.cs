using System.Collections.Generic;
using System.Text;
using System.Threading;

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
                List<Variable> args = script.GetFunctionArgs();
                return ((CustomFunction)result).ARun(args, script);
            }
            return Variable.Undefined;
        }
    }

    interface INumericFunction { }
    interface IArrayFunction { }
    interface IStringFunction { }



    class DataFunction : ParserFunction
    {
        public enum DataMode { ADD, SUBSCRIBE, SEND };

        DataMode m_mode;

        static string s_method;
        static string s_tracking;
        static bool s_updateImmediate = false;

        static StringBuilder s_data = new StringBuilder();

        public DataFunction(DataMode mode = DataMode.ADD)
        {
            m_mode = mode;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string result = "";

            switch (m_mode)
            {
                case DataMode.ADD:
                    Collect(args);
                    break;
                case DataMode.SUBSCRIBE:
                    Subscribe(args);
                    break;
                case DataMode.SEND:
                    result = SendData(s_data.ToString());
                    s_data.Clear();
                    break;
            }

            return new Variable(result);
        }

        public void Subscribe(List<Variable> args)
        {
            s_data.Clear();

            s_method = Utils.GetSafeString(args, 0);
            s_tracking = Utils.GetSafeString(args, 1);
            s_updateImmediate = Utils.GetSafeDouble(args, 2) > 0;
        }

        public void Collect(List<Variable> args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg.AsString());
            }
            if (s_updateImmediate)
            {
                SendData(sb.ToString());
            }
            else
            {
                s_data.AppendLine(sb.ToString());
            }
        }

        public string SendData(string data)
        {
            if (!string.IsNullOrWhiteSpace(s_method))
            {
                CustomFunction.ARun(s_method, new Variable(s_tracking),
                                   new Variable(data));
                return "";
            }
            return data;
        }
    }


}
