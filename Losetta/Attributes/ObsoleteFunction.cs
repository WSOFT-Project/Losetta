using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObsoleteFunction : AttributeFunction
    {
        public ObsoleteFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.OBSOLETE;
            Run += PInvokeFlagFunction_Run;
        }
        public ObsoleteFunction(string message, bool isError)
        {
            Message = message;
            IsError = isError;
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.OBSOLETE;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Message = Utils.GetSafeString(e.Args, 0, null);
            IsError = Utils.GetSafeBool(e.Args, 1);
        }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }
}
