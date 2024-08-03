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
        public static void Add(this VariableCollection ary, params Variable[] items)
        {
            ary.Add(items);
        }
        public static void AddRange(this VariableCollection ary, Variable[] items)
        {
            ary.AddRange(items);
        }
        public static int BinarySearch(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.BinarySearch(item);
        }
        public static void Clear(this VariableCollection ary)
        {
            ary.Tuple.Clear();
        }
        public static bool Contains(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.Contains(item);
        }
        public static bool Exists(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.Exists(x => x == item);
        }
        public static bool Exists(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Exists(x => func.Invoke(x, script).AsBool());
        }
        public static Variable Find(this VariableCollection ary, ParsingScript script, Variable match)
        {
            return ary.Tuple.Find(item => match == item);
        }
        public static Variable Find(this VariableCollection ary, ParsingScript script, DelegateObject match)
        {
            return ary.Tuple.Find(item => match.Invoke(item, script).AsBool());
        }
        public static IEnumerable<Variable> FindAll(this VariableCollection ary, ParsingScript script, Variable match)
        {
            return ary.Tuple.FindAll(item => match == item);
        }
        public static IEnumerable<Variable> FindAll(this VariableCollection ary, ParsingScript script, DelegateObject match)
        {
            return ary.Tuple.FindAll(item => match.Invoke(item, script).AsBool());
        }
        public static int FindIndex(this VariableCollection ary, ParsingScript script, Variable match)
        {
            return ary.Tuple.FindIndex(item => match == item);
        }
        public static int FindIndex(this VariableCollection ary, ParsingScript script, DelegateObject match)
        {
            return ary.Tuple.FindIndex(item => match.Invoke(item, script).AsBool());
        }
        public static Variable FindLast(this VariableCollection ary, ParsingScript script, Variable match)
        {
            return ary.Tuple.FindLast(item => match == item);
        }
        public static Variable FindLast(this VariableCollection ary, ParsingScript script, DelegateObject match)
        {
            return ary.Tuple.FindLast(item => match.Invoke(item, script).AsBool());
        }
        public static int FindLastIndex(this VariableCollection ary, ParsingScript script, Variable match)
        {
            return ary.Tuple.FindLastIndex(item => match == item);
        }
        public static int FindLastIndex(this VariableCollection ary, ParsingScript script, DelegateObject match)
        {
            return ary.Tuple.FindLastIndex(item => match.Invoke(item, script).AsBool());
        }
        public static void Insert(this VariableCollection ary, int index, Variable item)
        {
            ary.Insert(index, item);
        }
        public static bool All(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.All(item => func.Invoke(item, script).AsBool());
        }
        public static bool Any(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Any(item => func.Invoke(item, script).AsBool());
        }
        public static bool SequenceEqual(this VariableCollection ary, Variable[] items)
        {
            return ary.Tuple.SequenceEqual(items);
        }
        public static IEnumerable<Variable> OfType(this VariableCollection ary, TypeObject t)
        {
            return ary.Tuple.Where(item => item.AsType().Equals(t));
        }
        public static IEnumerable<Variable> Convert(this VariableCollection ary, TypeObject t)
        {
            return ary.Tuple.Select(item => item.Convert(t.Type));
        }
        public static IEnumerable<Variable> Where(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Where((item) => func.Invoke(item, script).AsBool());
        }
        public static IEnumerable<Variable> Distinct(this VariableCollection ary)
        {
            return ary.Tuple.Distinct();
        }
        public static IEnumerable<Variable> Skip(this VariableCollection ary, int count)
        {
            return ary.Tuple.Skip(count);
        }
        public static IEnumerable<Variable> SkipLast(this VariableCollection ary, int count)
        {
#if NETCOREAPP2_1_OR_GREATER
            return ary.Tuple.SkipLast(count);
#else
            // LINQが使えない場合は自分で実装したやつを使う
            if(count <= 0)
            {
                return new List<Variable>(ary.Tuple);;
            }
            // 実際に欲しいIEnumratableの先頭からの長さ
            int wantCount = ary.Count - count;

            // 何も残らない場合は空のリストを返しておく
            if(wantCount <= 0)
            {
                return new List<Variable>();
            }

            // 後はTakeするだけ
            return ary.Take(wantCount);
#endif
        }
        public static IEnumerable<Variable> SkipWhile(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.SkipWhile((item) => func.Invoke(item, script).AsBool());
        }
        public static IEnumerable<Variable> Take(this VariableCollection ary, int count)
        {
            return ary.Tuple.Take(count);
        }
        public static IEnumerable<Variable> TakeWhile(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.TakeWhile((item) => func.Invoke(item, script).AsBool());
        }
        public static IEnumerable<Variable> Select(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Select((item) => func.Invoke(item, script));
        }
        public static IEnumerable<Variable> OrderBy(this VariableCollection ary)
        {
            return ary.Tuple.OrderBy((item) => item);
        }
        public static IEnumerable<Variable> OrderBy(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.OrderBy((item) => func.Invoke(item, script));
        }
        public static IEnumerable<Variable> OrderByDescending(this VariableCollection ary)
        {
            return ary.Tuple.OrderByDescending((item) => item);
        }
        public static IEnumerable<Variable> OrderByDescending(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.OrderByDescending((item) => func.Invoke(item, script));
        }
        public static IEnumerable<Variable> Union(this VariableCollection ary, VariableCollection items)
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
        public static IEnumerator<object> GetEnumerator(this VariableCollection ary)
        {
            return ary.Tuple.GetEnumerator();
        }
        public static VariableCollection Repeat(this VariableCollection ary, uint repeat)
        {
            VariableCollection v = new VariableCollection();
            for (int i = 0; i < repeat; i++)
            {
                v.Tuple.AddRange(ary.Tuple);
            }
            return v;
        }
        public static int IndexOf(this VariableCollection ary, Variable item)
        {
            return ary.Tuple.IndexOf(item);
        }
        public static int IndexOf(this VariableCollection ary, Variable item, int index)
        {
            return ary.Tuple.IndexOf(item, index);
        }
        public static int IndexOf(this VariableCollection ary, Variable item, int index, int count)
        {
            return ary.Tuple.IndexOf(item, index, count);
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
        public static void Remove(this VariableCollection ary, params Variable[] items)
        {
            foreach (Variable item in items)
            {
                ary.Tuple.Remove(item);
            }
        }
        public static void RemoveRange(this VariableCollection ary, int index, int count)
        {
            ary.Tuple.RemoveRange(index, count);
        }
        public static void RemoveAt(this VariableCollection ary, int index)
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
        public static void Reverse(this VariableCollection ary, int index, int count)
        {
            ary.Tuple.Reverse(index, count);
        }
        public static void Flatten(this VariableCollection ary)
        {
            Variable v = new Variable();
            for (int i = 0; i < ary.Count; i++)
            {
                var item = ary[i];
                if (item.Type == Variable.VarType.ARRAY)
                {
                    foreach (var item2 in item.Tuple)
                    {
                        ary.Insert(i++, item2);
                    }
                }
            }
        }
        public static void Foreach(this VariableCollection ary, ParsingScript script, BindFunction func)
        {
            List<Variable> args = script.GetFunctionArgs(func, Constants.START_ARG, Constants.END_ARG);
            if (args.Count > 0 && args[0].Type == Variable.VarType.DELEGATE)
            {
                var d = args[0].AsDelegate();
                for (int i = 0; i < ary.Tuple.Count; i++)
                {
                    d.Invoke(new List<Variable> { ary.Tuple[i] }, script);
                }
            }
        }
        public static List<Variable> Slice(this List<Variable> list, int begin)
        {
            if(begin < 0)
            {
                begin += list.Count;
            }
            return Slice(list, begin, list.Count);
        }
        public static List<Variable> Slice(this List<Variable> list, int begin, int end)
        {
            if(begin < 0) 
            {
                begin += list.Count;
            }
            if(end < 0)
            {
                end += list.Count;
            }
            var length = Math.Max(0, end - begin);
            return list.GetRange(begin, length);
        }

        public static IEnumerable<VariableCollection> Chunk(this VariableCollection list, int size)
        {
#if NET6_0_OR_GREATER
            return list.Tuple.Chunk(size).Select(x => new VariableCollection(x.ToList()));
#else
            List<VariableCollection> result = new List<VariableCollection>();

            for (int i = 0; i < list.Count; i += size)
            {
                if (i + size > list.Count)
                {
                    result.Add(new VariableCollection(list.Tuple.Skip(i).ToList()));
                }
                else
                {
                    result.Add(new VariableCollection(list.Tuple.GetRange(i, size)));
                }
            }
            return result;
#endif
        }
        #region 配列集計
        public static double Mean(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Average(item => func.Invoke(item, script).AsDouble());
        }
        public static double Mean(this double[] ary)
        {
            return ary.Average();
        }
        public static double Average(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Average(item => func.Invoke(item, script).AsDouble());
        }
        public static double Max(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Max(item => func.Invoke(item, script).AsDouble());
        }
        public static double Max(this double[] ary)
        {
            return ary.Max();
        }
        public static double Min(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Min(item => func.Invoke(item, script).AsDouble());
        }
        public static double Min(this double[] ary)
        {
            return ary.Min();
        }
        public static double Sum(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Sum(item => func.Invoke(item, script).AsDouble());
        }
        public static double Sum(this double[] ary)
        {
            return ary.Sum();
        }
        public static double Median(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            double[] data = ary.Tuple.Select(item => func.Invoke(item, script).As<double>()).ToArray();
            return Median(data);
        }
        public static double Median(this double[] ary)
        {
            if(ary.Length % 2 == 0)
            {
                return ary.OrderBy(x => x).Skip((ary.Length / 2) - 1).Take(2).Average();
            }
            else
            {
                return ary.OrderBy(x => x).ElementAt(ary.Length / 2);
            }
        }
        public static Variable Mode(this VariableCollection ary)
        {
            return ary.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
        }
        public static Variable Mode(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Select(item => func.Invoke(item, script)).GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
        }
        public static double Mode(this double[] ary)
        {
            return ary.GroupBy(x => x).OrderByDescending(g => g.Count()).Select(g => g.Key).First();
        }
        /// <summary>
        /// 現在の配列内の数値の標準偏差を取得します。
        /// </summary>
        /// <param name="ary">取得する数値が格納された配列</param>
        /// <param name="sample">不偏標準偏差(母集団の標本)に対して標準偏差を取得する場合はtrue、標準偏差を取得する場合はfalse。</param>
        /// <returns>配列内の数値の標準偏差</returns>
        public static double StdDev(this double[] ary, bool sample = false)
        {
            return Math.Sqrt(Variance(ary, sample));
        }
        public static double StdDev(this VariableCollection ary, ParsingScript script, DelegateObject func, bool sample = false)
        {
            double[] data = ary.Tuple.Select(item => func.Invoke(item, script).As<double>()).ToArray();
            return StdDev(data, sample);
        }
        public static double Variance(this double[] ary, bool sample = false)
        {
            int n = ary.Length;
            double ave = ary.Average();
            double s = 0;
            for (int i = 0; i < n; ++i)
                s = s + (ary[i] - ave) * (ary[i] - ave);
            return sample ? s / (n - 1) : s / n;
        }
        public static double Variance(this VariableCollection ary, ParsingScript script, DelegateObject func, bool sample = false)
        {
            double[] data = ary.Tuple.Select(item => func.Invoke(item, script).As<double>()).ToArray();
            return Variance(data, sample);
        }
        public static Variable Aggregate(this VariableCollection ary, ParsingScript script, DelegateObject func)
        {
            return ary.Tuple.Aggregate((result, current) => func.Invoke(new List<Variable> { result, current }, script));
        }
        #endregion
        #region プロパティ
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Length(this VariableCollection ary)
        {
            return ary.Count;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static int Size(this VariableCollection ary)
        {
            return ary.Count;
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable First(this VariableCollection ary)
        {
            return ary.Tuple.First();
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable FirstOrDefault(this VariableCollection ary)
        {
            return ary.Tuple.FirstOrDefault();
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Last(this VariableCollection ary)
        {
            return ary.Tuple.Last();
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable LastOrDefault(this VariableCollection ary)
        {
            return ary.Tuple.LastOrDefault();
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable Signle(this VariableCollection ary)
        {
            return ary.Tuple.Single();
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable SignleOrDefault(this VariableCollection ary)
        {
            return ary.Tuple.SingleOrDefault();
        }
        #endregion
    }
}
