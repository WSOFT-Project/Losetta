namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    internal static class Alice_Initer
    {
        public static void Init()
        {
            //総合関数(コアプロパティ)
            Variable.AddFunc(new DisposeFunc());
            Variable.AddFunc(new EqualsFunc());
            Variable.AddFunc(new CloneFunc());
            Variable.AddFunc(new ResetFunc());
            Variable.AddFunc(new DeepCloneFunc());
            Variable.AddFunc(new ToStringFunc());
            Variable.AddFunc(new PropertiesFunc());
            Variable.AddFunc(new TypeFunc());
            Variable.AddFunc(new ConvertFunc());
            //統合関数(終わり)
            //複合関数(複数の型に対応する関数)
            Variable.AddFunc(new IndexOfFunc());
            Variable.AddFunc(new ContainsFunc());
            Variable.AddFunc(new KeysFunc());
            Variable.AddFunc(new list_InsertFunc());
            Variable.AddFunc(new RemoveAtFunc());
            Variable.AddFunc(new RemoveFunc());
            Variable.AddFunc(new SizeFunc());
            Variable.AddFunc(new LengthFunc());
            //複合関数(終わり)
            //String関数
            Variable.AddFunc(new string_TrimFunc(0), Constants.TRIM);
            Variable.AddFunc(new string_TrimFunc(1), Constants.TRIM_START);
            Variable.AddFunc(new string_TrimFunc(2), Constants.TRIM_END);
            Variable.AddFunc(new str_SEWithFunc(false), Constants.STARTS_WITH);
            Variable.AddFunc(new str_SEWithFunc(true), Constants.ENDS_WITH);
            Variable.AddFunc(new str_PadFunc(false), "PadLeft");
            Variable.AddFunc(new str_PadFunc(true), "PadRight");
            Variable.AddFunc(new str_NormalizeFunc());
            Variable.AddFunc(new str_CompareToFunc());
            Variable.AddFunc(new str_IsNormalizedFunc());
            Variable.AddFunc(new str_LastIndexOfFunc());
            Variable.AddFunc(new str_ReplaceFunc());
            Variable.AddFunc(new str_SplitFunc());
            Variable.AddFunc(new str_SubStringFunc());
            Variable.AddFunc(new str_ToLowerUpperFunc());
            Variable.AddFunc(new str_ToLowerUpperFunc(true));
            Variable.AddFunc(new str_EmptyOrWhiteFunc(true));
            Variable.AddFunc(new str_EmptyOrWhiteFunc(false));
            Variable.AddFunc(new str_FormatFunc());
            Variable.AddFunc(new str_JoinFunc());
            //String関数(終わり)
            //List関数
            Variable.AddFunc(new list_addFunc());
            Variable.AddFunc(new list_addRangeFunc());
            Variable.AddFunc(new list_allFunc());
            Variable.AddFunc(new list_anyFunc());
            Variable.AddFunc(new list_secenceEqualFunc());
            Variable.AddFunc(new list_ofTypeFunc());
            Variable.AddFunc(new list_whereFunc());
            Variable.AddFunc(new list_DistinctFunc());
            Variable.AddFunc(new list_skipFunc());
            Variable.AddFunc(new list_skipWhileFunc());
            Variable.AddFunc(new list_takeFunc());
            Variable.AddFunc(new list_takeWhileFunc());
            Variable.AddFunc(new list_SelectFunc());
            Variable.AddFunc(new list_OrderByFunc());
            Variable.AddFunc(new list_OrderByDescendingFunc());
            Variable.AddFunc(new list_UnionFunc());
            Variable.AddFunc(new list_ExceptFunc());
            Variable.AddFunc(new list_IntersectFunc());
            //List関数(終わり)
            //DELEGATE系(Delegate.csに本体あり)
            Variable.AddFunc(new InvokeFunc());
            Variable.AddFunc(new BeginInvokeFunc());
            Variable.AddFunc(new DelegateNameFunc());
            //DELEGATE系(終わり)
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

            NameSpace space = new NameSpace(Constants.TOP_NAMESPACE);
            space.Add(new DelayFunc());
            space.Add(new SingletonFunction());
            space.Add(new ExitFunction());
            space.Add(new IsNaNFunction());
            space.Add(new PrintFunction());
            space.Add(new PrintFunction(true));
            space.Add(new ReadFunction());
            space.Add(new StringFormatFunction());
            space.Add(new ExceptionObject());

            NameSpaceManerger.Add(space);

            FunctionBaseManerger.Add(new IfStatement());
            FunctionBaseManerger.Add(new DoWhileStatement());
            FunctionBaseManerger.Add(new WhileStatement());
            FunctionBaseManerger.Add(new SwitchStatement());
            FunctionBaseManerger.Add(new CaseStatement());
            FunctionBaseManerger.Add(new CaseStatement(), Constants.DEFAULT);
            FunctionBaseManerger.Add(new ForStatement());
            FunctionBaseManerger.Add(new ForeachStatement());
            FunctionBaseManerger.Add(new GotoGosubFunction(true));
            FunctionBaseManerger.Add(new GotoGosubFunction(false));
            FunctionBaseManerger.Add(new IncludeFile());
            FunctionBaseManerger.Add(new ReturnStatement());
            FunctionBaseManerger.Add(new ThrowFunction());
            FunctionBaseManerger.Add(new TryBlock());

            FunctionBaseManerger.Add(new NewObjectFunction());

            FunctionBaseManerger.Add(new UsingStatement());
            FunctionBaseManerger.Add(new ImportFunc());
            FunctionBaseManerger.Add(new DelegateCreator());
            FunctionBaseManerger.Add(new LockFunction());
        }
    }


    internal class ReturnStatement : FunctionBase
    {
        public ReturnStatement()
        {
            this.Name = Constants.RETURN;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += ReturnStatement_Run;
        }

        private void ReturnStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Script.MoveForwardIf(Constants.SPACE);
            if (!e.Script.FromPrev(Constants.RETURN.Length).Contains(Constants.RETURN))
            {
                e.Script.Backward();
            }
            Variable result = Utils.GetItem(e.Script);

            // Returnに到達したら終了
            e.Script.SetDone();
            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            result.IsReturn = true;

            e.Return = result;
        }

    }



    internal class IsNaNFunction : FunctionBase
    {
        public IsNaNFunction()
        {
            this.Name = Constants.ISNAN;
            this.MinimumArgCounts = 0;
            this.Run += IsNaNFunction_Run;
        }

        private void IsNaNFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable arg = e.Args[0];
            e.Return = new Variable(arg.Type != Variable.VarType.NUMBER || double.IsNaN(arg.Value));
        }
    }

    internal class ConvertFunc : FunctionBase
    {
        public ConvertFunc()
        {
            this.Name = "Convert";
            this.MinimumArgCounts = 1;
            this.Run += ConvertFunc_Run;
        }

        private void ConvertFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            if (e.Args[0].Type == Variable.VarType.OBJECT && e.Args[0].Object is TypeObject type)
            {
                e.Return = e.CurentVariable.Convert(type.Type);
            }
            else
            {
                throw new ScriptException("引数には変換先を表すTypeオブジェクトが必要です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);
            }

        }
    }

    internal class RemoveAtFunc : FunctionBase
    {
        public RemoveAtFunc()
        {
            this.Name = Constants.REMOVE_AT;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += RemoveAtFunc_Run;
        }

        private void RemoveAtFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type == Variable.VarType.NUMBER)
            {
                switch (e.CurentVariable.Type)
                {
                    case Variable.VarType.STRING:
                        {
                            e.Return = new Variable(e.CurentVariable.AsString().Remove(e.Args[0].AsInt()));
                            break;
                        }
                    case Variable.VarType.ARRAY:
                        {
                            if (e.CurentVariable.Tuple != null)
                            {
                                e.CurentVariable.Tuple.RemoveAt(e.Args[0].AsInt());
                            }
                            break;
                        }
                }
            }
        }
    }

    internal class RemoveFunc : FunctionBase
    {
        public RemoveFunc()
        {
            this.Name = Constants.REMOVE_ITEM;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += RemoveFunc_Run;
        }

        private void RemoveFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.CurentVariable.Type)
            {
                case Variable.VarType.STRING:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().Replace(e.Args[0].AsString(), ""));
                        break;
                    }
                case Variable.VarType.ARRAY:
                    {
                        if (e.CurentVariable.Tuple != null)
                        {
                            foreach (Variable v in e.Args)
                            {
                                if (e.CurentVariable.Tuple.Contains(v))
                                {
                                    e.CurentVariable.Tuple.Remove(v);
                                }
                                else
                                {
                                    throw new ScriptException("アイテムが配列内に存在しません", Exceptions.COULDNT_FIND_ITEM, e.Script);
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }

    internal class IndexOfFunc : FunctionBase
    {
        public IndexOfFunc()
        {
            this.Name = Constants.INDEX_OF;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            this.MinimumArgCounts = 1;
            this.Run += IndexOfFunc_Run;
        }

        private void IndexOfFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.CurentVariable.Type)
            {
                case Variable.VarType.STRING:
                    {
                        if (e.Args.Count == 1)
                        {
                            e.Return = new Variable(e.CurentVariable.AsString().IndexOf(e.Args[0].AsString()));
                        }
                        else if (e.Args.Count == 2)
                        {
                            e.Return = new Variable(e.CurentVariable.AsString().IndexOf(e.Args[0].AsString(), e.Args[1].AsInt()));
                        }
                        else
                        {
                            e.Return = new Variable(e.CurentVariable.AsString().IndexOf(e.Args[0].AsString(), e.Args[1].AsInt(), e.Args[2].AsInt()));
                        }
                        break;
                    }
                case Variable.VarType.ARRAY:
                    {
                        if (e.CurentVariable.Tuple != null)
                        {

                            e.Return = new Variable(e.CurentVariable.Tuple.IndexOf(e.Args[0]));
                        }
                        break;
                    }
            }
        }
    }

    internal class KeysFunc : FunctionBase
    {
        public KeysFunc()
        {
            this.Name = Constants.KEYS;
            this.RequestType = new TypeObject(Variable.VarType.MAP_NUM | Variable.VarType.MAP_STR);
            this.Run += KeysFunc_Run;

        }

        private void KeysFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.GetAllKeys());
        }
    }

    internal class PropertiesFunc : FunctionBase
    {
        public PropertiesFunc()
        {
            this.Name = Constants.OBJECT_PROPERTIES;
            this.Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.GetProperties());
        }
    }

    internal class TypeFunc : FunctionBase
    {
        public TypeFunc()
        {
            this.Name = Constants.OBJECT_TYPE;
            this.Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Variable.AsType(e.CurentVariable.Type);
        }
    }

    internal class LengthFunc : FunctionBase
    {
        public LengthFunc()
        {
            this.Name = Constants.LENGTH;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY);
            this.Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.GetLength());
        }
    }

    internal class SizeFunc : FunctionBase
    {
        public SizeFunc()
        {
            this.Name = Constants.SIZE;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY);
            this.Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.GetSize());
        }
    }

    internal class ToStringFunc : FunctionBase
    {
        public ToStringFunc()
        {
            this.Name = "To" + Constants.TO_STRING;
            this.Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString());
        }
    }

    internal class ContainsFunc : FunctionBase
    {
        public ContainsFunc()
        {
            this.Name = Constants.CONTAINS;
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY | Variable.VarType.DELEGATE);
            this.Run += ContainsFunc_Run;
        }

        private void ContainsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.CurentVariable.Type)
            {
                case Variable.VarType.STRING:
                    {
                        if (e.Args[0].Type == Variable.VarType.STRING)
                        {
                            return;
                        }
                        e.Return = new Variable(e.CurentVariable.AsString().Contains(e.Args[0].AsString()));
                        break;
                    }
                case Variable.VarType.ARRAY:
                    {
                        bool contains = e.CurentVariable.Tuple.Where(x => x.Equals(e.Args[0])).FirstOrDefault() != null;
                        e.Return = new Variable(contains);
                        break;
                    }
                case Variable.VarType.DELEGATE:
                    {
                        if (e.Args[0].Type == Variable.VarType.DELEGATE && e.Args[0].Delegate != null)
                        {
                            e.Return = new Variable(e.CurentVariable.Delegate.Contains(e.Args[0].Delegate));
                        }
                        break;
                    }
            }
        }
    }

    internal class str_EmptyOrWhiteFunc : FunctionBase
    {
        public str_EmptyOrWhiteFunc(bool isNullOr)
        {
            isNull = isNullOr;
            if (isNull)
            {
                this.Name = Constants.EMPTY_NULL;
            }
            else
            {
                this.Name = Constants.EMPTY_WHITE;
            }
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += String_EmptyOrWhiteFunc_Run;
        }

        private void String_EmptyOrWhiteFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (isNull)
            {
                e.Return = new Variable(string.IsNullOrEmpty(e.CurentVariable.AsString()));
            }
            else
            {
                e.Return = new Variable(string.IsNullOrWhiteSpace(e.CurentVariable.AsString()));
            }
        }
        private bool isNull;
    }

    internal class DelegateNameFunc : FunctionBase
    {
        public DelegateNameFunc()
        {
            this.Name = "Name";
            this.RequestType = new TypeObject(Variable.VarType.DELEGATE);
            this.Run += DelegateNameFunc_Run;
        }

        private void DelegateNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = new Variable(e.CurentVariable.Delegate.Name);
            }
        }
    }
    internal class InvokeFunc : FunctionBase
    {
        public InvokeFunc()
        {
            this.Name = "Invoke";
            this.RequestType = new TypeObject(Variable.VarType.DELEGATE);
            this.Run += InvokeFunc_Run;
        }

        private void InvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = e.CurentVariable.Delegate.Invoke(e.Args, e.Script);
            }

        }

    }

    internal class ResetFunc : FunctionBase
    {
        public ResetFunc()
        {
            this.Name = "Reset";
            this.Run += ResetFunc_Run;
        }

        private void ResetFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();
        }
    }

    internal class DeepCloneFunc : FunctionBase
    {
        public DeepCloneFunc()
        {
            this.Name = Constants.DEEP_CLONE;
            this.Run += DeepCloneFunc_Run;
        }

        private void DeepCloneFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.CurentVariable.DeepClone();
        }
    }

    internal class CloneFunc : FunctionBase
    {
        public CloneFunc()
        {
            this.Name = Constants.CLONE;
            this.Run += FinalizeFunc_Run;
        }

        private void FinalizeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.CurentVariable.Clone();
        }
    }

    internal class EqualsFunc : FunctionBase
    {
        public EqualsFunc()
        {
            this.Name = Constants.EQUALS;
            this.MinimumArgCounts = 1;
            this.Run += EqualsFunc_Run;
        }

        private void EqualsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.Equals(e.Args[0]));
        }
    }

    internal class BeginInvokeFunc : FunctionBase
    {
        public BeginInvokeFunc()
        {
            this.Name = "BeginInvoke";
            this.RequestType = new TypeObject(Variable.VarType.DELEGATE);
            this.Run += BeginInvokeFunc_Run;
        }

        private void BeginInvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Delegate.BeginInvoke(e.Args, e.Script);
        }


    }

    internal class DisposeFunc : FunctionBase
    {

        public DisposeFunc()
        {
            this.Name = "Dispose";
            this.Run += DisposeFunc_Run;

        }

        private void DisposeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();

        }
    }

    //ここより下は変数(Variable)オブジェクトの関数です


    internal class string_TrimFunc : FunctionBase
    {
        public string_TrimFunc(int trimtype = 0)
        {
            this.TrimType = trimtype;
            switch (TrimType)
            {
                case 0:
                    {
                        this.Name = Constants.TRIM;
                        break;
                    }
                case 1:
                    {
                        this.Name = Constants.TRIM_START;
                        break;
                    }
                case 2:
                    {
                        this.Name = Constants.TRIM_END;
                        break;
                    }
            }
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += String_TrimFunc_Run;
        }

        private int TrimType = 0;
        private void String_TrimFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (TrimType)
            {
                case 0:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.Trim());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.Trim(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }
                case 1:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.TrimStart());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.TrimStart(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }
                case 2:
                    {
                        if (e.Args.Count == 0)
                        {
                            string baseStr = e.CurentVariable.AsString();
                            e.Return = new Variable(baseStr.TrimEnd());
                        }
                        else
                        {
                            string baseStr = e.CurentVariable.AsString();

                            foreach (Variable v in e.Args)
                            {
                                if (v.Type.HasFlag(Variable.VarType.STRING))
                                {
                                    baseStr = baseStr.TrimEnd(v.AsString().ToCharArray());
                                }
                            }
                            e.Return = new Variable(baseStr);
                        }
                        break;
                    }

            }
        }
    }

    internal class str_CompareToFunc : FunctionBase
    {
        public str_CompareToFunc()
        {
            this.Name = "CompareTo";
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_IndexOfFunc_Run;
        }

        private void Str_IndexOfFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().CompareTo(e.Args[0].AsString()));
        }
    }

    internal class str_IsNormalizedFunc : FunctionBase
    {
        public str_IsNormalizedFunc()
        {
            this.Name = "IsNormalized";
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_IsNormalizedFunc_Run;
        }

        private void Str_IsNormalizedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().IsNormalized());
        }
    }

    internal class str_LastIndexOfFunc : FunctionBase
    {
        public str_LastIndexOfFunc()
        {
            this.Name = "LastIndexOf";
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_IndexOfFunc_Run;
        }

        private void Str_IndexOfFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.Args.Count)
            {
                default:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().LastIndexOf(e.Args[0].AsString()));
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().LastIndexOf(e.Args[0].AsString(), e.Args[1].AsInt()));
                        break;
                    }
                case 3:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().LastIndexOf(e.Args[0].AsString(), e.Args[1].AsInt(), e.Args[2].AsInt()));
                        break;
                    }
            }

        }
    }

    internal class str_NormalizeFunc : FunctionBase
    {
        public str_NormalizeFunc()
        {
            this.Name = "Normalize";
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_NormalizeFunc_Run1;
        }

        private void Str_NormalizeFunc_Run1(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().Normalize());
        }
    }

    internal class str_ReplaceFunc : FunctionBase
    {
        public str_ReplaceFunc()
        {
            this.Name = Constants.REPLACE;
            this.MinimumArgCounts = 2;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_ReplaceFunc_Run;
        }

        private void Str_ReplaceFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().Replace(e.Args[0].AsString(), e.Args[1].AsString()));
        }
    }

    internal class str_SplitFunc : FunctionBase
    {
        public str_SplitFunc()
        {
            this.Name = Constants.SPLIT;
            this.MinimumArgCounts = 0;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_SplitFunc_Run;
        }

        private void Str_SplitFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 1)
            {
                //引数がない場合は文字ずつに分割
                Variable v = new Variable(Variable.VarType.ARRAY);
                foreach (char c in e.CurentVariable.AsString())
                {
                    v.Tuple.Add(new Variable(c.ToString()));
                }
                e.Return = v;
            }
            else
            {
                e.Return = new Variable(e.CurentVariable.AsString().Split(new string[] { e.Args[0].AsString() }, StringSplitOptions.None));
            }

        }
    }

    internal class str_SubStringFunc : FunctionBase
    {
        public str_SubStringFunc()
        {
            this.Name = Constants.SUBSTRING;
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_SubStringFunc_Run;
        }

        private void Str_SubStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.Args.Count)
            {
                default:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().Substring(e.Args[0].AsInt()));
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(e.CurentVariable.AsString().Substring(e.Args[0].AsInt(), e.Args[1].AsInt()));
                        break;
                    }
            }
        }
    }

    internal class str_FormatFunc : FunctionBase
    {
        public str_FormatFunc()
        {
            this.Name = "Format";
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_FormatFunc_Run;
        }

        private void Str_FormatFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(StringFormatFunction.Format(e.CurentVariable.AsString(), e.Args));
        }
    }
    internal class str_JoinFunc : FunctionBase
    {
        public str_JoinFunc()
        {
            this.Name = "Join";
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.MinimumArgCounts = 2;
            this.Run += Str_JoinFunc_Run;
        }

        private void Str_JoinFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> vs = new List<string>();
            vs.Add(e.CurentVariable.AsString());
            foreach (Variable v in e.Args)
            {
                vs.Add(v.AsString());
            }
            e.Return = new Variable(String.Join(e.Args[0].AsString(), vs));
        }
    }
    internal class str_ToLowerUpperFunc : FunctionBase
    {
        public str_ToLowerUpperFunc(bool upper = false)
        {
            Upper = upper;
            if (upper) { this.Name = Constants.UPPER; } else { this.Name = Constants.LOWER; }
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_ToLowerUpperFunc_Run;
        }

        private void Str_ToLowerUpperFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Upper)
            {
                e.Return = new Variable(e.CurentVariable.AsString().ToUpper());
            }
            else
            {
                e.Return = new Variable(e.CurentVariable.AsString().ToLower());
            }
        }

        private bool Upper = false;
    }

    internal class str_SEWithFunc : FunctionBase
    {
        public str_SEWithFunc(bool endsWith = false)
        {
            this.EndWith = endsWith;
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_SEWithFunc_Run;
        }

        private void Str_SEWithFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (EndWith)
            {
                if (e.CurentVariable.AsString().EndsWith(e.Args[0].AsString()))
                {
                    e.Return = Variable.True;
                }
                else
                {
                    e.Return = Variable.False;
                }
            }
            else
            {
                if (e.CurentVariable.AsString().StartsWith(e.Args[0].AsString()))
                {
                    e.Return = Variable.True;
                }
                else
                {
                    e.Return = Variable.False;
                }
            }
        }

        private bool EndWith = false;
    }

    internal class str_PadFunc : FunctionBase
    {
        public str_PadFunc(bool right = false)
        {
            this.Right = right;
            this.MinimumArgCounts = 1;
            this.RequestType = new TypeObject(Variable.VarType.STRING);
            this.Run += Str_PadFunc_Run;
        }

        private void Str_PadFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Right)
            {
                if (e.Args.Count > 1)
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]));
                }
                else
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt()));
                }
            }
            else
            {
                if (e.Args.Count > 1)
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]));
                }
                else
                {
                    e.Return = new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt()));
                }
            }
        }

        private bool Right = false;
    }


    internal class ThrowFunction : FunctionBase
    {
        public ThrowFunction()
        {
            this.Name = "throw";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            this.MinimumArgCounts = 1;
            this.Run += ThrowFunction_Run;
        }

        private void ThrowFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.Args[0].Type)
            {
                case Variable.VarType.STRING:
                    {
                        throw new ScriptException(e.Args[0].AsString(), Exceptions.USER_DEFINED, e.Script);
                    }
                case Variable.VarType.NUMBER:
                    {
                        throw new ScriptException(Utils.GetSafeString(e.Args, 1), (Exceptions)e.Args[0].AsInt(), e.Script);
                    }
            }
        }
    }
    internal class GotoGosubFunction : FunctionBase
    {
        private bool m_isGoto = true;

        public GotoGosubFunction(bool gotoMode = true)
        {
            m_isGoto = gotoMode;
            if (m_isGoto)
            {
                this.Name = Constants.GOTO;
            }
            else
            {
                this.Name = Constants.GOSUB;
            }
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            Dictionary<string, int> labels;
            if (script.AllLabels == null || script.LabelToFile == null |
               !script.AllLabels.TryGetValue(script.FunctionName, out labels))
            {
                Utils.ThrowErrorMsg("次のラベルは関数内に存在しません [" + script.FunctionName + "]", Exceptions.COULDNT_FIND_LABEL_IN_FUNCTION,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            int gotoPointer;
            if (!labels.TryGetValue(labelName, out gotoPointer))
            {
                Utils.ThrowErrorMsg("ラベル:[" + labelName + "]は定義されていません", Exceptions.COULDNT_FIND_LABEL,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            string filename;
            if (script.LabelToFile.TryGetValue(labelName, out filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename);
                script.Filename = filename;
                script.String = newScript.String;
            }

            if (!m_isGoto)
            {
                script.PointersBack.Add(script.Pointer);
            }

            script.Pointer = gotoPointer;
            if (string.IsNullOrWhiteSpace(script.FunctionName))
            {
                script.Backward();
            }

            return Variable.EmptyInstance;
        }
    }



    internal class IncludeFile : FunctionBase
    {
        public IncludeFile()
        {
            this.Name = "include";
            this.MinimumArgCounts = 1;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            this.Run += IncludeFile_Run;
        }

        private void IncludeFile_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script == null)
            {
                e.Script = new ParsingScript("");
            }
            ParsingScript tempScript = e.Script.GetIncludeFileScript(e.Args[0].AsString());

            Variable result = null;
            while (tempScript.StillValid())
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
            if (result == null) { result = Variable.EmptyInstance; }
            e.Return = result;
        }

    }
    internal class list_ForeachFunc : FunctionBase
    {
        public list_ForeachFunc()
        {
            this.Name = Constants.FOREACH;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
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

    internal class str_ToLowerUpperInvariantFunc : FunctionBase
    {
        public str_ToLowerUpperInvariantFunc(bool upper = false)
        {
            Upper = upper;
            if (upper) { this.Name = "ToUpperInvariant"; } else { this.Name = "ToLowerInvariant"; }
            this.RequestType = new TypeObject(Variable.VarType.STRING);
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

    internal class bytes_toBase64Func : FunctionBase
    {
        public bytes_toBase64Func()
        {
            this.Name = "ToBase64";
            this.RequestType = new TypeObject(Variable.VarType.BYTES);
            this.Run += ToBase64Func_Run;
        }

        private void ToBase64Func_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Variable.FromText(System.Convert.ToBase64String(e.CurentVariable.AsByteArray()));
        }
    }

    internal class list_SortFunc : FunctionBase
    {
        public list_SortFunc()
        {
            this.Name = Constants.SORT;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_SortFunc_Run;
        }

        private void List_SortFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Sort();
        }
    }

    internal class list_ReverseFunc : FunctionBase
    {
        public list_ReverseFunc()
        {
            this.Name = Constants.REVERSE;
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
            this.Run += List_ReverseFunc_Run;
        }

        private void List_ReverseFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.CurentVariable.Tuple.Reverse();
        }
    }

    internal class list_flattenFunc : FunctionBase
    {
        public list_flattenFunc()
        {
            this.Name = "Flatten";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
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

    internal class list_marge2Func : FunctionBase
    {
        public list_marge2Func()
        {

            this.Name = "Merge";
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
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

    internal class list_FirstOrLastFunc : FunctionBase
    {
        public list_FirstOrLastFunc(bool isLast = false)
        {
            m_Last = isLast;
            if (m_Last)
            {
                this.Name = Constants.LAST;
            }
            else
            {
                this.Name = Constants.FIRST;
            }
            this.RequestType = new TypeObject(Variable.VarType.ARRAY);
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
