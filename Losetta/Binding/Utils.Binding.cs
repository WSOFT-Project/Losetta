using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (TryGetAttibutte<AliceNameSpaceAttribute>(type, out var attribute))
            {
                if (attribute.Name is not null)
                {
                    space.Name = attribute.Name;
                }
                needbind = attribute.DefaultState != AliceBindState.Enabled;
            }
            Dictionary<string, HashSet<MethodInfo>> methods = new Dictionary<string, HashSet<MethodInfo>>();
            foreach (var m in type.GetMethods())
            {
                if (m.IsPublic && m.IsStatic && !m.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    string methodName = m.Name.ToLowerInvariant();
                    if (!methods.ContainsKey(methodName))
                    {
                        methods[methodName] = new HashSet<MethodInfo>();
                    }
                    methods[methodName].Add(m);
                }
            }
            foreach (HashSet<MethodInfo> mi in methods.Values)
            {
                var func = CreateBindFunction(mi, needbind);
                if (func is not null)
                {
                    space.Add(func);
                }
            }
            foreach (var p in type.GetProperties())
            {
                var prop = CreateBindFunction(p, needbind);
                if (prop is not null)
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
                throw new ScriptException("外部に適切に定義された関数が見つかりませんでした", Exceptions.IDENTIFIER_NOT_FOUND);
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
        /// <param name="propertyInfo">バインドしたいプロパティ</param>
        /// <param name="needBind">メソッドにAliceMethod属性が必要かを表す値</param>
        /// <param name="staticOnly">静的メソッドのみをバインドする場合はtrue、通常のメソッドのみをバインドする場合はfalse</param>
        /// <returns>バインドされた関数</returns>
        public static BindValueFunction CreateBindFunction(PropertyInfo propertyInfo, bool needBind, bool staticOnly = true)
        {
            var func = new BindValueFunction();
            func.Name = propertyInfo.Name;
            func.HandleEvents = true;

            var getFunc = propertyInfo.GetGetMethod();
            var setFunc = propertyInfo.GetSetMethod();

            if (TryGetAttibutte<AlicePropertyAttribute>(propertyInfo, out var attribute))
            {
                if (attribute.Name is not null)
                {
                    func.Name = attribute.Name;
                }
                if (attribute.State == AliceBindState.Disabled)
                {
                    return null;
                }
            }
            else if (needBind)
            {
                return null;
            }

            if (setFunc is not null && setFunc.IsPublic && (staticOnly == setFunc.IsStatic) && TryGetAttibutte<AliceFunctionAttribute>(setFunc, out var attrS, true) && attrS.State == AliceBindState.Enabled)
            {
                func.CanSet = true;
                var load = new BindingOverloadFunction();
                load.TrueParameters = setFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var instance = Expression.Parameter(typeof(object), "instance");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();
                if (setFunc.IsStatic)
                {
                    load.VoidFunc = Expression.Lambda<Action<object[]>>(
                    Expression.Convert(
                        Expression.Call(setFunc, parameters),
                        typeof(void)),
                    args).Compile();
                }
                else
                {
                    load.InstanceVoidFunc = Expression.Lambda<Action<object, object[]>>(
                        Expression.Convert(
                            Expression.Call(Expression.Convert(instance, propertyInfo.DeclaringType), setFunc, parameters),
                            typeof(void)),
                        instance, args).Compile();
                    load.IsInstanceFunc = true;
                }
                load.IsVoidFunc = true;

                func.Set = load;
            }

            if (getFunc is not null && getFunc.IsPublic && (staticOnly == getFunc.IsStatic) && TryGetAttibutte<AliceFunctionAttribute>(getFunc, out var attrG, true) && attrG.State == AliceBindState.Enabled)
            {
                var load = new BindingOverloadFunction();
                load.TrueParameters = getFunc.GetParameters();
                var args = Expression.Parameter(typeof(object[]), "args");
                var instance = Expression.Parameter(typeof(object), "instance");
                var parameters = load.TrueParameters.Select((x, index) =>
                Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), GetTrueParametor(x.ParameterType))).ToArray();

                if (getFunc.IsStatic)
                {
                    load.ObjFunc = Expression.Lambda<Func<object[], object>>(
                    Expression.Convert(
                        Expression.Call(getFunc, parameters),
                        typeof(object)),
                    args).Compile();
                }
                else
                {
                    load.InstanceObjFunc = Expression.Lambda<Func<object, object[], object>>(
                        Expression.Convert(
                            Expression.Call(Expression.Convert(instance, propertyInfo.DeclaringType), getFunc, parameters),
                            typeof(object)),
                        instance, args).Compile();
                    load.IsInstanceFunc = true;
                }

                func.Get = load;
            }
            return func;
        }
        /// <summary>
        /// 指定された型にある関数やプロパティをバインドし、BindObjectを返します
        /// </summary>
        /// <param name="type">バインドしたい型</param>
        /// <returns>型がバインドされたBindObject</returns>
        public static BindObject CreateBindObject(Type type)
        {
            var obj = new BindObject();
            obj.Name = type.Name;
            obj.Type = type;

            bool defaultState = true;

            if (TryGetAttibutte<AliceObjectAttribute>(type, out var attr))
            {
                obj.Namespace = attr.NameSpace;
                if (!string.IsNullOrEmpty(attr.Name))
                {
                    obj.Name = attr.Name;
                }
                defaultState = attr.DefaultState == AliceBindState.Enabled;
            }
            Dictionary<string, HashSet<MethodInfo>> methods = new Dictionary<string, HashSet<MethodInfo>>();
            Dictionary<string, HashSet<MethodInfo>> staticmethods = new Dictionary<string, HashSet<MethodInfo>>();
            Dictionary<string, HashSet<MethodInfo>> operators = new Dictionary<string, HashSet<MethodInfo>>();
            foreach (var m in type.GetMethods())
            {
                if (m.IsPublic && !m.IsDefined(typeof(CompilerGeneratedAttribute)))
                {
                    if (TryGetAttibutte<AliceObjectOperatorAttribute>(m, out var opattr))
                    {
                        if (!operators.ContainsKey(opattr.Operator))
                        {
                            operators[opattr.Operator] = new HashSet<MethodInfo>();
                        }
                        operators[opattr.Operator].Add(m);
                    }
                    else if (m.IsStatic)
                    {
                        if (!staticmethods.ContainsKey(m.Name))
                        {
                            staticmethods[m.Name] = new HashSet<MethodInfo>();
                        }
                        staticmethods[m.Name].Add(m);
                    }
                    else
                    {
                        if (!methods.ContainsKey(m.Name))
                        {
                            methods[m.Name] = new HashSet<MethodInfo>();
                        }
                        methods[m.Name].Add(m);
                    }
                }
            }
            foreach (HashSet<MethodInfo> mi in methods.Values)
            {
                var func = CreateBindFunction(mi, !defaultState);
                if (func is not null)
                {
                    func.Parent = obj;
                    obj.AddFunction(func);
                }
            }
            foreach (HashSet<MethodInfo> mi in staticmethods.Values)
            {
                var func = CreateBindFunction(mi);
                if (func is not null)
                {
                    func.Parent = obj;
                    obj.StaticFunctions[func.Name] = func;
                }
            }
            foreach (KeyValuePair<string, HashSet<MethodInfo>> mkv in operators)
            {
                var func = CreateBindFunction(mkv.Value);
                if (func is not null)
                {
                    func.Parent = obj;
                    func.Name = $"operator{mkv.Key}";
                    obj.Operators[mkv.Key] = func;
                }
            }
            foreach (var p in type.GetProperties())
            {
                var prop = CreateBindFunction(p, !defaultState, false);
                if (prop is not null)
                {
                    prop.Parent = obj;
                    obj.AddFunction(prop);
                }
            }
            foreach (var p in type.GetProperties())
            {
                var prop = CreateBindFunction(p, !defaultState, true);
                if (prop is not null)
                {
                    prop.Parent = obj;
                    obj.StaticFunctions[prop.Name] = prop;
                }
            }
            obj.Constructor = BindFunction.CreateBindConstructor(type.GetConstructors(), !defaultState);
            if (obj.Constructor is not null)
            {
                obj.Constructor.Run += delegate (object sender, Functions.FunctionBaseEventArgs e)
                {
                    obj.Instance = e.Return.AsObject();
                };
            }

            return obj;
        }
        /// <summary>
        /// 引数リストからオーバーロードの優先順位を計算します
        /// </summary>
        /// <param name="parameters">オーバーロードの引数リスト</param>
        /// <returns>優先順位</returns>
        internal static uint CalcPriority(ParameterInfo[] parameters)
        {
            uint priority = 0;
            foreach (var param in parameters)
            {
                // この引数のポイント
                uint raw = 5;
                if (param.ParameterType == typeof(char))
                {
                    // charはstring(ほかの)よりも優先順位が高い
                    raw = 6;
                }
                else if (param.ParameterType == typeof(VariableCollection))
                {
                    raw = 4;
                }
                else if (param.ParameterType == typeof(Variable))
                {
                    raw = 2;
                }
                if (param.IsOptional)
                {
                    // 必須ではない引数は減点
                    raw--;
                }
                if (param.CustomAttributes.Any(attr => attr.AttributeType == typeof(BindInfoAttribute)))
                {
                    // BindInfoの場合は減点
                    raw = 1;
                }
                priority += raw;
            }
            return priority;
        }

        /// <summary>
        /// 指定されたメンバーに属性があるかどうかを取得します
        /// </summary>
        /// <typeparam name="T">想定する属性</typeparam>
        /// <param name="memberInfo">取得元のメンバー</param>
        /// <param name="attribute">属性があった場合はそのインスタンス、ない場合は<paramref name="createNew"/>の動作による</param>
        /// <param name="createNew">属性がない場合に新たに作成してtrueを返す場合はtrue、そうでない場合はfalse</param>
        /// <returns>属性があるか作成した場合はtrue、そうでない場合はfalse</returns>
        internal static bool TryGetAttibutte<T>(MemberInfo memberInfo, out T attribute, bool createNew = false) where T : Attribute, new()
        {
            attribute = null;
            var attr = Attribute.GetCustomAttributes(memberInfo, typeof(T));
            if (attr.Length > 0)
            {
                attribute = attr[0] as T;
                return true;
            }
            if (createNew)
            {
                attribute = new T();
                return true;
            }
            return false;
        }

        /// <summary>
        /// ref引数の場合、refの中身の型を取得します
        /// </summary>
        /// <param name="t">ref引数になる可能性のある型</param>
        /// <returns><paramref name="t"/>がref引数の場合はその中身の型、そうでない場合は<paramref name="t"/></returns>
        internal static Type GetTrueParametor(Type t)
        {
            return t.IsByRef ? t.GetElementType() : t;
        }
        internal static Delegate ConvertToDelegate(Type actionType, DelegateObject d, ParsingScript script)
        {
            if (actionType == typeof(Action))
            {
                return (Action)(() => d.Invoke(script));
            }
            if (actionType.GetGenericTypeDefinition() == typeof(Action<>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Action<,,,,,,,,,,,,,,,>))
            {
                return CreateTypedAction(actionType.GenericTypeArguments, args => d.Invoke(args.Select(a => Variable.From(a)).ToArray(), script, null));
            }
            if (actionType.GetGenericTypeDefinition() == typeof(Func<>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,>) ||
                actionType.GetGenericTypeDefinition() == typeof(Func<,,,,,,,,,,,,,,,>))
            {
                return CreateTypedFunc(actionType.GenericTypeArguments[0..^1], actionType.GenericTypeArguments[^1], args => d.Invoke(args.Select(a => Variable.From(a)).ToArray(), script, null).ConvertTo(actionType.GenericTypeArguments[^1]));
            }
            return null;
        }
        /// <summary>
        /// 各パラメータの型情報とActionの実装から、Action T1, T2...型にキャストできるデリゲートを生成します。パラメータは1~16個指定できます。
        /// </summary>
        /// <param name="parameterTypes">各ジェネリクスパラメータ</param>
        /// <param name="actionImpl">Actionの実装</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        internal static Delegate CreateTypedAction(Type[] parameterTypes, Action<object[]> actionImpl)
        {
            if (parameterTypes == null || parameterTypes.Length == 0)
                throw new ArgumentException("少なくとも1つのパラメータ型が必要です", nameof(parameterTypes));

            // パラメータ式の配列を作成
            ParameterExpression[] parameters = parameterTypes
                .Select((type, index) => Expression.Parameter(type, $"param{index}"))
                .ToArray();

            // パラメータをobject[]にパックする式を作成
            Expression[] parameterConversions = parameters
                .Select(param => Expression.Convert(param, typeof(object)))
                .ToArray();

            // object[]を作成する式
            NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), parameterConversions);

            // アクション実装を呼び出す式
            MethodCallExpression actionCall = Expression.Call(
                Expression.Constant(actionImpl.Target),
                actionImpl.Method,
                argsArray);

            Type actionType = GetActionType(parameterTypes);

            LambdaExpression lambda = Expression.Lambda(actionType, actionCall, parameters);
            return lambda.Compile();
        }
        internal static Delegate CreateTypedFunc(Type[] parameterTypes, Type returnType, Func<object[], object> funcImpl)
        {
            // パラメータ式の配列を作成
            ParameterExpression[] parameters = parameterTypes
                .Select((type, index) => Expression.Parameter(type, $"param{index}"))
                .ToArray();

            // パラメータをobject[]にパックする式を作成
            Expression[] parameterConversions = parameters
                .Select(param => Expression.Convert(param, typeof(object)))
                .ToArray();

            // object[]を作成する式
            NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), parameterConversions);

            // アクション実装を呼び出す式
            MethodCallExpression actionCall = Expression.Call(
                Expression.Constant(funcImpl.Target),
                funcImpl.Method,
                argsArray);

            // 戻り値を指定された型に変換する式
            Expression convertedReturnValue = Expression.Convert(actionCall, returnType);

            Type actionType = GetFuncType(parameterTypes, returnType);

            LambdaExpression lambda = Expression.Lambda(actionType, convertedReturnValue, parameters);
            return lambda.Compile();
        }

        /// <summary>
        /// 指定されたパラメータ型に基づいてAction T1, T2, ... を取得します
        /// </summary>
        private static Type GetActionType(Type[] parameterTypes)
        {
            if (parameterTypes.Length == 1)
                return typeof(Action<>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 2)
                return typeof(Action<,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 3)
                return typeof(Action<,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 4)
                return typeof(Action<,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 5)
                return typeof(Action<,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 6)
                return typeof(Action<,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 7)
                return typeof(Action<,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 8)
                return typeof(Action<,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 9)
                return typeof(Action<,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 10)
                return typeof(Action<,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 11)
                return typeof(Action<,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 12)
                return typeof(Action<,,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 13)
                return typeof(Action<,,,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 14)
                return typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 15)
                return typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else if (parameterTypes.Length == 16)
                return typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(parameterTypes);
            else
                throw new ArgumentException($"パラメータ数 {parameterTypes.Length} はサポートされていません。16個までのパラメータがサポートされています。");
        }
        private static Type GetFuncType(Type[] parameterTypes, Type returnType)
        {
            Type[] genericTypes = [.. parameterTypes, returnType];
            if (genericTypes.Length == 1)
                return typeof(Func<>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 2)
                return typeof(Func<,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 3)
                return typeof(Func<,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 4)
                return typeof(Func<,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 5)
                return typeof(Func<,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 6)
                return typeof(Func<,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 7)
                return typeof(Func<,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 8)
                return typeof(Func<,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 9)
                return typeof(Func<,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 10)
                return typeof(Func<,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 11)
                return typeof(Func<,,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 12)
                return typeof(Func<,,,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 13)
                return typeof(Func<,,,,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 14)
                return typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 15)
                return typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(genericTypes);
            else if (genericTypes.Length == 16)
                return typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes);
            else
                throw new ArgumentException($"パラメータ数 {genericTypes.Length} はサポートされていません。16個までのパラメータがサポートされています。");
        }
    }
}
