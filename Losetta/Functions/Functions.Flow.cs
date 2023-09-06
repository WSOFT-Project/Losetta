using AliceScript.Objects;
using AliceScript.Parsing;
using System.Text;

namespace AliceScript.Functions
{
    internal sealed class IsUndefinedFunction : ParserFunction
    {
        private string m_argument;
        private string m_action;

        public IsUndefinedFunction(string arg = "", string action = "")
        {
            m_argument = arg;
            m_action = action;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            var variable = ParserFunction.GetVariable(m_argument, script);
            var varValue = variable == null ? null : variable.GetValue(script);
            bool isUndefined = varValue == null || varValue.Type == Variable.VarType.UNDEFINED;

            bool result = m_action == "===" || m_action == "==" ? isUndefined :
                          !isUndefined;
            return new Variable(result);
        }
    }

    internal sealed class CustomMethodFunction : FunctionBase
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
                Run += CustomMethodFunction_Run;
            }
        }

        private void CustomMethodFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Function.GetVariable(e.Script, e.CurentVariable);
        }

        public CustomFunction Function { get; set; }
    }
    internal sealed class ArrayTypeFunction : FunctionBase
    {
        public ArrayTypeFunction()
        {
            Name = "array";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            Run += ArrayTypeFunction_Run;
        }

        private void ArrayTypeFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0 && e.Args[0].Object is TypeObject t)
            {
                var to = new TypeObject(Variable.VarType.ARRAY);
                to.ArrayType = t;
                e.Return = new Variable(to);
            }
            else
            {
                e.Return = Variable.AsType(Variable.VarType.ARRAY);
            }
        }
    }

    internal sealed class EnumFunction : FunctionBase
    {
        public EnumFunction()
        {
            Name = Constants.ENUM;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += EnumFunction_Run;
        }

        private void EnumFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            List<string> properties = Utils.ExtractTokens(e.Script);

            if (properties.Count == 1 && properties[0].Contains("."))
            {
                e.Return = UseExistingEnum(properties[0]);
                return;
            }

            Variable enumVar = new Variable(Variable.VarType.ENUM);
            for (int i = 0; i < properties.Count; i++)
            {
                enumVar.SetEnumProperty(properties[i], new Variable(i));
            }

            e.Return = enumVar;
        }
        public static Variable UseExistingEnum(string enumName)
        {
            Type enumType = GetEnumType(enumName);
            if (enumType == null || !enumType.IsEnum)
            {
                return Variable.EmptyInstance;
            }

            var names = Enum.GetNames(enumType);

            Variable enumVar = new Variable(Variable.VarType.ENUM);
            for (int i = 0; i < names.Length; i++)
            {
                var numValue = Enum.Parse(enumType, names[i], true);
                enumVar.SetEnumProperty(names[i], new Variable((int)numValue));
            }

            return enumVar;
        }

        public static Type GetEnumType(string enumName)
        {
            string[] tokens = enumName.Split('.');

            Type enumType = null;
            int index = 0;
            string typeName = "";
            while (enumType == null && index < tokens.Length)
            {
                if (!string.IsNullOrWhiteSpace(typeName))
                {
                    typeName += ".";
                }
                typeName += tokens[index];
                enumType = GetType(typeName);
                index++;
            }

            for (int i = index; i < tokens.Length && enumType != null; i++)
            {
                enumType = enumType.GetNestedType(tokens[i]);
            }

            return enumType == null || !enumType.IsEnum ? null : enumType;
        }

        public static Type GetType(string typeName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName, false, true);
                if (type != null)
                {
                    return type;
                }
            }
            return null;
        }
    }


    internal sealed class StatementFunction : FunctionBase
    {
        public StatementFunction(string body, ParsingScript script)
        {
            Name = "Statement";
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Script = script.GetTempScript(body, this);
            Run += StatementFunction_Run;
        }

        private void StatementFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Script.Process();
        }

        public ParsingScript Script { get; set; }
    }

    internal sealed class StringOrNumberFunction : FunctionBase
    {
        public StringOrNumberFunction()
        {
            Name = "StringOrNumber";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += StringOrNumberFunction_Run;
        }

        private void StringOrNumberFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // 文字列型かどうか確認
            if (!string.IsNullOrEmpty(Item))
            {
                if (StringMode)
                {
                    bool sq = Item[0] == Constants.QUOTE1 && Item[Item.Length - 1] == Constants.QUOTE1;
                    bool dq = Item[0] == Constants.QUOTE && Item[Item.Length - 1] == Constants.QUOTE;
                    Name = "StringLiteral";
                    if (dq || sq)
                    {
                        //文字列型
                        string result = Item.Substring(1, Item.Length - 2);
                        //文字列補間

                        result = result.Replace("\\'", "'");
                        //ダブルクォーテーションで囲まれている場合、より多くのエスケープ文字を認識
                        if (dq)
                        {
                            //[\\]は一時的に0x0011(装置制御1)に割り当て
                            result = result.Replace("\\\\", "\u0011");
                            result = result.Replace("\\\"", "\"");
                            result = result.Replace("\\n", "\n");
                            result = result.Replace("\\0", "\0");
                            result = result.Replace("\\a", "\a");
                            result = result.Replace("\\b", "\b");
                            result = result.Replace("\\f", "\f");
                            result = result.Replace("\\r", "\r");
                            result = result.Replace("\\t", "\t");
                            result = result.Replace("\\v", "\v");
                            //result = Utils.ConvertUnicodeLiteral(result);
                        }

                        if (DetectionStringFormat)
                        {
                            var stb = new StringBuilder();
                            int blackCount = 0;
                            bool beforeEscape = false;
                            var nowBlack = new StringBuilder();


                            Name = "StringInterpolationLiteral";

                            foreach (char r in result)
                            {
                                switch (r)
                                {
                                    case Constants.START_GROUP:
                                        {
                                            if (blackCount == 0)
                                            {
                                                if (!beforeEscape)
                                                {
                                                    blackCount++;
                                                }
                                                else
                                                {
                                                    stb.Append(r);
                                                }
                                            }
                                            else
                                            {
                                                nowBlack.Append(r);
                                                blackCount++;
                                            }
                                            beforeEscape = false;
                                            break;
                                        }
                                    case Constants.END_GROUP:
                                        {
                                            if (blackCount == 1)
                                            {
                                                blackCount--;
                                                //この波かっこを抜けるとき
                                                string code = nowBlack.ToString();
                                                ParsingScript tempScript = e.Script.GetTempScript(code);
                                                var rrr = tempScript.Process();
                                                if (rrr == null)
                                                {
                                                    rrr = Variable.EmptyInstance;
                                                }
                                                stb.Append(rrr.AsString());
                                                nowBlack.Clear();
                                            }
                                            else
                                            {
                                                if (!beforeEscape)
                                                {
                                                    blackCount--;
                                                    nowBlack.Append(r);
                                                }
                                                else
                                                {
                                                    stb.Append(r);
                                                }
                                            }
                                            beforeEscape = false;
                                            break;
                                        }
                                    case '\\':
                                        {
                                            beforeEscape = !beforeEscape;
                                            break;
                                        }
                                    default:
                                        {
                                            beforeEscape = false;
                                            if (blackCount > 0)
                                            {
                                                nowBlack.Append(r);
                                            }
                                            else
                                            {
                                                stb.Append(r);
                                            }
                                            break;
                                        }
                                }
                            }
                            if (blackCount > 0)
                            {
                                throw new ScriptException("波括弧が不足しています", Exceptions.NEED_BRACKETS, e.Script);
                            }
                            else if (blackCount < 0)
                            {
                                throw new ScriptException("終端の波括弧は不要です", Exceptions.UNNEED_TO_BRACKETS, e.Script);
                            }
                            result = stb.ToString();
                        }

                        if (dq)
                        {
                            //[\\]を\に置き換えます(装置制御1から[\]に置き換え)
                            result = result.Replace("\u0011", "\\");
                        }
                        if (DetectionUTF8_Literal)
                        {
                            //UTF-8リテラルの時はUTF-8バイナリを返す
                            e.Return = new Variable(Encoding.UTF8.GetBytes(result));
                            return;
                        }
                        else
                        {
                            e.Return = new Variable(result);
                            return;
                        }
                    }
                }
                else
                {
                    // 数値として処理
                    Name = "NumberLiteral";
                    double num = Utils.ConvertToDouble(Item, e.Script);
                    e.Return = new Variable(num);
                }
            }

        }

        public bool StringMode { get; set; }
        public string Item { private get; set; }
        public bool DetectionUTF8_Literal { get; set; }
        public bool DetectionStringFormat { get; set; }


    }
    // Get a value of a variable or of an array element
    public class GetVarFunction : FunctionBase
    {
        public GetVarFunction(Variable value)
        {
            m_value = value;
            Name = "Variable";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            //this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += GetVarFunction_Run;
        }

        private void GetVarFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // 要素が配列の一部かを確認
            if (e.Script.TryPrev() == Constants.START_ARRAY)
            {
                switch (m_value.Type)
                {
                    case Variable.VarType.ARRAY:
                        {
                            break;
                        }

                    case Variable.VarType.DELEGATE:
                        {
                            break;
                        }
                    case Variable.VarType.STRING:
                        {
                            break;
                        }
                    default:
                        {
                            throw new ScriptException("この変数で配列添え字演算子を使用することはできません。", Exceptions.VARIABLE_CANT_USE_WITH_ARRAY_SUBSCRIPT, e.Script);
                        }
                }

                if (m_arrayIndices == null)
                {
                    string startName = e.Script.Substr(e.Script.Pointer - 1);
                    m_arrayIndices = Utils.GetArrayIndices(e.Script, startName, m_delta, (newStart, newDelta) => { startName = newStart; m_delta = newDelta; }, this);
                }

                e.Script.Forward(m_delta);
                while (e.Script.MoveForwardIf(Constants.END_ARRAY))
                {
                    ;
                }

                Variable result = Utils.ExtractArrayElement(m_value, m_arrayIndices, e.Script);
                if (e.Script.Prev == '.')
                {
                    e.Script.Backward();
                }

                if (e.Script.TryCurrent() != '.')
                {
                    e.Return = result;
                    return;
                }
                e.Script.Forward();

                m_propName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
                Variable propValue = result.GetProperty(m_propName, e.Script);
                Utils.CheckNotNull(propValue, m_propName, e.Script);
                e.Return = propValue;
                return;
            }

            // Now check that this is an object:
            if (!string.IsNullOrWhiteSpace(m_propName))
            {
                string temp = m_propName;
                m_propName = null; // Need this to reset for recursive calls
                Variable propValue = m_value.Type == Variable.VarType.ENUM ?
                                     m_value.GetEnumProperty(temp, e.Script) :
                                     m_value.GetProperty(temp, e.Script);
                Utils.CheckNotNull(propValue, temp, e.Script);
                e.Return = EvaluateFunction(propValue, e.Script, m_propName, this);
                return;
            }

            // Otherwise just return the stored value.
            e.Return = m_value;
        }
        public static Variable EvaluateFunction(Variable var, ParsingScript script, string m_propName, FunctionBase callFrom)
        {
            if (var != null && var.CustomFunctionGet != null)
            {
                List<Variable> args = script.Prev == '(' ? script.GetFunctionArgs(callFrom) : new List<Variable>();
                if (var.StackVariables != null)
                {
                    args.AddRange(var.StackVariables);
                }
                return var.CustomFunctionGet.ARun(args, script);
            }
            return var != null && !string.IsNullOrWhiteSpace(var.CustomGet) ? ParsingScript.RunString(var.CustomGet, script) : var;
        }
        public int Delta
        {
            set => m_delta = value;
        }
        public Variable Value => m_value;
        public List<Variable> Indices
        {
            set => m_arrayIndices = value;
        }
        public string PropertyName
        {
            set => m_propName = value;
        }

        internal Variable m_value;
        private int m_delta = 0;
        private List<Variable> m_arrayIndices = null;
        private string m_propName;
    }

    internal sealed class IncrementDecrementFunction : ActionFunction, INumericFunction
    {
        public IncrementDecrementFunction()
        {
            Name = "IncrementDecrement";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += IncrementDecrementFunction_Run;
        }

        private void IncrementDecrementFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool prefix = string.IsNullOrWhiteSpace(Name);
            if (prefix)
            {// If it is a prefix we do not have the variable name yet.
                Name = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            }

            Utils.CheckLegalName(Name);

            // Value to be added to the variable:
            int valueDelta = m_action == Constants.INCREMENT ? 1 : -1;
            int returnDelta = prefix ? valueDelta : 0;

            // Check if the variable to be set has the form of x[a][b],
            // meaning that this is an array element.
            double newValue = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(e.Script, m_name, (name) => { m_name = name; }, this);

            ParserFunction func = GetVariable(m_name, e.Script);
            Utils.CheckNotNull(m_name, func, e.Script);

            Variable currentValue = func.GetValue(e.Script);
            currentValue = currentValue.DeepClone();

            if (arrayIndices.Count > 0 || e.Script.TryCurrent() == Constants.START_ARRAY)
            {
                if (prefix)
                {
                    string tmpName = m_name + e.Script.Rest;
                    int delta = 0;
                    arrayIndices = Utils.GetArrayIndices(e.Script, tmpName, delta, (t, d) => { tmpName = t; delta = d; }, this);
                    e.Script.Forward(Math.Max(0, delta - tmpName.Length));
                }

                Variable element = Utils.ExtractArrayElement(currentValue, arrayIndices, e.Script);
                e.Script.MoveForwardIf(Constants.END_ARRAY);

                newValue = element.Value + returnDelta;
                element.Value += valueDelta;
            }
            else
            { // A normal variable.
                newValue = currentValue.Value + returnDelta;
                currentValue.Value += valueDelta;
            }

            ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                    new GetVarFunction(currentValue), e.Script);
            e.Return = new Variable(newValue);
        }


        public override ParserFunction NewInstance()
        {
            return new IncrementDecrementFunction();
        }
    }

    internal sealed class OperatorAssignFunction : ActionFunction
    {
        public OperatorAssignFunction()
        {
            Name = "OperatorAssign";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Run += OperatorAssignFunction_Run;
        }

        private void OperatorAssignFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // Value to be added to the variable:
            Variable right = Utils.GetItem(e.Script);


            Variable currentValue = GetObjectFunction(m_name, e.Script, new HashSet<string>())?.GetValue(e.Script);
            bool isobj = true;
            List<Variable> arrayIndices = new List<Variable>();
            if (currentValue == null)
            {
                isobj = false;
                arrayIndices = Utils.GetArrayIndices(e.Script, m_name, (name) => { m_name = name; }, this);

                ParserFunction func = GetVariable(m_name, e.Script);
                if (!Utils.CheckNotNull(func, m_name, e.Script))
                {
                    return;
                }
                currentValue = func.GetValue(e.Script);
            }

            //currentValue = currentValue.DeepClone();
            Variable left = currentValue;

            if (arrayIndices.Count > 0)
            {// array element
                left = Utils.ExtractArrayElement(currentValue, arrayIndices, e.Script);
                e.Script.MoveForwardIf(Constants.END_ARRAY);
            }
            if (m_action == "??=")
            {
                if (left.IsNull())
                {
                    left.Assign(right);
                }
                e.Return = left;
                return;
            }
            else
            {
                switch (left.Type)
                {
                    case Variable.VarType.NUMBER:
                        {
                            NumberOperator(left, right, m_action);
                            break;
                        }
                    case Variable.VarType.ARRAY:
                        {
                            ArrayOperator(left, right, m_action, e.Script);
                            break;
                        }
                    case Variable.VarType.DELEGATE:
                        {
                            DelegateOperator(left, right, m_action, e.Script);
                            break;
                        }
                    case Variable.VarType.OBJECT:
                        {
                            if (left.Object is ObjectBase obj)
                            {
                                obj.Operator(left, right, m_action, e.Script);
                            }
                            break;
                        }
                    default:
                        {
                            StringOperator(left, right, m_action);
                            break;
                        }
                }
            }

            if (arrayIndices.Count > 0)
            {// array element
                AssignFunction.ExtendArray(currentValue, arrayIndices, 0, left);
                ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                         new GetVarFunction(currentValue), e.Script);
            }
            else if (isobj)
            {
                e.Return = AssignFunction.ProcessObject(m_name, e.Script, left);
                return;
            }
            else
            {
                ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                         new GetVarFunction(left), e.Script);
            }
            e.Return = left;
        }

        private static void DelegateOperator(Variable valueA, Variable valueB, string action, ParsingScript script)
        {
            switch (action)
            {
                case "+=":
                    {
                        valueA.Delegate.Add(valueB.Delegate);
                        break;
                    }
                case "+":
                    {
                        Variable v = new Variable(Variable.VarType.DELEGATE);
                        v.Delegate = new DelegateObject(valueA.Delegate);
                        v.Delegate.Add(valueB.Delegate);
                        valueA.Delegate = v.Delegate;
                        break;
                    }
                case "-=":
                    {
                        if (valueA.Delegate.Remove(valueB.Delegate))
                        {
                            break;
                        }
                        else
                        {
                            Utils.ThrowErrorMsg("デリゲートにに対象の変数が見つかりませんでした", Exceptions.COULDNT_FIND_ITEM,
                         script, action);
                            break;
                        }
                    }
                case "-":
                    {
                        Variable v = new Variable(Variable.VarType.DELEGATE);
                        v.Delegate = new DelegateObject(valueA.Delegate);
                        v.Delegate.Remove(valueB.Delegate);
                        valueA.Delegate = v.Delegate;
                        break;
                    }
                default:
                    {
                        Utils.ThrowErrorMsg(action + "は有効な演算子ではありません", Exceptions.INVALID_OPERAND,
                                        script, action);
                        break;
                    }
            }
        }

        private static void ArrayOperator(Variable valueA, Variable valueB, string action, ParsingScript script)
        {
            switch (action)
            {
                case "+=":
                    {

                        valueA.Tuple.Add(valueB);
                        break;
                    }
                case "+":
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);
                        if (valueB.Type == Variable.VarType.ARRAY)
                        {
                            v.Tuple.AddRange(valueA.Tuple);
                            v.Tuple.AddRange(valueB.Tuple);
                        }
                        else
                        {
                            v.Tuple.AddRange(valueA.Tuple);
                            v.Tuple.Add(valueB);
                        }
                        break;
                    }
                case "-=":
                    {
                        if (valueA.Tuple.Remove(valueB))
                        {
                            break;
                        }
                        else
                        {
                            Utils.ThrowErrorMsg("配列に対象の変数が見つかりませんでした", Exceptions.COULDNT_FIND_ITEM,
                         script, action);
                            break;
                        }
                    }
                case "-":
                    {
                        Variable v = new Variable(Variable.VarType.ARRAY);

                        v.Tuple.AddRange(valueA.Tuple);
                        v.Tuple.Remove(valueB);

                        break;
                    }
                default:
                    {
                        Utils.ThrowErrorMsg(action + "は有効な演算子ではありません", Exceptions.INVALID_OPERAND,
                                        script, action);
                        return;
                    }
            }

        }

        private static void NumberOperator(Variable valueA,
                                   Variable valueB, string action)
        {
            switch (action)
            {
                case "+=":
                    valueA.Value += valueB.Value;
                    break;
                case "-=":
                    valueA.Value -= valueB.Value;
                    break;
                case "*=":
                    valueA.Value *= valueB.Value;
                    break;
                case "/=":
                    valueA.Value /= valueB.Value;
                    break;
                case "%=":
                    valueA.Value %= valueB.Value;
                    break;
                case "&=":
                    valueA.Value = (int)valueA.Value & (int)valueB.Value;
                    break;
                case "|=":
                    valueA.Value = (int)valueA.Value | (int)valueB.Value;
                    break;
                case "^=":
                    valueA.Value = (int)valueA.Value ^ (int)valueB.Value;
                    break;
            }
        }

        private static void StringOperator(Variable valueA,
          Variable valueB, string action)
        {
            switch (action)
            {
                case "+=":
                    if (valueB.Type == Variable.VarType.STRING)
                    {
                        valueA.String += valueB.AsString();
                    }
                    else
                    {
                        valueA.String += valueB.Value;
                    }
                    break;
            }
        }

        public override ParserFunction NewInstance()
        {
            return new OperatorAssignFunction();
        }
    }

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
                // This is not a "normal index" but a new string for the dictionary.
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

    internal sealed class LabelFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // ラベル名のため何もしない
            return Variable.EmptyInstance;
        }
    }

    public interface INumericFunction { }

    public interface IArrayFunction { }

    public interface IStringFunction { }

}
