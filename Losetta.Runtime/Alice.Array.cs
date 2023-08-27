﻿namespace AliceScript
{
    internal sealed class list_addFunc : FunctionBase
    {
        public list_addFunc()
        {
            Name = Constants.ADD;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_addFunc_Run;
        }

        private void List_addFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple != null)
            {
                foreach (Variable a in e.Args)
                {
                    e.CurentVariable.Tuple.Add(a);
                }
            }
        }
    }

    internal sealed class list_addRangeFunc : FunctionBase
    {
        public list_addRangeFunc()
        {
            Name = Constants.ADD_RANGE;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_addFunc_Run;
        }

        private void List_addFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple != null)
            {
                foreach (Variable a in e.Args)
                {
                    if (a.Type == Variable.VarType.ARRAY && a.Tuple != null)
                    {
                        e.CurentVariable.Tuple.AddRange(a.Tuple);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }

    internal sealed class list_InsertFunc : FunctionBase
    {
        public list_InsertFunc()
        {
            Name = Constants.INSERT;
            RequestType = new TypeObject(Variable.VarType.ARRAY | Variable.VarType.STRING);
            MinimumArgCounts = 2;
            Run += List_InsertFunc_Run;
        }

        private void List_InsertFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.CurentVariable.Type)
            {
                case Variable.VarType.ARRAY:
                    {
                        if (e.CurentVariable.Tuple != null && e.Args[0].Type == Variable.VarType.NUMBER)
                        {
                            e.CurentVariable.Tuple.Insert(e.Args[0].AsInt(), e.Args[1]);
                        }
                        break;
                    }
                case Variable.VarType.STRING:
                    {
                        if (e.Args[0].Type == Variable.VarType.NUMBER && e.Args[1].Type == Variable.VarType.STRING)
                        {
                            e.Return = new Variable(e.CurentVariable.AsString().Insert(e.Args[0].AsInt(), e.Args[1].AsString()));
                        }
                        break;
                    }
            }

        }
    }


    internal sealed class list_allFunc : FunctionBase
    {
        public list_allFunc()
        {
            Name = "All";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.All(item => e.Args[0].Delegate.Invoke(new List<Variable> { item }, e.Script).AsBool()));
        }
    }
    internal sealed class list_anyFunc : FunctionBase
    {
        public list_anyFunc()
        {
            Name = "Any";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Any(item => e.Args[0].Delegate.Invoke(new List<Variable> { item }, e.Script).AsBool()));
        }
    }
    internal sealed class list_secenceEqualFunc : FunctionBase
    {
        public list_secenceEqualFunc()
        {
            Name = "SequenceEqual";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_secenceEqualFunc_Run;
        }

        private void List_secenceEqualFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.ARRAY)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.SequenceEqual(e.Args[0].Tuple));
        }
    }
    internal sealed class list_ofTypeFunc : FunctionBase
    {
        public list_ofTypeFunc()
        {
            Name = "ofType";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.OBJECT || !(e.Args[0].Object is TypeObject))
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var filter = e.Args[0].Object as TypeObject;
            e.Return = new Variable(e.CurentVariable.Tuple.Where(item => item.AsType().Equals(filter)));
        }
    }

    internal sealed class list_whereFunc : FunctionBase
    {
        public list_whereFunc()
        {
            Name = "Where";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var filter = e.Args[0].Delegate;
            e.Return = new Variable(e.CurentVariable.Tuple.Where(item => filter.Invoke(new List<Variable> { item }, e.Script).AsBool()));
        }
    }
    internal sealed class list_DistinctFunc : FunctionBase
    {
        public list_DistinctFunc()
        {
            Name = "Distinct";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var list = new List<Variable>();
            foreach (var v in e.CurentVariable.Tuple)
            {
                if (!list.Contains(v))
                {
                    list.Add(v);
                }
            }
            e.Return = new Variable(list);
        }
    }
    internal sealed class list_skipFunc : FunctionBase
    {
        public list_skipFunc()
        {
            Name = "Skip";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.NUMBER)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Skip(e.Args[0].AsInt()));
        }
    }
    internal sealed class list_skipWhileFunc : FunctionBase
    {
        public list_skipWhileFunc()
        {
            Name = "SkipWhile";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var filter = e.Args[0].Delegate;
            e.Return = new Variable(e.CurentVariable.Tuple.SkipWhile(item => filter.Invoke(new List<Variable> { item }, e.Script).AsBool()));
        }
    }
    internal sealed class list_takeFunc : FunctionBase
    {
        public list_takeFunc()
        {
            Name = "take";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.NUMBER)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Take(e.Args[0].AsInt()));
        }
    }
    internal sealed class list_takeWhileFunc : FunctionBase
    {
        public list_takeWhileFunc()
        {
            Name = "takeWhile";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var filter = e.Args[0].Delegate;
            e.Return = new Variable(e.CurentVariable.Tuple.TakeWhile(item => filter.Invoke(new List<Variable> { item }, e.Script).AsBool()));
        }
    }
    internal sealed class list_SelectFunc : FunctionBase
    {
        public list_SelectFunc()
        {
            Name = "Select";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.DELEGATE)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            var filter = e.Args[0].Delegate;
            e.Return = new Variable(e.CurentVariable.Tuple.Select(item => filter.Invoke(new List<Variable> { item }, e.Script)));
        }
    }
    internal sealed class list_OrderByFunc : FunctionBase
    {
        public list_OrderByFunc()
        {
            Name = "OrderBy";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            if (e.Args.Count > 0 && e.Args[0].Type == Variable.VarType.DELEGATE)
            {
                var filter = e.Args[0].Delegate;
                e.Return = new Variable(e.CurentVariable.Tuple.OrderBy(item => filter.Invoke(new List<Variable> { item }, e.Script)));
            }
            e.Return = new Variable(e.CurentVariable.Tuple.OrderBy(item => item));
        }
    }
    internal sealed class list_OrderByDescendingFunc : FunctionBase
    {
        public list_OrderByDescendingFunc()
        {
            Name = "OrderByDescending";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            if (e.Args.Count > 0 && e.Args[0].Type == Variable.VarType.DELEGATE)
            {
                var filter = e.Args[0].Delegate;
                e.Return = new Variable(e.CurentVariable.Tuple.OrderByDescending(item => filter.Invoke(new List<Variable> { item }, e.Script)));
            }
            e.Return = new Variable(e.CurentVariable.Tuple.OrderByDescending(item => item));
        }
    }
    internal sealed class list_UnionFunc : FunctionBase
    {
        public list_UnionFunc()
        {
            Name = "Union";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.ARRAY)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Union(e.Args[0].Tuple));
        }
    }
    internal sealed class list_ExceptFunc : FunctionBase
    {
        public list_ExceptFunc()
        {
            Name = "Except";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.ARRAY)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Except(e.Args[0].Tuple));
        }
    }
    internal sealed class list_IntersectFunc : FunctionBase
    {
        public list_IntersectFunc()
        {
            Name = "Intersect";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_allFunc_Run;
        }

        private void List_allFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple == null || e.Args[0].Type != Variable.VarType.ARRAY)
            {
                throw new ScriptException("指定された型の変数は比較に使用できません。", Exceptions.WRONG_TYPE_VARIABLE);
            }
            e.Return = new Variable(e.CurentVariable.Tuple.Intersect(e.Args[0].Tuple));
        }
    }
}
