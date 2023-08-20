using System.Text;

namespace AliceScript
{
    internal class IsUndefinedFunction : ParserFunction
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

    internal class CustomMethodFunction : FunctionBase
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
                this.Run += CustomMethodFunction_Run;
            }
        }

        private void CustomMethodFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Function.GetVariable(e.Script, e.CurentVariable);
        }

        public CustomFunction Function { get; set; }
    }
    internal class ArrayTypeFunction : FunctionBase
    {
        public ArrayTypeFunction()
        {
            this.Name = "array";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += ArrayTypeFunction_Run;
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
    internal class FunctionCreator : FunctionBase
    {
        public FunctionCreator()
        {
            this.Name = Constants.FUNCTION;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += FunctionCreator_Run;
        }

        private void FunctionCreator_Run(object sender, FunctionBaseEventArgs e)
        {
            string funcName = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            bool? mode = null;
            bool isGlobal = this.Keywords.Contains(Constants.PUBLIC);
            bool isCommand = this.Keywords.Contains(Constants.COMMAND);
            if (this.Keywords.Contains(Constants.OVERRIDE))
            {
                mode = true;
            }
            else if (this.Keywords.Contains(Constants.VIRTUAL))
            {
                mode = false;
            }

            funcName = Constants.ConvertName(funcName);

            string[] args = Utils.GetFunctionSignature(e.Script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = new string[0];
            }

            e.Script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);
            /*string line = */
            e.Script.GetOriginalLine(out _);

            int parentOffset = e.Script.Pointer;

            if (e.Script.CurrentClass != null)
            {
                parentOffset += e.Script.CurrentClass.ParentOffset;
            }

            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP, Constants.END_GROUP);
            e.Script.MoveForwardIf(Constants.END_GROUP);

            CustomFunction customFunc = new CustomFunction(funcName, body, args, e.Script);
            customFunc.ParentScript = e.Script;
            customFunc.ParentOffset = parentOffset;
            if (isCommand)
            {
                customFunc.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            }
            if (mode != null)
            {
                customFunc.IsVirtual = true;
            }
            if (customFunc.IsMethod)
            {
                if (isGlobal)
                {
                    FunctionBase fb;
                    if (!Variable.Functions.TryGetValue(funcName, out fb) || !fb.IsVirtual)
                    {
                        Variable.AddFunc(new CustomMethodFunction(customFunc, funcName));
                    }
                    else
                    {
                        throw new ScriptException("指定されたメソッドはすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                    }
                }
                else
                {
                    throw new ScriptException("メソッドはグローバル関数である必要があります", Exceptions.FUNCTION_NOT_GLOBAL, e.Script);
                }
            }
            else
            if (e.Script.CurrentClass != null)
            {
                e.Script.CurrentClass.AddMethod(funcName, args, customFunc);
            }
            else
            {
                if (!FunctionExists(funcName, e.Script, out _) || (mode == true && FunctionIsVirtual(funcName, e.Script)))
                {
                    FunctionBaseManerger.Add(customFunc, funcName, e.Script, isGlobal);
                }
                else
                {
                    throw new ScriptException("指定された関数はすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED, e.Script);
                }
            }

        }

        private bool FunctionIsVirtual(string name, ParsingScript script)
        {
            ParserFunction impl;
            if (script != null && script.TryGetFunction(name, out impl))
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

    public class AliceScriptClass : FunctionBase
    {
        public AliceScriptClass()
        {
            this.Name = "Class";
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
        }

        public AliceScriptClass(string className)
        {
            Name = className;
            RegisterClass(className, this);
        }

        public AliceScriptClass(string className, string[] baseClasses, ParsingScript script)
        {
            Name = className;
            RegisterClass(className, this);

            this.BaseClasses = baseClasses;

            foreach (string baseClass in baseClasses)
            {
                var bc = AliceScriptClass.GetClass(baseClass, script);
                if (bc == null)
                {
                    throw new ScriptException(" `" + className + "` の基底クラス `" + baseClass + "` が見つかりませんでした。", Exceptions.COULDNT_FIND_CLASS);
                }

                foreach (var entry in bc.m_classProperties)
                {
                    m_classProperties[entry.Key] = entry.Value;
                }
                foreach (var entry in bc.m_customFunctions)
                {
                    m_customFunctions[entry.Key] = entry.Value;
                }
            }
        }
        /// <summary>
        /// このクラスの継承元を表します。ない場合はnullとなります。
        /// </summary>
        public string[] BaseClasses
        {
            get; set;
        }
        public Dictionary<string, FunctionBase> StaticFunctions
        {
            get => m_static_customFunctions;
            set => m_static_customFunctions = value;
        }
        public static void RegisterClass(string className, AliceScriptClass obj)
        {
            obj.Namespace = ParserFunction.GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(obj.Namespace))
            {
                className = obj.Namespace + "." + className;
            }

            obj.Name = className;
            className = Constants.ConvertName(className);
            s_allClasses[className] = obj;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            script.GetFunctionArgs(this);

            // TODO: Work in progress, currently not functional
            return Variable.EmptyInstance;
        }

        public void AddMethod(string name, string[] args, CustomFunction method)
        {
            if (name == m_name)
            {
                m_constructors[args.Length] = method;
                for (int i = 0; i < method.DefaultArgsCount && i < args.Length; i++)
                {
                    m_constructors[args.Length - i - 1] = method;
                }
            }
            else
            {
                m_customFunctions[name] = method;
            }
        }

        public void AddProperty(string name, Variable property)
        {
            m_classProperties[name] = new PropertyBase(property);
        }

        public static AliceScriptClass GetClass(string name, ParsingScript script)
        {
            string currNamespace = ParserFunction.GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(currNamespace))
            {
                bool namespacePresent = name.Contains(".");
                if (!namespacePresent)
                {
                    name = currNamespace + "." + name;
                }
            }

            AliceScriptClass theClass = null;
            if (s_allClasses.TryGetValue(name, out theClass))
            {
                return theClass;
            }
            var cls = GetFromNS(name, script);
            if (cls != null)
            {
                return cls;
            }

            //ちょっとでも高速化（ここのロジックは時間がかかる）
            if (name.Contains("."))
            {
                string namespacename = string.Empty;

                foreach (string nsn in NameSpaceManerger.NameSpaces.Keys)
                {
                    //より長い名前（AliceとAlice.IOならAlice.IO）を採用
                    if (name.StartsWith(nsn.ToLower() + ".", StringComparison.Ordinal) && nsn.Length > namespacename.Length)
                    {
                        namespacename = nsn.ToLower();
                    }
                }

                //完全修飾名で関数を検索
                if (namespacename != string.Empty)
                {
                    var cfc = NameSpaceManerger.NameSpaces.Where(x => x.Key.ToLower() == namespacename).FirstOrDefault().Value.Classes.Where((x) => name.EndsWith(x.Name.ToLower(), StringComparison.Ordinal)).FirstOrDefault();
                    if (cfc != null)
                    {
                        return cfc;
                    }
                }
            }

            return null;
        }
        private static AliceScriptClass GetFromNS(string name, ParsingScript script)
        {
            foreach (var nm in script.UsingNamespaces)
            {
                var fc = nm.Classes.Where((x) => x.Name.ToLower() == name.ToLower()).FirstOrDefault();
                if (fc != null)
                {
                    return fc;
                }
            }
            if (script.ParentScript != null)
            {
                return GetFromNS(name, script.ParentScript);
            }
            return null;
        }

        private static Dictionary<string, AliceScriptClass> s_allClasses =
            new Dictionary<string, AliceScriptClass>();
        private Dictionary<int, CustomFunction> m_constructors =
            new Dictionary<int, CustomFunction>();
        protected Dictionary<string, FunctionBase> m_customFunctions =
            new Dictionary<string, FunctionBase>();
        protected Dictionary<string, PropertyBase> m_classProperties =
            new Dictionary<string, PropertyBase>();
        protected Dictionary<string, FunctionBase> m_static_customFunctions =
            new Dictionary<string, FunctionBase>();

        public ParsingScript ParentScript = null;
        public int ParentOffset = 0;

        public string Namespace { get; internal set; }

        public class ClassInstance : ScriptObject
        {
            public ClassInstance(string instanceName, string className, List<Variable> args,
                                 ParsingScript script = null)
            {
                InstanceName = instanceName;
                m_cscsClass = AliceScriptClass.GetClass(className, script);
                if (m_cscsClass == null)
                {
                    throw new ScriptException("基底クラス `" + className + "` が見つかりませんでした。", Exceptions.COULDNT_FIND_CLASS);
                }

                // Copy over all the properties defined for this class.
                foreach (var entry in m_cscsClass.m_classProperties)
                {
                    SetProperty(entry.Key, entry.Value.Value);
                }

                // Run "constructor" if any is defined for this number of args.
                CustomFunction constructor = null;
                if (m_cscsClass.m_constructors.TryGetValue(args.Count, out constructor))
                {
                    constructor.ARun(args, script, this);
                }
            }

            public string InstanceName { get; set; }

            private AliceScriptClass m_cscsClass;
            private Dictionary<string, Variable> m_properties = new Dictionary<string, Variable>();
            private HashSet<string> m_propSet = new HashSet<string>();
            private HashSet<string> m_propSetLower = new HashSet<string>();

            public override string ToString()
            {
                FunctionBase customFunction = null;
                if (!m_cscsClass.m_customFunctions.TryGetValue(Constants.PROP_TO_STRING.ToLower(),
                     out customFunction))
                {
                    return m_cscsClass.Name + "." + InstanceName;
                }

                Variable result = customFunction.Evaluate(new List<Variable>(), null);
                return result.ToString();
            }

            public Task<Variable> SetProperty(string name, Variable value)
            {
                m_properties[name] = value;
                m_propSet.Add(name);
                m_propSetLower.Add(name.ToLower());
                return Task.FromResult(Variable.EmptyInstance);
            }

            public async Task<Variable> GetProperty(string name, List<Variable> args = null, ParsingScript script = null)
            {
                if (m_properties.TryGetValue(name, out Variable value))
                {
                    return value;
                }

                if (!m_cscsClass.m_customFunctions.TryGetValue(name, out FunctionBase customFunction))
                {
                    return null;
                }
                if (args == null)
                {
                    return Variable.EmptyInstance;
                }

                foreach (var entry in m_cscsClass.m_classProperties)
                {
                    args.Add(entry.Value.Value);
                }

                Variable result = customFunction.Evaluate(args, script, this);
                return result;
            }

            public List<KeyValuePair<string, Variable>> GetPropList()
            {
                List<KeyValuePair<string, Variable>> props = new List<KeyValuePair<string, Variable>>();
                foreach (var entry in m_properties)
                {
                    props.Add(new KeyValuePair<string, Variable>(entry.Key, entry.Value));
                }
                return props;
            }

            public List<string> GetProperties()
            {
                List<string> props = new List<string>(m_properties.Keys);
                props.AddRange(m_cscsClass.m_customFunctions.Keys);

                return props;
            }
            public bool PropertyExists(string name)
            {
                return m_propSetLower.Contains(name.ToLower());
            }

            public bool FunctionExists(string name)
            {
                if (!m_cscsClass.m_customFunctions.TryGetValue(name, out FunctionBase customFunction))
                {
                    return false;
                }
                return true;
            }
        }
    }

    internal class EnumFunction : FunctionBase
    {
        public EnumFunction()
        {
            this.Name = Constants.ENUM;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += EnumFunction_Run;
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

            if (enumType == null || !enumType.IsEnum)
            {
                return null;
            }

            return enumType;
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

    public class ClassCreator : FunctionBase
    {
        public ClassCreator()
        {
            this.Name = Constants.CLASS;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += ClassCreator_Run;
        }

        private void ClassCreator_Run(object sender, FunctionBaseEventArgs e)
        {
            string className = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            className = Constants.ConvertName(className);
            string[] baseClasses = Utils.GetBaseClasses(e.Script);
            AliceScriptClass newClass = new AliceScriptClass(className, baseClasses, e.Script);

            e.Script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);

            newClass.ParentOffset = e.Script.Pointer;
            newClass.ParentScript = e.Script;
            /*string line = */
            e.Script.GetOriginalLine(out _);

            string scriptExpr = Utils.GetBodyBetween(e.Script, Constants.START_GROUP,
                                                     Constants.END_GROUP);
            e.Script.MoveForwardIf(Constants.END_GROUP);

            string body = Utils.ConvertToScript(scriptExpr, out _, out var def);

            Variable result = null;
            ParsingScript tempScript = e.Script.GetTempScript(body);
            tempScript.Defines = def;
            tempScript.CurrentClass = newClass;
            tempScript.DisableBreakpoints = true;

            while (tempScript.Pointer < body.Length - 1 &&
                  (result == null || !result.IsReturn))
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
        }

    }

    public class NamespaceFunction : FunctionBase
    {
        public NamespaceFunction()
        {
            this.Name = Constants.NAMESPACE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += NamespaceFunction_Run;
        }

        private void NamespaceFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string namespaceName = Utils.GetToken(e.Script, Constants.NEXT_OR_END_ARRAY);
            //Utils.CheckNotEnd(script, m_name);
            Variable result = null;
            try
            {
                e.Script.MoveForwardIf(Constants.START_GROUP);
                string scriptExpr = Utils.GetBodyBetween(e.Script, Constants.START_GROUP,
                                                         Constants.END_GROUP);
                e.Script.MoveForwardIf(Constants.END_GROUP);

                Dictionary<int, int> char2Line;
                string body = Utils.ConvertToScript(scriptExpr, out char2Line, out var def);

                ParsingScript tempScript = e.Script.GetTempScript(body);
                tempScript.Defines = def;
                tempScript.DisableBreakpoints = true;
                tempScript.MoveForwardIf(Constants.START_GROUP);



                while (tempScript.Pointer < body.Length - 1 &&
                      (result == null || !result.IsReturn))
                {
                    result = tempScript.Execute();
                    tempScript.GoToNextStatement();
                }
            }
            finally
            {
                ParserFunction.PopNamespace();
            }

            e.Return = result;
        }

    }

    public class CustomFunction : FunctionBase
    {
        public CustomFunction(string funcName,
                                string body, string[] args, ParsingScript script, bool forceReturn = false)
        {
            Name = funcName;
            m_body = body;
            m_forceReturn = forceReturn;

            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;

            this.Run += CustomFunction_Run;

            //正確な変数名の一覧
            List<string> trueArgs = new List<string>();
            //m_args = RealArgs = args;

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
                    var stb = new List<string>(arg.Split(' '));
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
                    parms = (options.Contains(Constants.PARAMS));
                    refs = (options.Contains(Constants.REF));
                    if (options.Contains("this"))
                    {
                        if (m_this == -1)
                        {
                            m_this = i;
                        }
                        else
                        {
                            throw new ScriptException("this修飾子は一つのメソッドに一つのみ設定可能です", Exceptions.INVAILD_ARGUMENT_FUNCTION, script);
                        }

                    }
                    else if (!refs && options.Count > 1)
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
                        string argName = arg;// RealArgs[i].ToLower();
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
                m_args = RealArgs = trueArgs.ToArray();
            }

        }

        private void CustomFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            List<Variable> args = Constants.FUNCT_WITH_SPACE.Contains(m_name) ?
                // Special case of extracting args.
                Utils.GetFunctionArgsAsStrings(e.Script) :
                e.Script.GetFunctionArgs(this);

            Utils.ExtractParameterNames(args, m_name, e.Script);

            if (m_args == null)
            {
                m_args = new string[0];
            }
            if (args.Count + m_defaultArgs.Count < m_args.Length)
            {
                throw new ScriptException($"関数`{m_args.Length}`は引数`{m_args.Length}`を受取ることが出来ません。", Exceptions.TOO_MANY_ARGUREMENTS, e.Script);
            }
            Variable result = ARun(args, e.Script);
            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = child.Evaluate(e.Script);
                }
            }
            e.Return = result;
            return;
        }

        private int parmsindex = -1;
        public void RegisterArguments(List<Variable> args,
                                      List<KeyValuePair<string, Variable>> args2 = null, Variable current = null, ParsingScript script = null)
        {
            if (args == null)
            {
                args = new List<Variable>();
            }
            if (m_this != -1)
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

                    if (ArgMap.TryGetValue(arg.CurrentAssign, out argIndex))
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
                foreach (var entry in args2)
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
                    foreach (Variable argx in args.GetRange(i, args.Count - i))
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
                        if (refd)
                        {
                            val = args[i];
                        }
                        else
                        {
                            throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                        }
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
                    if (refd)
                    {
                        val = args[i];
                    }
                    else
                    {
                        throw new ScriptException("引数 `" + i + "` は `" + Constants.REF + "` キーワードと共に渡さなければなりません。", Exceptions.ARGUMENT_MUST_BE_PASSED_WITH_KEYWORD, script);
                    }
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
                //m_VarMap[args[i].ParamName] = arg;
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
        internal List<CustomFunction> Children
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
                script.CloneThrowTryInfo(tempScript);
                tempScript.m_stacktrace = new List<ParsingScript.StackInfo>(script.m_stacktrace);
                tempScript.m_stacktrace.Add(new ParsingScript.StackInfo(this, script.OriginalLine, script.OriginalLineNumber, script.Filename));
            }
            tempScript.Tag = m_tag;
            //tempScript.Variables = m_VarMap;
            List<KeyValuePair<string, Variable>> args2 = instance == null ? null : instance.GetPropList();
            // ひとまず引数をローカルに追加
            RegisterArguments(args, args2, current, tempScript);

            // さて実行


            while (tempScript.Pointer < m_body.Length - 1 &&
                  (result == null || !result.IsReturn))
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }


            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            else if (result.IsReturn || m_forceReturn)
            {
                result.IsReturn = false;
            }
            else
            {
                result = Variable.EmptyInstance;
            }

            return result;
        }


        public override ParserFunction NewInstance()
        {
            var newInstance = (CustomFunction)this.MemberwiseClone();
            return newInstance;
        }

        public ParsingScript ParentScript { set => m_parentScript = value; }
        public int ParentOffset { set => m_parentOffset = value; }
        public string Body => m_body;

        public int ArgumentCount => m_args.Length;
        public string Argument(int nIndex) { return m_args[nIndex]; }

        public StackLevel NamespaceData { get; set; }
        public bool IsMethod => (m_this != -1);
        public TypeObject MethodRequestType
        {
            get
            {
                if (IsMethod && m_typArgMap.ContainsKey(m_this))
                {
                    return m_typArgMap[m_this];
                }
                else
                {
                    return new TypeObject();
                }
            }
        }

        public int DefaultArgsCount => m_defaultArgs.Count;

        public string Header => Constants.FUNCTION + " " + Constants.GetRealName(Name) + " " +
                       Constants.START_ARG + string.Join(", ", m_args) +
                       Constants.END_ARG + " " + Constants.START_GROUP;

        protected int m_this = -1;
        protected string m_body;
        protected object m_tag;
        protected bool m_forceReturn;
        protected string[] m_args;
        protected ParsingScript m_parentScript = null;
        protected int m_parentOffset = 0;
        private List<Variable> m_defaultArgs = new List<Variable>();
        private List<int> m_refMap = new List<int>();
        //private Dictionary<string, ParserFunction> m_VarMap = new Dictionary<string, ParserFunction>();
        private Dictionary<int, int> m_defArgMap = new Dictionary<int, int>();
        private Dictionary<int, TypeObject> m_typArgMap = new Dictionary<int, TypeObject>();

        public Dictionary<string, int> ArgMap { get; private set; } = new Dictionary<string, int>();
    }

    internal class StringOrNumberFunction : FunctionBase
    {
        public StringOrNumberFunction()
        {
            this.Name = "StringOrNumber";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Run += StringOrNumberFunction_Run;
        }

        private void StringOrNumberFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // 文字列型かどうか確認
            if (!string.IsNullOrEmpty(Item))
            {
                if (StringMode)
                {
                    bool sq = (Item[0] == Constants.QUOTE1 && Item[Item.Length - 1] == Constants.QUOTE1);
                    bool dq = (Item[0] == Constants.QUOTE && Item[Item.Length - 1] == Constants.QUOTE);
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
                            result = Utils.ConvertUnicodeLiteral(result);
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
    internal class IdentityFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            return script.Execute(Constants.END_ARG_ARRAY);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await script.ExecuteAsync(Constants.END_ARG_ARRAY);
        }
    }


    // Get a value of a variable or of an array element
    public class GetVarFunction : FunctionBase
    {
        public GetVarFunction(Variable value)
        {
            m_value = value;
            this.Name = "Variable";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            //this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Run += GetVarFunction_Run;
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
            if (var != null && !string.IsNullOrWhiteSpace(var.CustomGet))
            {
                return ParsingScript.RunString(var.CustomGet, script);
            }
            return var;
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

    internal class IncrementDecrementFunction : ActionFunction, INumericFunction
    {
        public IncrementDecrementFunction()
        {
            this.Name = "IncrementDecrement";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Run += IncrementDecrementFunction_Run;
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
            List<Variable> arrayIndices = Utils.GetArrayIndices(e.Script, m_name, (string name) => { m_name = name; }, this);

            ParserFunction func = ParserFunction.GetVariable(m_name, e.Script);
            Utils.CheckNotNull(m_name, func, e.Script);

            Variable currentValue = func.GetValue(e.Script);
            currentValue = currentValue.DeepClone();

            if (arrayIndices.Count > 0 || e.Script.TryCurrent() == Constants.START_ARRAY)
            {
                if (prefix)
                {
                    string tmpName = m_name + e.Script.Rest;
                    int delta = 0;
                    arrayIndices = Utils.GetArrayIndices(e.Script, tmpName, delta, (string t, int d) => { tmpName = t; delta = d; }, this);
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


        override public ParserFunction NewInstance()
        {
            return new IncrementDecrementFunction();
        }
    }

    internal class OperatorAssignFunction : ActionFunction
    {
        public OperatorAssignFunction()
        {
            this.Name = "OperatorAssign";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Run += OperatorAssignFunction_Run;
        }

        private void OperatorAssignFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            // Value to be added to the variable:
            Variable right = Utils.GetItem(e.Script);


            Variable currentValue = ParserFunction.GetObjectFunction(m_name, e.Script, new List<string>())?.GetValue(e.Script);
            bool isobj = true;
            List<Variable> arrayIndices = new List<Variable>();
            if (currentValue == null)
            {
                isobj = false;
                arrayIndices = Utils.GetArrayIndices(e.Script, m_name, (string name) => { m_name = name; }, this);

                ParserFunction func = ParserFunction.GetVariable(m_name, e.Script);
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

        override public ParserFunction NewInstance()
        {
            return new OperatorAssignFunction();
        }
    }

    public class AssignFunction : ActionFunction
    {
        public AssignFunction()
        {
            this.Name = "Assign";

            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.RelatedNameSpace = Constants.PARSING_NAMESPACE;
            this.Run += AssignFunction_Run;
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

            bool registVar = this.Keywords.Contains(Constants.VAR);
            bool registConst = this.Keywords.Contains(Constants.CONST);
            bool isGlobal = this.Keywords.Contains(Constants.PUBLIC);

            script.MoveBackIfPrevious(Constants.END_ARG);
            if (varValue == null)
            {
                return Variable.EmptyInstance;
            }
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
                    List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; }, this);
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

                    ParserFunction pf = ParserFunction.GetVariable(m_name, script, false, this.Keywords);
                    array = pf != null ? (pf.GetValue(script)) : new Variable();

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
                    throw new ScriptException("定数に値を代入することはできません", Exceptions.CANT_ASSIGN_VALUE_TO_CONSTANT, script);
                }
            }
            else
            {
                // First try processing as an object (with a dot notation):
                Variable result = ProcessObject(m_name, script, varValue);
                if (result != null)
                {
                    if (script.CurrentClass == null && script.ClassInstance == null)
                    {
                        //TODO:これ無効化したけど大丈夫そ？
                        //  ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(result), baseScript, localIfPossible, registVar, isGlobal);
                    }
                    return result;
                }

                // 設定する変数が x[a][b]... のような形式かどうかをチェックする
                // つまり、配列添え字演算子が書いてあるかどうかを確認
                List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; }, this);

                if (arrayIndices.Count == 0)
                {
                    ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(varValue), baseScript, localIfPossible, registVar, isGlobal);
                    Variable retVar = varValue.DeepClone();
                    retVar.CurrentAssign = m_name;
                    return retVar;
                }

                Variable array;

                ParserFunction pf = ParserFunction.GetVariable(m_name, baseScript);
                array = pf != null ? (pf.GetValue(script)) : new Variable();

                ExtendArray(array, arrayIndices, 0, varValue);

                ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(array), baseScript, localIfPossible, registVar, isGlobal);
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

            Utils.CheckLegalName(varName);

            string name = varName.Substring(0, ind);
            string prop = varName.Substring(ind + 1);

            if (ParserFunction.TryAddToNamespace(prop, name, varValue))
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


        override public ParserFunction NewInstance()
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

}
