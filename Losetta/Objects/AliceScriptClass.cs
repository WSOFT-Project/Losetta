using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    /// <summary>
    /// ユーザー定義のクラス
    /// </summary>
    public class AliceScriptClass : FunctionBase
    {
        public AliceScriptClass()
        {
            Name = "Class";
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
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

            BaseClasses = baseClasses;

            foreach (string baseClass in baseClasses)
            {
                var bc = GetClass(baseClass, script);
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
            obj.Namespace = GetCurrentNamespace;
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
            string currNamespace = GetCurrentNamespace;
            if (!string.IsNullOrWhiteSpace(currNamespace))
            {
                bool namespacePresent = name.Contains(".");
                if (!namespacePresent)
                {
                    name = currNamespace + "." + name;
                }
            }

            if (s_allClasses.TryGetValue(name, out AliceScriptClass theClass))
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

                foreach (string nsn in NameSpaceManager.NameSpaces.Keys)
                {
                    //より長い名前（AliceとAlice.IOならAlice.IO）を採用
                    if (name.StartsWith(nsn.ToLowerInvariant() + ".", StringComparison.Ordinal) && nsn.Length > namespacename.Length)
                    {
                        namespacename = nsn.ToLowerInvariant();
                    }
                }
                    
                //完全修飾名で関数を検索
                if (namespacename != string.Empty)
                {
                    var cfc = NameSpaceManager.NameSpaces.Where(x => x.Key.Equals(namespacename, StringComparison.OrdinalIgnoreCase)).FirstOrDefault().Value.Classes.Where((x) => name.EndsWith(x.Name.ToLowerInvariant(), StringComparison.Ordinal)).FirstOrDefault();
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
            foreach(NameSpace ns in script.UsingNamespaces)
            {
                var fc=ns.Classes.Where((x) => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (fc != null)
                {
                    return fc;
                }
            }
            return script.ParentScript != null ? GetFromNS(name, script.ParentScript) : null;
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
                m_cscsClass = GetClass(className, script);
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
                if (m_cscsClass.m_constructors.TryGetValue(args.Count, out CustomFunction constructor))
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
                if (!m_cscsClass.m_customFunctions.TryGetValue(Constants.PROP_TO_STRING.ToLowerInvariant(),
                     out FunctionBase customFunction))
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
                m_propSetLower.Add(name.ToLowerInvariant());
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
                return m_propSetLower.Contains(name.ToLowerInvariant());
            }

            public bool FunctionExists(string name)
            {
                return m_cscsClass.m_customFunctions.TryGetValue(name, out FunctionBase customFunction);
            }
        }
    }
    internal sealed class ClassCreator : FunctionBase
    {
        public ClassCreator()
        {
            Name = Constants.CLASS;
            RelatedNameSpace = Constants.PARSING_NAMESPACE;
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += ClassCreator_Run;
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

            string body = Utils.ConvertToScript(scriptExpr, out _, out var def, out var settings);

            Variable result = null;
            ParsingScript tempScript = e.Script.GetTempScript(body);
            tempScript.Settings = settings;
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
}
