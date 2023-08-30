using System.Linq.Expressions;
using System.Reflection;

namespace AliceScript.Interop
{
    /// <summary>
    /// AliceScriptで使用できる関数として公開するメソッド
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AliceFunctionAttribute : Attribute
    {
        public string Name = null;
    }

    /// <summary>
    /// AliceScriptの名前空間として公開するクラス
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AliceNameSpaceAttribute : Attribute
    {
        public string Name = null;
        public bool NeedBindAttribute = true;
    }
    public class BindingFunction : FunctionBase
    {
        public BindingFunction()
        {
            Run += BindingFunction_Run;
        }

        private void BindingFunction_Run(object sender, FunctionBaseEventArgs e)
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
                        e.Return = Variable.ConvetFrom(load.ObjFunc.Invoke(args));
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
            List<MethodInfo> sameNames = new List<MethodInfo>();
            string prevName = "";
            foreach (var m in type.GetMethods())
            {
                if (m.Name != prevName && sameNames.Count > 0)
                {
                    var func = CreateBindingFunction(sameNames.ToArray(), needbind);
                    if (func != null)
                    {
                        space.Add(func);
                    }
                    sameNames.Clear();
                }
                prevName = m.Name;
                sameNames.Add(m);
            }
            return space;
        }
        private static FunctionBase CreateBindingFunction(MethodInfo[] methodInfos, bool needBind)
        {
            var func = new BindingFunction();
            foreach (var methodInfo in methodInfos)
            {
                string name = methodInfo.Name;
                if (!methodInfo.IsStatic) { return null; }
                if (TryGetAttibutte<AliceFunctionAttribute>(methodInfo, out var attribute) && attribute.Name != null)
                {
                    name = attribute.Name;
                }
                else if (needBind)
                {
                    return null;
                }

                func.Name = name;
                var load = new BindingOverloadFunction();
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

                if (args.Count != TrueParameters.Length)
                {
                    return false;
                }
                for (int i = 0; i < TrueParameters.Length; i++)
                {
                    if(args[i].TryConvertTo(TrueParameters[i].ParameterType,out var result))
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
        private List<BindingOverloadFunction> Overloads = new List<BindingOverloadFunction>();

    }
    public class NetLibraryLoader
    {
        public static void LoadLibrary(string path)
        {
            try
            {
                byte[] raw = File.ReadAllBytes(path);
                LoadLibrary(raw);
            }
            catch (Exception ex) { throw new ScriptException(ex.Message, Exceptions.FILE_NOT_FOUND); }

        }
        public static void LoadLibrary(byte[] rawAssembly)
        {
            try
            {
                string iPluginName = typeof(ILibrary).FullName;

                // アセンブリとして読み込む
                System.Reflection.Assembly asm = System.Reflection.Assembly.Load(rawAssembly);

                foreach (Type type in asm.GetTypes())
                {
                    try
                    {
                        // アセンブリ内のすべての型について、プラグインとして有効か調べる
                        if (type.IsClass && type.IsPublic && !type.IsAbstract)
                        {
                            if (type.GetInterface(iPluginName) != null)
                            {
                                if (!Loadeds.Contains(asm.GetHashCode()))
                                {
                                    Loadeds.Add(asm.GetHashCode());
                                    ILibrary libraryInstance = (ILibrary)asm.CreateInstance(type.FullName);
                                    libraryInstance.Main();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new ScriptException(ex.Message, Exceptions.LIBRARY_EXCEPTION);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ScriptException(ex.Message, Exceptions.LIBRARY_EXCEPTION);
            }
        }

        private static List<int> Loadeds = new List<int>();
    }
    public interface ILibrary
    {
        string Name { get; }
        void Main();
    }

    /// <summary>
    /// ネイティブプラグインの基礎です。このクラスを継承してネイティブプラグインを作成します。
    /// </summary>
    public class LibraryBase : ILibrary
    {
        public virtual void Main()
        {
        }

        public string Name { get; set; }

    }
}
