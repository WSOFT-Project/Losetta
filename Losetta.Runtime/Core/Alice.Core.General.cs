using AliceScript.Binding;
using AliceScript.Objects;

namespace AliceScript.NameSpaces.Core
{
    [AliceNameSpace(Name = "Alice")]
    internal static partial class ExFunctions
    {
        public static void Dispose(this Variable v)
        {
            v.AssignNull();
        }
        public static void Reset(this Variable v)
        {
            v.AssignNull();
        }
        public static bool IsNull(this Variable v)
        {
            return v.IsNull();
        }
        public static bool Equals(this Variable v, Variable other)
        {
            return v.Equals(other);
        }
        public static Variable Clone(this Variable v)
        {
            return v.Clone();
        }
        public static Variable DeepClone(this Variable v)
        {
            return v.DeepClone();
        }
        public static string ToString(this Variable v)
        {
            return v.AsString();
        }
        public static Variable Convert(this Variable v, TypeObject t, bool throwError = false)
        {
            return v.Convert(t.Type, throwError);
        }
        public static int CompareTo(this Variable v,Variable other)
        {
            return v.CompareTo(other);
        }
    }
}
