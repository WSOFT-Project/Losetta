using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceScript.Functions
{
    /// <summary>
    /// ユーザー定義の関数またはデリゲートを表します
    /// </summary>
    public class CustomFunction : FunctionBase
    {
        /// <summary>
        /// 新しいユーザー定義関数を作成します
        /// </summary>
        /// <param name="funcName">関数の名前</param>
        /// <param name="body">関数の本文</param>
        /// <param name="args">引数の一覧</param>
        /// <param name="script">この関数が定義されているスクリプト</param>
        /// <param name="forceReturn">この関数が最後に評価した値を返す場合はtrue、そうでない場合はfalse</param>
        /// <param name="returnType">戻り値の型</param>
        /// <param name="nullable"></param>
        /// <exception cref="ScriptException"></exception>
        public CustomFunction(string funcName,
                                string body, string[] args, ParsingScript script, bool forceReturn = false, Variable.VarType returnType = Variable.VarType.VARIABLE, bool nullable = true)
        {
            Name = funcName;
            m_body = body;
            m_nullable = nullable;
            m_forceReturn = forceReturn;
            m_returnType = returnType;
            Run += CustomFunction_Run;

            if (m_returnType == Variable.VarType.VOID)
            {
                m_nullable = true;
            }

            //正確な変数名の一覧
            List<string> trueArgs = new List<string>();

            bool parms = false;
            bool refs = false;
            bool readonlys = false;

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
                if (arg.Contains(Constants.SPACE))
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
                        if (option.StartsWith('=') || option.EndsWith('='))
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
                    int zure = 0;
                    parms = options.Contains(Constants.PARAMS);
                    refs = options.Contains(Constants.REF);
                    readonlys = options.Contains(Constants.READONLY);
                    if(parms)
                    {
                        zure++;
                    }
                    if(refs)
                    {
                        zure++;
                    }
                    if (readonlys)
                    {
                        zure++;
                    }
                    if (options.Contains(Constants.THIS))
                    {
                        if (refs)
                        {
                            throw new ScriptException("拡張メソッドのレシーバにrefキーワードを使用することはできません", Exceptions.INVALID_KEYWORD_CONBINATION, script);
                        }
                        else
                        {
                            zure++;
                            m_this = m_this == -1 ? i : throw new ScriptException("this修飾子は一つのメソッドに一つのみ設定可能です", Exceptions.INVALID_ARGUMENT_FUNCTION, script);

                        }
                    }
                    // 型宣言がある場合
                    if (options.Count > zure + 1)
                    {
                        string typeStr = options[options.Count - 2];
                        Variable v;
                        if(typeStr.Equals(Constants.VAR, StringComparison.OrdinalIgnoreCase))
                        {
                            // varキーワードの場合
                            v = new Variable(new TypeObject());
                        }
                        else
                        {
                            // 他の型の場合は実行してみて確認
                            v = script.GetTempScript(options[options.Count - 2]).Execute();
                        }
                        if (v is not null && v.Type == Variable.VarType.OBJECT && v.Object is TypeObject to)
                        {
                            reqType = to;
                        }
                        m_typArgMap[i] = reqType;
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
                        string argName = arg.Trim();
                        if (parms && refs)
                        {
                            throw new ScriptException(Constants.PARAMS + "パラメータを参照渡しに設定することはできません。", Exceptions.INCOMPLETE_FUNCTION_DEFINITION, script);
                        }

                        if (refs)
                        {
                            m_refMap.Add(i);
                        }
                        if (parms)
                        {
                            parmsindex = i;
                        }
                        if (readonlys)
                        {
                            m_readonlyMap.Add(i);
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

            if (m_args is null)
            {
                m_args = Array.Empty<string>();
            }
            if (e.Args.Count + m_defaultArgs.Count + (e.CurentVariable is not null ? 1 : 0) < m_args.Length)
            {
                throw new ScriptException($"関数`{m_args.Length}`は引数`{m_args.Length}`を受取ることが出来ません。", Exceptions.TOO_MANY_ARGUREMENTS, e.Script);
            }
            Variable result = ARun(e.Args, e.Script, e.ClassInstance, e.CurentVariable);
            if (m_nullable && m_returnType != Variable.VarType.VOID && result.IsNull())
            {
                // nullをとる場合は妥当なnull許容型に置き換える
                result.Type = m_returnType;
            }
            if (m_nullable)
            {
                result.Nullable = true;
            }
            if ((m_returnType != Variable.VarType.VARIABLE && (!result.Type.HasFlag(m_returnType) || (!m_nullable && result.Nullable))) || (m_returnType == Variable.VarType.VOID && result.Type != Variable.VarType.VOID))
            {
                throw new ScriptException($"関数は宣言とは異なり{result.Type}{(result.Nullable ? "?" : "")}型を返しました", Exceptions.TYPE_MISMATCH, m_parentScript);
            }
            e.Return = result;
        }

        private int parmsindex = -1;
        private void RegisterArguments(List<Variable> args,
                                      List<KeyValuePair<string, Variable>> args2 = null, Variable current = null, ParsingScript script = null)
        {
            if (args is null)
            {
                args = new List<Variable>();
            }
            if (m_this != -1 && current is not null)
            {
                args.Insert(m_this, current);
            }
            if (m_args is null)
            {
                m_args = new List<string>().ToArray();
            }
            int missingArgs = m_args.Length - args.Count;
            bool namedParameters = false;
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                int argIndex = -1;
                if (m_typArgMap.Count > i && !m_typArgMap[i].Match(arg))
                {
                    throw new ScriptException("この引数にその型を使用することはできません", Exceptions.WRONG_TYPE_VARIABLE);
                }
                else
                {

                    if (arg is not null && ArgMap.TryGetValue(arg.CurrentAssign, out argIndex))
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
                        throw new ScriptException("関数の引数と値 `" + m_name + "` は一対一で一致する必要があります。", Exceptions.INVALID_ARGUMENT_FUNCTION);
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
                        if (args[i].Type == Variable.VarType.VOID ||
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

            if (args2 is not null)
            {
                foreach (var entry in args2)
                {
                    var val = new Variable();
                    val.Assign(entry.Value);
                    var arg = new ValueFunction(val);
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
                    var arg = new ValueFunction(parmsarg);
                    arg.Name = m_args[i];
                    //m_VarMap[m_args[i]] = arg;
                    script.Variables[m_args[i]] = arg;
                    i = maxSize;
                }
                else
                {
                    Variable val;

                    bool refd = args[i].Type == Variable.VarType.REFERENCE;
                    if (m_refMap.Contains(i))
                    {
                        if(!refd)
                        {
                            throw new ScriptException("引数 `" + ArgMap.Where(kvp => kvp.Value == i).FirstOrDefault().Key + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                        }
                        var v = args[i].Reference;
                        if(v is ValueFunction vf)
                        {
                            val = vf.Value;
                        }
                        else
                        {
                            throw new ScriptException($"引数 `{ArgMap.Where(kvp => kvp.Value == i).FirstOrDefault().Key}` で、変数以外への参照が渡されました", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                        }
                    }
                    else
                    {
                        if (refd)
                        {
                            throw new ScriptException("引数 `" + ArgMap.Where(kvp => kvp.Value == i).FirstOrDefault().Key + "` は `" + Constants.REF + "' キーワードと共に使用することができません。", Exceptions.ARGUMENT_CANT_USE_WITH_KEYWORD, script);
                        }
                        val = args[i];
                    }
                    if (m_readonlyMap.Contains(i))
                    {
                        m_readonlyTypeMap[i] = val.Readonly;
                        val.Readonly = true;
                    }
                    var arg = new ValueFunction(val);
                    arg.Name = m_args[i];
                    //m_VarMap[m_args[i]] = arg;
                    script.Variables[m_args[i]] = arg;
                }
            }
            if (parmsindex < 0 && m_args.Length < args.Count)
            {
                throw new ScriptException($"関数 `{m_name}`は、{m_args.Length}個よりも多く引数を持つことができません", Exceptions.TOO_MANY_ARGUREMENTS, script);
            }
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
            return args.Count + m_defaultArgs.Count + 1 < m_args.Length
                ? throw new ScriptException("この関数は、最大で" + (args.Count + m_defaultArgs.Count + 1) + "個の引数を受け取ることができますが、" + m_args.Length + "個の引数が渡されました", Exceptions.TOO_MANY_ARGUREMENTS, script)
                : ARun(args, script, null, current);
        }
        public Variable ARun(List<Variable> args = null, ParsingScript script = null,
                            AliceScriptClass.ClassInstance instance = null, Variable current = null)
        {

            Variable result = null;
            ParsingScript tempScript = Utils.GetTempScript(m_body, m_parentScript,
                                                           m_parentScript, m_parentOffset, instance);
            tempScript.Filename = m_parentScript.Filename;
            if (script is not null)
            {
                tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(script.m_stacktrace);
                tempScript.m_stacktrace.Add(new ParsingScript.StackInfo(this, script.OriginalLine, script.OriginalLineNumber, script.Filename));
            }
            tempScript.Tag = m_tag;
            //tempScript.Variables = m_VarMap;
            List<KeyValuePair<string, Variable>> args2 = instance is null ? null : instance.GetPropList();
            // ひとまず引数をローカルに追加
            RegisterArguments(args, args2, current, tempScript);

            // さて実行

            while (tempScript.Pointer < m_body.Length &&
                  (result is null || !result.IsReturn))
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }

            if (result is null || (!result.IsReturn && !m_forceReturn))
            {
                result = new Variable(Variable.VarType.VOID);
            }

            result.IsReturn = false;

            foreach (var entry in m_readonlyTypeMap)
            {
                args[entry.Key].Readonly = entry.Value;
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

        public TypeObject MethodRequestType => IsMethod && m_typArgMap.Count >= m_this ? m_typArgMap[m_this] : new TypeObject();

        public int DefaultArgsCount => m_defaultArgs.Count;

        public Variable.VarType ReturnType => m_returnType;

        protected int m_this = -1;
        protected string m_body;
        protected object m_tag;
        protected bool m_forceReturn;
        protected bool m_nullable;
        protected string[] m_args;
        protected Variable.VarType m_returnType;
        protected ParsingScript m_parentScript = null;
        protected int m_parentOffset = 0;
        private Dictionary<int, TypeObject> m_typArgMap = new Dictionary<int, TypeObject>();
        private List<Variable> m_defaultArgs = new List<Variable>();
        private List<int> m_refMap = new List<int>();
        private List<int> m_readonlyMap = new List<int>();
        private Dictionary<int, bool> m_readonlyTypeMap = new Dictionary<int, bool>();
        private Dictionary<int, int> m_defArgMap = new Dictionary<int, int>();

        private Dictionary<string, int> ArgMap { get; set; } = new Dictionary<string, int>();
    }
}
