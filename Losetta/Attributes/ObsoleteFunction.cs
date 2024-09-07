using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObsoleteFunction : FunctionBase, ICallingHandleAttribute
    {
        public ObsoleteFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.OBSOLETE;
            Run += PInvokeFlagFunction_Run;
        }
        public ObsoleteFunction(bool isError, string message)
        {
            Message = message;
            IsError = isError;
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.OBSOLETE;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            IsError = Utils.GetSafeBool(e.Args, 0);
            Message = Utils.GetSafeString(e.Args, 1, null);
        }
        public void PreCall(FunctionBase function, FunctionBaseEventArgs args)
        {
            if (Interpreter.Instance.DebugMode)
            {
                string mes = string.IsNullOrEmpty(Message) ? $"`{this.Name}`は旧形式です。" : $"`{this.Name}`は旧形式です。:{Message}";
                if (IsError)
                {
                    throw new ScriptException(mes, Exceptions.FUNCTION_IS_OBSOLETE);
                }
                else
                {
                    Interpreter.Instance.AppendDebug($"FUNCTION_IS_OBSOLETE(0x04f): {mes}", true);
                }
            }
        }
        public void PostCall(FunctionBase function, FunctionBaseEventArgs args)
        {

        }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
