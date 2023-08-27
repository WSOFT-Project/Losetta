using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    internal sealed class Alice_Regex_Initer
    {
        public static void Init()
        {
            try
            {
                NameSpace space = new NameSpace("Alice.Regex");

                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Escape));
                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.IsMatch));
                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Match));
                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Matches));
                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Replace));
                space.Add(new RegexSingleArgFunc(RegexSingleArgFunc.FuncMode.Split));

                Variable.AddFunc(new str_IsMatchFunc());
                Variable.AddFunc(new str_MatchesFunc());

                NameSpaceManerger.Add(space);
            }
            catch { }
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

    internal sealed class RegexSingleArgFunc : FunctionBase
    {
        public enum FuncMode
        {
            Escape, IsMatch, Match, Matches, Replace, Split
        }

        public RegexSingleArgFunc(FuncMode mode)
        {
            Mode = mode;

            Run += RegexSingleArgFunc_Run;
            switch (Mode)
            {
                case FuncMode.Escape:
                    {
                        Name = "Regex_Escape";
                        MinimumArgCounts = 1;
                        break;
                    }
                case FuncMode.IsMatch:
                    {
                        Name = "Regex_IsMatch";
                        MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Match:
                    {
                        Name = "Regex_Match";
                        MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Matches:
                    {
                        Name = "Regex_Matches";
                        MinimumArgCounts = 2;
                        break;
                    }
                case FuncMode.Replace:
                    {
                        Name = "Regex_Replace";
                        MinimumArgCounts = 3;
                        break;
                    }
                case FuncMode.Split:
                    {
                        Name = "Regex_Split";
                        MinimumArgCounts = 2;
                        break;
                    }
            }
        }

        private void RegexSingleArgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (Mode)
            {
                case FuncMode.Escape:
                    {
                        e.Return = new Variable(Regex.Escape(e.Args[0].AsString()));
                        break;
                    }
                case FuncMode.IsMatch:
                    {
                        e.Return = new Variable(Regex.IsMatch(e.Args[0].AsString(), e.Args[1].AsString()));
                        break;
                    }
                case FuncMode.Match:
                    {
                        e.Return = new Variable(Regex.Match(e.Args[0].AsString(), e.Args[1].AsString()).Value);
                        break;
                    }
                case FuncMode.Matches:
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);
                        foreach (Match m in Regex.Matches(e.Args[0].AsString(), e.Args[1].AsString()))
                        {
                            v.Tuple.Add(new Variable(m.Value));
                        }
                        e.Return = v;
                        break;
                    }
                case FuncMode.Replace:
                    {
                        e.Return = new Variable(Regex.Replace(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString()));
                        break;
                    }
                case FuncMode.Split:
                    {
                        e.Return = new Variable(Regex.Split(e.Args[0].AsString(), e.Args[1].AsString()));
                        break;
                    }
            }
        }

        private FuncMode Mode;
    }
}
