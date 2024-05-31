using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    internal class TestCallFunction : FunctionBase
    {
        public TestCallFunction()
        {
            this.Name = Constants.ANNOTATION_FUNCTION_REFIX + "testcall";
        }
    }
}
