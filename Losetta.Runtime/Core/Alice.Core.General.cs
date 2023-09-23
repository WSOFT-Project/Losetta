using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;

namespace AliceScript.NameSpaces.Core
{
    [AliceNameSpace(Name = Constants.TOP_NAMESPACE)]
    internal static partial class CoreFunctions
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
        public static void Assign(this Variable v, Variable other)
        {
            v.Assign(other);
        }
        public static int CompareTo(this Variable v, Variable other)
        {
            return v.CompareTo(other);
        }
        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static TypeObject Type(this Variable v)
        {
            return v.AsType();
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static Variable Value(this Variable v)
        {
            if (v.IsNull())
            {
                throw new ScriptException("変数がnullです", Exceptions.VARIABLE_IS_NULL);
            }
            else
            {
                var result = v.Clone();
                result.Nullable = false;
                return result;
            }
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static bool HasValue(this Variable v)
        {
            return !v.IsNull();
        }
        #endregion
    }
}
