using System.Text;

namespace AliceScript
{
    public class ParserFunction
    {
        public static Action<string, Variable, bool> OnVariableChange;

        /// <summary>
        /// オーバーライド可能かどうかを表す値
        /// </summary>
        public bool IsVirtual { get; set; }

        public List<string> Keywords
        {
            get => m_keywords;
            set => m_keywords = value;
        }

        private List<string> m_keywords = new List<string>();

        public ParserFunction()
        {
            m_impl = this;
        }

        // "仮想"コントラクスタ
        public ParserFunction(ParsingScript script, string item, char ch, ref string action, List<string> keywords = null)
        {
            if (keywords == null)
            {
                keywords = new List<string>();
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


            if (m_impl == s_strOrNumFunction && string.IsNullOrWhiteSpace(item))
            {
                string problem = (!string.IsNullOrWhiteSpace(action) ? action : ch.ToString());
                string restData = ch.ToString() + script.Rest;
                throw new ScriptException("`"+restData+"` 内の `"+problem+"` をパースできませんでした。",Exceptions.COULDNT_PARSE,script);
            }

            // Function not found, will try to parse this as a string in quotes or a number.
            s_strOrNumFunction.Item = item;
            m_impl = s_strOrNumFunction;
        }

        public static ParserFunction CheckString(ParsingScript script, string item, char ch)
        {
            s_strOrNumFunction.DetectionStringFormat = false;
            s_strOrNumFunction.DetectionUTF8_Literal = false;
            if (item.Length > 3 && item.StartsWith(Constants.UTF8_LITERAL))
            {
                item=item.Substring(Constants.UTF8_LITERAL.Length);
                s_strOrNumFunction.DetectionUTF8_Literal = true;
            }
            if(item.Length > 2 && item.StartsWith(Constants.DOLLER))
            {
                item = item.Substring(1);
                s_strOrNumFunction.DetectionStringFormat = true;
            }
            if (item.Length > 1 &&
              (((item[0] == Constants.QUOTE) && item[item.Length - 1] == Constants.QUOTE) ||
               (item[0] == Constants.QUOTE1 && item[item.Length - 1] == Constants.QUOTE1)))
            {
                // We are dealing with a string.
                s_strOrNumFunction.Item = item;
                return s_strOrNumFunction;
            }
            if (script.ProcessingList && ch == ':')
            {
                s_strOrNumFunction.Item = '"' + item + '"';
                return s_strOrNumFunction;
            }
            return null;
        }

        public static ParserFunction GetArrayFunction(string name, ParsingScript script, string action)
        {
            int arrayStart = name.IndexOf(Constants.START_ARRAY);
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

            string varName = arrayName.Substring(0,arrayStart);
            Variable ary = Utils.GetItem(script.GetTempScript(varName));
            int max = ary == null ? 0 : ary.Count;
            int delta = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, arrayName, delta, (string arr, int del) => { arrayName = arr; delta = del; },null,max);

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

        public static ParserFunction GetObjectFunction(string name, ParsingScript script, List<string> keywords)
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
            int ind = name.IndexOf('.');
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
                    pf = Utils.ExtractArrayElement(baseName,script);
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
            return !string.IsNullOrWhiteSpace(action) && action.EndsWith("=") && action.Length > 1;
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
                CustomFunction customFunc = new CustomFunction("", body, args, script, true);
                customFunc.ParentScript = script;
                customFunc.ParentOffset = parentOffset;
                action = null;
                return new GetVarFunction(new Variable(customFunc));
            }
            return null;
        }
        public static ParserFunction GetRegisteredAction(string name, ParsingScript script, ref string action)
        {
            if (Constants.CheckReserved(name))
            {
                return null;
            }

            if (false && ActionForUndefined(action) && script.Rest.StartsWith(Constants.UNDEFINED))
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
            StackLevel level;
            if (string.IsNullOrWhiteSpace(nameSpace) ||
               !s_namespaces.TryGetValue(nameSpace, out level))
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

            int ind = nameSpace.IndexOf('.');
            string prop = "";
            if (ind >= 0)
            {
                prop = name;
                name = nameSpace.Substring(ind + 1);
                nameSpace = nameSpace.Substring(0, ind);
            }

            StackLevel level;
            if (!s_namespaces.TryGetValue(nameSpace, out level))
            {
                return null;
            }

            if (!name.StartsWith(nameSpace, StringComparison.OrdinalIgnoreCase))
            {
                name = nameSpace + "." + name;
            }

            var vars = level.Variables;
            ParserFunction impl;
            if (!vars.TryGetValue(name, out impl) &&
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

        public static ParserFunction GetVariable(string name, ParsingScript script = null, bool force = false, List<string> keywords = null)
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

        public static Variable GetVariableValue(string name, ParsingScript script = null)
        {
            name = Constants.ConvertName(name);
            ParserFunction impl = null;
            StackLevel localStack = script != null && script.StackLevel != null ?
                 script.StackLevel : s_locals.Count > StackLevelDelta ? s_lastExecutionLevel : null;
            if (localStack != null && localStack.Variables.TryGetValue(name, out impl) &&
                impl is GetVarFunction)
            {
                return (impl as GetVarFunction).Value;
            }

            string scopeName = script == null || script.Filename == null ? "" : script.Filename;
            impl = GetLocalScopeVariable(name, scopeName);
            if (impl == null && script != null && script.TryGetVariable(name, out impl))
            {
                impl = impl.NewInstance();
            }
            if (impl == null && s_variables.TryGetValue(name, out impl))
            {
                impl = impl.NewInstance();
            }
            if (impl != null && impl is GetVarFunction)
            {
                return (impl as GetVarFunction).Value;
            }

            return null;
        }

        public static ParserFunction GetFunction(string name, ParsingScript script, bool toDelegate = false)
        {
            //TODO:関数の取得部分
            name = Constants.ConvertName(name);
            ParserFunction impl;
            if (script.TryGetFunction(name, out impl))
            {
                //ローカル関数として登録されている
                if (toDelegate && impl is CustomFunction cf)
                {
                    return new GetVarFunction(new Variable(cf));
                }
                return impl.NewInstance();
            }
            if (s_functions.TryGetValue(name, out impl))
            {
                //グローバル関数として登録されている
                if (toDelegate && impl is CustomFunction cf)
                {
                    return new GetVarFunction(new Variable(cf));
                }
                return impl.NewInstance();
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

                foreach (string nsn in NameSpaceManerger.NameSpaces.Keys)
                {
                    //より長い名前（AliceとAlice.IOならAlice.IO）を採用
                    if (name.StartsWith(nsn.ToLower() + ".") && nsn.Length > namespacename.Length)
                    {
                        namespacename = nsn.ToLower();
                    }
                }

                //完全修飾名で関数を検索
                if (namespacename != string.Empty)
                {
                    fc = NameSpaceManerger.NameSpaces.Where(x => x.Key.ToLower() == namespacename).FirstOrDefault().Value.Functions.Where((x) => name.StartsWith(namespacename + "." + x.Name.ToLower())).FirstOrDefault();
                    if (fc != null)
                    {
                        return fc;
                    }
                    var cc = NameSpaceManerger.NameSpaces.Where(x => x.Key.ToLower() == namespacename).FirstOrDefault().Value.Classes.Where((x) => name.StartsWith(namespacename + "." + x.Name.ToLower())).FirstOrDefault();
                    if (cc != null)
                    {
                        return new GetVarFunction(new Variable(new TypeObject(cc)));
                    }
                }
            }
            string className = Constants.ConvertName(name);

            var csClass = AliceScriptClass.GetClass(className, script);
            if (csClass != null)
            {
                return new GetVarFunction(new Variable(new TypeObject(csClass)));
            }

            return GetFromNamespace(name, script);
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
            if (script.ParentScript != null)
            {
                return GetFromNS(name, script.ParentScript);
            }
            return null;
        }
        public static void UpdateFunction(string name, ParserFunction function)
        {
            name = Constants.ConvertName(name);
            Utils.CheckLegalName(name);
            lock (s_variables)
            {
                // First search among local variables.
                if (s_lastExecutionLevel != null && s_locals.Count > StackLevelDelta)
                {
                    Dictionary<string, ParserFunction> local = s_lastExecutionLevel.Variables;

                    if (local.ContainsKey(name))
                    {
                        // Local function exists (a local variable)
                        local[name] = function;
                        return;
                    }
                }
            }
            // If it's not a local variable, update global.
            s_variables[name] = function;
        }
        public static ActionFunction GetAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return null;
            }

            ActionFunction impl;
            if (s_actions.TryGetValue(action, out impl))
            {
                // Action exists and is registered (e.g. =, +=, --, etc.)
                return impl;
            }

            return null;
        }

        public static bool FunctionExists(string item, ParsingScript script)
        {
            // If it is not defined locally, then check globally:
            return LocalNameExists(item, script) || GlobalNameExists(item);
        }

        public static void AddGlobalOrLocalVariable(string name, GetVarFunction function,
            ParsingScript script, bool localIfPossible = false, bool registVar = false, bool globalOnly = false)
        {
            name = Constants.ConvertName(name);
            Utils.CheckLegalName(name, script);

            Dictionary<string, ParserFunction> lastLevel = GetLastLevel();
            if (!globalOnly && lastLevel != null && s_lastExecutionLevel.IsNamespace && !string.IsNullOrWhiteSpace(s_namespace))
            {
                name = s_namespacePrefix + name;
            }

            function.Name = Constants.GetRealName(name);
            function.Value.ParamName = function.Name;

            if (!globalOnly && !localIfPossible && script != null && script.StackLevel != null && !GlobalNameExists(name))
            {
                script.StackLevel.Variables[name] = function;
            }


            if (globalOnly)
            {
                AddLocalVariable(function, ParsingScript.GetTopLevelScript(script), "", true, registVar);
            }
            else
            {
                AddLocalVariable(function, script, "", true, registVar);
            }
        }

        private static string CreateVariableEntry(Variable var, string name, bool isLocal = false)
        {
            try
            {
                string value = var.AsString(true, true, 16);
                string localGlobal = isLocal ? "0" : "1";
                string varData = name + ":" + localGlobal + ":" +
                                 Constants.TypeToString(var.Type).ToLower() + ":" + value;
                return varData.Trim();
            }
            catch (Exception exc)
            {
                // TODO: Clean up not used objects.
                bool removed = isLocal ? PopLocalVariable(name) : RemoveGlobal(name);
                Console.WriteLine("Object {0} is probably dead ({1}): {2}. Removing it.", name, removed, exc);
                return null;
            }
        }

        private static void GetVariables(Dictionary<string, ParserFunction> variablesScope,
                                 StringBuilder sb, bool isLocal = false)
        {
            var all = variablesScope.Values.ToList();
            for (int i = 0; i < all.Count; i++)
            {
                var variable = all[i];
                GetVarFunction gvf = variable as GetVarFunction;
                if (gvf == null || string.IsNullOrWhiteSpace(variable.Name))
                {
                    continue;
                }

                string varData = CreateVariableEntry(gvf.Value, variable.Name, isLocal);
                if (!string.IsNullOrWhiteSpace(varData))
                {
                    sb.AppendLine(varData);
                    if (gvf.Value.Type == Variable.VarType.OBJECT)
                    {
                        var props = gvf.Value.GetProperties();
                        foreach (Variable var in props)
                        {
                            var val = gvf.Value.GetProperty(var.AsString());
                            varData = CreateVariableEntry(val, variable.Name + "." + var.AsString(), isLocal);
                            if (!string.IsNullOrWhiteSpace(varData))
                            {
                                sb.AppendLine(varData);
                            }
                        }
                    }
                }
            }
        }

        public static string GetVariables(ParsingScript script)
        {
            StringBuilder sb = new StringBuilder();
            // Locals, if any:
            if (s_lastExecutionLevel != null)
            {
                Dictionary<string, ParserFunction> locals = s_lastExecutionLevel.Variables;
                GetVariables(locals, sb, true);
            }

            // Variables in the local file scope:
            if (script != null && script.Filename != null)
            {
                Dictionary<string, ParserFunction> localScope;
                string scopeName = Path.GetFileName(script.Filename);
                if (s_localScope.TryGetValue(scopeName, out localScope))
                {
                    GetVariables(localScope, sb, true);
                }
            }

            // Globals:
            GetVariables(s_variables, sb, false);

            return sb.ToString().Trim();
        }

        private static Dictionary<string, ParserFunction> GetLastLevel()
        {
            lock (s_variables)
            {
                if (s_lastExecutionLevel == null || s_locals.Count <= StackLevelDelta)
                {
                    return null;
                }
                var result = s_lastExecutionLevel.Variables;
                return result;
            }
        }

        public static bool LocalNameExists(string name, ParsingScript script)
        {
            if (script != null && (script.ContainsVariable(name, out _) || script.ContainsFunction(name) || script.ContainsConst(name)))
            {
                return true;
            }
            Dictionary<string, ParserFunction> lastLevel = GetLastLevel();
            if (lastLevel == null)
            {
                return false;
            }
            name = Constants.ConvertName(name);
            return lastLevel.ContainsKey(name);
        }

        public static bool GlobalNameExists(string name)
        {
            name = Constants.ConvertName(name);
            return s_variables.ContainsKey(name) || s_functions.ContainsKey(name) || Constants.CONSTS.ContainsKey(name);
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
                StackLevel level;
                if (s_namespaces.TryGetValue(s_namespace, out level) &&
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
                if ((s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                throw new ScriptException("指定された関数はすでに登録されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static void RegisterScriptFunction(string name, ParserFunction function, ParsingScript script, bool isNative = true, bool isLocal = true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

            if (!string.IsNullOrWhiteSpace(s_namespace))
            {
                StackLevel level;
                if (s_namespaces.TryGetValue(s_namespace, out level) &&
                   function is CustomFunction)
                {
                    ((CustomFunction)function).NamespaceData = level;
                    name = s_namespacePrefix + name;
                }
            }
            ParserFunction impl = null;
            if (isLocal && (!script.ContainsFunction(name) || (script.TryGetFunction(name, out impl) && impl.IsVirtual)))
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
                if ((s_functions.ContainsKey(name) && s_functions[name].IsVirtual))
                {
                    //オーバーライドした関数にもVirtual属性を引き継ぐ
                    function.IsVirtual = true;
                }
            }
            else
            {
                throw new ScriptException("指定された関数はすでに登録されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
            }
        }
        public static bool UnregisterScriptFunction(string name, ParsingScript script)
        {
            name = Constants.ConvertName(name);
            if (script != null && script.Functions.Remove(name))
            {
                return true;
            }
            return s_functions.Remove(name);
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

        private static void AddVariables(List<Variable> vars, Dictionary<string, ParserFunction> dict)
        {
            foreach (var val in dict.Values)
            {
                if (val.isNative || !(val is GetVarFunction))
                {
                    continue;
                }
                Variable var = ((GetVarFunction)val).Value.DeepClone();
                var.ParamName = ((GetVarFunction)val).Name;
                vars.Add(var);
            }
        }

        public static List<Variable> VariablesSnaphot(ParsingScript script = null, bool includeGlobals = false)
        {
            List<Variable> vars = new List<Variable>();
            if (includeGlobals)
            {
                AddVariables(vars, s_variables);
            }
            Dictionary<string, ParserFunction> lastLevel = GetLastLevel();
            if (lastLevel != null)
            {
                AddVariables(vars, lastLevel);
            }
            if (script != null && script.StackLevel != null)
            {
                AddVariables(vars, script.StackLevel.Variables);
            }
            return vars;
        }


        public static void AddLocalScopeVariable(string name, string scopeName, ParserFunction variable)
        {
            name = Constants.ConvertName(name);
            variable.isNative = false;
            variable.Name = Constants.GetRealName(name);
            if (variable is GetVarFunction)
            {
                ((GetVarFunction)variable).Value.ParamName = variable.Name;
            }

            if (scopeName == null)
            {
                scopeName = "";
            }

            Dictionary<string, ParserFunction> localScope;
            if (!s_localScope.TryGetValue(scopeName, out localScope))
            {
                localScope = new Dictionary<string, ParserFunction>();
            }
            localScope[name] = variable;
            s_localScope[scopeName] = localScope;
        }

        private static ParserFunction GetLocalScopeVariable(string name, string scopeName)
        {
            scopeName = Path.GetFileName(scopeName);
            Dictionary<string, ParserFunction> localScope;
            if (!s_localScope.TryGetValue(scopeName, out localScope))
            {
                return null;
            }

            name = Constants.ConvertName(name);
            ParserFunction function = null;
            localScope.TryGetValue(name, out function);
            return function;
        }

        public static void AddAction(string name, ActionFunction action)
        {
            s_actions[name] = action;
        }

        public static void AddLocalVariables(StackLevel locals)
        {
            lock (s_variables)
            {
                s_locals.Push(locals);
                s_lastExecutionLevel = locals;
            }
        }

        public static void AddNamespace(string namespaceName)
        {
            namespaceName = Constants.ConvertName(namespaceName);
            if (!string.IsNullOrWhiteSpace(s_namespace))
            {
                throw new ScriptException("名前空間 `"+s_namespace+"` をネストすることはできません。",Exceptions.NAMESPACE_CANT_BE_NESTED);
            }

            StackLevel level;
            if (!s_namespaces.TryGetValue(namespaceName, out level))
            {
                level = new StackLevel(namespaceName, true); ;
            }

            lock (s_variables)
            {
                s_locals.Push(level);
                s_lastExecutionLevel = level;
            }

            s_namespaces[namespaceName] = level;

            s_namespace = namespaceName;
            s_namespacePrefix = namespaceName + ".";
        }

        public static void PopNamespace()
        {
            s_namespace = s_namespacePrefix = "";
            lock (s_variables)
            {
                while (s_locals.Count > 0)
                {
                    var level = s_locals.Pop();
                    s_lastExecutionLevel = s_locals.Count == 0 ? null : s_locals.Peek();
                    if (level.IsNamespace)
                    {
                        return;
                    }
                }
            }
        }

        public static void AddLocalVariable(ParserFunction local, ParsingScript script, string varName = "", bool setScript = true, bool registVar = false)
        {
            NormalizeValue(local);
            local.m_isGlobal = false;
            if (setScript)
            {
                var name = Constants.ConvertName(string.IsNullOrWhiteSpace(varName) ? local.Name : varName);

                local.Name = Constants.GetRealName(name);
                if (local is GetVarFunction)
                {
                    ((GetVarFunction)local).Value.ParamName = local.Name;
                }
                bool exists = script.ContainsVariable(name, out var func);
                bool unneed = script.ContainsSymbol(Constants.UNNEED_VAR);

                if (exists && registVar)
                {
                    throw new ScriptException("変数[" + name + "]はすでに定義されています", Exceptions.VARIABLE_ALREADY_DEFINED, script);
                }
                else if (!exists && !registVar && !unneed && !string.IsNullOrEmpty(name))
                {
                    throw new ScriptException("変数[" + name + "]は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
                }
                if (func is GetVarFunction v)
                {
                    //代入の場合
                    if (v.Value.Parent == null)
                    {
                        v.Value.Parent = script;
                    }
                    if (local is GetVarFunction g2)
                    {
                        v.Value.Assign(g2.Value);
                    }
                    /*
                    if (v.Value.Parent.Variables[name] is GetVarFunction g && local is GetVarFunction g2)
                    {
                        g.Value.Assign(g2.Value);
                    }*/
                    //v.Value.Parent.Variables[name] = local;
                }
                else
                {
                    //変数定義の場合
                    if (local is GetVarFunction v2)
                    {
                        v2.Value.Parent = script;
                    }
                    script.Variables[name] = local;
                }
            }
            else
            {
                lock (s_variables)
                {

                    if (s_lastExecutionLevel == null)
                    {
                        s_lastExecutionLevel = new StackLevel();
                        s_locals.Push(s_lastExecutionLevel);
                    }
                }

                var name = Constants.ConvertName(string.IsNullOrWhiteSpace(varName) ? local.Name : varName);
                local.Name = Constants.GetRealName(name);
                if (local is GetVarFunction)
                {
                    ((GetVarFunction)local).Value.ParamName = local.Name;
                }

                var handle = OnVariableChange;
                bool exists = handle != null && s_lastExecutionLevel.Variables.ContainsKey(name);

                s_lastExecutionLevel.Variables[name] = local;

                if (handle != null && local is GetVarFunction)
                {
                    handle.Invoke(local.Name, ((GetVarFunction)local).Value, exists);
                }
            }
        }

        public static void PopLocalVariables(int id)
        {
            lock (s_variables)
            {
                if (s_lastExecutionLevel == null)
                {
                    return;
                }
                if (id < 0 || s_lastExecutionLevel.Id == id)
                {
                    s_locals.Pop();
                    s_lastExecutionLevel = s_locals.Count == 0 ? null : s_locals.Peek();
                    return;
                }

                var array = s_locals.ToArray();
                for (int i = 1; i < array.Length; i++)
                {
                    var stack = array[i];
                    if (stack.Id == id)
                    {
                        for (int j = 0; j < i + 1 && s_locals.Count > 0; j++)
                        {
                            s_locals.Pop();
                        }
                        for (int j = 0; j < i; j++)
                        {
                            s_locals.Push(array[j]);
                        }
                        s_lastExecutionLevel = s_locals.Peek();
                        return;
                    }
                }
            }
        }

        public static int GetCurrentStackLevel()
        {
            lock (s_variables)
            {
                return s_locals.Count;
            }
        }

        public static void InvalidateStacksAfterLevel(int level)
        {
            lock (s_variables)
            {
                while (level >= 0 && s_locals.Count > level)
                {
                    s_locals.Pop();
                }
                s_lastExecutionLevel = s_locals.Count == 0 ? null : s_locals.Peek();
            }
        }

        public static bool PopLocalVariable(string name)
        {
            if (s_lastExecutionLevel == null)
            {
                return false;
            }
            Dictionary<string, ParserFunction> locals = s_lastExecutionLevel.Variables;
            name = Constants.ConvertName(name);
            return locals.Remove(name);
        }

        public Variable GetValue(ParsingScript script)
        {
            return m_impl.Evaluate(script);
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

        private static StringOrNumberFunction s_strOrNumFunction =
          new StringOrNumberFunction();
        private static IdentityFunction s_idFunction =
          new IdentityFunction();

        public static int StackLevelDelta { get; set; }
    }

    public abstract class ActionFunction : FunctionBase
    {
        protected string m_action;
        public string Action { set => m_action = value; }
    }
}