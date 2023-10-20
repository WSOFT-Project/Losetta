using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AliceScript.Binding
{
    /// <summary>
    /// .NETのメソッドと対応するAliceScriptの関数
    /// </summary>
    public class BindFunction : FunctionBase
    {
        public BindFunction()
        {
            Run += BindFunction_Run;
        }

        private void BindFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool wantMethod = e.CurentVariable != null;
            foreach (var load in Overloads)
            {
                if ((!wantMethod || load.IsMethod) && load.TryConvertParameters(e, this, out var args))
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

        /// <summary>
        /// メソッドからBindFunctionを生成
        /// </summary>
        /// <param name="methodInfos">同じメソッド名のオーバーロード</param>
        /// <param name="needBind">このメソッドをバインドするには属性が必要</param>
        /// <returns>生成されたFunctionBase</returns>
        internal static BindFunction CreateBindFunction(HashSet<MethodInfo> methodInfos, bool needBind)
        {
            var func = new BindFunction();
            foreach (var methodInfo in methodInfos)
            {
                string name = methodInfo.Name;
                if (TryGetAttibutte<AliceFunctionAttribute>(methodInfo, out var attribute))
                {
                    if (attribute.Name != null)
                    {
                        name = attribute.Name;
                    }
                    if (attribute.MethodOnly)
                    {
                        func.MethodOnly = true;
                    }
                    func.Attribute = attribute.Attribute;
                }
                else if (needBind)
                {
                    return null;
                }

                func.Name = name;
                var load = new BindingOverloadFunction
                {
                    TrueParameters = methodInfo.GetParameters()
                };

                if (load.TrueParameters.Length > 0)
                {
                    load.HasParams = load.TrueParameters[^1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    load.IsMethod = methodInfo.IsDefined(typeof(ExtensionAttribute), true);
                    func.RequestType = load.IsMethod ? new TypeObject() : null;
                }
                int i = 0;
                for (; i < load.TrueParameters.Length; i++)
                {
                    if (load.TrueParameters[i].HasDefaultValue)
                    {
                        break;
                    }
                }
                load.MinimumArgCounts = i;

                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), Utils.GetTrueParametor(x.ParameterType))).ToArray();
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
        internal static bool TryGetAttibutte<T>(MemberInfo memberInfo, out T attribute) where T : Attribute
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

        private SortedSet<BindingOverloadFunction> Overloads = new SortedSet<BindingOverloadFunction>();
    }
}
