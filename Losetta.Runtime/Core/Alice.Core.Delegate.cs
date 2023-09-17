using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces.Core
{
    internal partial class ExFunctions
    {
        public static bool Contains(DelegateObject func,DelegateObject d)
        {
            return func.Contains(d);
        }
        public static Variable Invoke(DelegateObject func, ParsingScript script)
        {
            return func.Invoke(null, script);
        }
        public static Variable Invoke(DelegateObject func,ParsingScript script,params Variable[] args)
        {
            return func.Invoke(args.ToList(),script);
        }
        public static void BeginInvoke(DelegateObject func, ParsingScript script)
        {
            func.BeginInvoke(null, script);
        }
        public static void BeginInvoke(DelegateObject func, ParsingScript script, params Variable[] args)
        {
            func.BeginInvoke(args.ToList(), script);
        }
    }
}
