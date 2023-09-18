using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static bool Contains(this DelegateObject func, DelegateObject d)
        {
            return func.Contains(d);
        }
        public static Variable Invoke(this DelegateObject func, ParsingScript script)
        {
            return func.Invoke(new List<Variable> { }, script);
        }
        public static Variable Invoke(this DelegateObject func, ParsingScript script, params Variable[] args)
        {
            return func.Invoke(args.ToList(), script);
        }
        public static void BeginInvoke(this DelegateObject func, ParsingScript script)
        {
            func.BeginInvoke(null, script);
        }
        public static void BeginInvoke(this DelegateObject func, ParsingScript script, params Variable[] args)
        {
            func.BeginInvoke(args.ToList(), script);
        }
    }
}
