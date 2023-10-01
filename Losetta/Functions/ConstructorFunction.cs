using AliceScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript.Functions
{
    internal class ConstructorFunction : FunctionBase
    {
        public ConstructorFunction(TypeObject type)
        {
            Type = type;
            Run += ConstructorFunction_Run;
        }
        public TypeObject Type { get; set; }
        private void ConstructorFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Type.Activate(e.Args,e.Script);
        }
    }
}
