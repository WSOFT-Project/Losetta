using AliceScript.Binding;
using AliceScript.Interop;
using AliceScript.Objects;
using System.Text;

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
        public static DelegateObject Interop_GetInvoker(string procName, string libraryName, string returnType, string[] parameterTypes, string entryPoint = null, bool? useUnicode = null)
        {
            BindFunction func = BindFunction.CreateExternBindFunction(procName, libraryName, returnType, parameterTypes, libraryName, useUnicode);
            return new DelegateObject(func);
        }
        private static Type InvokeStringToType(string typeStr)
        {
            switch (typeStr.ToUpperInvariant())
            {
                case "HWND":
                case "HANDLE":
                case "INTPTR":
                    return typeof(nint);
                case "UINTPTR":
                    return typeof(UIntPtr);
                case "INT8":
                case "SCHAR":
                    return typeof(sbyte);
                case "UINT":
                case "UCHAR":
                case "CHAR":
                case "BYTE":
                case "BOOLEAN":
                    return typeof(byte);
                case "INT16":
                case "SHORT":
                    return typeof(short);
                case "UINT16":
                case "USHORT":
                case "WORD":
                    return typeof(ushort);
                case "INT32":
                case "INT":
                case "LONG32":
                case "LONG":
                    return typeof(int);
                case "UINT32":
                case "ULONG32":
                case "ULONG":
                case "DWORD":
                    return typeof(uint);
                case "INT64":
                case "LONG64":
                case "LONGLONG":
                    return typeof(long);
                case "UINT64":
                case "ULONG64":
                case "ULONGLONG":
                    return typeof(ulong);
                case "BOOL":
                    return typeof(bool);
                case "LPSTR":
                case "LPTSTR":
                    return typeof(StringBuilder);
                case "LPCSTR":
                case "LPCTSTR":
                case "LPWSTR":
                    return typeof(string);
                case "FLOAT":
                case "SINGLE":
                    return typeof(float);
                case "DOUBLE":
                    return typeof(double);
                default:
                    return null;
            }
        }
        private static Type[] InvokeStringToType(string[] typeStrs)
        {
            var types = new List<Type>();

            foreach (string typeStr in typeStrs)
            {
                types.Add(InvokeStringToType(typeStr));
            }

            return types.ToArray();
        }
    }
}
