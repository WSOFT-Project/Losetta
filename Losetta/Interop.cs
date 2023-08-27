namespace AliceScript.Interop
{
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
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ScriptClass : System.Attribute
    {
        public ScriptClass()
        {

        }
    }
}
