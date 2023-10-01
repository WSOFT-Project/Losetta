using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System.Collections;
using System.Reflection;

namespace AliceScript.Binding
{
    /// <summary>
    /// 任意の静的メソッドを表すオブジェクト
    /// </summary>
    public sealed class BindingOverloadFunction : IComparable<BindingOverloadFunction>
    {
        /// <summary>
        /// パラメーターの最期がparamsの場合にtrue
        /// </summary>
        public bool HasParams { get; set; }

        /// <summary>
        /// この関数に必要な引数の最小個数
        /// </summary>
        public int MinimumArgCounts { get; set; }
        public ParameterInfo[] TrueParameters { get; set; }
        public Action<object[]> VoidFunc { get; set; }
        public Func<object[], object> ObjFunc { get; set; }
        public Action<object,object[]> InstanceVoidFunc { get; set; }
        public Func<object, object[], object> InstanceObjFunc { get; set; }
        public bool IsVoidFunc { get; set; }
        public bool IsInstanceFunc { get; set; }
        public bool IsMethod { get; set; }

        public int CompareTo(BindingOverloadFunction other)
        {
            int result = MinimumArgCounts.CompareTo(other.MinimumArgCounts);

            if (result == 0)
            {
                // 比較に困る場合、引数のVariableCollection(なんでも配列型)の数で比べる
                int? r = TrueParameters.Where(item => item.ParameterType == typeof(VariableCollection))?.Count().CompareTo(other.TrueParameters.Where(item => item.ParameterType == typeof(VariableCollection))?.Count());
                result = r.HasValue ? r.Value : result;
            }
            if (result == 0)
            {
                // それでも比較に困る場合、引数のVariable(なんでも型)の数で比べる
                int? r = TrueParameters.Where(item => item.ParameterType == typeof(Variable))?.Count().CompareTo(other.TrueParameters.Where(item => item.ParameterType == typeof(Variable))?.Count());
                result = r.HasValue ? r.Value : result;
            }
            if (other.HasParams || result == 0)
            {
                result = -1;
            }

            return result;
        }

        public bool TryConvertParameters(FunctionBaseEventArgs e, FunctionBase parent, out object[] converted)
        {
            converted = null;

            var parametors = new List<object>(e.Args.Count);
            ArrayList paramsList = null;
            bool inParams = false;
            Type paramType = null;

            if (!HasParams && e.Args.Count + (parent.IsMethod && e.CurentVariable != null ? 1 : 0) > TrueParameters.Length)
            {
                //入力の引数の方が多い場合かつparamsではない場合
                return false;
            }
            int i;
            int diff = 0;//TrueParametersとargsのインデックスのずれ
            for (i = 0; i < TrueParameters.Length; i++)
            {

                paramType = TrueParameters[i].ParameterType;

                if (paramType == typeof(FunctionBaseEventArgs))
                {
                    diff++;
                    parametors.Add(e);
                    continue;
                }
                if (paramType == typeof(ParsingScript))
                {
                    diff++;
                    parametors.Add(e.Script);
                    continue;
                }
                if (paramType == typeof(FunctionBase))
                {
                    diff++;
                    parametors.Add(parent);
                    continue;
                }
                if (paramType == typeof(BindFunction) && parent is BindFunction)
                {
                    diff++;
                    parametors.Add(parent);
                    continue;
                }
                if (paramType == typeof(BindValueFunction) && parent is BindFunction)
                {
                    diff++;
                    parametors.Add(parent);
                    continue;
                }
                if (paramType == typeof(BindingOverloadFunction))
                {
                    diff++;
                    parametors.Add(this);
                    continue;
                }
                if (i == 0 && IsMethod && e.CurentVariable != null)
                {
                    diff++;
                    if (e.CurentVariable.TryConvertTo(paramType, out var r))
                    {
                        parametors.Add(r);
                    }
                    else
                    {
                        return false;
                    }
                    continue;
                }

                if (i > e.Args.Count + diff - 1)
                {
                    //引数が足りない場合
                    if (TrueParameters[i].IsOptional)
                    {
                        parametors.Add(TrueParameters[i].DefaultValue);
                        continue;
                    }
                    else
                    {
                        //規定値がないためマッチしない
                        return false;
                    }
                }

                if (HasParams && i == TrueParameters.Length - 1 && paramType.IsArray && e.Args[i - diff].Type != Variable.VarType.ARRAY)
                {
                    //この引数が最後の場合で、それがparamsの場合かつ、配列として渡されていない場合
                    paramType = paramType.GetElementType();
                    paramsList = new ArrayList();
                    inParams = true;
                }

                var item = e.Args[i - diff];
                if (item == null)
                {
                    if (inParams)
                    {
                        paramsList.Add(item);
                    }
                    else
                    {
                        parametors.Add(item);
                    }
                }
                else if (item.TryConvertTo(paramType, out var result))
                {
                    if (inParams)
                    {
                        paramsList.Add(result);
                    }
                    else
                    {
                        parametors.Add(result);
                    }
                }
                else
                {
                    return false;
                }
            }

            for (; i - diff < e.Args.Count; i++)
            {
                //paramsでまだ指定したい変数がある場合
                if (inParams)
                {
                    var item = e.Args[i - diff];
                    if (item == null)
                    {
                        if (inParams)
                        {
                            paramsList.Add(item);
                        }
                        else
                        {
                            parametors.Add(item);
                        }
                    }
                    else if (item.TryConvertTo(paramType, out var result))
                    {
                        if (inParams)
                        {
                            paramsList.Add(result);
                        }
                        else
                        {
                            parametors.Add(result);
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            if (inParams)
            {
                parametors.Add(paramsList.ToArray(paramType));
            }
            converted = parametors.ToArray();
            return true;
        }
    }
}
