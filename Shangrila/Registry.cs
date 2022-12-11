using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliceScript;
using AliceScript.Interop;
using Microsoft.Win32;

namespace Shangrila
{
    internal class ShangrilaLibrary : ILibrary
    {
        public string Name
        {
            get { return "Shangrila"; }
        }
        public void Main()
        {
            NameSpace space = new NameSpace("Shangrila.Registry");

            space.Add(new Registry_ClassesRootFunc());

            NameSpaceManerger.Add(space);
            Security_init.Init();
        }
    }
    internal class Registry_ClassesRootFunc: FunctionBase
    {
        public Registry_ClassesRootFunc()
        {
            this.Name = "registry_ClassesRoot";
            this.Run += Registry_ClassesRootFunc_Run;
        }

        private void Registry_ClassesRootFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(new RegistryKeyObject(Registry.ClassesRoot));
        }
    }
    internal class RegistryKeyObject : ObjectBase
    {
        public RegistryKeyObject(RegistryKey reg)
        {
            RegistryKey = reg;
            this.Name = "RegistryKey";
            this.Functions.Add("SetValue",new Reg_SetValueFunc(this.RegistryKey));
            this.Functions.Add("CreateSubKey",new Reg_CreateKeyFunc(this.RegistryKey));
            this.Functions.Add("DeleteSubKey",new Reg_DeleteKeyFunc(this.RegistryKey));
        }
        public RegistryKey RegistryKey { get; set; }

        internal class Reg_SetValueFunc : FunctionBase
        {
            public Reg_SetValueFunc(RegistryKey registry)
            {
                this.Name = "SetValue";
                this.MinimumArgCounts = 2;
                this.Registry = registry;
                this.Run += Reg_SetValueFunc_Run;
            }
            public RegistryKey Registry;
            private void Reg_SetValueFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                if (e.Args[1].Type == Variable.VarType.NUMBER)
                {
                    Registry.SetValue(e.Args[0].AsString(), e.Args[1].AsDouble());
                }
                else
                {
                    Registry.SetValue(e.Args[0].AsString(), e.Args[1].AsString());
                }
                Registry.Close();
            }
        }

        internal class Reg_CreateKeyFunc : FunctionBase
        {
            public Reg_CreateKeyFunc(RegistryKey registry)
            {
                this.Name = "CreateSubKey";
                this.MinimumArgCounts = 1;
                this.Registry = registry;
                this.Run += Reg_SetValueFunc_Run;
            }
            public RegistryKey Registry;
            private void Reg_SetValueFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                Registry.CreateSubKey(e.Args[0].AsString());
                Registry.Close();
            }
        }
        internal class Reg_DeleteKeyFunc : FunctionBase
        {
            public Reg_DeleteKeyFunc(RegistryKey registry)
            {
                this.Name = "DeleteSubKey";
                this.MinimumArgCounts = 1;
                this.Registry = registry;
                this.Run += Reg_SetValueFunc_Run;
            }
            public RegistryKey Registry;
            private void Reg_SetValueFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                Registry.DeleteSubKeyTree(e.Args[0].AsString());
                Registry.Close();
            }
        }
    }
}
