using System;
using System.Collections.Generic;
using System.Linq;
using AliceScript.Binding;
using AliceScript.Functions;

namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static IEnumerable<Variable> ToArray(this Dictionary<Variable, Variable> dict)
        {
            return dict.Select(kvp => new Variable(kvp));
        }
        public static void Add(this Dictionary<Variable, Variable> dict, Variable key, Variable value)
        {
            dict.Add(key, value);
        }
        public static void Add(this Dictionary<Variable, Variable> dict, KeyValuePair<Variable, Variable> kvp)
        {
            dict.Add(kvp.Key, kvp.Value);
        }
        public static bool TryAdd(this Dictionary<Variable, Variable> dict, Variable key, Variable value)
        {
            return dict.TryAdd(key, value);
        }
        public static bool TryAdd(this Dictionary<Variable, Variable> dict, KeyValuePair<Variable, Variable> kvp)
        {
            return dict.TryAdd(kvp.Key, kvp.Value);
        }
        public static void Clear(this Dictionary<Variable, Variable> dict)
        {
            dict.Clear();
        }
        public static bool ContainsKey(this Dictionary<Variable, Variable> dict, Variable key)
        {
            return dict.ContainsKey(key);
        }
        public static bool ContainsValue(this Dictionary<Variable, Variable> dict, Variable value)
        {
            return dict.ContainsValue(value);
        }
        public static int EnsureCapacity(this Dictionary<Variable, Variable> dict, int capacity)
        {
            return dict.EnsureCapacity(capacity);
        }
        public static bool Remove(this Dictionary<Variable, Variable> dict, Variable key)
        {
            return dict.Remove(key);
        }
        public static void TrimExcess(this Dictionary<Variable, Variable> dict)
        {
            dict.TrimExcess();
        }
        public static void TrimExcess(this Dictionary<Variable, Variable> dict, int capacity)
        {
            dict.TrimExcess(capacity);
        }
        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this Dictionary<Variable, Variable> dict)
        {
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
