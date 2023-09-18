﻿using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.Functions
{
    /// <summary>
    /// ユーザー定義の関数またはデリゲートを表します
    /// </summary>
    public class CustomFunction : FunctionBase
    {
        public CustomFunction(string funcName,
                                string body, string[] args, ParsingScript script, bool forceReturn = false, Variable.VarType returnType = Variable.VarType.VARIABLE, bool nullable = true)
        {
            Name = funcName;
            m_body = body;
            m_nullable = nullable;
            m_forceReturn = forceReturn;
            m_returnType = returnType;
            Run += CustomFunction_Run;

            if (m_returnType == Variable.VarType.NONE)
            {
                m_nullable = true;
            }

            //正確な変数名の一覧
            List<string> trueArgs = new List<string>();

            bool parms = false;
            bool refs = false;

            for (int i = 0; i < args.Length; i++)
            {
                //変数名
                string arg = args[i];
                //属性等
                List<string> options = new List<string>();
                if (parms)
                {
                    throw new ScriptException("parmsキーワードより後にパラメータを追加することはできません", Exceptions.COULDNT_ADD_PARAMETERS_AFTER_PARMS_KEYWORD, script);
                }
                if (arg.Contains(" "))
                {
                    //属性等の指定がある場合
                    var stb = new HashSet<string>(arg.Split(' '));
                    //もし'='があればその前後のトークンを連結
                    string oldtoken = "";
                    bool connectnexttoken = false;
                    foreach (string option in stb)
                    {
                        if (connectnexttoken)
                        {
                            oldtoken = oldtoken + option;
                            connectnexttoken = false;
                            options.Add(oldtoken);
                        }
                        else
                        if (option.StartsWith("=", StringComparison.Ordinal) || option.EndsWith("=", StringComparison.Ordinal))
                        {
                            oldtoken += option;
                            connectnexttoken = true;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(oldtoken))
                            {
                                options.Add(oldtoken);
                            }
                            oldtoken = option;
                        }

                    }
                    if (!string.IsNullOrEmpty(oldtoken))
                    {
                        options.Add(oldtoken);
                    }
                    //最後のトークンを変数名として解釈
                    arg = options[options.Count - 1];
                    trueArgs.Add(arg);
                }
                else
                {
                    trueArgs.Add(arg);
                }
                TypeObject reqType = new TypeObject();
                if (options.Count > 0)
                {
                    parms = options.Contains(Constants.PARAMS);
                    refs = options.Contains(Constants.REF);
                    if (options.Contains(Constants.THIS))
                    {
                        m_this = m_this == -1 ? i : throw new ScriptException("this修飾子は一つのメソッドに一つのみ設定可能です", Exceptions.INVAILD_ARGUMENT_FUNCTION, script);

                    }
                    if (!refs && options.Count > 1)
                    {
                        Variable v = script.GetTempScript(options[options.Count - 2]).Execute();
                        if (v != null && v.Type == Variable.VarType.OBJECT && v.Object is TypeObject to)
                        {
                            reqType = to;
                        }
                        m_typArgMap.Add(i, reqType);
                    }

                    int ind = arg.IndexOf('=', StringComparison.Ordinal);
                    if (ind > 0)
                    {

                        trueArgs[i] = arg.Substring(0, ind).Trim();
                        string defValue = ind >= arg.Length - 1 ? "" : arg.Substring(ind + 1).Trim();

                        Variable defVariable = Utils.GetVariableFromString(defValue, script, this);
                        defVariable.CurrentAssign = m_args[i];
                        defVariable.Index = i;

                        if (!reqType.Match(defVariable))
                        {
                            throw new ScriptException("この引数にその型を使用することはできません", Exceptions.WRONG_TYPE_VARIABLE, script);
                        }

                        m_defArgMap[i] = m_defaultArgs.Count;
                        m_defaultArgs.Add(defVariable);

                    }
                    else
                    {
                        string argName = arg;// RealArgs[i].ToLowerInvariant();
                        if (parms)
                        {
                            parmsindex = i;
                            argName = argName.Substring(Constants.PARAMS.Length);
                            argName = argName.Trim();
                        }
                        if (parms && refs)
                        {
                            throw new ScriptException(Constants.PARAMS + "パラメータを参照渡しに設定することはできません。", Exceptions.INCOMPLETE_FUNCTION_DEFINITION, script);
                        }
                        else if (refs)
                        {
                            m_refMap.Add(i);
                            //argName = argName.Substring(Constants.REF.Length);
                            argName = argName.Trim();
                        }
                        trueArgs[i] = argName;

                    }
                    ArgMap[trueArgs[i]] = i;
                }
                if (m_this != -1)
                {
                    RequestType = reqType;
                }
                m_args = RealArgs = trueArgs.ToArray();
            }

        }

        private void CustomFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Utils.ExtractParameterNames(e.Args, m_name, e.Script);

            if (m_args == null)
            {
                m_args = new string[0];
            }
            if (e.Args.Count + m_defaultArgs.Count + (e.CurentVariable != null ? 1 : 0) < m_args.Length)
            {
                throw new ScriptException($"関数`{m_args.Length}`は引数`{m_args.Length}`を受取ることが出来ません。", Exceptions.TOO_MANY_ARGUREMENTS, e.Script);
            }
            Variable result = ARun(e.Args, e.Script, e.ClassInstance, e.CurentVariable);
            if ((m_returnType != Variable.VarType.VARIABLE && m_returnType != result.Type) || (!m_nullable && result.IsNull()))
            {
                throw new ScriptException($"関数は宣言とは異なり{result.Type}{(result.IsNull() ? "?" : "")}型を返しました", Exceptions.TYPE_MISMATCH, m_parentScript);
            }
            e.Return = result;
        }

        private int parmsindex = -1;
        private void RegisterArguments(List<Variable> args,
                                      List<KeyValuePair<string, Variable>> args2 = null, Variable current = null, ParsingScript script = null)
        {
            if (args == null)
            {
                args = new List<Variable>();
            }
            if (m_this != -1 && current != null)
            {
                args.Insert(m_this, current);
            }
            if (m_args == null)
            {
                m_args = new List<string>().ToArray();
            }
            int missingArgs = m_args.Length - args.Count;
            bool namedParameters = false;
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                int argIndex = -1;
                if (m_typArgMap.ContainsKey(i) && !m_typArgMap[i].Match(arg))
                {
                    throw new ScriptException("この引数にその型を使用することはできません", Exceptions.WRONG_TYPE_VARIABLE);
                }
                else
                {

                    if (arg != null && ArgMap.TryGetValue(arg.CurrentAssign, out argIndex))
                    {
                        namedParameters = true;
                        if (i != argIndex)
                        {
                            args[i] = argIndex < args.Count ? args[argIndex] : args[i];
                            while (argIndex > args.Count - 1)
                            {
                                args.Add(Variable.EmptyInstance);
                            }
                            args[argIndex] = arg;
                        }
                    }
                    else if (namedParameters)
                    {
                        throw new ScriptException("関数の引数と値 `" + m_name + "` は一対一で一致する必要があります。", Exceptions.INVAILD_ARGUMENT_FUNCTION);
                    }
                }
            }

            if (missingArgs > 0 && missingArgs <= m_defaultArgs.Count)
            {
                if (!namedParameters)
                {
                    for (int i = m_defaultArgs.Count - missingArgs; i < m_defaultArgs.Count; i++)
                    {
                        args.Add(m_defaultArgs[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < args.Count; i++)
                    {
                        if (args[i].Type == Variable.VarType.NONE ||
                           (!string.IsNullOrWhiteSpace(args[i].CurrentAssign) &&
                            args[i].CurrentAssign != m_args[i]))
                        {
                            int defIndex = -1;
                            if (!m_defArgMap.TryGetValue(i, out defIndex))
                            {
                                throw new ScriptException("関数 `" + m_name + "` で引数 `" + m_args[i] + "` が指定されていません。", Exceptions.INCOMPLETE_ARGUMENTS);
                            }
                            args[i] = m_defaultArgs[defIndex];
                        }
                    }
                }
            }
            for (int i = args.Count; i < m_args.Length; i++)
            {
                int defIndex = -1;
                if (!m_defArgMap.TryGetValue(i, out defIndex))
                {
                    throw new ScriptException("関数 `" + m_name + "` で引数 `" + m_args[i] + "` が指定されていません。", Exceptions.INCOMPLETE_ARGUMENTS);
                }
                args.Add(m_defaultArgs[defIndex]);
            }

            if (args2 != null)
            {
                foreach (var entry in Utils.GetSpan(args2))
                {
                    var val = new Variable();
                    val.Assign(entry.Value);
                    var arg = new GetVarFunction(val);
                    arg.Name = entry.Key;
                    //m_VarMap[entry.Key] = arg;
                    script.Variables[entry.Key] = arg;
                }
            }

            int maxSize = Math.Min(args.Count, m_args.Length);
            for (int i = 0; i < maxSize; i++)
            {
                if (parmsindex == i)
                {
                    Variable parmsarg = new Variable(Variable.VarType.ARRAY);
                    foreach (Variable argx in Utils.GetSpan(args).Slice(i, args.Count - i))
                    {
                        var val = new Variable();
                        val.Assign(argx);
                        parmsarg.Tuple.Add(val);
                    }
                    var arg = new GetVarFunction(parmsarg);
                    arg.Name = m_args[i];
                    //m_VarMap[m_args[i]] = arg;
                    script.Variables[m_args[i]] = arg;
                }
                else
                {
                    Variable val;

                    bool refd = args[i].Keywords.Contains(Constants.REF);
                    if (m_refMap.Contains(i))
                    {
                        val = refd
                            ? args[i]
                            : throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                    }
                    else
                    {
                        if (refd)
                        {
                            throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "' キーワードと共に使用することができません。", Exceptions.ARGUMENT_CANT_USE_WITH_KEYWORD, script);
                        }
                        val = new Variable();
                        val.Assign(args[i]);
                    }
                    var arg = new GetVarFunction(val);
                    arg.Name = m_args[i];
                    //m_VarMap[m_args[i]] = arg;
                    script.Variables[m_args[i]] = arg;
                }
            }
            for (int i = m_args.Length; i < args.Count; i++)
            {
                Variable val;

                bool refd = args[i].Keywords.Contains(Constants.REF);
                if (m_refMap.Contains(i))
                {
                    val = refd
                        ? args[i]
                        : throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                }
                else
                {
                    if (refd)
                    {
                        throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "' キーワードと共に使用することができません。", Exceptions.ARGUMENT_CANT_USE_WITH_KEYWORD, script);
                    }
                    val = new Variable();
                    val.Assign(args[i]);
                }
                var arg = new GetVarFunction(val);
                if (i + 1 > m_args.Length)
                {
                    throw new ScriptException($"関数 `{m_name}`は、{m_args.Length}個よりも多く引数を持つことができません", Exceptions.TOO_MANY_ARGUREMENTS, script);
                }
                arg.Name = m_args[i];
                script.Variables[args[i].ParamName] = arg;
            }

            if (NamespaceData != null)
            {
                var vars = NamespaceData.Variables;
                string prefix = NamespaceData.Name + ".";
                foreach (KeyValuePair<string, ParserFunction> elem in vars)
                {
                    string key = elem.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ?
                        elem.Key.Substring(prefix.Length) : elem.Key;

                    //m_VarMap[key] = elem.Value;
                    script.Variables[key] = elem.Value;
                }
            }

        }
        internal HashSet<CustomFunction> Children
        {
            get; set;
        }
        public Variable GetVariable(ParsingScript script, Variable current)
        {
            List<Variable> args = Constants.FUNCT_WITH_SPACE.Contains(m_name) ?
             // Special case of extracting args.
             Utils.GetFunctionArgsAsStrings(script) :
             script.GetFunctionArgs(this);

            Utils.ExtractParameterNames(args, m_name, script);

            script.MoveBackIf(Constants.START_GROUP);
            //これはメソッドで呼び出される。そのため[this]代入分として1を足す。
            if (args.Count + m_defaultArgs.Count + 1 < m_args.Length)
            {
                throw new ScriptException("この関数は、最大で" + (args.Count + m_defaultArgs.Count + 1) + "個の引数を受け取ることができますが、" + m_args.Length + "個の引数が渡されました", Exceptions.TOO_MANY_ARGUREMENTS, script);
            }

            Variable result = ARun(args, script, null, current);
            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = child.GetVariable(script, current);
                }
            }
            return result;
        }
        public Variable ARun(List<Variable> args = null, ParsingScript script = null,
                            AliceScriptClass.ClassInstance instance = null, Variable current = null)
        {

            Variable result = null;
            ParsingScript tempScript = Utils.GetTempScript(m_body, null, m_name, m_parentScript,
                                                           m_parentScript, m_parentOffset, instance);
            tempScript.Filename = m_parentScript.Filename;
            if (script != null)
            {
                tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(script.m_stacktrace);
                tempScript.m_stacktrace.Add(new ParsingScript.StackInfo(this, script.OriginalLine, script.OriginalLineNumber, script.Filename));
            }
            tempScript.Tag = m_tag;
            //tempScript.Variables = m_VarMap;
            List<KeyValuePair<string, Variable>> args2 = instance == null ? null : instance.GetPropList();
            // ひとまず引数をローカルに追加
            RegisterArguments(args, args2, current, tempScript);

            // さて実行

            while (tempScript.Pointer < m_body.Length &&
                  (result == null || !result.IsReturn))
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }

            if (result == null || (!result.IsReturn && !m_forceReturn))
            {
                result = Variable.EmptyInstance;
            }

            result.IsReturn = false;


            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = child.ARun(args, script, instance, current);
                }
            }

            return result;
        }


        public override ParserFunction NewInstance()
        {
            var newInstance = (CustomFunction)MemberwiseClone();
            return newInstance;
        }
        /// <summary>
        /// この関数の定義元のスクリプト
        /// </summary>
        public ParsingScript ParentScript { set => m_parentScript = value; }
        /// <summary>
        /// この関数が定義されたときのオフセット
        /// </summary>
        public int ParentOffset { set => m_parentOffset = value; }
        /// <summary>
        /// この関数の実行内容を表す本文
        /// </summary>
        public string Body => m_body;
        /// <summary>
        /// この関数のとる引数の個数
        /// </summary>
        public int ArgumentCount => m_args.Length;

        public StackLevel NamespaceData { get; set; }
        public TypeObject MethodRequestType => IsMethod && m_typArgMap.ContainsKey(m_this) ? m_typArgMap[m_this] : new TypeObject();

        public int DefaultArgsCount => m_defaultArgs.Count;


        protected int m_this = -1;
        protected string m_body;
        protected object m_tag;
        protected bool m_forceReturn;
        protected bool m_nullable;
        protected string[] m_args;
        protected Variable.VarType m_returnType;
        protected ParsingScript m_parentScript = null;
        protected int m_parentOffset = 0;
        private List<Variable> m_defaultArgs = new List<Variable>();
        private List<int> m_refMap = new List<int>();
        private Dictionary<int, int> m_defArgMap = new Dictionary<int, int>();
        private Dictionary<int, TypeObject> m_typArgMap = new Dictionary<int, TypeObject>();

        private Dictionary<string, int> ArgMap { get; set; } = new Dictionary<string, int>();
    }

    internal sealed class FunctionCreator : FunctionBase
    {
        public FunctionCreator()
        {
            Name = Constants.FUNCTION;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += FunctionCreator_Run;
        }
        internal static bool DefineFunction(string funcName, ParsingScript script, HashSet<string> keywords)
        {
            bool? mode = null;
            bool isGlobal = keywords.Contains(Constants.PUBLIC);
            bool isCommand = keywords.Contains(Constants.COMMAND);
            bool isExtension = keywords.Contains(Constants.EXTENSION);
            if (keywords.Contains(Constants.OVERRIDE))
            {
                mode = true;
            }
            else if (keywords.Contains(Constants.VIRTUAL))
            {
                mode = false;
            }

            Variable.VarType type_modifer = Variable.VarType.VARIABLE;
            bool nullable = false;
            foreach (string str in keywords)
            {
                string type_str = str.TrimEnd(Constants.TERNARY_OPERATOR);
                nullable = type_str.Length != str.Length;
                if (Constants.TYPE_MODIFER.Contains(type_str))
                {
                    type_modifer = Constants.StringToType(str);
                    break;
                }
            }

            funcName = Constants.ConvertName(funcName);

            string[] args = Utils.GetFunctionSignature(script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = new string[0];
            }
            if (script.Current != Constants.START_GROUP)
            {
                return false;
            }

            script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            script.GetOriginalLine(out _);

            int parentOffset = script.Pointer;

            if (script.CurrentClass != null)
            {
                parentOffset += script.CurrentClass.ParentOffset;
            }

            string body = Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP);
            script.MoveForwardIf(Constants.END_GROUP);

            CustomFunction customFunc = new CustomFunction(funcName, body, args, script, false, type_modifer, nullable);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            customFunc.MethodOnly = !isExtension;
            if (isCommand)
            {
                customFunc.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            }
            if (mode != null)
            {
                customFunc.IsVirtual = true;
            }
            if (script.CurrentClass != null)
            {
                script.CurrentClass.AddMethod(funcName, args, customFunc);
            }
            else
            {
                if (!FunctionExists(funcName, script, out _) || (mode == true && FunctionIsVirtual(funcName, script)))
                {
                    FunctionBaseManager.Add(customFunc, funcName, script, isGlobal);
                }
                else
                {
                    throw new ScriptException("指定された関数はすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED, script);
                }
            }
            return true;
        }

        private void FunctionCreator_Run(object sender, FunctionBaseEventArgs e)
        {
            string funcName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            DefineFunction(funcName, e.Script, Keywords);
        }

        private static bool FunctionIsVirtual(string name, ParsingScript script)
        {
            if (script != null && script.TryGetFunction(name, out ParserFunction impl))
            {
                if (impl.IsVirtual)
                {
                    return true;
                }
            }
            if (s_functions.TryGetValue(name, out impl))
            {
                if (impl.IsVirtual)
                {
                    return true;
                }
            }
            return false;
        }
    }

}
