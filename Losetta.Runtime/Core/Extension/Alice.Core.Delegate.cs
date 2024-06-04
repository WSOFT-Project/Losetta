using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static IEnumerator<object> GetEnumerator(this DelegateObject func)
        {
            return func.Functions.GetEnumerator();
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

        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this DelegateObject func)
        {
            return func.Length;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Size(this DelegateObject func)
        {
            return func.Length;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static string Name(this DelegateObject func)
        {
            return func.Name;
        }
        #endregion
    }
}
