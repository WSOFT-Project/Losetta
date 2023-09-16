﻿using AliceScript.Functions;
using AliceScript.NameSpaces;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

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

        /// <summary>
        /// 指定された型で公開されている静的メソッドをバインドし、名前空間を返します。
        /// </summary>
        /// <param name="type">バインドの対象となる型</param>
        /// <returns>バインド済み関数が所属する名前空間</returns>
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
                var func = CreateBindFunction(mi, needbind);
                if (func != null)
                {
                    space.Add(func);
                }
            }
            return space;
        }
        /// <summary>
        /// メソッドからBindFunctionを生成
        /// </summary>
        /// <param name="methodInfos">同じメソッド名のオーバーロード</param>
        /// <param name="needBind">このメソッドをバインドするには属性が必要</param>
        /// <returns>生成されたFunctionBase</returns>
        private static FunctionBase CreateBindFunction(HashSet<MethodInfo> methodInfos, bool needBind)
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

                if (load.TrueParameters.Length > 0)
                {
                    load.HasParams = load.TrueParameters[^1].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
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
        /// <summary>
        /// 任意の静的メソッドを表すオブジェクト
        /// </summary>
        private sealed class BindingOverloadFunction : IComparable<BindingOverloadFunction>
        {
            public FunctionAttribute Attribute { get; set; }
            /// <summary>
            /// パラメーターの最期がparamsの場合にtrue
            /// </summary>
            public bool HasParams { get; set; }

            /// <summary>
            /// この関数に必要な引数の最小個数
            /// </summary>
            public int MinimumArgCounts { get; set; }
            public ParameterInfo[] TrueParameters { get; set; }
            public Action<object[]> VoidFunc { get; set; }
            public Func<object[], object> ObjFunc { get; set; }
            public bool IsVoidFunc { get; set; }

            public int CompareTo(BindingOverloadFunction other)
            {
                int result = MinimumArgCounts.CompareTo(other.MinimumArgCounts);

                if (other.HasParams || result == 0)
                {
                    result = -1;
                }

                return result;
            }

            public bool TryConvertParameters(List<Variable> args, out object[] converted)
            {
                converted = null;

                var parametors = new List<object>(args.Count);
                ArrayList paramsList = null;
                bool inParams = false;
                Type paramType = null;

                if (!HasParams && args.Count > TrueParameters.Length)
                {
                    //入力の引数の方が多い場合かつparamsではない場合
                    return false;
                }
                int i;
                for (i = 0; i < TrueParameters.Length; i++)
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
                            //規定値がないためマッチしない
                            return false;
                        }
                    }

                    paramType = TrueParameters[i].ParameterType;
                    if (HasParams && i == TrueParameters.Length - 1 && paramType.IsArray && args[i].Type != Variable.VarType.ARRAY)
                    {
                        //この引数が最後の場合で、それがparamsの場合かつ、配列として渡されていない場合
                        paramType = paramType.GetElementType();
                        paramsList = new ArrayList();
                        inParams = true;
                    }

                    if (args[i].TryConvertTo(paramType, out var result))
                    {
                        if (inParams)
                        {
                            paramsList.Add(result);
                        }
                        else
                        {
                            parametors.Add(result);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                for (; i < args.Count; i++)
                {
                    //paramsでまだ指定したい変数がある場合
                    if (inParams)
                    {
                        if (args[i].TryConvertTo(paramType, out var result))
                        {
                            paramsList.Add(result);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                if (inParams)
                {
                    parametors.Add(paramsList.ToArray(paramType));
                }
                converted = parametors.ToArray();
                return true;
            }
        }
        private SortedSet<BindingOverloadFunction> Overloads = new SortedSet<BindingOverloadFunction>();

    }
}
