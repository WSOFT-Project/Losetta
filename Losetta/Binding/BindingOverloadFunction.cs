using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AliceScript.Binding
{
    /// <summary>
    /// 任意の静的メソッドのオーバーロードひとつを表すオブジェクト
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

        /// <summary>
        /// このオーバーロードが持つ引数
        /// </summary>
        public ParameterInfo[] TrueParameters { get; set; }

        /// <summary>
        /// このオーバーロードを表すAction
        /// </summary>
        public Action<object[]> VoidFunc { get; set; }

        /// <summary>
        /// このオーバーロードを表すFunc
        /// </summary>
        public Func<object[], object> ObjFunc { get; set; }

        /// <summary>
        /// このインスタンスオーバーロードを表すAction
        /// </summary>
        public Action<object, object[]> InstanceVoidFunc { get; set; }

        /// <summary>
        /// このインスタンスオーバーロードを表すFunc
        /// </summary>
        public Func<object, object[], object> InstanceObjFunc { get; set; }

        /// <summary>
        /// このオーバーロードがActionである場合はtrue、そうでない場合はfalse
        /// </summary>
        /// <value></value>
        public bool IsVoidFunc { get; set; }

        /// <summary>
        /// このオーバーロードがインスタンスに属する場合はtrue、そうでない場合はfalse
        /// </summary>
        /// <value></value>
        public bool IsInstanceFunc { get; set; }

        /// <summary>
        /// このオーバーロードが拡張メソッドである場合はtrue、そうでない場合はfalse
        /// </summary>
        /// <value></value>
        public bool IsMethod { get; set; }

        /// <summary>
        /// このオーバーロードの優先順位
        /// </summary>
        public uint Priority { get; set; }

        /// <summary>
        /// このオーバーロードともう一方のオーバーロードのどちらが優先されるかを判断します。
        /// </summary>
        /// <param name="other">比較する一方のオーバーロードを表すオブジェクト</param>
        /// <returns>より先に解決されるべき場合は1,より後に解決されるべき場合は-1</returns>
        public int CompareTo(BindingOverloadFunction other)
        {
            int result = Priority.CompareTo(other.Priority) * -1;

            return result;
        }

        /// <summary>
        /// AliceScriptの関数に渡された引数をこのメソッドで使用する引数に変換できるか試みます
        /// </summary>
        /// <param name="e">AliceScriptの関数の呼び出し情報</param>
        /// <param name="parent">このメソッドを呼び出したAliceScriptの関数</param>
        /// <param name="converted">変換された引数。ただし、返還できなかった場合はnull。</param>
        /// <returns>変換できた場合はtrue,そうでない場合はfalse</returns>
        public bool TryConvertParameters(FunctionBaseEventArgs e, FunctionBase parent, out object[] converted)
        {
            converted = null;

            var parametors = new List<object>(e.Args.Count);
            ArrayList paramsList = null;
            bool inParams = false;
            Type paramType = null;

            if (!HasParams && e.Args.Count + (parent.IsMethod && e.CurentVariable is not null ? 1 : 0) > TrueParameters.Length)
            {
                //入力の引数の方が多い場合かつparamsではない場合
                return false;
            }
            int i;
            int diff = 0;//TrueParametersとargsのインデックスのずれ
            for (i = 0; i < TrueParameters.Length; i++)
            {
                paramType = TrueParameters[i].ParameterType;

                if(TrueParameters[i].CustomAttributes.Any(attr => attr.AttributeType == typeof(BindInfoAttribute)))
                {
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
                }
                
                if (i == 0 && IsMethod && e.CurentVariable is not null)
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

                if (HasParams && i == TrueParameters.Length - 1 && paramType.IsArray /*&& e.Args[i - diff].Type != Variable.VarType.ARRAY*/)
                {
                    //この引数が最後の場合で、それがparamsの場合かつ、配列として渡されていない場合
                    paramType = paramType.GetElementType();
                    paramsList = new ArrayList();
                    inParams = true;
                }

                var item = e.Args[i - diff];
                if(TrueParameters[i].CustomAttributes.Any(attr => attr.AttributeType == typeof(RefAttribute)) && paramType == typeof(Variable))
                {
                    if(item.Type == Variable.VarType.REFERENCE)
                    {
                        if(item.Reference is ValueFunction value)
                        {
                            parametors.Add(value.Value);
                        }
                        else
                        {
                            throw new ScriptException($"引数 `{TrueParameters[i].Name}` で、変数以外への参照が渡されました", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD);
                        }
                    }
                    else
                    {
                        throw new ScriptException("引数 `" + TrueParameters[i].Name + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD);
                    }
                }
                else if(item.Type == Variable.VarType.REFERENCE)
                {
                    throw new ScriptException("引数 `" + TrueParameters[i].Name + "` は `" + Constants.REF + "' キーワードと共に使用することができません。", Exceptions.ARGUMENT_CANT_USE_WITH_KEYWORD);
                }
                else if (item is null)
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
                    if (item is null)
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
