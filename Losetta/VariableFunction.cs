using System;
using System.Collections.Generic;

namespace AliceScript
{
    internal static class VariableFunctionIniter
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
            //Type関数
            Variable.AddFunc(new type_ActivateFunc());
            //Type関数8(終わり)
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
            Variable.AddFunc(new list_InsertRangeFunc());
            Variable.AddFunc(new list_RemoveRangeFunc());
            //List関数(終わり)
            //DELEGATE系(Delegate.csに本体あり)
            Variable.AddFunc(new InvokeFunc());
            Variable.AddFunc(new BeginInvokeFunc());
            //DELEGATE系(終わり)
        }
    }

    internal class CustomMethodFunction : FunctionBase
    {
        public CustomMethodFunction(CustomFunction func, string name = "")
        {
            Function = func;
            Name = name;
            if (Function.IsMethod)
            {
                RequestType = Function.MethodRequestType;
                isNative = Function.isNative;
                IsVirtual = Function.IsVirtual;
                Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
                this.Run += CustomMethodFunction_Run;
            }
        }

        private void CustomMethodFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Function.GetVariable(e.Script, e.CurentVariable);
        }

        public CustomFunction Function { get; set; }
    }

    internal class list_RemoveRangeFunc : FunctionBase
    {
        public list_RemoveRangeFunc()
        {
            this.Name = Constants.REMOVE_RANGE;
            this.MinimumArgCounts = 1;
            this.RequestType = Variable.VarType.ARRAY;
            this.Run += List_RemoveRangeFunc_Run;
        }

        private void List_RemoveRangeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type == Variable.VarType.NUMBER && e.Args[1].Type == Variable.VarType.NUMBER && e.CurentVariable.Tuple != null)
            {
                e.CurentVariable.Tuple.RemoveRange(e.Args[0].AsInt(), e.Args[1].AsInt());
            }
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
            if (e.Args[0].Type == Variable.VarType.TYPE)
            {
                e.Return = e.CurentVariable.Convert(e.Args[0].VariableType);
            }
            else
            {
                ThrowErrorManerger.OnThrowError("Type型である必要があります", Exceptions.COULDNT_CONVERT_VARIABLE);
            }
        }
    }

    internal class RemoveAtFunc : FunctionBase
    {
        public RemoveAtFunc()
        {
            this.Name = Constants.REMOVE_AT;
            this.RequestType = Variable.VarType.STRING | Variable.VarType.ARRAY;
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
            this.RequestType = Variable.VarType.STRING | Variable.VarType.ARRAY;
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
                                    ThrowErrorManerger.OnThrowError("アイテムが配列内に存在しません", Exceptions.COULDNT_FIND_ITEM, e.Script);
                                    return;
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
            this.RequestType = Variable.VarType.STRING | Variable.VarType.ARRAY;
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
                            if (e.Args.Count == 1)
                            {
                                e.Return = new Variable(e.CurentVariable.Tuple.IndexOf(e.Args[0]));
                            }
                            else if (e.Args.Count == 2)
                            {
                                e.Return = new Variable(e.CurentVariable.Tuple.IndexOf(e.Args[0], e.Args[1].AsInt()));
                            }
                            else
                            {
                                e.Return = new Variable(e.CurentVariable.Tuple.IndexOf(e.Args[0], e.Args[1].AsInt(), e.Args[2].AsInt()));
                            }
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
            this.RequestType = Variable.VarType.MAP_NUM | Variable.VarType.MAP_STR;
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
            this.RequestType = Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY;
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
            this.RequestType = Variable.VarType.STRING | Variable.VarType.BYTES | Variable.VarType.DELEGATE | Variable.VarType.ARRAY;
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
            this.RequestType = Variable.VarType.STRING | Variable.VarType.ARRAY | Variable.VarType.DELEGATE;
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
                        e.Return = new Variable(e.CurentVariable.Tuple.Contains(e.Args[0]));
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
            this.RequestType = Variable.VarType.STRING;
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

    internal class InvokeFunc : FunctionBase
    {
        public InvokeFunc()
        {
            this.FunctionName = "Invoke";
            this.RequestType = Variable.VarType.DELEGATE;
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
            this.RequestType = Variable.VarType.DELEGATE;
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
            this.FunctionName = "Dispose";
            this.Run += DisposeFunc_Run;

        }

        private void DisposeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.CurentVariable.Reset();

        }
    }

    //ここより下は変数(Variable)オブジェクトの関数です
    internal class type_ActivateFunc : FunctionBase
    {
        public type_ActivateFunc()
        {
            this.Name = "Activate";
            this.RequestType = Variable.VarType.TYPE;
            this.Run += Type_ActivateFunc_Run;
        }

        private void Type_ActivateFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(e.CurentVariable.VariableType);
        }
    }

    internal class string_TrimFunc : FunctionBase
    {
        public string_TrimFunc(int trimtype = 0)
        {
            this.TrimType = trimtype;
            switch (TrimType)
            {
                case 0:
                    {
                        this.FunctionName = Constants.TRIM;
                        break;
                    }
                case 1:
                    {
                        this.FunctionName = Constants.TRIM_START;
                        break;
                    }
                case 2:
                    {
                        this.FunctionName = Constants.TRIM_END;
                        break;
                    }
            }
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
            this.Run += Str_SplitFunc_Run;
        }

        private void Str_SplitFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType=Variable.VarType.STRING;
            this.MinimumArgCounts = 2;
            this.Run += Str_JoinFunc_Run;
        }

        private void Str_JoinFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> vs = new List<string>();
            vs.Add(e.CurentVariable.AsString());
            foreach(Variable v in e.Args)
            {
                vs.Add(v.AsString());
            }
            e.Return = new Variable(String.Join(e.Args[0].AsString(),vs));
        }
    }
    internal class str_ToLowerUpperFunc : FunctionBase
    {
        public str_ToLowerUpperFunc(bool upper = false)
        {
            Upper = upper;
            if (upper) { this.Name = Constants.UPPER; } else { this.Name = Constants.LOWER; }
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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
            this.RequestType = Variable.VarType.STRING;
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

    internal class list_addFunc : FunctionBase
    {
        public list_addFunc()
        {
            this.FunctionName = Constants.ADD;
            this.RequestType = Variable.VarType.ARRAY;
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

    internal class list_addRangeFunc : FunctionBase
    {
        public list_addRangeFunc()
        {
            this.FunctionName = Constants.ADD_RANGE;
            this.RequestType = Variable.VarType.ARRAY;
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

    internal class list_InsertFunc : FunctionBase
    {
        public list_InsertFunc()
        {
            this.FunctionName = Constants.INSERT;
            this.RequestType = Variable.VarType.ARRAY | Variable.VarType.STRING;
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

    internal class list_InsertRangeFunc : FunctionBase
    {
        public list_InsertRangeFunc()
        {
            this.FunctionName = Constants.INSERT_RANGE;
            this.RequestType = Variable.VarType.ARRAY;
            this.MinimumArgCounts = 2;
            this.Run += List_InsertFunc_Run;
        }

        private void List_InsertFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.CurentVariable.Tuple != null && e.Args[0].Type == Variable.VarType.NUMBER && e.Args[1].Type == Variable.VarType.ARRAY && e.Args[1].Tuple != null)
            {
                e.CurentVariable.Tuple.InsertRange(e.Args[0].AsInt(), e.Args[1].Tuple);
            }
        }
    }

}
