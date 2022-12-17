namespace AliceScript
{
    public class TypeObject : ObjectBase
    {
        public TypeObject()
        {
            this.Name = "Type";
            this.Functions.Add("Activate", new ActivateFunction(this));
        }
        public TypeObject(Variable.VarType type)
        {
            this.Name = "Type";
            Type = type;
            this.Functions.Add("Activate", new ActivateFunction(this));
        }
        public TypeObject(AliceScriptClass type)
        {
            this.Name = "Type";
            this.ClassType = type;
            this.Functions.Add("Activate",new ActivateFunction(this));
            foreach (var kvs in type.StaticFunctions)
            {
                this.Functions.Add(kvs.Key, kvs.Value);
            }
        }
        public Variable.VarType Type { get; set; }
        public AliceScriptClass ClassType { get; set; }

        internal class ActivateFunction : FunctionBase
        {
            public ActivateFunction(TypeObject type)
            {
                this.Name = "Activate";
                this.Run += Type_ActivateFunc_Run;
                this.Type = type;
            }
            public TypeObject Type { get; set; }
            private void Type_ActivateFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Type.ClassType != null)
                {
                    //TODO:非ObjectBaseのクラスのアクティベート
                    ObjectBase csClass = Type.ClassType as ObjectBase;
                    if (csClass != null)
                    {
                        Variable obj = csClass.GetImplementation(e.Args, e.Script);
                        e.Return = obj;
                        return;
                    }
                }
                else
                {
                    e.Return = new Variable(Type.Type);
                }
            }
        }
    }
}
