using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Regex
    {
        public static void Init()
        {
            Variable.AddFunc(new str_IsMatchFunc());
            Variable.AddFunc(new str_MatchesFunc());
            Alice.RegisterFunctions<RegexFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.Regex")]
    internal sealed class RegexFunctions
    {
        public static string Regex_Escape(string text)
        {
            return Regex.Escape(text);
        }
        public static bool Regex_IsMatch(string input, string pattern)
        {
            return Regex.IsMatch(input, pattern);
        }
        public static string Regex_Match(string input, string pattern)
        {
            return Regex.Match(input, pattern).Value;
        }
        public static string[] Regex_Matches(string input, string pattern)
        {
            var result = new List<string>();
            foreach (Match m in Regex.Matches(input, pattern))
            {
                result.Add(m.Value);
            }
            return result.ToArray();
        }
        public static string Regex_Replace(string input, string pattern, string replacement)
        {
            return Regex.Replace(input, pattern, replacement);
        }
        public static string[] Regex_Split(string input, string pattern)
        {
            return Regex.Split(input, pattern);
        }
    }
    internal sealed class str_IsMatchFunc : FunctionBase
    {
        public str_IsMatchFunc()
        {
            Name = "IsMatch";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IsMatchFunc_Run;
        }

        private void Str_IsMatchFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Regex.IsMatch(e.CurentVariable.AsString(), e.Args[0].AsString()));
        }
    }

    internal sealed class str_MatchesFunc : FunctionBase
    {
        public str_MatchesFunc()
        {
            Name = "Matches";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IsMatchFunc_Run;
        }

        private void Str_IsMatchFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            var mc = Regex.Matches(e.CurentVariable.AsString(), e.Args[0].AsString());
            Variable r = new Variable(Variable.VarType.ARRAY);
            foreach (Match m in mc)
            {
                r.Tuple.Add(new Variable(m.Value));
            }
            e.Return = r;
        }
    }

}
