﻿using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Parsing;
using System.Collections;
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
                if (m.IsPublic && m.IsStatic && !(m.IsDefined(typeof(CompilerGeneratedAttribute))))
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
            foreach(var p in type.GetProperties())
            {
                var prop = BindValueFunction.CreateBindValueFunction(p);
                if(prop != null)
                {
                    space.Add(prop);
                }
            }
            return space;
        }
        public static BindFunction CreateExternBindFunction(string procName, string libraryFile, string returnType, string[] parameterTypes, string entryPoint = null, bool? useUnicode = null)
        {
            string moduleName = Path.GetFileNameWithoutExtension(libraryFile.ToUpper());
            AssemblyBuilder asmBld = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName("Invoke_Asm" + moduleName), AssemblyBuilderAccess.Run);

            ModuleBuilder modBld = asmBld.DefineDynamicModule(
                "Invoke_Mod" + moduleName);

            TypeBuilder typBld = modBld.DefineType(
                "Invoke_Class" + moduleName,
                TypeAttributes.Public | TypeAttributes.Class);

            MethodBuilder methodBuilder = typBld.DefinePInvokeMethod(
                procName, libraryFile, entryPoint ?? procName,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl | MethodAttributes.HideBySig, CallingConventions.Standard,
                Constants.InvokeStringToType(returnType), Constants.InvokeStringToType(parameterTypes),
                CallingConvention.StdCall,
                useUnicode.HasValue ? useUnicode.Value ? CharSet.Unicode : CharSet.Ansi : CharSet.Auto);
            methodBuilder.SetImplementationFlags(MethodImplAttributes.PreserveSig);

            typBld.CreateType().GetMethod(procName);

            MethodInfo method = typBld.CreateType().GetMethod(procName);

            try
            {
                // 一度正しく呼び出せるか試してみる
                method.Invoke(null, null);
            }
            catch (TargetInvocationException)
            {
                throw new ScriptException("外部に適切に定義された関数が見つかりませんでした", Exceptions.COULDNT_FIND_VARIABLE);
            }
            catch
            {
                // 他の例外の場合は呼び出せた
            }


            return BindFunction.CreateBindFunction(method);
        }
        public static BindFunction CreateBindFunction(MethodInfo methodInfo, bool needBind = false)
        {
            return CreateBindFunction(new HashSet<MethodInfo> { methodInfo }, needBind);
        }
        /// <summary>
        /// メソッドからBindFunctionを生成
        /// </summary>
        /// <param name="methodInfos">同じメソッド名のオーバーロード</param>
        /// <param name="needBind">このメソッドをバインドするには属性が必要</param>
        /// <returns>生成されたFunctionBase</returns>
        private static BindFunction CreateBindFunction(HashSet<MethodInfo> methodInfos, bool needBind)
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
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();
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
        internal static Type GetTrueParametor(Type t)
        {
            return t.IsByRef ? t.GetElementType() : t;
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
