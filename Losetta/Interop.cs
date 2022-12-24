using System.Reflection;

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
            catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.FILE_NOT_FOUND); }

        }
        public static void LoadLibrary(byte[] rawassembly)
        {
            try
            {
                string ipluginName = typeof(ILibrary).FullName;
                //アセンブリとして読み込む
                System.Reflection.Assembly asm =
                    System.Reflection.Assembly.Load(rawassembly);
                foreach (Type t in asm.GetTypes())
                {
                    try
                    {
                        //アセンブリ内のすべての型について、
                        //プラグインとして有効か調べる
                        if (t.IsClass && t.IsPublic && !t.IsAbstract)
                        {
                            if (t.GetInterface(ipluginName) != null)
                            {
                                if (!Loadeds.Contains(asm.GetHashCode()))
                                {
                                    Loadeds.Add(asm.GetHashCode());
                                    ((ILibrary)asm.CreateInstance(t.FullName)).Main();
                                }
                            }
                            else if (t.IsSubclassOf(typeof(ObjectBase)) && t.GetCustomAttribute(typeof(ScriptClass)) != null)
                            {

                            }
                        }
                    }
                    catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.LIBRARY_EXCEPTION); }
                }
            }
            catch (Exception ex) { ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.LIBRARY_EXCEPTION); }

        }
        private static List<int> Loadeds = new List<int>();
    }
    public static class GCManerger
    {
        public static bool CollectAfterExecute = false;
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
