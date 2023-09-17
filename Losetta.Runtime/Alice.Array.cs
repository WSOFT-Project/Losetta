using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;

namespace AliceScript
{
    [AliceNameSpace(Name ="Alice")]
    internal static class ExFunctions
    {
        public static void Add(this VariableCollection ary,params Variable[] items)
        {
            ary.AddRange(items);
        }
        public static void AddRange(this VariableCollection ary, Variable[] items)
        {
            ary.AddRange(items);
        }
        public static void Insert(this VariableCollection ary,int index,Variable item)
        {
            ary.Insert(index,item);
        }
        public static bool All(this VariableCollection ary,ParsingScript script,DelegateObject func)
        {
            return ary.Tuple.All(item => func.Invoke(new List<Variable> { item},script).AsBool());
        }
        public static bool Any(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Any(item => func.Invoke(new List<Variable> { item }, script).AsBool());
        }
        public static bool SequenceEqual(this VariableCollection ary, Variable[] items)
        {
            return ary.Tuple.SequenceEqual(items);
        }
        public static IEnumerable<Variable> OfType(this VariableCollection ary,TypeObject t)
        {
            return ary.Tuple.Where(item => item.AsType().Equals(t));
        }
        public static IEnumerable<Variable> Where(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Where((item) => func.Invoke(new List<Variable> { item},script).AsBool());
        }
        public static IEnumerable<Variable> Distinct(this VariableCollection ary)
        {
            return ary.Tuple.Distinct();
        }
        public static IEnumerable<Variable> Skip(this VariableCollection ary,int count)
        {
            return ary.Tuple.Skip(count);
        }
        public static IEnumerable<Variable> SkipWhile(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.SkipWhile((item) => func.Invoke(new List<Variable> { item },script).AsBool());
        }
        public static IEnumerable<Variable> Take(this VariableCollection ary, int count)
        {
            return ary.Tuple.Take(count);
        }
        public static IEnumerable<Variable> TakeWhile(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.TakeWhile((item) => func.Invoke(new List<Variable> { item }, script).AsBool());
        }
        public static IEnumerable<Variable> Select(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Select((item) => func.Invoke(new List<Variable> { item }, script));
        }
        public static IEnumerable<Variable> OrderBy(this VariableCollection ary)
        {
            return ary.Tuple.OrderBy((item) => item);
        }
        public static IEnumerable<Variable> OrderBy(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.OrderBy((item) => func.Invoke(new List<Variable> { item},script));
        }
        public static IEnumerable<Variable> OrderByDescending(this VariableCollection ary)
        {
            return ary.Tuple.OrderByDescending((item) => item);
        }
        public static IEnumerable<Variable> OrderByDescending(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.OrderByDescending((item) => func.Invoke(new List<Variable> { item }, script));
        }
        public static IEnumerable<Variable> Union(this VariableCollection ary , VariableCollection items)
        {
            return ary.Tuple.Union(items);
        }
        public static IEnumerable<Variable> Except(this VariableCollection ary, VariableCollection items)
        {
            return ary.Tuple.Except(items);
        }
        public static IEnumerable<Variable> Intersect(this VariableCollection ary, VariableCollection items)
        {
            return ary.Tuple.Intersect(items);
        }

        public static string Insert(this string str , int index, Variable item)
        {
            return str.Insert(index,item.AsString());
        }
    }
}
