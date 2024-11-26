using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;

namespace AliceScript.Objects
{
    public class TypeObject : ObjectBase, IEquatable<TypeObject>
    {
        public static TypeObject GetType(Type type)
        {
            return new TypeObject(Utils.CreateBindObject(type));
        }
        public static TypeObject GetType<T>()
        {
            return GetType(typeof(T));
        }
        public static TypeObject GetType(Variable.VarType type)
        {
            return new TypeObject(type);
        }
        public TypeObject()
        {
            Init();
            Type = Variable.VarType.VARIABLE;
        }
        public TypeObject(Variable.VarType type)
        {
            Init();
            Type = type;
            if (type == Variable.VarType.VOID)
            {
                Nullable = true;
            }
        }
        public TypeObject(BindObject bind)
        {
            Init();
            Type = Variable.VarType.OBJECT;
            ClassType = bind;
            Constructor = bind.Constructor;
            foreach (var kvs in bind.StaticFunctions)
            {
                Functions.Add(kvs.Key, kvs.Value);
            }
        }
        public TypeObject(AliceScriptClass type)
        {
            Init();
            Type = Variable.VarType.OBJECT;
            ClassType = type;
            foreach (var kvs in type.StaticFunctions)
            {
                Functions.Add(kvs.Key, kvs.Value);
            }
        }
        public TypeObject(TypeObject other)
        {
            Init();
            Type = other.Type;
            ClassType = other.ClassType;
            ArrayType = other.ArrayType;
            Nullable = other.Nullable;
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
        public bool Nullable { get; set; } = false;
        public override string ToString()
        {
            string typeName;
            if (ClassType is not null && ClassType is BindObject bind)
            {
                typeName = bind.Name;
            }
            else if (ClassType is not null)
            {
                typeName = ClassType.ToString();
            }
            else
            {
                typeName = Constants.TypeToString(Type);
            }
            if (Nullable)
            {
                typeName += "?";
            }
            return typeName;
        }
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
                    e.Return = type.ClassType is not null ? new Variable(Variable.VarType.OBJECT) : Variable.From(new TypeObject(type.Type));
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
                if (ClassType is BindObject bind)
                {
                    return new Variable(bind.Constructor.Evaluate(args, script));
                }
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
            if (!Nullable)
            {
                switch (Type)
                {
                    case Variable.VarType.BOOLEAN:
                        return new Variable(args.Count > 0 ? args[0].AsBool() : false);
                    case Variable.VarType.NUMBER:
                        return new Variable(args.Count > 0 ? args[0].AsDouble() : 0);
                    case Variable.VarType.STRING:
                        return new Variable(args.Count > 0 ? args[0].AsString() : "");
                    case Variable.VarType.VOID:
                        return Variable.EmptyInstance;
                }
            }
            return new Variable(Type);
        }

        public bool Match(Variable item)
        {
            if (!Nullable && item.Nullable)
            {
                return false;
            }
            if (Type == Variable.VarType.VARIABLE)
            {
                return true;
            }
            if (item.Type.HasFlag(Type))
            {
                if (Type == Variable.VarType.OBJECT && item.Object is BindObject bind && item.Is(bind.Type, out _))
                {
                    return true;
                }
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
        internal class ConstructorFunction : FunctionBase
        {
            public ConstructorFunction()
            {
                Name = "Constructor";
                Run += ConstructorFunction_Run;
                MinimumArgCounts = 0;
            }
            private void ConstructorFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                if (e.Args.Count == 1 && e.Args[0].Type == Variable.VarType.STRING)
                {
                    e.Return = new Variable(new TypeObject(Constants.StringToType(e.Args[0].AsString())));
                }
                e.Return = new Variable();
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
                e.Return = Variable.FromText(Type.ToString());
                return;
                if (Type.ClassType is not null && Type.ClassType is TypeObject to)
                {
                    e.Return = new Variable("Alice.Interpreter.Type");
                    return;
                }
                e.Return = (Type.ClassType is not null ? new Variable(Type.ClassType.ToString()) : new Variable(Constants.TypeToString(Type.Type)));
            }
        }
    }
}
