using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObsoleteFunction : FunctionBase
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
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
