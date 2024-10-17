using System.Collections.Generic;
using System.Linq;
using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static IEnumerable<Variable> ToArray(this Dictionary<Variable, Variable> dict)
        {
            return dict.Select(kvp => new Variable(kvp));
        }
    }
}