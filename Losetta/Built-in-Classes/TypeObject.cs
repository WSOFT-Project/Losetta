using AliceScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class TypeObject : ObjectBase
    {
        public TypeObject()
        {
            this.Name = "Type";
            
        }
        public  TypeObject(Variable.VarType type)
        {
            this.Name = "Type";
            Type = type;
        }
        public TypeObject(AliceScriptClass type)
        {
            this.Name = "Type";
            this.ClassType = type;
            foreach(var kvs in type.StaticFunctions)
            {
                this.Functions.Add(kvs.Key,kvs.Value);
            }
        }
        public Variable.VarType Type { get; set; }
        public AliceScriptClass ClassType { get; set; }
    }
}
