namespace AliceScript
{
    public class ParserFunction
    {
        public static Action<string, Variable, bool> OnVariableChange;

        /// <summary>
        /// オーバーライド可能かどうかを表す値
        /// </summary>
        public bool IsVirtual { get; set; }

        public HashSet<string> Keywords
        {
            get => m_keywords;
            set => m_keywords = value;
        }

        private HashSet<string> m_keywords = new HashSet<string>();

        public ParserFunction()
        {
            m_impl = this;
        }

        public ParserFunction(ParsingScript script, string item, char ch, ref string action, HashSet<string> keywords = null)
        {
            if (keywords == null)
            {
                keywords = new HashSet<string>();
            }


            m_impl = CheckString(script, item, ch);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetLambdaFunction(script, item, ch, ref action);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            item = Constants.ConvertName(item);

            m_impl = GetRegisteredAction(item, script, ref action);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetArrayFunction(item, script, action);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetObjectFunction(item, script, keywords);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetVariable(item, script, false, keywords);
            if (m_impl != null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            if (m_impl == null)
            {
                Utils.ProcessErrorMsg(item, script);
            }

        }

        public static ParserFunction CheckString(ParsingScript script, string item, char ch)
        {
            StringOrNumberFunction stringOrNumberFunction = new StringOrNumberFunction();

            if (item.Length > 0 && char.IsDigit(item[0]))
            {
                stringOrNumberFunction.Item = item;
                stringOrNumberFunction.StringMode = false;
                return stringOrNumberFunction;
            }

            if (item.Length > 3 && item.StartsWith(Constants.UTF8_LITERAL_PREFIX, StringComparison.Ordinal))
            {
                item = item.Substring(Constants.UTF8_LITERAL_PREFIX.Length);
                stringOrNumberFunction.DetectionUTF8_Literal = true;
            }

            if (item.Length > 2 && item.StartsWith(Constants.DOLLER))
            {
                item = item.Substring(1);
                stringOrNumberFunction.DetectionStringFormat = true;
            }

            if (IsQuotedString(item))
            {
                stringOrNumberFunction.Item = item;
                stringOrNumberFunction.StringMode = true;
                return stringOrNumberFunction;
            }

            if (script.ProcessingList && ch == ':')
            {
                stringOrNumberFunction.Item = '"' + item + '"';
                stringOrNumberFunction.StringMode = true;
                return stringOrNumberFunction;
            }

            return null;
        }

        private static bool IsQuotedString(string item)
        {
            return item.Length > 1 && ((item[0] == Constants.QUOTE && item[item.Length - 1] == Constants.QUOTE) ||
                    (item[0] == Constants.QUOTE1 && item[item.Length - 1] == Constants.QUOTE1));
        }


        public static ParserFunction GetArrayFunction(string name, ParsingScript script, string action)
        {
            int arrayStart = name.IndexOf(Constants.START_ARRAY, StringComparison.Ordinal);
            if (arrayStart < 0)
            {
                return null;
            }

            if (arrayStart == 0)
            {
                //Variable arr = Utils.ProcessArrayMap(new ParsingScript(name));
                Variable arr = Utils.ProcessArrayMap(script.GetTempScript(name));
                return new GetVarFunction(arr);
            }

            string arrayName = name;

            string varName = arrayName.Substring(0, arrayStart);
            Variable ary = Utils.GetItem(script.GetTempScript(varName));
            int max = ary == null ? 0 : ary.Count;
            int delta = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, arrayName, delta, (string arr, int del) => { arrayName = arr; delta = del; }, null, max);

            if (arrayIndices.Count == 0)
            {
                return null;
            }

            ParserFunction pf = ParserFunction.GetVariable(arrayName, script);
            GetVarFunction varFunc = pf as GetVarFunction;
            if (varFunc == null)
            {
                return null;
            }

            // we temporarily backtrack for the processing
            script.Backward(name.Length - arrayStart - 1);
            script.Backward(action != null ? action.Length : 0);
            // delta shows us how manxy chars we need to advance forward in GetVarFunction()
            delta -= arrayName.Length;
            delta += action != null ? action.Length : 0;

            varFunc.Indices = arrayIndices;
            varFunc.Delta = delta;
            return varFunc;
        }

        public static ParserFunction GetObjectFunction(string name, ParsingScript script, HashSet<string> keywords)
        {
            if (script.CurrentClass != null && script.CurrentClass.Name == name)
            {
                script.Backward(name.Length + 1);
                return new FunctionCreator();
            }
            if (script.ClassInstance != null &&
               (script.ClassInstance.PropertyExists(name) || script.ClassInstance.FunctionExists(name)))
            {
                name = script.ClassInstance.InstanceName + "." + name;
            }
            //int ind = name.LastIndexOf('.');
            int ind = name.IndexOf('.', StringComparison.Ordinal);
            if (ind <= 0)
            {
                return null;
            }
            string baseName = name.Substring(0, ind);
            if (s_namespaces.ContainsKey(baseName))
            {
                int ind2 = name.IndexOf('.', ind + 1);
                if (ind2 > 0)
                {
                    ind = ind2;
                    baseName = name.Substring(0, ind);
                }
            }

            string prop = name.Substring(ind + 1);

            ParserFunction pf = ParserFunction.GetFromNamespace(prop, baseName, script);
            if (pf != null)
            {
                pf.Keywords = keywords;
                return pf;
            }

            pf = ParserFunction.GetVariable(baseName, script, true);
            if (pf == null || !(pf is GetVarFunction))
            {
                pf = ParserFunction.GetFunction(baseName, script);
                if (pf == null)
                {
                    pf = Utils.ExtractArrayElement(baseName, script);
                }
            }

            GetVarFunction varFunc = pf as GetVarFunction;
            if (varFunc == null)
            {
                return null;
            }

            varFunc.PropertyName = prop;
            varFunc.Keywords = keywords;
            return varFunc;
        }

        private static bool ActionForUndefined(string action)
        {
            return !string.IsNullOrWhiteSpace(action) && action.EndsWith("=", StringComparison.Ordinal) && action.Length > 1;
        }
        public static ParserFunction GetLambdaFunction(ParsingScript script, string item, char ch, ref string action)
        {
            if (action == Constants.ARROW)
            {
                string[] args = Utils.GetFunctionSignature(script.GetTempScript(item), true);
                if (args.Length > 0 && args[0].Trim() == Constants.DESTRUCTION.ToString())
                {
                    args = new string[] { };
                }

                string body = Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG, Constants.TOKENS_SEPARATION_WITHOUT_BRACKET);

                int parentOffset = script.Pointer;
                if (script.CurrentClass != null)
                {
                    parentOffset += script.CurrentClass.ParentOffset;
                }

                CustomFunction customFunc = CreateCustomFunction(body, args, script, parentOffset);

                action = null;
                return new GetVarFunction(new Variable(customFunc));
            }
            return null;
        }

        private static CustomFunction CreateCustomFunction(string body, string[] args, ParsingScript script, int parentOffset)
        {
            CustomFunction customFunc = new CustomFunction("", body, args, script, true);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            return customFunc;
        }

        public static ParserFunction GetRegisteredAction(string name, ParsingScript script, ref string action)
        {
            if (Constants.CheckReserved(name))
            {
                return null;
            }

            if (false && ActionForUndefined(action) && script.Rest.StartsWith(Constants.UNDEFINED, StringComparison.Ordinal))
            {
                IsUndefinedFunction undef = new IsUndefinedFunction(name, action);
                return undef;
            }

            ActionFunction actionFunction = GetAction(action);

            // If passed action exists and is registered we are done.
            if (actionFunction == null)
            {
                return null;
            }

            ActionFunction theAction = actionFunction.NewInstance() as ActionFunction;
            theAction.Name = name;
            theAction.Action = action;

            action = null;
            return theAction;
        }

        public static bool TryAddToNamespace(string name, string nameSpace, Variable varValue)
        {
            if (string.IsNullOrWhiteSpace(nameSpace) ||
               !s_namespaces.TryGetValue(nameSpace, out StackLevel level))
            {
                return false;
            }

            var vars = level.Variables;
            vars[name] = new GetVarFunction(varValue);

            return true;
        }

        public static ParserFunction GetFromNamespace(string name, ParsingScript script)
        {
            ParserFunction result = GetFromNamespace(name, s_namespace, script);
            return result;
        }

        public static ParserFunction GetFromNamespace(string name, string nameSpace, ParsingScript script)
        {
            if (string.IsNullOrWhiteSpace(nameSpace))
            {
                return null;
            }

            int ind = nameSpace.IndexOf('.', StringComparison.Ordinal);
            string prop = "";
            if (ind >= 0)
            {
                prop = name;
                name = nameSpace.Substring(ind + 1);
                nameSpace = nameSpace.Substring(0, ind);
            }

            if (!s_namespaces.TryGetValue(nameSpace, out StackLevel level))
            {
                return null;
            }

            if (!name.StartsWith(nameSpace, StringComparison.OrdinalIgnoreCase))
            {
                name = nameSpace + "." + name;
            }

            var vars = level.Variables;
            if (!vars.TryGetValue(name, out ParserFunction impl) &&
                !s_variables.TryGetValue(name, out impl) &&
                !s_functions.TryGetValue(name, out impl)
                )
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(prop) && impl is GetVarFunction)
            {
                ((GetVarFunction)impl).PropertyName = prop;
            }
            return impl;
        }

        public static ParserFunction GetVariable(string name, ParsingScript script = null, bool force = false, HashSet<string> keywords = null)
        {
            if (!force && script != null && script.TryPrev() == Constants.START_ARG)
            {
                return GetFunction(name, script);
            }

            name = Constants.ConvertName(name);

            ParserFunction impl;
            StackLevel localStack = script != null && script.StackLevel != null ?
                 script.StackLevel : s_locals.Count > StackLevelDelta ? s_lastExecutionLevel : null;
            if (localStack != null)
            {
                Dictionary<string, ParserFunction> local = localStack.Variables;
                if (local.TryGetValue(name, out impl))
                {
                    return impl;
                }
            }
            //ローカルスコープに存在するか確認
            string scopeName = script == null || script.Filename == null ? "" : script.Filename;
            impl = GetLocalScopeVariable(name, scopeName);
            if (impl != null)
            {
                return impl;
            }
            if (script != null && script.TryGetVariable(name, out impl))
            {
                return impl.NewInstance();
            }
            if (s_variables.TryGetValue(name, out impl))
            {
                return impl.NewInstance();
            }

            //定数に存在するか確認
            if (script != null && script.TryGetConst(name, out impl) && impl != null)
            {
                return impl.NewInstance();
            }
            if (Constants.CONSTS.ContainsKey(name))
            {
                return new GetVarFunction(Constants.CONSTS[name]);
            }

            //関数として取得を続行
            var pfx = GetFunction(name, script, true);
            if (pfx != null)
            {
                pfx.Keywords = keywords;
            }
            return pfx;
        }

        public static ParserFunction GetFunction(string name, ParsingScript script, bool toDelegate = false)
        {
            //TODO:関数の取得部分
            name = Constants.ConvertName(name);
            if (script.TryGetFunction(name, out ParserFunction impl) || s_functions.TryGetValue(name, out impl))
            {
                if (toDelegate && impl is CustomFunction cf)
                {
                    //デリゲートとして返したい場合
                    var f = new Variable(cf);
                    f.Readonly = f.TypeChecked = true;
                    return new GetVarFunction(f);
                }
                else
                {
                    return impl.NewInstance();
                }
            }
            if (script.TryGetVariable(name, out impl) || s_variables.TryGetValue(name, out impl))
            {
                //それがデリゲート型の変数である場合
                if (impl is GetVarFunction gv && gv.Value.Type == Variable.VarType.DELEGATE && !gv.Value.IsNull())
                {
                    return gv.Value.Delegate.Function;
                }
            }

            var fc = GetFromNS(name, script);
            if (fc != null)
            {
                return fc;
            }

            //ちょっとでも高速化（ここのロジックは時間がかかる）
            if (name.Contains("."))
            {
                string namespacename = string.Empty;

                foreach (var ns in NameSpaceManager.NameSpaces)
                {
                    var nsn = ns.Key.ToLower();
                    //より長い名前（AliceとAlice.IOならAlice.IO）を採用
                    if (name.StartsWith(nsn + ".", StringComparison.Ordinal) && nsn.Length > namespacename.Length)
                    {
                        namespacename = nsn;
                    }
                }

                //完全修飾名で関数を検索
                if (namespacename != string.Empty)
                {
                    fc = NameSpaceManager.NameSpaces.Where(x => x.Key.ToLower() == namespacename).FirstOrDefault().Value.Functions.Where((x) => name.StartsWith(namespacename + "." + x.Name.ToLower(), StringComparison.Ordinal)).FirstOrDefault();
                    if (fc != null)
                    {
                        return fc;
                    }
                    var cc = NameSpaceManager.NameSpaces.Where(x => x.Key.ToLower() == namespacename).FirstOrDefault().Value.Classes.Where((x) => name.StartsWith(namespacename + "." + x.Name.ToLower(), StringComparison.Ordinal)).FirstOrDefault();
                    if (cc != null)
                    {
                        return new GetVarFunction(new Variable(new TypeObject(cc)));
                    }
                }
            }
            string className = Constants.ConvertName(name);

            var csClass = AliceScriptClass.GetClass(className, script);
            return csClass != null ? new GetVarFunction(new Variable(new TypeObject(csClass))) : GetFromNamespace(name, script);
        }
        private static ParserFunction GetFromNS(string name, ParsingScript script)
        {
            foreach (var nm in script.UsingNamespaces)
            {
                var fc = nm.Functions.Where((x) => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (fc != null)
                {
                    return fc;
                }
                var cc = nm.Classes.Where((x) => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (cc != null)
                {
                    return new GetVarFunction(new Variable(new TypeObject(cc)));
                }
            }
            return script.ParentScript != null ? GetFromNS(name, script.ParentScript) : null;
        }
        public static ActionFunction GetAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            if (s_actions.TryGetValue(action, out ActionFunction impl))
            {
                // Action exists and is registered (e.g. =, +=, --, etc.)
                return impl;
            }

            return null;
        }

        public static bool FunctionExists(string item, ParsingScript script, out ParserFunction func, bool continueConst = false)
        {
            // If it is not defined locally, then check globally:
            return (script != null && script.TryGetLocal(item, out func)) || TryGetGlobal(item, out func, continueConst);
        }

        public static void AddGlobalOrLocalVariable(string name, GetVarFunction function,
            ParsingScript script, bool localIfPossible = false, bool registVar = false, bool globalOnly = false, string type_modifer = null, bool isReadOnly = false, bool fromAssign = false)
        {
            name = Constants.ConvertName(name);
            Utils.CheckLegalName(name, fromAssign);


            function.Name = Constants.GetRealName(name);
            function.Value.ParamName = function.Name;

            if (globalOnly)
            {
                script = ParsingScript.GetTopLevelScript(script);
            }

            bool type_inference = script.TypeInference;
            NormalizeValue(function);
            function.m_isGlobal = false;
            name = Constants.ConvertName(function.Name);

            function.Name = Constants.GetRealName(name);
            if (function is GetVarFunction)
            {
                function.Value.ParamName = function.Name;
            }
            bool exists = FunctionExists(name, script, out var func);
            bool unneed = script.UnneedVarKeyword;

            if (exists && registVar)
            {
                throw new ScriptException("変数[" + name + "]はすでに定義されています", Exceptions.VARIABLE_ALREADY_DEFINED, script);
            }
            else if (!exists && !registVar && !unneed && !string.IsNullOrEmpty(name))
            {
                throw new ScriptException("変数[" + name + "]は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
            }
            if (func != null && func is GetVarFunction v)
            {
                //代入の場合
                if (v.Value.Parent == null)
                {
                    v.Value.Parent = script;
                }
                if (function is GetVarFunction g2)
                {
                    v.Value.Assign(g2.Value);
                }
            }
            else if (func == null)
            {
                //変数定義の場合
                if (function is GetVarFunction v2)
                {
                    Variable newVar = Variable.EmptyInstance;
                    newVar.Parent = script;
                    if (type_modifer != null)
                    {
                        newVar.TypeChecked = true;
                        if (type_modifer.EndsWith("?", StringComparison.Ordinal))
                        {
                            newVar.Nullable = true;
                            type_modifer = type_modifer.Substring(0, type_modifer.Length - 1);
                        }
                        newVar.Type = Constants.StringToType(type_modifer);
                    }
                    else
                    {
                        //型指定がない場合はnullを許容する
                        newVar.Nullable = true;
                    }
                    newVar.Assign(v2.Value);
                    if (type_inference && type_modifer == Constants.VAR)
                    {
                        newVar.TypeChecked = true;
                    }
                    newVar.Readonly = isReadOnly;
                    function = new GetVarFunction(newVar);
                }
                script.Variables[name] = function;
            }

        }

        public static bool TryGetGlobal(string name, out ParserFunction function, bool continueConst = false)
        {
            name = Constants.ConvertName(name);
            if (s_variables.TryGetValue(name, out function) || s_functions.TryGetValue(name, out function))
            {
                return true;
            }
            if (Constants.CONSTS.TryGetValue(name, out var v) && !continueConst)
            {
                function = new GetVarFunction(v);
                return true;
            }
            return false;
        }

        public static Variable RegisterEnum(string varName, string enumName, ParsingScript script = null)
        {
            Variable enumVar = EnumFunction.UseExistingEnum(enumName);
            if (enumVar == Variable.EmptyInstance)
            {
                return enumVar;
            }
            if (script == null)
            {
                RegisterFunction(varName, new GetVarFunction(enumVar));
            }
            else
            {
                RegisterScriptFunction(varName, new GetVarFunction(enumVar), script);
            }
            return enumVar;
        }

        public static void RegisterFunction(string name, ParserFunction function,
                                            bool isNative = true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

            if (!string.IsNullOrWhiteSpace(s_namespace))
            {
                if (s_namespaces.TryGetValue(s_namespace, out StackLevel level) &&
                   function is CustomFunction)
                {
                    ((CustomFunction)function).NamespaceData = level;
                    name = s_namespacePrefix + name;
                }
            }
            if (!s_functions.ContainsKey(name) || (s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
            {
                //まだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                s_functions[name] = function;
                function.isNative = isNative;
                if (s_functions.ContainsKey(name) && s_functions[name].IsVirtual)
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static void RegisterScriptFunction(string name, ParserFunction function, ParsingScript script, bool isNative = true, bool isLocal = true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

            if (isLocal && (!FunctionExists(name, script, out ParserFunction impl, true) || impl.IsVirtual))
            {
                //ローカル関数でまだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                script.Functions[name] = function;
                function.isNative = isNative;
                if (impl != null)
                {
                    impl.IsVirtual = true;
                }
            }
            else if (!isLocal && (!s_functions.ContainsKey(name) || (s_functions.ContainsKey(name) && s_functions[name].IsVirtual)))
            {
                //まだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                s_functions[name] = function;
                function.isNative = isNative;
                if (s_functions.ContainsKey(name) && s_functions[name].IsVirtual)
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static bool UnregisterScriptFunction(string name, ParsingScript script)
        {
            name = Constants.ConvertName(name);
            return script != null && script.Functions.Remove(name) ? true : s_functions.Remove(name);
        }
        public static bool UnregisterFunction(string name)
        {
            name = Constants.ConvertName(name);

            bool removed = s_functions.Remove(name);
            return removed;
        }

        public static bool RemoveGlobal(string name)
        {
            name = Constants.ConvertName(name);
            return s_variables.Remove(name);
        }

        private static void NormalizeValue(ParserFunction function)
        {
            GetVarFunction gvf = function as GetVarFunction;
            if (gvf != null)
            {
                gvf.Value.CurrentAssign = "";
            }
        }
        private static ParserFunction GetLocalScopeVariable(string name, string scopeName)
        {
            scopeName = Path.GetFileName(scopeName);
            if (!s_localScope.TryGetValue(scopeName, out Dictionary<string, ParserFunction> localScope))
            {
                return null;
            }

            name = Constants.ConvertName(name);
            localScope.TryGetValue(name, out ParserFunction function);
            return function;
        }

        public static void AddAction(string name, ActionFunction action)
        {
            s_actions[name] = action;
        }


        public static int GetCurrentStackLevel()
        {
            lock (s_variables)
            {
                return s_locals.Count;
            }
        }

        public Variable GetValue(ParsingScript script)
        {
            return m_impl?.Evaluate(script);
        }

        public async Task<Variable> GetValueAsync(ParsingScript script)
        {
            return await m_impl.EvaluateAsync(script);
        }

        protected virtual Variable Evaluate(ParsingScript script)
        {
            // The real implementation will be in the derived classes.
            return new Variable();
        }

        protected virtual Task<Variable> EvaluateAsync(ParsingScript script)
        {
            // If not overriden, the non-sync version will be called.
            return Task.FromResult(Evaluate(script));
        }

        // Derived classes may want to return a new instance in order to
        // not to use same object in calculations.
        public virtual ParserFunction NewInstance()
        {
            return this;
        }

        public static void CleanUp()
        {
            s_functions.Clear();
            s_actions.Clear();
            CleanUpVariables();
        }

        public static void CleanUpVariables()
        {
            s_variables.Clear();
            s_locals.Clear();
            s_localScope.Clear();
            s_namespaces.Clear();
            s_namespace = s_namespacePrefix = "";
        }

        public string Name
        {
            get => m_name;
            set => m_name = value;
        }
        protected string m_name = "";

        protected bool m_isGlobal = true;
        public bool isGlobal { get => m_isGlobal; set => m_isGlobal = value; }

        protected bool m_isNative = true;
        public bool isNative { get => m_isNative; set => m_isNative = value; }

        internal ParserFunction m_impl;

        // Global functions:
        public static Dictionary<string, ParserFunction> s_functions = new Dictionary<string, ParserFunction>();

        // Global variables:
        public static Dictionary<string, ParserFunction> s_variables = new Dictionary<string, ParserFunction>();

        // Global actions to functions map:
        private static Dictionary<string, ActionFunction> s_actions = new Dictionary<string, ActionFunction>();

        // Local scope variables:
        private static Dictionary<string, Dictionary<string, ParserFunction>> s_localScope =
           new Dictionary<string, Dictionary<string, ParserFunction>>();

        public static bool IsNumericFunction(string paramName, ParsingScript script = null)
        {
            ParserFunction function = ParserFunction.GetFunction(paramName, script);
            return function is INumericFunction;
        }



        public class StackLevel
        {
            private static int s_id;

            public StackLevel(string name = null, bool isNamespace = false)
            {
                Id = ++s_id;
                Name = name;
                IsNamespace = isNamespace;
                Variables = new Dictionary<string, ParserFunction>();
            }

            public string Name { get; private set; }
            public bool IsNamespace { get; private set; }
            public int Id { get; private set; }

            public Dictionary<string, ParserFunction> Variables { get; set; }
        }

        // Local variables:
        // Stack of the functions being executed:
        private static Stack<StackLevel> s_locals = new Stack<StackLevel>();
        public static Stack<StackLevel> ExecutionStack => s_locals;

        private static StackLevel s_lastExecutionLevel;
        private static Dictionary<string, StackLevel> s_namespaces = new Dictionary<string, StackLevel>();
        private static string s_namespace;
        private static string s_namespacePrefix;

        public static string GetCurrentNamespace => s_namespace;
        public static int StackLevelDelta { get; set; }
    }

    public abstract class ActionFunction : FunctionBase
    {
        protected string m_action;
        public string Action { set => m_action = value; }
    }
}