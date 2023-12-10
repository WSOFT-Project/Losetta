using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Objects
{

    public class TypeObject : ObjectBase, IEquatable<TypeObject>
    {
        public TypeObject()
        {
            Init();
            Type = Variable.VarType.VARIABLE;
        }
        public TypeObject(Variable.VarType type)
        {
            Init();
            Type = type;
        }
        public TypeObject(AliceScriptClass type)
        {
            Init();
            ClassType = type;
            foreach (var kvs in type.StaticFunctions)
            {
                Functions.Add(kvs.Key, kvs.Value);
            }
        }
        private void Init()
        {
            Name = "Type";
            Functions.Add("Activate", new ActivateFunction(this));
            Functions.Add("ToString", new ToStringFunction(this));
            Functions.Add("ToNativeProperty", new ToNativeProperty(this));
            Functions.Add("IsObject", new IsObjectProperty(this));
            Functions.Add("Namespace", new NamespaceProperty(this));
            Functions.Add("Base", new BaseProperty(this));
        }
        public Variable.VarType Type { get; set; }
        public TypeObject ArrayType { get; set; }
        public AliceScriptClass ClassType { get; set; }
        internal class NamespaceProperty : ValueFunction
        {
            public NamespaceProperty(TypeObject type)
            {
                Name = "Namespace";
                HandleEvents = true;
                CanSet = false;
                Getting += delegate (object sender, ValueFunctionEventArgs e)
                {
                    e.Value = type.ClassType is not null ? new Variable(type.ClassType.Namespace) : Variable.EmptyInstance;
                };
            }
        }
        internal class BaseProperty : ValueFunction
        {
            public BaseProperty(TypeObject type)
            {
                Name = "Base";
                HandleEvents = true;
                CanSet = false;
                Getting += delegate (object sender, ValueFunctionEventArgs e)
                {
                    e.Value = type.ClassType is not null ? new Variable(type.ClassType.BaseClasses) : Variable.EmptyInstance;
                };
            }
        }
        internal class IsObjectProperty : ValueFunction
        {
            public IsObjectProperty(TypeObject type)
            {
                Name = "IsObject";
                HandleEvents = true;
                CanSet = false;
                Getting += delegate (object sender, ValueFunctionEventArgs e)
                {
                    e.Value = new Variable(type.ClassType is not null);
                };
            }
        }
        internal class ToNativeProperty : FunctionBase
        {
            public ToNativeProperty(TypeObject type)
            {
                Name = "ToNativeProperty";
                Run += delegate (object sender, FunctionBaseEventArgs e)
                {
                    e.Return = type.ClassType is not null ? new Variable(Variable.VarType.OBJECT) : new Variable(new TypeObject(type.Type));
                };
            }
        }
        /// <summary>
        /// このオブジェクトの表す型がもう一方の型と等しいかどうかを表す値を取得します
        /// </summary>
        /// <param name="other">比較する型</param>
        /// <returns>もう一方の型と等しければTrue、それ以外の場合はFalse</returns>
        public bool Equals(TypeObject other)
        {
            return ClassType is not null && other.ClassType is not null
                ? ClassType.ToString() == other.ClassType.ToString()
                : ClassType is not null || other.ClassType is not null ? false : Type == other.Type;
        }

        public Variable Activate(List<Variable> args, ParsingScript script)
        {
            if (ClassType is not null)
            {
                //TODO:非ObjectBaseのクラスのアクティベート
                if (ClassType is ObjectBase csClass)
                {
                    return csClass.GetImplementation(args, script);
                }
            }
            else if (Type == Variable.VarType.ARRAY)
            {
                Variable v = new Variable(Variable.VarType.ARRAY);
                v.Tuple.Type = ArrayType;
                return v;
            }
            return new Variable(Type);
        }

        public bool Match(Variable item)
        {
            if (Type == Variable.VarType.VARIABLE)
            {
                return true;
            }
            if (item.Type.HasFlag(Type))
            {
                if (Type == Variable.VarType.OBJECT && item.Object is AliceScriptClass c && ClassType != c)
                {
                    return false;
                }
                else if (item.Type != Variable.VarType.STRING && Type == Variable.VarType.ARRAY && item.Tuple.Type != ArrayType)
                {
                    return false;
                }
                return true;
            }
            else { return false; }
        }
        internal class ActivateFunction : FunctionBase
        {
            public ActivateFunction(TypeObject type)
            {
                Name = "Activate";
                Run += Type_ActivateFunc_Run;
                Type = type;
            }
            public TypeObject Type { get; set; }
            private void Type_ActivateFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                e.Return = Type.Activate(e.Args, e.Script);
            }
        }
        internal class ToStringFunction : FunctionBase
        {
            public ToStringFunction(TypeObject type)
            {
                Name = "ToString";
                Run += ToStringFunction_Run;
                Type = type;
            }
            public TypeObject Type { get; set; }
            private void ToStringFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                if (Type.ClassType is not null && Type.ClassType is TypeObject to)
                {
                    e.Return = new Variable("Alice.Interpreter.Type");
                    return;
                }
                e.Return = Type.ClassType is not null ? new Variable(Type.ClassType.ToString()) : new Variable(Constants.TypeToString(Type.Type));
            }
        }
    }
}
