using AliceScript.Functions;
using AliceScript.NameSpaces;
using System.Linq.Expressions;
using System.Reflection;

namespace AliceScript.Binding
{
    public class BindFunction : FunctionBase
    {
        public BindFunction()
        {
            Run += BindFunction_Run;
        }

        private void BindFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            foreach (var load in Overloads)
            {
                if (load.TryConvertParameters(e.Args, out var args))
                {
                    if (load.IsVoidFunc)
                    {
                        load.VoidFunc.Invoke(args);
                    }
                    else
                    {
                        e.Return = new Variable(load.ObjFunc.Invoke(args));
                    }
                    return;
                }
            }
            throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
        }

        public static NameSpace BindToNameSpace(Type type)
        {
            var space = new NameSpace(type.Name);
            bool needbind = false;
            if (TryGetAttibutte<AliceNameSpaceAttribute>(type, out var attribute))
            {
                if (attribute.Name != null)
                {
                    space.Name = attribute.Name;
                }
                needbind = attribute.NeedBindAttribute;
            }
            Dictionary<string, HashSet<MethodInfo>> methods = new Dictionary<string, HashSet<MethodInfo>>();
            foreach (var m in type.GetMethods())
            {
                if (m.IsPublic && m.IsStatic)
                {
                    if (!methods.ContainsKey(m.Name))
                    {
                        methods[m.Name] = new HashSet<MethodInfo>();
                    }
                    methods[m.Name].Add(m);
                }
            }
            foreach (HashSet<MethodInfo> mi in methods.Values)
            {
                var method = mi.OrderByDescending(x => x.GetParameters().Length);
                var func = CreateBindFunction(method.ToArray(), needbind);
                if (func != null)
                {
                    space.Add(func);
                }
            }
            return space;
        }
        private static FunctionBase CreateBindFunction(MethodInfo[] methodInfos, bool needBind)
        {
            var func = new BindFunction();
            foreach (var methodInfo in methodInfos)
            {
                string name = methodInfo.Name;
                FunctionAttribute funcAttribute = FunctionAttribute.GENERAL;
                if (TryGetAttibutte<AliceFunctionAttribute>(methodInfo, out var attribute))
                {
                    if (attribute.Name != null)
                    {
                        name = attribute.Name;
                    }
                    funcAttribute = attribute.Attribute;
                }
                else if (needBind)
                {
                    return null;
                }

                func.Name = name;
                var load = new BindingOverloadFunction();
                load.Attribute = funcAttribute;
                load.TrueParameters = methodInfo.GetParameters();

                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), x.ParameterType)).ToArray();
                if (methodInfo.ReturnType == typeof(void))
                {
                    load.VoidFunc = Expression.Lambda<Action<object[]>>(
                    Expression.Convert(
                        Expression.Call(methodInfo, parameters),
                        typeof(void)),
                    args).Compile();
                    load.IsVoidFunc = true;
                }
                else
                {
                    load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                    Expression.Convert(
                        Expression.Call(methodInfo, parameters),
                        typeof(object)),
                    args).Compile();
                }
                func.Overloads.Add(load);
            }


            return func;
        }

        private static bool TryGetAttibutte<T>(MemberInfo memberInfo, out T attribute) where T : Attribute
        {
            attribute = null;
            var attr = System.Attribute.GetCustomAttributes(memberInfo, typeof(T));
            if (attr.Length > 0)
            {
                attribute = attr[0] as T;
                return true;
            }
            return false;
        }
        private sealed class BindingOverloadFunction : FunctionBase
        {
            public ParameterInfo[] TrueParameters { get; set; }
            public Action<object[]> VoidFunc { get; set; }
            public Func<object[], object> ObjFunc { get; set; }
            public bool IsVoidFunc { get; set; }
            public bool TryConvertParameters(List<Variable> args, out object[] converted)
            {
                converted = null;

                var parametors = new List<object>(args.Count);
                if (args.Count > TrueParameters.Length)
                {
                    //入力の引数の方が多い場合
                    return false;
                }
                for (int i = 0; i < TrueParameters.Length; i++)
                {
                    if (i > args.Count - 1)
                    {
                        //マッチしたい引数の数の方が多い場合
                        if (TrueParameters[i].IsOptional)
                        {
                            parametors.Add(TrueParameters[i].DefaultValue);
                            continue;
                        }
                        else
                        {
                            return false;
                        }
                    }

                    if (args[i].TryConvertTo(TrueParameters[i].ParameterType, out var result))
                    {
                        parametors.Add(result);
                    }
                    else
                    {
                        return false;
                    }
                }
                converted = parametors.ToArray();
                return true;
            }
        }
        private HashSet<BindingOverloadFunction> Overloads = new HashSet<BindingOverloadFunction>();

    }
}
