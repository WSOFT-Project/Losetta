using AliceScript.Binding;
using AliceScript.Interop;
using AliceScript.Objects;
using System.Reflection;

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
        public static TypeObject Interop_GetType(string typeName, string asmName = null, string asmLocate = null)
        {
            Type type = null;
            if (!string.IsNullOrEmpty(asmName))
            {
                typeName += $",{asmName}";
            }
            if (string.IsNullOrEmpty(asmLocate))
            {
                type = Type.GetType(typeName);
            }
            else
            {
                var asm = Assembly.LoadFrom(asmLocate);
                type = asm.GetType(typeName);
            }
            return type is not null ? new TypeObject(Utils.CreateBindObject(type)) : null;
        }
        public static DelegateObject Interop_GetInvoker(string procName, string libraryName, string returnType, string[] parameterTypes, string entryPoint = null, bool? useUnicode = null)
        {
            BindFunction func = Utils.CreateExternBindFunction(procName, libraryName, returnType, parameterTypes, libraryName, useUnicode);
            return new DelegateObject(func);
        }
    }
}
