using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class IndependentFunction : FunctionBase
    {
        public IndependentFunction()
        {
            this.Name = Constants.ANNOTATION_FUNCTION_REFIX + "independent";
        }
    }
}
