using System.Text;

namespace AliceScript.NameSpaces
{
    //このクラスはデフォルトで読み込まれるため読み込み処理が必要です
    internal sealed class Alice_Initer
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
            Variable.AddFunc(new ConvertFunc());
            Variable.AddProp(new PropertiesProp());
            Variable.AddProp(new TypProp());
            //統合関数(終わり)
            //複合関数(複数の型に対応する関数)
            Variable.AddFunc(new IndexOfFunc());
            Variable.AddFunc(new ContainsFunc());
            Variable.AddFunc(new list_InsertFunc());
            Variable.AddFunc(new RemoveAtFunc());
            Variable.AddFunc(new RemoveFunc());
            Variable.AddProp(new LengthSizeProp(), Constants.LENGTH);
            Variable.AddProp(new LengthSizeProp(), Constants.SIZE);
            Variable.AddProp(new KeysFunc());
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
            Variable.AddFunc(new str_ToLowerUpperInvariantFunc());
            Variable.AddFunc(new str_ToLowerUpperInvariantFunc(true));
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

    internal sealed class ReturnStatement : FunctionBase
    {
        public ReturnStatement()
        {
            Name = Constants.RETURN;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ReturnStatement_Run;
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



    internal sealed class IsNaNFunction : FunctionBase
    {
        public IsNaNFunction()
        {
            Name = Constants.ISNAN;
            MinimumArgCounts = 0;
            Run += IsNaNFunction_Run;
        }

        private void IsNaNFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Variable arg = e.Args[0];
            e.Return = new Variable(arg.Type != Variable.VarType.NUMBER || double.IsNaN(arg.Value));
        }
    }

    internal sealed class ConvertFunc : FunctionBase
    {
        public ConvertFunc()
        {
            Name = "Convert";
            MinimumArgCounts = 1;
            Run += ConvertFunc_Run;
        }

        private void ConvertFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.Return = e.Args[0].Type == Variable.VarType.OBJECT && e.Args[0].Object is TypeObject type
                ? e.CurentVariable.Convert(type.Type)
                : throw new ScriptException("引数には変換先を表すTypeオブジェクトが必要です", Exceptions.COULDNT_CONVERT_VARIABLE, e.Script);

        }
    }

    internal sealed class RemoveAtFunc : FunctionBase
    {
        public RemoveAtFunc()
        {
            Name = Constants.REMOVE_AT;
            RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += RemoveAtFunc_Run;
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

    internal sealed class RemoveFunc : FunctionBase
    {
        public RemoveFunc()
        {
            Name = Constants.REMOVE_ITEM;
            RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += RemoveFunc_Run;
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

    internal sealed class IndexOfFunc : FunctionBase
    {
        public IndexOfFunc()
        {
            Name = Constants.INDEX_OF;
            RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += IndexOfFunc_Run;
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
                        else
                        {
                            e.Return = e.Args.Count == 2
                                ? new Variable(e.CurentVariable.AsString().IndexOf(e.Args[0].AsString(), e.Args[1].AsInt()))
                                : new Variable(e.CurentVariable.AsString().IndexOf(e.Args[0].AsString(), e.Args[1].AsInt(), e.Args[2].AsInt()));
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

    internal sealed class KeysFunc : PropertyBase
    {
        public KeysFunc()
        {
            Name = Constants.KEYS;
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.MAP_NUM | Variable.VarType.MAP_STR;
            Getting += KeysFunc_Getting;

        }

        private void KeysFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetAllKeys());
        }

    }

    internal sealed class PropertiesProp : PropertyBase
    {
        public PropertiesProp()
        {
            Name = Constants.OBJECT_PROPERTIES;
            CanSet = false;
            HandleEvents = true;
            Getting += PropertiesProp_Getting;
        }

        private void PropertiesProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetProperties());
        }
    }

    internal sealed class TypProp : PropertyBase
    {
        public TypProp()
        {
            Name = Constants.OBJECT_TYPE;
            CanSet = false;
            HandleEvents = true;
            Getting += TypProp_Getting;
        }

        private void TypProp_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = Variable.AsType(e.Parent.Type);
        }

    }

    internal sealed class LengthSizeProp : PropertyBase
    {
        public LengthSizeProp()
        {
            CanSet = false;
            HandleEvents = true;
            Type = Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY;
            Getting += LengthFunc_Getting;
        }

        private void LengthFunc_Getting(object sender, PropertyBaseEventArgs e)
        {
            e.Value = new Variable(e.Parent.GetLength());
        }

    }

    internal sealed class ToStringFunc : FunctionBase
    {
        public ToStringFunc()
        {
            Name = "To" + Constants.TO_STRING;
            Run += ToStringFunc_Run;
        }

        private void ToStringFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString());
        }
    }

    internal sealed class ContainsFunc : FunctionBase
    {
        public ContainsFunc()
        {
            Name = Constants.CONTAINS;
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING | Variable.VarType.ARRAY | Variable.VarType.DELEGATE);
            Run += ContainsFunc_Run;
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

    internal sealed class str_EmptyOrWhiteFunc : FunctionBase
    {
        public str_EmptyOrWhiteFunc(bool isNullOr)
        {
            isNull = isNullOr;
            Name = isNull ? Constants.EMPTY_NULL : Constants.EMPTY_WHITE;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += String_EmptyOrWhiteFunc_Run;
        }

        private void String_EmptyOrWhiteFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = isNull
                ? new Variable(string.IsNullOrEmpty(e.CurentVariable.AsString()))
                : new Variable(string.IsNullOrWhiteSpace(e.CurentVariable.AsString()));
        }
        private bool isNull;
    }

    internal sealed class DelegateNameFunc : FunctionBase
    {
        public DelegateNameFunc()
        {
            Name = "Name";
            RequestType = new TypeObject(Variable.VarType.DELEGATE);
            Run += DelegateNameFunc_Run;
        }

        private void DelegateNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = new Variable(e.CurentVariable.Delegate.Name);
            }
        }
    }
    internal sealed class InvokeFunc : FunctionBase
    {
        public InvokeFunc()
        {
            Name = "Invoke";
            RequestType = new TypeObject(Variable.VarType.DELEGATE);
            Run += InvokeFunc_Run;
        }

        private void InvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Delegate != null)
            {
                e.Return = e.CurentVariable.Delegate.Invoke(e.Args, e.Script);
            }

        }

    }

    internal sealed class ResetFunc : FunctionBase
    {
        public ResetFunc()
        {
            Name = "Reset";
            Run += ResetFunc_Run;
        }

        private void ResetFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();
        }
    }

    internal sealed class DeepCloneFunc : FunctionBase
    {
        public DeepCloneFunc()
        {
            Name = Constants.DEEP_CLONE;
            Run += DeepCloneFunc_Run;
        }

        private void DeepCloneFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.CurentVariable.DeepClone();
        }
    }

    internal sealed class CloneFunc : FunctionBase
    {
        public CloneFunc()
        {
            Name = Constants.CLONE;
            Run += FinalizeFunc_Run;
        }

        private void FinalizeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = e.CurentVariable.Clone();
        }
    }

    internal sealed class EqualsFunc : FunctionBase
    {
        public EqualsFunc()
        {
            Name = Constants.EQUALS;
            MinimumArgCounts = 1;
            Run += EqualsFunc_Run;
        }

        private void EqualsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.Equals(e.Args[0]));
        }
    }

    internal sealed class BeginInvokeFunc : FunctionBase
    {
        public BeginInvokeFunc()
        {
            Name = "BeginInvoke";
            RequestType = new TypeObject(Variable.VarType.DELEGATE);
            Run += BeginInvokeFunc_Run;
        }

        private void BeginInvokeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Delegate.BeginInvoke(e.Args, e.Script);
        }


    }

    internal sealed class DisposeFunc : FunctionBase
    {

        public DisposeFunc()
        {
            Name = "Dispose";
            Run += DisposeFunc_Run;

        }

        private void DisposeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();

        }
    }

    //ここより下は変数(Variable)オブジェクトの関数です


    internal sealed class string_TrimFunc : FunctionBase
    {
        public string_TrimFunc(int trimtype = 0)
        {
            TrimType = trimtype;
            switch (TrimType)
            {
                case 0:
                    {
                        Name = Constants.TRIM;
                        break;
                    }
                case 1:
                    {
                        Name = Constants.TRIM_START;
                        break;
                    }
                case 2:
                    {
                        Name = Constants.TRIM_END;
                        break;
                    }
            }
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += String_TrimFunc_Run;
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

    internal sealed class str_CompareToFunc : FunctionBase
    {
        public str_CompareToFunc()
        {
            Name = "CompareTo";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IndexOfFunc_Run;
        }

        private void Str_IndexOfFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().CompareTo(e.Args[0].AsString()));
        }
    }

    internal sealed class str_IsNormalizedFunc : FunctionBase
    {
        public str_IsNormalizedFunc()
        {
            Name = "IsNormalized";
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IsNormalizedFunc_Run;
        }

        private void Str_IsNormalizedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().IsNormalized());
        }
    }

    internal sealed class str_LastIndexOfFunc : FunctionBase
    {
        public str_LastIndexOfFunc()
        {
            Name = "LastIndexOf";
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_IndexOfFunc_Run;
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

    internal sealed class str_NormalizeFunc : FunctionBase
    {
        public str_NormalizeFunc()
        {
            Name = "Normalize";
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_NormalizeFunc_Run1;
        }

        private void Str_NormalizeFunc_Run1(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.AsString().Normalize());
        }
    }

    internal sealed class str_ReplaceFunc : FunctionBase
    {
        public str_ReplaceFunc()
        {
            Name = Constants.REPLACE;
            MinimumArgCounts = 2;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_ReplaceFunc_Run;
        }

        private void Str_ReplaceFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 3)
            {
                var sb = new StringBuilder(e.CurentVariable.AsString());
                sb.Replace(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsInt(), e.Args[3].AsInt());
                e.Return = new Variable(sb.ToString());
            }
            else
            {
                e.Return = new Variable(e.CurentVariable.AsString().Replace(e.Args[0].AsString(), e.Args[1].AsString()));
            }
        }
    }

    internal sealed class str_SplitFunc : FunctionBase
    {
        public str_SplitFunc()
        {
            Name = Constants.SPLIT;
            MinimumArgCounts = 0;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_SplitFunc_Run;
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

    internal sealed class str_SubStringFunc : FunctionBase
    {
        public str_SubStringFunc()
        {
            Name = Constants.SUBSTRING;
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_SubStringFunc_Run;
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

    internal sealed class str_FormatFunc : FunctionBase
    {
        public str_FormatFunc()
        {
            Name = "Format";
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_FormatFunc_Run;
        }

        private void Str_FormatFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(StringFormatFunction.Format(e.CurentVariable.AsString(), e.Args));
        }
    }
    internal sealed class str_JoinFunc : FunctionBase
    {
        public str_JoinFunc()
        {
            Name = "Join";
            RequestType = new TypeObject(Variable.VarType.STRING);
            MinimumArgCounts = 2;
            Run += Str_JoinFunc_Run;
        }

        private void Str_JoinFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> vs = new List<string>();
            vs.Add(e.CurentVariable.AsString());
            foreach (Variable v in e.Args)
            {
                vs.Add(v.AsString());
            }
            e.Return = new Variable(string.Join(e.Args[0].AsString(), vs));
        }
    }
    internal sealed class str_ToLowerUpperFunc : FunctionBase
    {
        public str_ToLowerUpperFunc(bool upper = false)
        {
            Upper = upper;
            Name = upper ? Constants.UPPER : Constants.LOWER;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_ToLowerUpperFunc_Run;
        }

        private void Str_ToLowerUpperFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Upper ? new Variable(e.CurentVariable.AsString().ToUpper()) : new Variable(e.CurentVariable.AsString().ToLower());
        }

        private bool Upper = false;
    }

    internal sealed class str_SEWithFunc : FunctionBase
    {
        public str_SEWithFunc(bool endsWith = false)
        {
            EndWith = endsWith;
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_SEWithFunc_Run;
        }

        private void Str_SEWithFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (EndWith)
            {
                e.Return = e.CurentVariable.AsString().EndsWith(e.Args[0].AsString(), StringComparison.Ordinal) ? Variable.True : Variable.False;
            }
            else
            {
                e.Return = e.CurentVariable.AsString().StartsWith(e.Args[0].AsString(), StringComparison.Ordinal) ? Variable.True : Variable.False;
            }
        }

        private bool EndWith = false;
    }

    internal sealed class str_PadFunc : FunctionBase
    {
        public str_PadFunc(bool right = false)
        {
            Right = right;
            MinimumArgCounts = 1;
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_PadFunc_Run;
        }

        private void Str_PadFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Right)
            {
                e.Return = e.Args.Count > 1
                    ? new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]))
                    : new Variable(e.CurentVariable.AsString().PadRight(e.Args[0].AsInt()));
            }
            else
            {
                e.Return = e.Args.Count > 1
                    ? new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt(), e.Args[1].AsString().ToCharArray()[0]))
                    : new Variable(e.CurentVariable.AsString().PadLeft(e.Args[0].AsInt()));
            }
        }

        private bool Right = false;
    }


    internal sealed class ThrowFunction : FunctionBase
    {
        public ThrowFunction()
        {
            Name = "throw";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            MinimumArgCounts = 1;
            Run += ThrowFunction_Run;
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
                default:
                    {
                        if (e.Args[0].Object is ExceptionObject eo)
                        {
                            var s = eo.MainScript ?? e.Script;
                            throw new ScriptException(eo.Message, eo.ErrorCode, s);
                        }
                        break;
                    }
            }
        }
    }
    internal sealed class GotoGosubFunction : FunctionBase
    {
        private bool m_isGoto = true;

        public GotoGosubFunction(bool gotoMode = true)
        {
            m_isGoto = gotoMode;
            Name = m_isGoto ? Constants.GOTO : Constants.GOSUB;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var labelName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);

            if (script.AllLabels == null || script.LabelToFile == null |
               !script.AllLabels.TryGetValue(script.FunctionName, out Dictionary<string, int> labels))
            {
                Utils.ThrowErrorMsg("次のラベルは関数内に存在しません [" + script.FunctionName + "]", Exceptions.COULDNT_FIND_LABEL_IN_FUNCTION,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            if (!labels.TryGetValue(labelName, out int gotoPointer))
            {
                Utils.ThrowErrorMsg("ラベル:[" + labelName + "]は定義されていません", Exceptions.COULDNT_FIND_LABEL,
                                    script, m_name);
                return Variable.EmptyInstance;
            }

            if (script.LabelToFile.TryGetValue(labelName, out string filename) &&
                filename != script.Filename && !string.IsNullOrWhiteSpace(filename))
            {
                var newScript = script.GetIncludeFileScript(filename, this);
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



    internal sealed class IncludeFile : FunctionBase
    {
        public IncludeFile()
        {
            Name = "include";
            MinimumArgCounts = 1;
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            Run += IncludeFile_Run;
        }

        private void IncludeFile_Run(object sender, FunctionBaseEventArgs e)
        {
            /*
            if (e.Script == null)
            {
                e.Script = new ParsingScript("");
            }
            */
            ParsingScript tempScript = e.Script.GetIncludeFileScript(e.Args[0].AsString(), this);

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
    internal sealed class list_ForeachFunc : FunctionBase
    {
        public list_ForeachFunc()
        {
            Name = Constants.FOREACH;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            MinimumArgCounts = 1;
            Run += List_ForeachFunc_Run;
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

    internal sealed class str_ToLowerUpperInvariantFunc : FunctionBase
    {
        public str_ToLowerUpperInvariantFunc(bool upper = false)
        {
            Upper = upper;
            Name = upper ? "ToUpperInvariant" : "ToLowerInvariant";
            RequestType = new TypeObject(Variable.VarType.STRING);
            Run += Str_ToLowerUpperFunc_Run;
        }

        private void Str_ToLowerUpperFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Upper
                ? new Variable(e.CurentVariable.AsString().ToUpperInvariant())
                : new Variable(e.CurentVariable.AsString().ToLowerInvariant());
        }

        private bool Upper = false;
    }

    internal sealed class bytes_toBase64Func : FunctionBase
    {
        public bytes_toBase64Func()
        {
            Name = "ToBase64";
            RequestType = new TypeObject(Variable.VarType.BYTES);
            Run += ToBase64Func_Run;
        }

        private void ToBase64Func_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Variable.FromText(System.Convert.ToBase64String(e.CurentVariable.AsByteArray()));
        }
    }

    internal sealed class list_SortFunc : FunctionBase
    {
        public list_SortFunc()
        {
            Name = Constants.SORT;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_SortFunc_Run;
        }

        private void List_SortFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Sort();
        }
    }

    internal sealed class list_ReverseFunc : FunctionBase
    {
        public list_ReverseFunc()
        {
            Name = Constants.REVERSE;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_ReverseFunc_Run;
        }

        private void List_ReverseFunc_Run(object sender, FunctionBaseEventArgs e)
        {

            e.CurentVariable.Tuple.Reverse();
        }
    }

    internal sealed class list_flattenFunc : FunctionBase
    {
        public list_flattenFunc()
        {
            Name = "Flatten";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_flattenFunc_Run;
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

    internal sealed class list_marge2Func : FunctionBase
    {
        public list_marge2Func()
        {

            Name = "Merge";
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_marge2Func_Run;
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

    internal sealed class list_FirstOrLastFunc : FunctionBase
    {
        public list_FirstOrLastFunc(bool isLast = false)
        {
            m_Last = isLast;
            Name = m_Last ? Constants.LAST : Constants.FIRST;
            RequestType = new TypeObject(Variable.VarType.ARRAY);
            Run += List_FirstOrLastFunc_Run;
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
