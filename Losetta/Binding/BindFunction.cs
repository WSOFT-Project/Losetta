using AliceScript.Functions;
using AliceScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AliceScript.Binding
{
    /// <summary>
    /// .NETのメソッドと対応するAliceScriptの関数
    /// </summary>
    public class BindFunction : FunctionBase
    {
        /// <summary>
        /// BindFunctionを初期化します
        /// </summary>
        public BindFunction()
        {
            HandleAttributes = new HashSet<ICallingHandleAttribute>();
            Run += BindFunction_Run;
        }

        private void BindFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool wantMethod = e.CurentVariable is not null;
            foreach (var load in Overloads)
            {
                if ((!wantMethod || load.IsMethod) && load.TryConvertParameters(e, this, out var args))
                {
                    if(e.AttributeFunctions?.OfType<TestCallFunction>().FirstOrDefault() is not null)
                    {
                        e.Return = Variable.EmptyInstance;
                        return;
                    }
                    if (load.IsInstanceFunc)
                    {
                        if (Parent?.Instance is not null)
                        {
                            if (load.IsVoidFunc)
                            {
                                load.InstanceVoidFunc.Invoke(Parent?.Instance, args);
                            }
                            else
                            {
                                e.Return = new Variable(load.InstanceObjFunc.Invoke(Parent?.Instance, args));
                            }
                        }
                    }
                    else
                    {
                        if (load.IsVoidFunc)
                        {
                            load.VoidFunc.Invoke(args);
                        }
                        else
                        {
                            e.Return = new Variable(load.ObjFunc.Invoke(args));
                        }
                    }
                    return;
                }
            }
            throw new ScriptException($"`{Name}`に対応するオーバーロードを解決できませんでした", Exceptions.COULDNT_FIND_FUNCTION);
        }

        /// <summary>
        /// メソッドからBindFunctionを生成します
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
                if(Utils.TryGetAttibutte<ObsoleteAttribute>(methodInfo, out var obs))
                {
                    func.HandleAttributes.Add(new ObsoleteFunction(obs.IsError, obs.Message));
                }
                if (Utils.TryGetAttibutte<AliceFunctionAttribute>(methodInfo, out var attribute))
                {
                    if (attribute.State == AliceBindState.Disabled)
                    {
                        continue;
                    }
                    if (attribute.Name is not null)
                    {
                        name = attribute.Name;
                    }
                    if (attribute.MethodOnly)
                    {
                        func.MethodOnly = true;
                    }
                    func.Context = attribute.Context;
                    func.Attribute = attribute.Attribute;
                }
                else if (needBind)
                {
                    return null;
                }

                func.Name = name;
                var load = new BindingOverloadFunction();
                load.TrueParameters = methodInfo.GetParameters();

                if (load.TrueParameters.Length > 0)
                {
                    load.HasParams = load.TrueParameters[^1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
                    load.IsMethod = methodInfo.IsDefined(typeof(ExtensionAttribute), true);
                    load.Priority = Utils.CalcPriority(load.TrueParameters);
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
                var instance = Expression.Parameter(typeof(object), "instance");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), Utils.GetTrueParametor(x.ParameterType))).ToArray();
                if (methodInfo.ReturnType == typeof(void))
                {
                    if (methodInfo.IsStatic)
                    {
                        load.VoidFunc = Expression.Lambda<Action<object[]>>(
                        Expression.Convert(
                            Expression.Call(methodInfo, parameters),
                            typeof(void)),
                        args).Compile();
                    }
                    else
                    {
                        load.InstanceVoidFunc = Expression.Lambda<Action<object, object[]>>(
                        Expression.Convert(
                            Expression.Call(Expression.Convert(instance, methodInfo.DeclaringType), methodInfo, parameters),
                            typeof(void)),
                        instance, args).Compile();
                        load.IsInstanceFunc = true;
                    }
                    load.IsVoidFunc = true;
                }
                else
                {
                    if (methodInfo.IsStatic)
                    {
                        load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                        Expression.Convert(
                            Expression.Call(methodInfo, parameters),
                            typeof(object)),
                        args).Compile();
                    }
                    else
                    {
                        load.InstanceObjFunc = Expression.Lambda<Func<object, object[], object>>(
                        Expression.Convert(
                            Expression.Call(Expression.Convert(instance, methodInfo.DeclaringType), methodInfo, parameters),
                            typeof(object)),
                        instance, args).Compile();
                        load.IsInstanceFunc = true;
                    }
                }
                func.Overloads.Add(load);
            }

            return func.Overloads.Count > 0 ? func : null;
        }
        /// <summary>
        /// コンストラクタからBindFunctionを生成します
        /// </summary>
        /// <param name="constructors">同じクラスのコンストラクタ</param>
        /// <param name="needBind">このメソッドをバインドするには属性が必要</param>
        /// <returns>生成されたFunctionBase</returns>
        internal static BindFunction CreateBindConstructor(ConstructorInfo[] constructors, bool needBind)
        {
            var func = new BindFunction();
            foreach (var methodInfo in constructors)
            {
                string name = methodInfo.Name;
                if (Utils.TryGetAttibutte<ObsoleteAttribute>(methodInfo, out var obs))
                {
                    func.HandleAttributes.Add(new ObsoleteFunction(obs.IsError, obs.Message));
                }
                if (Utils.TryGetAttibutte<AliceFunctionAttribute>(methodInfo, out var attribute))
                {
                    if (attribute.Name is not null)
                    {
                        name = attribute.Name;
                    }
                    if (attribute.MethodOnly)
                    {
                        func.MethodOnly = true;
                    }
                    func.Context = attribute.Context;
                    if (attribute.State == AliceBindState.Disabled)
                    {
                        continue;
                    }
                    func.Attribute = attribute.Attribute;
                }
                else if (needBind)
                {
                    return null;
                }
                func.Name = name;
                var load = new BindingOverloadFunction();
                load.TrueParameters = methodInfo.GetParameters();

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
                var instance = Expression.Parameter(typeof(object), "instance");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), Utils.GetTrueParametor(x.ParameterType))).ToArray();
                load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(
                    Expression.New(methodInfo, parameters),
                    typeof(object)),
                args).Compile();
                func.Overloads.Add(load);
            }

            return func.Overloads.Count > 0 ? func : null;
        }

        private List<BindingOverloadFunction> Overloads = new List<BindingOverloadFunction>();
        /// <summary>
        /// この関数がBindObjectのメソッドの場合、そのオブジェクト
        /// </summary>
        public BindObject Parent { get; set; }
    }
}
