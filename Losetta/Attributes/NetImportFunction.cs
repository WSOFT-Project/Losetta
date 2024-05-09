using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    internal class NetImportFunction : AttributeFunction
    {
        public NetImportFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + Constants.NET_IMPORT;
            MinimumArgCounts = 1;
            Run += PInvokeFlagFunction_Run;
        }

        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string asmName = Utils.GetSafeString(e.Args, 1, null);
            string asmLocate = Utils.GetSafeString(e.Args, 2, null);
            string typeName = e.Args[0].AsString();

            if (!string.IsNullOrEmpty(asmName))
            {
                typeName += $",{asmName}";
            }
            if (string.IsNullOrEmpty(asmLocate))
            {
                Class = Type.GetType(typeName, false, true);
            }
            else
            {
                var asm = Assembly.LoadFrom(asmLocate);
                Class = asm.GetType(typeName);
            }
        }
        public Type Class { get; set; }
    }
}
