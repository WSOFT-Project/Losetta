using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public interface ICallingHandleAttribute
    {
        public void PreCall(FunctionBase function, FunctionBaseEventArgs args);
        public void PostCall(FunctionBase function, FunctionBaseEventArgs args);
    }
}
