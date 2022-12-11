using AliceScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Shangrila
{
    internal class Security_init
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Shangrila.Security");
            space.Add(new Security_IsAdminFunc());
            NameSpaceManerger.Add(space);
        }
    }
    internal class Security_IsAdminFunc : FunctionBase
    {
        public Security_IsAdminFunc()
        {
            this.Name = "Security_IsAdmin";
            this.Run += Security_IsAdminFunc_Run;
        }

        private void Security_IsAdminFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            e.Return=new Variable(principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator));
        }
    }
}
