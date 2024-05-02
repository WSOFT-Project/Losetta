using AliceScript.NameSpaces;
using AliceScript.Objects;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AliceScript.Functions
{
    /// <summary>
    /// AliceScriptの関数
    /// </summary>
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
            if (keywords is null)
            {
                keywords = new HashSet<string>();
            }
            else if (keywords.Contains(Constants.FUNCTION) || keywords.Contains(Constants.OVERRIDE) || keywords.Contains(Constants.VIRTUAL))
            {
                // overrideやvirtualなど、関数定義時にしか使わないキーワードがあれば先に関数モードにする
                m_impl = TryCustomFunction(item, script, keywords);
                if (m_impl is not null)
                {
                    m_impl.Keywords = keywords;
                    return;
                }
            }

            m_impl = CheckGroup(script, ref item, ch, ref action);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = CheckDefineFunction(script, item, keywords);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = CheckString(script, item, ch);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetLambdaFunction(script, item, ch, ref action);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            item = Constants.ConvertName(item);

            m_impl = GetRegisteredAction(item, script, ref action);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetArrayFunction(item, script, action);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetObjectFunction(item, script, keywords);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = GetVariable(item, script, false, keywords);
            if (keywords.Contains(Constants.NEW) && m_impl is ValueFunction vf && vf.Value.Object is TypeObject t)
            {
                m_impl = new ConstructorFunction(t);
            }
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            m_impl = TryCustomFunction(item, script, keywords);
            if (m_impl is not null)
            {
                m_impl.Keywords = keywords;
                return;
            }

            if (m_impl is null)
            {
                Utils.ProcessErrorMsg(item, script);
            }
        }
        public static ParserFunction CheckGroup(ParsingScript script, ref string item, char ch, ref string action)
        {
            if (string.IsNullOrEmpty(item))
            {
                string body = script.Prev == Constants.START_GROUP
                    ? Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP)
                    : Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG, ";\0");

                if (script.TryNext() == Constants.ARROW[0] && script.TryNext(2) == Constants.ARROW[1])
                {
                    // このかっこはラムダ式のものだった
                    action = Constants.ARROW;
                    item = body;
                    return null;
                }
                else
                {
                    action = null;
                    return new StatementFunction(body, script);
                }
            }
            return null;
        }
        public static ParserFunction CheckDefineFunction(ParsingScript script, string item, HashSet<string> keywords)
        {
            return keywords.Count > 0 && (keywords.Contains(Constants.OVERRIDE) || keywords.Contains(Constants.VIRTUAL))
                ? TryCustomFunction(item, script, keywords)
                : null;
        }
        public static ParserFunction CheckString(ParsingScript script, string item, char ch)
        {
            LiteralFunction literalFunction = new LiteralFunction();

            if (item.Length > 0 && char.IsDigit(item[0]))
            {
                literalFunction.Item = item;
                literalFunction.StringMode = false;
                return literalFunction;
            }

            if (item.Length > 3 && item.StartsWith(Constants.UTF8_LITERAL_PREFIX, StringComparison.Ordinal))
            {
                item = item.Substring(Constants.UTF8_LITERAL_PREFIX.Length);
                literalFunction.DetectionUTF8_Literal = true;
            }

            if (item.Length > 2 && item.StartsWith(Constants.DOLLER))
            {
                item = item.Substring(1);
                literalFunction.DetectionStringFormat = true;
            }

            if (IsQuotedString(item))
            {
                literalFunction.Item = item;
                literalFunction.StringMode = true;
                return literalFunction;
            }

            if (script.ProcessingList && ch == ':')
            {
                literalFunction.Item = '"' + item + '"';
                literalFunction.StringMode = true;
                return literalFunction;
            }

            return null;
        }
        public static ParserFunction TryCustomFunction(string name, ParsingScript script = null, HashSet<string> keywords = null)
        {
            if (script is not null && !string.IsNullOrEmpty(name) && script.TryPrev() == Constants.START_ARG)
            {
                //ここまでくる=その関数は存在しない=存在チェックは不要
                return FunctionCreator.DefineFunction(name, script, keywords, Parser.m_attributeFuncs) ? new ValueFunction(Variable.EmptyInstance) : null;
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
                return new ValueFunction(arr);
            }

            string arrayName = name;

            string varName = arrayName.Substring(0, arrayStart);
            Variable ary = Utils.GetItem(script.GetTempScript(varName));
            int max = ary is null ? 0 : ary.Count;
            int delta = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, arrayName, delta, (arr, del) => { arrayName = arr; delta = del; }, null, max);

            if (arrayIndices.Count == 0)
            {
                return null;
            }

            ParserFunction pf = GetVariable(arrayName, script);
            ValueFunction varFunc = pf as ValueFunction;
            if (varFunc is null)
            {
                return null;
            }

            // we temporarily backtrack for the processing
            script.Backward(name.Length - arrayStart - 1);
            script.Backward(action is not null ? action.Length : 0);
            // delta shows us how manxy chars we need to advance forward in ValueFunction()
            delta -= arrayName.Length;
            delta += action is not null ? action.Length : 0;

            varFunc.Indices = arrayIndices;
            varFunc.Delta = delta;
            return varFunc;
        }

        public static ParserFunction GetObjectFunction(string name, ParsingScript script, HashSet<string> keywords)
        {
            if (script.CurrentClass is not null && script.CurrentClass.Name == name)
            {
                script.Backward(name.Length + 1);
                return new FunctionCreator();
            }
            if (script.ClassInstance is not null &&
               (script.ClassInstance.PropertyExists(name) || script.ClassInstance.FunctionExists(name)))
            {
                name = script.ClassInstance.InstanceName + "." + name;
            }
            int ind = name.LastIndexOf('.');
            if (ind <= 0)
            {
                return null;
            }
            string baseName = name.Substring(0, ind);

            string prop = name.Substring(ind + 1);

            /*
            ParserFunction pf = GetFromNamespace(prop, baseName, script);
            if (pf is not null)
            {
                pf.Keywords = keywords;
                return pf;
            }*/

            ParserFunction pf = GetVariable(baseName, script, true);
            if (pf is null || !(pf is ValueFunction))
            {
                pf = GetFunction(baseName, script);
                if (pf is null)
                {
                    pf = Utils.ExtractArrayElement(baseName, script);
                }
            }

            ValueFunction varFunc = pf as ValueFunction;
            if (varFunc is null)
            {
                return null;
            }

            varFunc.PropertyName = prop;
            varFunc.Keywords = keywords;
            return varFunc;
        }

        private static bool ActionForUndefined(string action)
        {
            return !string.IsNullOrWhiteSpace(action) && action.EndsWith('=') && action.Length > 1;
        }
        public static ParserFunction GetLambdaFunction(ParsingScript script, string item, char ch, ref string action)
        {
            if (action == Constants.ARROW)
            {
                string[] args = Utils.GetFunctionSignature(script.GetTempScript(item), true);
                if (args.Length > 0 && args[0].Trim() == Constants.DESTRUCTION.ToString())
                {
                    args = Array.Empty<string>();
                }

                script.MoveForwardIf(new char[] { Constants.END_ARG });

                // アロー演算子が残ってる場合はスキップ
                if (script.Current == Constants.ARROW[0] && script.Next == Constants.ARROW[1])
                {
                    script.Forward(2);
                }

                // ステートメント形式のラムダなら波かっこ内を取得し、それ以外なら丸かっこ内を取得
                string body = script.Current == Constants.START_GROUP
                    ? Utils.GetBodyBetween(script, Constants.START_GROUP, Constants.END_GROUP)
                    : Utils.GetBodyBetween(script, Constants.START_ARG, Constants.END_ARG);


                int parentOffset = script.Pointer;
                if (script.CurrentClass is not null)
                {
                    parentOffset += script.CurrentClass.ParentOffset;
                }

                CustomFunction customFunc = CreateCustomFunction(body, args, script, parentOffset);

                action = null;
                return new ValueFunction(new Variable(customFunc));
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
            if (false && ActionForUndefined(action) && script.Rest.StartsWith(Constants.UNDEFINED, StringComparison.Ordinal))
            {
                IsUndefinedFunction undef = new IsUndefinedFunction(name, action);
                return undef;
            }

            ActionFunction actionFunction = GetAction(action);

            // If passed action exists and is registered we are done.
            if (actionFunction is null)
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
            vars[name] = new ValueFunction(varValue);

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
                !s_functions.TryGetValue(name, out impl)
                )
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(prop) && impl is ValueFunction)
            {
                ((ValueFunction)impl).PropertyName = prop;
            }
            return impl;
        }

        public static ParserFunction GetVariable(string name, ParsingScript script = null, bool force = false, HashSet<string> keywords = null)
        {
            if (!force && script is not null && script.TryPrev() == Constants.START_ARG && !keywords.Contains(Constants.NEW))
            {
                return GetFunction(name, script);
            }

            name = Constants.ConvertName(name);

            ParserFunction impl;
            StackLevel localStack = script is not null && script.StackLevel is not null ?
                 script.StackLevel : s_locals.Count > StackLevelDelta ? s_lastExecutionLevel : null;
            if (localStack is not null)
            {
                Dictionary<string, ParserFunction> local = localStack.Variables;
                if (local.TryGetValue(name, out impl))
                {
                    return impl;
                }
            }
            //ローカルスコープに存在するか確認
            if (script is not null && script.TryGetVariable(name, out impl))
            {
                return impl.NewInstance();
            }

            //定数に存在するか確認
            if (Constants.CONSTS.TryGetValue(name, out Variable value))
            {
                return new ValueFunction(value);
            }

            //関数として取得を続行
            var pfx = GetFunction(name, script);
            if (pfx is not null)
            {
                if (pfx is FunctionBase cf && !(cf.Attribute.HasFlag(FunctionAttribute.LANGUAGE_STRUCTURE) || cf.Attribute.HasFlag(FunctionAttribute.FUNCT_WITH_SPACE)))
                {
                    //デリゲートとして返したい場合
                    var f = new Variable(new DelegateObject(cf));
                    f.Readonly = f.TypeChecked = true;
                    return new ValueFunction(f);
                }
            }
            return pfx;
        }

        public static ParserFunction GetFunction(string name, ParsingScript script, bool wantMethod = false)
        {
            //TODO:関数の取得部分
            name = Constants.ConvertName(name);
            if (script.TryGetFunction(name, out ParserFunction impl) || s_functions.TryGetValue(name, out impl))
            {
                if (impl is FunctionBase fb)
                {
                    if (wantMethod && fb.IsMethod)
                    {
                        return impl.NewInstance();
                    }
                    else if (!fb.IsMethod || !fb.MethodOnly)
                    {
                        return fb.NewInstance();
                    }
                }
                else
                {
                    return impl.NewInstance();
                }
            }
            if (script.TryGetVariable(name, out impl))
            {
                //それがデリゲート型の変数である場合
                if (!wantMethod && impl is ValueFunction gv && gv.Value.Type == Variable.VarType.DELEGATE && !gv.Value.IsNull())
                {
                    return gv.Value.Delegate.Function;
                }
            }

            // 現在の名前空間を検索
            if (script.NameSpace is not null && (script.NameSpace.InternalFunctions.TryGetValue(name, out FunctionBase func) || script.NameSpace.Functions.TryGetValue(name, out func)))
            {
                if (wantMethod && func.IsMethod)
                {
                    return func.NewInstance();
                }
                else if (!func.IsMethod || !func.MethodOnly)
                {
                    return func.NewInstance();
                }
            }

            // 完全修飾名を検索
            func = GetFromNamespcaeName(name);
            if (func is not null)
            {
                if (wantMethod && func.IsMethod)
                {
                    return func.NewInstance();
                }
                else if (!func.IsMethod || !func.MethodOnly)
                {
                    return func.NewInstance();
                }
            }

            // usingした名前空間を検索
            func = GetFromUsingNamespace(name, script);
            if (func is not null)
            {
                if (wantMethod && func.IsMethod)
                {
                    return func.NewInstance();
                }
                else if (!func.IsMethod || !func.MethodOnly)
                {
                    return func.NewInstance();
                }
            }

            string className = Constants.ConvertName(name);

            var csClass = AliceScriptClass.GetClass(className, script);
            return csClass is not null ? new ValueFunction(new Variable(new TypeObject(csClass))) : GetFromNamespace(name, script);
        }
        internal static FunctionBase GetFromNamespcaeName(string name)
        {
            int ind = name.LastIndexOf('.');
            string spaceName = string.Empty;
            if (ind > 0)
            {
                spaceName = name.Substring(0, ind);
            }
            string funcName = name.Substring(ind + 1);
            return NameSpaceManager.NameSpaces.TryGetValue(spaceName, out var space) && space.Functions.TryGetValue(funcName, out var func)
                ? func
                : null;
        }
        internal static FunctionBase GetFromUsingNamespace(string name, ParsingScript script)
        {
            FunctionBase func = null;
            string lastSpaceName = null;
            foreach (string spaceName in script.UsingNamespaces)
            {
                var temp = GetFromNamespcaeName(spaceName + "." + name);
                if (lastSpaceName is not null && temp is not null)
                {
                    throw new ScriptException($"`{name}`は`{lastSpaceName}`と`{spaceName}`間があいまいです", Exceptions.AMBIGUOUS_IDENTIFIER);
                }
                if (temp is not null)
                {
                    func = temp;
                    lastSpaceName = spaceName;
                }
            }
            return func is not null
                ? func
                : !script.TopInFile && script.ParentScript is not null ? GetFromUsingNamespace(name, script.ParentScript) : null;
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
            return (script is not null && script.TryGetLocal(item, out func)) || TryGetGlobal(item, out func, continueConst);
        }
        public static bool FunctionIsVirtual(string name, ParsingScript script)
        {
            if (script is not null && script.TryGetFunction(name, out ParserFunction impl))
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

        public static void AddGlobalOrLocalVariable(string name, ValueFunction function,
            ParsingScript script, bool localIfPossible = false, bool registVar = false, AccessModifier accessModifier = AccessModifier.PRIVATE, string type_modifer = null, bool isReadOnly = false, bool fromAssign = false)
        {
            name = Constants.ConvertName(name);
            Utils.CheckLegalName(name, fromAssign);

            function.Name = Constants.GetRealName(name);
            function.Value.ParamName = function.Name;

            bool type_inference = script.TypeInference;
            NormalizeValue(function);
            function.m_isGlobal = false;
            name = Constants.ConvertName(function.Name);

            function.Name = Constants.GetRealName(name);
            if (function is ValueFunction)
            {
                function.Value.ParamName = function.Name;
            }
            var func = GetVariable(name, script, true);
            bool exists = func is not null;
            bool unneed = script.UnneedVarKeyword;

            if (exists && registVar)
            {
                throw new ScriptException("変数[" + name + "]はすでに定義されています", Exceptions.VARIABLE_ALREADY_DEFINED, script);
            }
            else if (!exists && !registVar && !unneed && !string.IsNullOrEmpty(name))
            {
                throw new ScriptException("変数[" + name + "]は定義されていません", Exceptions.COULDNT_FIND_VARIABLE, script);
            }
            if (func is not null && func is ValueFunction v)
            {
                //代入の場合
                if (v.Value.Parent is null)
                {
                    v.Value.Parent = script;
                }
                v.Value = function.Value;
            }
            else if (func is null)
            {
                //変数定義の場合
                ValueFunction value = new ValueFunction();
                Variable newVar = value.Value;
                newVar.Parent = script;
                if (type_modifer != Constants.VAR)
                {
                    newVar.TypeChecked = true;
                    if (type_modifer is not null)
                    {
                        if (type_modifer.EndsWith('?'))
                        {
                            newVar.Nullable = true;
                            type_modifer = type_modifer.Substring(0, type_modifer.Length - 1);
                        }
                        newVar.Type = Constants.StringToType(type_modifer);
                    }
                }
                else
                {
                    //型指定がない場合は一時的にnullを許容する
                    newVar.Nullable = true;
                }
                newVar.Assign(function.Value);
                if (type_inference && type_modifer == Constants.VAR)
                {
                    newVar.TypeChecked = true;
                    if (!newVar.IsNull())
                    {
                        newVar.Nullable = false;
                    }
                }
                newVar.Readonly = isReadOnly;
                function = value;
                function.AccessModifier = accessModifier;
                if (accessModifier == AccessModifier.PRIVATE)
                {
                    script.Variables[name] = function;
                }
                else if (accessModifier == AccessModifier.PUBLIC)
                {
                    script.NameSpace.Functions[name] = function;
                }
                else if (accessModifier == AccessModifier.PROTECTED)
                {
                    script.NameSpace.InternalFunctions[name] = function;
                }
            }
        }

        public static bool TryGetGlobal(string name, out ParserFunction function, bool continueConst = false)
        {
            name = Constants.ConvertName(name);
            if (s_functions.TryGetValue(name, out function))
            {
                return true;
            }
            if (Constants.CONSTS.TryGetValue(name, out var v) && !continueConst)
            {
                function = new ValueFunction(v);
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
            if (script is null)
            {
                RegisterFunction(varName, new ValueFunction(enumVar));
            }
            else
            {
                RegisterScriptFunction(varName, new ValueFunction(enumVar), script);
            }
            return enumVar;
        }

        public static void RegisterFunction(string name, ParserFunction function,
                                            bool isNative = true)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

            if (!s_functions.TryGetValue(name, out ParserFunction value) || (s_functions.ContainsKey(name) && value.IsVirtual))
            {
                //まだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                s_functions[name] = function;
                function.isNative = isNative;
                if (s_functions.TryGetValue(name, out ParserFunction value2) && value2.IsVirtual)
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
        public static void RegisterScriptFunction(string name, ParserFunction function, ParsingScript script)
        {
            name = Constants.ConvertName(name);
            function.Name = Constants.GetRealName(name);

            if (!FunctionExists(name, script, out ParserFunction impl, true) || impl.IsVirtual)
            {
                //ローカル関数でまだ登録されていないか、すでに登録されていて、オーバーライド可能な場合
                script.Functions[name] = function;
                if (impl is not null)
                {
                    impl.IsVirtual = true;
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
            return (script is not null && script.Functions.Remove(name)) || s_functions.Remove(name);
        }
        public static bool UnregisterFunction(string name)
        {
            name = Constants.ConvertName(name);

            bool removed = s_functions.Remove(name);
            return removed;
        }


        private static void NormalizeValue(ParserFunction function)
        {
            ValueFunction gvf = function as ValueFunction;
            if (gvf is not null)
            {
                gvf.Value.CurrentAssign = "";
            }
        }

        public static void AddAction(string name, ActionFunction action)
        {
            s_actions[name] = action;
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
            s_locals.Clear();
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

        // Global actions to functions map:
        private static Dictionary<string, ActionFunction> s_actions = new Dictionary<string, ActionFunction>();


        public static bool IsNumericFunction(string paramName, ParsingScript script = null)
        {
            ParserFunction function = GetFunction(paramName, script);
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