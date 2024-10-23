﻿using AliceScript.Parsing;
using System;
using System.Collections.Generic;

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
            if (baseScript is null)
            {
                baseScript = script;
            }
            if (varValue is null)
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

            bool registVar = type_modifer is not null;
            AccessModifier accessModifier = Keywords.Contains(Constants.PUBLIC) ? AccessModifier.PUBLIC : AccessModifier.PRIVATE;
            accessModifier = Keywords.Contains(Constants.PRIVATE) ? AccessModifier.PUBLIC : accessModifier;
            accessModifier = Keywords.Contains(Constants.PROTECTED) ? AccessModifier.PROTECTED : accessModifier;
            bool isReadOnly = Keywords.Contains(Constants.READONLY);

            if (Keywords.Contains(Constants.CONST))
            {
                registVar = true;
                isReadOnly = true;
            }

            script.MoveBackIfPrevious(Constants.END_ARG);
            //varValue.TrySetAsMap();

            if (script.Current == ' ' || script.Prev == ' ')
            {
                Utils.ThrowErrorMsg("[" + script.Rest + "]は無効なトークンです", Exceptions.INVALID_TOKEN,
                                    script, m_name);
            }
            // First try processing as an object (with a dot notation):
            Variable result = ProcessObject(m_name, script, varValue);
            if (result is not null)
            {
                return result;
            }

            // 設定する変数が x[a][b]... のような形式かどうかをチェックする
            // つまり、配列添え字演算子が書いてあるかどうかを確認
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (name) => { m_name = name; }, this);

            if (arrayIndices.Count == 0)
            {
                AddGlobalOrLocalVariable(m_name, new ValueFunction(varValue), baseScript, localIfPossible, registVar, accessModifier, type_modifer, isReadOnly, true);
                Variable retVar = varValue.DeepClone();
                retVar.CurrentAssign = m_name;
                return retVar;
            }

            Variable array;

            ParserFunction pf = GetVariable(m_name, baseScript);
            array = pf is not null ? pf.GetValue(script) : new Variable();

            ExtendArray(array, arrayIndices, 0, varValue);

            AddGlobalOrLocalVariable(m_name, new ValueFunction(array), baseScript, localIfPossible, registVar, accessModifier, type_modifer, isReadOnly, true);
            return array;
        }
        internal static Variable ProcessObject(string m_name, ParsingScript script, Variable varValue)
        {
            if (script.CurrentClass is not null)
            {
                script.CurrentClass.AddProperty(m_name, varValue);
                return varValue.DeepClone();
            }
            string varName = m_name;
            if (script.ClassInstance is not null)
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

            ParserFunction existing = GetVariable(name, script);
            Variable baseValue = existing is not null ? existing.GetValue(script) : new Variable(Variable.VarType.ARRAY);
            baseValue.SetProperty(prop, varValue, script, name);

            AddGlobalOrLocalVariable(name, new ValueFunction(baseValue), script);
            //ParserFunction.AddGlobal(name, new ValueFunction(baseValue), false);

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

            // 辞書への代入の場合
            if (parent.Type == Variable.VarType.DICTIONARY)
            {
                parent.Dictionary[index] = varValue;
                Variable son = parent.Dictionary[index];
                ExtendArray(son, arrayIndices, indexPtr + 1, varValue);
                return;
            }

            int currIndex = parent.GetArrayIndex(index) ?? throw new IndexOutOfRangeException("インデクサーには整数値が必要です。");

            switch (parent.Type)
            {
                case Variable.VarType.ARRAY:
                    if (arrayIndices.Count - 1 == indexPtr)
                    {
                        parent.Tuple[currIndex] = varValue;
                    }
                    Variable son = parent.Tuple[currIndex];
                    ExtendArray(son, arrayIndices, indexPtr + 1, varValue);
                    return;
            }
        }
    }
}
