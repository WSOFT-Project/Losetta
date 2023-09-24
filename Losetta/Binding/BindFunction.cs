using AliceScript.Functions;
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

            return BindFunction.CreateBindFunction(method);
        }
        public static BindFunction CreateBindFunction(MethodInfo methodInfo,bool needBind = false)
        {
            return CreateBindFunction(new HashSet<MethodInfo> { methodInfo},needBind);
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
            public bool IsMethod { get; set; }

            public int CompareTo(BindingOverloadFunction other)
            {
                int result = MinimumArgCounts.CompareTo(other.MinimumArgCounts);

                if (result == 0)
                {
                    // 比較に困る場合、引数のVariableCollection(なんでも配列型)の数で比べる
                    int? r = TrueParameters.Where(item => item.ParameterType == typeof(VariableCollection))?.Count().CompareTo(other.TrueParameters.Where(item => item.ParameterType == typeof(VariableCollection))?.Count());
                    result = r.HasValue ? r.Value : result;
                }
                if (result == 0)
                {
                    // それでも比較に困る場合、引数のVariable(なんでも型)の数で比べる
                    int? r = TrueParameters.Where(item => item.ParameterType == typeof(Variable))?.Count().CompareTo(other.TrueParameters.Where(item => item.ParameterType == typeof(Variable))?.Count());
                    result = r.HasValue ? r.Value : result;
                }
                if (other.HasParams || result == 0)
                {
                    result = -1;
                }

                return result;
            }

            public bool TryConvertParameters(FunctionBaseEventArgs e, BindFunction parent, out object[] converted)
            {
                converted = null;

                var parametors = new List<object>(e.Args.Count);
                ArrayList paramsList = null;
                bool inParams = false;
                Type paramType = null;

                if (!HasParams && e.Args.Count + (parent.IsMethod && e.CurentVariable != null ? 1 : 0) > TrueParameters.Length)
                {
                    //入力の引数の方が多い場合かつparamsではない場合
                    return false;
                }
                int i;
                int diff = 0;//TrueParametersとargsのインデックスのずれ
                for (i = 0; i < TrueParameters.Length; i++)
                {
                    if (TrueParameters[i].ParameterType == typeof(FunctionBaseEventArgs))
                    {
                        diff++;
                        parametors.Add(e);
                        continue;
                    }
                    if (TrueParameters[i].ParameterType == typeof(ParsingScript))
                    {
                        diff++;
                        parametors.Add(e.Script);
                        continue;
                    }
                    if (TrueParameters[i].ParameterType == typeof(BindFunction))
                    {
                        diff++;
                        parametors.Add(parent);
                        continue;
                    }
                    if (TrueParameters[i].ParameterType == typeof(BindingOverloadFunction))
                    {
                        diff++;
                        parametors.Add(this);
                        continue;
                    }

                    paramType = TrueParameters[i].ParameterType;
                    if (i == 0 && IsMethod && e.CurentVariable != null)
                    {
                        diff++;
                        if (e.CurentVariable.TryConvertTo(paramType, out var r))
                        {
                            parametors.Add(r);
                        }
                        else
                        {
                            return false;
                        }
                        continue;
                    }

                    if (i > e.Args.Count + diff - 1)
                    {
                        //引数が足りない場合
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

                    if (HasParams && i == TrueParameters.Length - 1 && paramType.IsArray && e.Args[i - diff].Type != Variable.VarType.ARRAY)
                    {
                        //この引数が最後の場合で、それがparamsの場合かつ、配列として渡されていない場合
                        paramType = paramType.GetElementType();
                        paramsList = new ArrayList();
                        inParams = true;
                    }

                    var item = e.Args[i - diff];
                    if (item == null)
                    {
                        if (inParams)
                        {
                            paramsList.Add(item);
                        }
                        else
                        {
                            parametors.Add(item);
                        }
                    }
                    else if (item.TryConvertTo(paramType, out var result))
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

                for (; i - diff < e.Args.Count; i++)
                {
                    //paramsでまだ指定したい変数がある場合
                    if (inParams)
                    {
                        var item = e.Args[i - diff];
                        if (item == null)
                        {
                            if (inParams)
                            {
                                paramsList.Add(item);
                            }
                            else
                            {
                                parametors.Add(item);
                            }
                        }
                        else if (item.TryConvertTo(paramType, out var result))
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
