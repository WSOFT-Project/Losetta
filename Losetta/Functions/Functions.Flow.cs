using AliceScript.Parsing;

namespace AliceScript.Functions
{
    internal sealed class IsUndefinedFunction : ParserFunction
    {
        private string m_argument;
        private string m_action;

        public IsUndefinedFunction(string arg = "", string action = "")
        {
            m_argument = arg;
            m_action = action;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var variable = GetVariable(m_argument, script);
            var varValue = variable is null ? null : variable.GetValue(script);
            bool isUndefined = varValue is null || varValue.Type == Variable.VarType.UNDEFINED;

            bool result = m_action == "===" || m_action == "==" ? isUndefined :
                          !isUndefined;
            return new Variable(result);
        }
    }

    /// <summary>
    /// (1+2)や、{;}などの空のステートメントを実行するための関数
    /// </summary>
    internal sealed class StatementFunction : FunctionBase
    {
        public StatementFunction(string body, ParsingScript script)
        {
            Name = "Statement";
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Script = script.GetTempScript(body, this);
            Run += StatementFunction_Run;
        }

        private void StatementFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Script.Process();
        }
        public ParsingScript Script { get; set; }
    }


    public interface INumericFunction { }

    public interface IArrayFunction { }
}
