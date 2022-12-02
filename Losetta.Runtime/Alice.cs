using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    static class Alice_Initer
    {
        public static void Init()
        {
            Variable.AddFunc(new list_SortFunc());
            Variable.AddFunc(new list_ReverseFunc());
            Variable.AddFunc(new list_FirstOrLastFunc());
            Variable.AddFunc(new list_FirstOrLastFunc(true));
            Variable.AddFunc(new list_flattenFunc());
            Variable.AddFunc(new list_marge2Func());
            Variable.AddFunc(new list_ForeachFunc());

            Variable.AddFunc(new bytes_toBase64Func());

            Variable.AddFunc(new str_ToLowerUpperInvariantFunc());
            Variable.AddFunc(new str_ToLowerUpperInvariantFunc(true));
        }
    }
    class list_ForeachFunc : FunctionBase
    {
        public list_ForeachFunc()
        {
            this.Name = Constants.FOREACH;
            this.RequestType = Variable.VarType.ARRAY;
            this.MinimumArgCounts = 1;
            this.Run += List_ForeachFunc_Run;
        }

        private void List_ForeachFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple != null && e.Args[0].Type == Variable.VarType.DELEGATE && e.Args[0].Delegate != null)
            {
                foreach (Variable v in e.CurentVariable.Tuple)
                {
                    e.Args[0].Delegate.Invoke(new List<Variable> { v }, e.Script);
                }
            }
        }
    }
    class str_ToLowerUpperInvariantFunc : FunctionBase
    {
        public str_ToLowerUpperInvariantFunc(bool upper = false)
        {
            Upper = upper;
            if (upper) { this.Name = "UpperInvariant"; } else { this.Name = "LowerInvariant"; }
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_ToLowerUpperFunc_Run;
        }

        private void Str_ToLowerUpperFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Upper)
            {
                e.Return = new Variable(e.CurentVariable.AsString().ToUpperInvariant());
            }
            else
            {
                e.Return = new Variable(e.CurentVariable.AsString().ToLowerInvariant());
            }
        }

        private bool Upper = false;
    }

    class bytes_toBase64Func : FunctionBase
    {
        public bytes_toBase64Func()
        {
            this.FunctionName = "ToBase64";
            this.RequestType = Variable.VarType.BYTES;
            this.Run += ToBase64Func_Run;
        }

        private void ToBase64Func_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Variable.FromText(System.Convert.ToBase64String(e.CurentVariable.AsByteArray()));
        }
    }
    class list_SortFunc : FunctionBase
    {
        public list_SortFunc()
        {
            this.FunctionName = Constants.SORT;
            this.RequestType = Variable.VarType.ARRAY;
            this.Run += List_SortFunc_Run;
        }

        private void List_SortFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Sort();
        }
    }
    class list_ReverseFunc : FunctionBase
    {
        public list_ReverseFunc()
        {
            this.FunctionName = Constants.REVERSE;
            this.RequestType = Variable.VarType.ARRAY;
            this.Run += List_ReverseFunc_Run;
        }

        private void List_ReverseFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                e.CurentVariable.Tuple.Reverse(e.Args[0].AsInt(), e.Args[1].AsInt());
            }
            else
            {
                e.CurentVariable.Tuple.Reverse();
            }
        }
    }
        class list_flattenFunc : FunctionBase
        {
            public list_flattenFunc()
            {
                this.FunctionName = "Flatten";
                this.RequestType = Variable.VarType.ARRAY;
                this.Run += List_flattenFunc_Run;
            }

            private void List_flattenFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                Variable v = new Variable();
                foreach (var strLst in e.CurentVariable.Tuple)
                {
                    if (strLst.Type == Variable.VarType.ARRAY)
                    {
                        v.Tuple.AddRange(strLst.Tuple);
                    }
                    else
                    {
                        v.Tuple.Add(strLst);
                    }
                }
                e.CurentVariable.Tuple = v.Tuple;
            }
        }

        class list_marge2Func : FunctionBase
        {
            public list_marge2Func()
            {

                this.FunctionName = "Merge";
                this.RequestType = Variable.VarType.ARRAY;
                this.Run += List_marge2Func_Run;
            }

            private void List_marge2Func_Run(object sender, FunctionBaseEventArgs e)
            {
                Variable r = new Variable(Variable.VarType.ARRAY);

                r.Tuple.AddRange(e.CurentVariable.Tuple);

                foreach (Variable v1 in e.Args)
                {
                    if (v1.Type == Variable.VarType.ARRAY)
                    {
                        r.Tuple.AddRange(v1.Tuple);
                    }
                    else
                    {
                        r.Tuple.Add(v1);
                    }
                }

                e.CurentVariable.Tuple = r.Tuple;
            }
        }
        class list_FirstOrLastFunc : FunctionBase
        {
            public list_FirstOrLastFunc(bool isLast = false)
            {
                m_Last = isLast;
                if (m_Last)
                {
                    this.FunctionName = Constants.LAST;
                }
                else
                {
                    this.FunctionName = Constants.FIRST;
                }
                this.RequestType = Variable.VarType.ARRAY;
                this.Run += List_FirstOrLastFunc_Run;
            }

            private void List_FirstOrLastFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                if (e.CurentVariable.Tuple != null && e.CurentVariable.Tuple.Count > 0)
                {
                    e.Return = m_Last ? e.CurentVariable.Tuple[0] : e.CurentVariable.Tuple[e.CurentVariable.Tuple.Count - 1];
                }
            }

            private bool m_Last;
        }


    }
