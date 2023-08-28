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
            foreach (var m in type.GetMethods())
            {
                var func = CreateBindingFunction(m, needbind);
                if (func != null)
                {
                    space.Add(func);
                }
            }
            return space;
        }
        private static FunctionBase CreateBindingFunction(MethodInfo methodInfo, bool needBind)
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

            var func = new BindingFunction();
            func.Name = name;
            func.TrueParameters = methodInfo.GetParameters();

            var args = Expression.Parameter(typeof(object[]), "args");
            var parameters = func.TrueParameters.Select((x, index) =>
            Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(index)), x.ParameterType)).ToArray();

            Func<object[], object> lambda = null;
            Action<object[]> lambda2 = null;
            if (methodInfo.ReturnType == typeof(void))
            {
                lambda2 = Expression.Lambda<Action<object[]>>(
                Expression.Convert(
                    Expression.Call(methodInfo, parameters),
                    typeof(void)),
                args).Compile();
            }
            else
            {
                lambda = Expression.Lambda<Func<object[], object>>(
                Expression.Convert(
                    Expression.Call(methodInfo, parameters),
                    typeof(object)),
                args).Compile();
            }

            func.Run += delegate (object sender, FunctionBaseEventArgs e)
            {
                var parametors = new List<object>(e.Args.Count);

                if (e.Args.Count != func.TrueParameters.Length)
                {
                    throw new ScriptException($"`{func.Name}`は{func.TrueParameters.Length}個の引数をとる必要があります", Exceptions.INCOMPLETE_ARGUMENTS);
                }
                for (int i = 0; i < func.TrueParameters.Length; i++)
                {
                    parametors.Add(e.Args[i].ConvertTo(func.TrueParameters[i].ParameterType));
                }

                // これでふつーのInvokeで呼び出せるように！
                if (lambda2 != null)
                {
                    lambda2.Invoke(parametors.ToArray());
                    e.Return = Variable.EmptyInstance;
                }
                else
                {
                    e.Return = Variable.ConvetFrom(lambda.Invoke(parametors.ToArray()));
                };
            };
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
        private ParameterInfo[] TrueParameters { get; set; }
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
