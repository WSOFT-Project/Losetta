namespace AliceScript
{
    internal sealed class list_addFunc : FunctionBase
    {
        public list_addFunc()
        {
            this.Name = Constants.ADD;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_addFunc_Run;
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
            this.Name = Constants.ADD_RANGE;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_addFunc_Run;
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
            this.Name = Constants.INSERT;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY | Variable.VarType.STRING);
            this.MinimumArgCounts = 2;
            this.Run += List_InsertFunc_Run;
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
            this.Name = "All";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "Any";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "SequenceEqual";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_secenceEqualFunc_Run;
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
            this.Name = "ofType";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "Where";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "Distinct";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
            this.Name = "Skip";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "SkipWhile";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "take";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "takeWhile";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "Select";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += List_allFunc_Run;
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
            this.Name = "OrderBy";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
            this.Name = "OrderByDescending";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
            this.Name = "Union";
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
            this.Name = "Except";
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
            this.Name = "Intersect";
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_allFunc_Run;
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
