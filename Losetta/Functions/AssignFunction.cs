using AliceScript.Parsing;

namespace AliceScript.Functions
{
    internal sealed class AssignFunction : ActionFunction
    {
        public AssignFunction()
        {
            Name = "Assign";

            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += AssignFunction_Run;
        }

        private void AssignFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Assign(e.Script, m_name);
        }

        public Variable Assign(ParsingScript script, string varName, bool localIfPossible = false, ParsingScript baseScript = null)
        {
            m_name = Constants.GetRealName(varName);
            script.CurrentAssign = m_name;
            Variable varValue = Utils.GetItem(script);
            if (baseScript == null)
            {
                baseScript = script;
            }
            if (varValue == null)
            {
                return Variable.EmptyInstance;
            }

            string type_modifer = null;

            foreach (string str in Keywords)
            {
                if (Constants.TYPE_MODIFER.Contains(str.TrimEnd(Constants.TERNARY_OPERATOR)))
                {
                    type_modifer = str;
                    break;
                }
            }

            bool registVar = type_modifer != null || Keywords.Contains(Constants.VAR);
            bool registConst = Keywords.Contains(Constants.CONST);
            bool isGlobal = Keywords.Contains(Constants.PUBLIC);
            bool isReadOnly = Keywords.Contains(Constants.READONLY);

            script.MoveBackIfPrevious(Constants.END_ARG);
            varValue.TrySetAsMap();

            if (script.Current == ' ' || script.Prev == ' ')
            {
                Utils.ThrowErrorMsg("[" + script.Rest + "]は無効なトークンです", Exceptions.INVALID_TOKEN,
                                    script, m_name);
            }
            if (registConst)
            {
                Utils.CheckLegalName(m_name);
                //定数定義
                if (!FunctionExists(m_name, script, out var func))
                {
                    // Check if the variable to be set has the form of x[a][b]...,
                    // meaning that this is an array element.
                    List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (name) => { m_name = name; }, this);
                    m_name = Constants.ConvertName(m_name);
                    if (arrayIndices.Count == 0)
                    {
                        if (isGlobal)
                        {
                            ParsingScript.GetTopLevelScript(baseScript).Consts.Add(m_name, new GetVarFunction(varValue));
                        }
                        else
                        {
                            baseScript.Consts.Add(m_name, new GetVarFunction(varValue));
                        }
                        Variable retVar = varValue.DeepClone();
                        retVar.CurrentAssign = m_name;
                        return retVar;
                    }

                    Variable array;

                    ParserFunction pf = ParserFunction.GetVariable(m_name, script, false, Keywords);
                    array = pf != null ? pf.GetValue(script) : new Variable();

                    ExtendArray(array, arrayIndices, 0, varValue);
                    if (isGlobal)
                    {
                        ParsingScript.GetTopLevelScript(baseScript).Consts.Add(m_name, new GetVarFunction(varValue));
                    }
                    else
                    {
                        baseScript.Consts.Add(m_name, new GetVarFunction(varValue));
                    }
                    return array;
                }
                else
                {
                    throw new ScriptException("定数に値を代入することはできません", Exceptions.CANT_ASSIGN_TO_READ_ONLY, script);
                }
            }
            else
            {
                // First try processing as an object (with a dot notation):
                Variable result = ProcessObject(m_name, script, varValue);
                if (result != null)
                {
                    return result;
                }

                // 設定する変数が x[a][b]... のような形式かどうかをチェックする
                // つまり、配列添え字演算子が書いてあるかどうかを確認
                List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (name) => { m_name = name; }, this);

                if (arrayIndices.Count == 0)
                {
                    ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(varValue), baseScript, localIfPossible, registVar, isGlobal, type_modifer, isReadOnly, true);
                    Variable retVar = varValue.DeepClone();
                    retVar.CurrentAssign = m_name;
                    return retVar;
                }

                Variable array;

                ParserFunction pf = ParserFunction.GetVariable(m_name, baseScript);
                array = pf != null ? pf.GetValue(script) : new Variable();

                ExtendArray(array, arrayIndices, 0, varValue);

                ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(array), baseScript, localIfPossible, registVar, isGlobal, type_modifer, isReadOnly, true);
                return array;
            }
        }
        internal static Variable ProcessObject(string m_name, ParsingScript script, Variable varValue)
        {
            if (script.CurrentClass != null)
            {
                script.CurrentClass.AddProperty(m_name, varValue);
                return varValue.DeepClone();
            }
            string varName = m_name;
            if (script.ClassInstance != null)
            {
                //varName = script.ClassInstance.InstanceName + "." + m_name;
                varValue = script.ClassInstance.SetProperty(m_name, varValue).Result;
                return varValue.DeepClone();
            }

            int ind = varName.IndexOf('.', StringComparison.Ordinal);
            if (ind <= 0)
            {
                return null;
            }

            string name = varName.Substring(0, ind);
            string prop = varName.Substring(ind + 1);

            if (TryAddToNamespace(prop, name, varValue))
            {
                return varValue.DeepClone();
            }

            ParserFunction existing = ParserFunction.GetVariable(name, script);
            Variable baseValue = existing != null ? existing.GetValue(script) : new Variable(Variable.VarType.ARRAY);
            baseValue.SetProperty(prop, varValue, script, name);


            ParserFunction.AddGlobalOrLocalVariable(name, new GetVarFunction(baseValue), script);
            //ParserFunction.AddGlobal(name, new GetVarFunction(baseValue), false);

            return varValue.DeepClone();
        }


        public override ParserFunction NewInstance()
        {
            return new AssignFunction();
        }

        public static void ExtendArray(Variable parent,
                         List<Variable> arrayIndices,
                         int indexPtr,
                         Variable varValue)
        {
            if (arrayIndices.Count <= indexPtr)
            {
                return;
            }

            Variable index = arrayIndices[indexPtr];
            int currIndex = ExtendArrayHelper(parent, index);

            if (arrayIndices.Count - 1 == indexPtr)
            {
                parent.Tuple[currIndex] = varValue;
                return;
            }

            Variable son = parent.Tuple[currIndex];
            ExtendArray(son, arrayIndices, indexPtr + 1, varValue);
        }

        private static int ExtendArrayHelper(Variable parent, Variable indexVar)
        {
            parent.SetAsArray();

            int arrayIndex = parent.GetArrayIndex(indexVar);
            if (arrayIndex < 0)
            {
                // このとき、インデックスではなく辞書配列のkeyが指定された
                string hash = indexVar.AsString();
                arrayIndex = parent.SetHashVariable(hash, Variable.NewEmpty());
                return arrayIndex;
            }

            if (parent.Tuple.Count <= arrayIndex)
            {
                for (int i = parent.Tuple.Count; i <= arrayIndex; i++)
                {
                    parent.Tuple.Add(Variable.NewEmpty());
                }
            }
            return arrayIndex;
        }
    }
}
