﻿using AliceScript.Binding;
using AliceScript.NameSpaces;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AliceScript
{
    public static partial class Utils
    {
        /// <summary>
        /// 指定された型で公開されている静的メソッドとプロパティをバインドし、名前空間を返します。
        /// </summary>
        /// <param name="type">バインドの対象となる型</param>
        /// <returns>バインド済み関数が所属する名前空間</returns>
        public static NameSpace BindToNameSpace(Type type)
        {
            var space = new NameSpace(type.Name);
            bool needbind = false;
            if (BindFunction.TryGetAttibutte<AliceNameSpaceAttribute>(type, out var attribute))
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
                if (m.IsPublic && m.IsStatic && !m.IsDefined(typeof(CompilerGeneratedAttribute)))
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
            foreach (var p in type.GetProperties())
            {
                var prop = CreateBindFunction(p);
                if (prop != null)
                {
                    space.Add(prop);
                }
            }
            return space;
        }
        /// <summary>
        /// 指定されたオーバーライドを含むメソッドのリストをバインドし、関数を返します。
        /// </summary>
        /// <param name="methodInfos">オーバーライドを含むメソッドのリスト</param>
        /// <param name="needBind">メソッドにAliceMethod属性が必要かを表す値</param>
        /// <returns>バインドされた関数</returns>
        public static BindFunction CreateBindFunction(IEnumerable<MethodInfo> methodInfos, bool needBind = false)
        {
            return BindFunction.CreateBindFunction(methodInfos.ToHashSet(), needBind);
        }
        /// <summary>
        /// 指定されたメソッドをバインドし、関数を返します。
        /// </summary>
        /// <param name="methodInfo">バインドしたいメソッド</param>
        /// <param name="needBind">メソッドにAliceMethod属性が必要かを表す値</param>
        /// <returns>バインドされた関数</returns>
        public static BindFunction CreateBindFunction(MethodInfo methodInfo, bool needBind = false)
        {
            return BindFunction.CreateBindFunction(new HashSet<MethodInfo> { methodInfo }, needBind);
        }
        /// <summary>
        /// 指定された情報に一致するC-Style関数をバインドし、AliceScriptの関数を返します。
        /// </summary>
        /// <param name="procName">関数の名前</param>
        /// <param name="libraryFile">関数が定義されているファイル</param>
        /// <param name="returnType">関数の戻り値の型を表す文字列</param>
        /// <param name="parameterTypes">関数の引数の型を表す文字列のリスト</param>
        /// <param name="entryPoint">関数があるエントリポイント</param>
        /// <param name="useUnicode">呼び出しにUnicodeを使用する場合はtrue、ANSIを使用する場合はfalse、自動判別する場合はnull</param>
        /// <returns>バインドされた関数</returns>
        /// <exception cref="ScriptException">適切な関数が見つからなかった場合に発生する例外</exception>
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


            return CreateBindFunction(method);
        }

        /// <summary>
        /// 指定されたプロパティをバインドし、関数を返します。
        /// </summary>
        /// <param name="methodInfo">バインドしたいプロパティ</param>
        /// <returns>バインドされた関数</returns>
        public static BindValueFunction CreateBindFunction(PropertyInfo propertyInfo)
        {
            var func = new BindValueFunction();
            func.Name = propertyInfo.Name;
            func.HandleEvents = true;

            var getFunc = propertyInfo.GetGetMethod();
            var setFunc = propertyInfo.GetSetMethod();

            if (BindFunction.TryGetAttibutte<AlicePropertyAttribute>(propertyInfo, out var attribute))
            {
                if (attribute.Name != null)
                {
                    func.Name = attribute.Name;
                }
            }

            if (setFunc != null && setFunc.IsPublic)
            {
                func.CanSet = true;
                var load = new BindingOverloadFunction();
                load.TrueParameters = setFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();
                load.VoidFunc = Expression.Lambda<Action<object[]>>(
                Expression.Convert(
                    Expression.Call(setFunc, parameters),
                    typeof(void)),
                args).Compile();
                load.IsVoidFunc = true;

                func.Set = load;
            }
            if (getFunc != null && getFunc.IsPublic)
            {
                var load = new BindingOverloadFunction();
                load.TrueParameters = getFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();

                load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                    Expression.Convert(
                        Expression.Call(getFunc, parameters),
                        typeof(object)),
                    args).Compile();

                func.Get = load;
            }
            return func;
        }

        internal static Type GetTrueParametor(Type t)
        {
            return t.IsByRef ? t.GetElementType() : t;
        }
    }
}