using System;
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
        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this Dictionary<Variable, Variable> dict, BindingOverloadFunction func)
        {
            Console.WriteLine(func.Priority);
            return dict.Count;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Size(this Dictionary<Variable, Variable> dict)
        {
            return dict.Count;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static IEnumerable<Variable> Keys(this Dictionary<Variable, Variable> dict)
        {
            return dict.Keys;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static IEnumerable<Variable> Values(this Dictionary<Variable, Variable> dict)
        {
            return dict.Values;
        }
        #endregion
    }
}