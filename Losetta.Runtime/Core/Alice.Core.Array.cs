using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Runtime.CompilerServices;

namespace AliceScript.NameSpaces.Core
{
    partial class CoreFunctions
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
        public static int IndexOf(this VariableCollection ary,Variable item)
        {
            return ary.Tuple.IndexOf(item);
        }
        public static int IndexOf(this VariableCollection ary, Variable item,int index)
        {
            return ary.Tuple.IndexOf(item,index);
        }
        public static int IndexOf(this VariableCollection ary, Variable item, int index,int count)
        {
            return ary.Tuple.IndexOf(item, index,count);
        }
        public static int LastIndexOf(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.LastIndexOf(item);
        }
        public static int LastIndexOf(this VariableCollection ary, Variable item, int index)
        {
            return ary.Tuple.IndexOf(item, index);
        }
        public static int LastIndexOf(this VariableCollection ary, Variable item, int index, int count)
        {
            return ary.Tuple.IndexOf(item, index, count);
        }
        public static bool Contains(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.Contains(item);
        }
        public static void Remove(this VariableCollection ary,params Variable[] items)
        {
            foreach(Variable item in items)
            {
                ary.Tuple.Remove(item);
            }
        }
        public static void RemoveRange(this VariableCollection ary,int index,int count)
        {
            ary.Tuple.RemoveRange(index,count);
        }
        public static void RemoveAt(this VariableCollection ary,int index)
        {
            ary.Tuple.RemoveAt(index);
        }
        public static void Sort(this VariableCollection ary)
        {
            ary.Tuple.Sort();
        }
        public static void Reverse(this VariableCollection ary)
        {
            ary.Tuple.Reverse();
        }
        public static void Reverse(this VariableCollection ary,int index,int count)
        {
            ary.Tuple.Reverse(index,count);
        }
        public static void Flatten(this VariableCollection ary)
        {
            Variable v = new Variable();
            for(int i = 0; i < ary.Count; i++)
            {
                var item = ary[i];
                if(item.Type == Variable.VarType.ARRAY)
                {
                    foreach(var item2 in item.Tuple)
                    {
                        ary.Insert(i++,item2);
                    }
                }
            }
        }
        public static void Foreach(this VariableCollection ary,ParsingScript script,DelegateObject func)
        {
            foreach(Variable item in ary.Tuple)
            {
                func.Invoke(new List<Variable> { item }, script);
            }
        }
    }
}
