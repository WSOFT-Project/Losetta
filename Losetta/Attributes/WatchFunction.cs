using AliceScript.Functions;

namespace AliceScript
{
    public class WatchFunction : FunctionBase, ICallingHandleAttribute
    {
        public WatchFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.WATCH;
            Run += PInvokeFlagFunction_Run;
        }
        public WatchFunction(string message)
        {
            Message = message;
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.WATCH;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Message = Utils.GetSafeString(e.Args, 0, string.Empty);
        }
        public void PreCall(FunctionBase function, FunctionBaseEventArgs args)
        {
            if (Interpreter.Instance.DebugMode)
            {
                throw new ScriptException(Message, Exceptions.BREAK_POINT);
            }
        }
        public void PostCall(FunctionBase function, FunctionBaseEventArgs args)
        {

        }
        public string Message { get; set; }
    }
}
