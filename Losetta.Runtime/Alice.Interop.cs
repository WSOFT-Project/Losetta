using AliceScript.Binding;
using AliceScript.Interop;
using AliceScript.Objects;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Interop
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(InteropFunctions));
        }
    }
    [AliceNameSpace(Name = "Alice.Interop")]
    internal static class InteropFunctions
    {
        public static void Interop_LoadLibrary(string path)
        {
            NetLibraryLoader.LoadLibrary(path);
        }
        public static void Interop_LoadLibrary(byte[] rawAsm)
        {
            NetLibraryLoader.LoadLibrary(rawAsm);
        }
        public static TypeObject Interop_GetType(string typaname, string locate = null)
        {
            var t = Type.GetType(typaname + (locate == null ? string.Empty : "," + locate));
            return t != null ? new TypeObject(Utils.CreateBindObject(t)) : null;
        }
        public static DelegateObject Interop_GetInvoker(string procName, string libraryName, string returnType, string[] parameterTypes, string entryPoint = null, bool? useUnicode = null)
        {
            BindFunction func = Utils.CreateExternBindFunction(procName, libraryName, returnType, parameterTypes, libraryName, useUnicode);
            return new DelegateObject(func);
        }
    }
}
