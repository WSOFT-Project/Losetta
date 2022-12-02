using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AliceScript
{
    internal class ReturnStatement : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            script.MoveForwardIf(Constants.SPACE);
            if (!script.FromPrev(Constants.RETURN.Length).Contains(Constants.RETURN))
            {
                script.Backward();
            }
            Variable result = Utils.GetItem(script);

            // If we are in Return, we are done:
            script.SetDone();
            result.IsReturn = true;

            return result;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            script.MoveForwardIf(Constants.SPACE);
            if (!script.FromPrev(Constants.RETURN.Length).Contains(Constants.RETURN))
            {
                script.Backward();
            }
            Variable result = await Utils.GetItemAsync(script);

            // If we are in Return, we are done:
            script.SetDone();
            result.IsReturn = true;

            return result;
        }
    }

    internal class TryBlock : FunctionBase
    {
        public TryBlock()
        {
            this.Name = Constants.TRY;
            this.Attribute = FunctionAttribute.CONTROL_FLOW | FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += TryBlock_Run;
        }

        private void TryBlock_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Interpreter.Instance.ProcessTry(e.Script);
        }
    }

    internal class ExitFunction : FunctionBase
    {
        public ExitFunction()
        {
            this.FunctionName = Constants.EXIT;
            this.MinimumArgCounts = 0;
            this.Run += ExitFunction_Run;
        }

        private void ExitFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                Alice.OnExiting(0);
            }
            else
            {
                Alice.OnExiting(Utils.GetSafeInt(e.Args, 0, 0));
            }
        }
    }

    internal class IsNaNFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);
            Variable arg = args[0];
            return new Variable(arg.Type != Variable.VarType.NUMBER || double.IsNaN(arg.Value));
        }
    }

    internal class ReturnValueFunction : FunctionBase, INumericFunction
    {
        public ReturnValueFunction(Variable value)
        {
            Value = value;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            return new Variable(Value);
        }
        private Variable Value;
    }

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

    internal class ObjectPropsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            Variable obj = Utils.GetItem(script, true);
            string propName = Utils.GetItem(script, true).AsString();
            script.MoveForwardIf(',');

            Variable value = Utils.GetProperties(script);
            obj.SetProperty(propName, value, script);

            ParserFunction.AddGlobal(obj.ParamName, new GetVarFunction(obj), false);

            return new Variable(obj.ParamName);
        }
    }

    internal class ThrowFunction : FunctionBase
    {
        public ThrowFunction()
        {
            this.Name = "throw";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            this.MinimumArgCounts = 1;
            this.Run += ThrowFunction_Run;
        }

        private void ThrowFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (e.Args[0].Type)
            {
                case Variable.VarType.STRING:
                    {
                        ThrowErrorManerger.OnThrowError(e.Args[0].AsString(), Exceptions.USER_DEFINED, e.Script);
                        break;
                    }
                case Variable.VarType.NUMBER:
                    {
                        ThrowErrorManerger.OnThrowError(Utils.GetSafeString(e.Args, 1), (Exceptions)e.Args[0].AsInt(), e.Script);
                        break;
                    }
            }
        }
    }
    internal class VarFunction : ParserFunction
    {
        private bool m_Const = false;
        public VarFunction(bool isConst=false)
        {
            m_Const = isConst;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            var args = Utils.GetTokens(script);
            Variable result = Variable.EmptyInstance;
            foreach (var arg in args)
            {
                string a = arg;
                bool isGlobal = a.StartsWith("global ");
                if (isGlobal)
                {
                    a = a.Substring(6).TrimStart();
                }
                var ind = a.IndexOf('=');
                if (ind <= 0)
                {
                    if (!FunctionExists(a,script)&&!m_Const)
                    {
                        AddGlobalOrLocalVariable(a, new GetVarFunction(new Variable(Variable.VarType.NONE)), script,false,true,isGlobal);
                    }
                    continue;
                }
                var varName = a.Substring(0, ind);
                ParsingScript tempScript = script.GetTempScript(a.Substring(ind + 1));
                AssignFunction assign = new AssignFunction();
                result = assign.Assign(tempScript, varName, false,true,m_Const,script,isGlobal);
            }
            return result;
        }

        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            var args = Utils.GetTokens(script);
            Task<Variable> result = null;
            foreach (var arg in args)
            {
                string a = arg;
                bool isGlobal = a.StartsWith("global ");
                if (isGlobal)
                {
                    a = a.Substring(6).TrimStart();
                }
                var ind = a.IndexOf('=');
                if (ind <= 0)
                {
                    if (!FunctionExists(a, script)&&!m_Const)
                    {
                        AddGlobalOrLocalVariable(a, new GetVarFunction(new Variable(Variable.VarType.NONE)), script, false, true, isGlobal);
                    }
                    continue;
                }
                var varName = a.Substring(0, ind);
                ParsingScript tempScript = script.GetTempScript(a.Substring(ind + 1));
                AssignFunction assign = new AssignFunction();
                result = assign.AssignAsync(tempScript, varName, false, true,m_Const, script, isGlobal);
            }

            return result == null ? Variable.EmptyInstance : await result;
        }
    }

    internal class FunctionCreator : ParserFunction
    {
        public FunctionCreator()
        {

        }
        protected override Variable Evaluate(ParsingScript script)
        {
            string funcName="";
            bool? mode = null;
            bool isGlobal = false;
            while (true)
            {
                //トークンが得られなくなるまでループ
                string token = Utils.GetToken(script,Constants.TOKEN_SEPARATION);
                if (string.IsNullOrEmpty(token))
                {
                    break;
                }
                switch (token.ToLower())
                {
                    default:
                        {
                            funcName = token;
                            break;
                        }
                    case "override":
                        {
                            mode = true;
                            break;
                        }
                    case "virtual":
                        {
                            mode = false;
                            break;
                        }
                    case "global":
                        {
                            isGlobal = true;
                            break;
                        }
                }
            }
            funcName = Constants.ConvertName(funcName);

            string[] args = Utils.GetFunctionSignature(script);
            if (args.Length == 1 && string.IsNullOrWhiteSpace(args[0]))
            {
                args = new string[0];
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

            CustomFunction customFunc = new CustomFunction(funcName, body, args, script);
            customFunc.ParentScript = script;
            customFunc.ParentOffset = parentOffset;
            if (mode !=null)
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
                        ThrowErrorManerger.OnThrowError("指定されたメソッドはすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                    }
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("メソッドはグローバル関数である必要があります",Exceptions.FUNCTION_NOT_GLOBAL);
                }
            }else
            if (script.CurrentClass != null)
            {
                script.CurrentClass.AddMethod(funcName, args, customFunc);
            }
            else
            {
                if (!FunctionExists(funcName,script)||(mode==true&&FunctionIsVirtual(funcName,script)))
                {
                    ParserFunction.RegisterScriptFunction(funcName, customFunc, script,false /* not native */,!isGlobal);
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("指定された関数はすでに登録されていて、オーバーライド不可能です。関数にoverride属性を付与することを検討してください。", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                }
            }

            return Variable.EmptyInstance;
        }
        private bool FunctionIsVirtual(string name,ParsingScript script)
        {
            ParserFunction impl;
            if(script!=null&&script.TryGetFunction(name,out impl))
            {
                if (impl.IsVirtual)
                {
                    return true;
                }
            }
            if(s_functions.TryGetValue(name,out impl))
            {
                if (impl.IsVirtual)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class AliceScriptClass : ParserFunction
    {
        public AliceScriptClass() { }

        public AliceScriptClass(string className)
        {
            Name = className;
            RegisterClass(className, this);
        }

        public AliceScriptClass(string className, string[] baseClasses)
        {
            Name = className;
            RegisterClass(className, this);

            foreach (string baseClass in baseClasses)
            {
                var bc = AliceScriptClass.GetClass(baseClass);
                if (bc == null)
                {
                    throw new ArgumentException("継承元クラスである [" + baseClass + "] が存在しません");
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
            script.GetFunctionArgs();

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
            m_classProperties[name] = property;
        }

        public static AliceScriptClass GetClass(string name)
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
            s_allClasses.TryGetValue(name, out theClass);
            return theClass;
        }

        private static Dictionary<string, AliceScriptClass> s_allClasses =
            new Dictionary<string, AliceScriptClass>();
        private Dictionary<int, CustomFunction> m_constructors =
            new Dictionary<int, CustomFunction>();
        private Dictionary<string, CustomFunction> m_customFunctions =
            new Dictionary<string, CustomFunction>();
        private Dictionary<string, Variable> m_classProperties =
            new Dictionary<string, Variable>();

        public ParsingScript ParentScript = null;
        public int ParentOffset = 0;

        public string Namespace { get; private set; }

        public class ClassInstance : ScriptObject
        {
            public ClassInstance(string instanceName, string className, List<Variable> args,
                                 ParsingScript script = null)
            {
                InstanceName = instanceName;
                m_cscsClass = AliceScriptClass.GetClass(className);
                if (m_cscsClass == null)
                {
                    throw new ArgumentException("継承元クラスである [" + className + "] が存在しません");
                }

                // Copy over all the properties defined for this class.
                foreach (var entry in m_cscsClass.m_classProperties)
                {
                    SetProperty(entry.Key, entry.Value);
                }

                // Run "constructor" if any is defined for this number of args.
                CustomFunction constructor = null;
                if (m_cscsClass.m_constructors.TryGetValue(args.Count, out constructor))
                {
                    constructor.Run(args, script, this);
                }
            }

            public string InstanceName { get; set; }

            private AliceScriptClass m_cscsClass;
            private Dictionary<string, Variable> m_properties = new Dictionary<string, Variable>();
            private HashSet<string> m_propSet = new HashSet<string>();
            private HashSet<string> m_propSetLower = new HashSet<string>();

            public override string ToString()
            {
                CustomFunction customFunction = null;
                if (!m_cscsClass.m_customFunctions.TryGetValue(Constants.PROP_TO_STRING.ToLower(),
                     out customFunction))
                {
                    return m_cscsClass.Name + "." + InstanceName;
                }

                Variable result = customFunction.Run(null, null, this);
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

                if (!m_cscsClass.m_customFunctions.TryGetValue(name, out CustomFunction customFunction))
                {
                    return null;
                }
                if (args == null)
                {
                    return Variable.EmptyInstance;
                }

                foreach (var entry in m_cscsClass.m_classProperties)
                {
                    args.Add(entry.Value);
                }

                Variable result = await customFunction.RunAsync(args, script, this);
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
                if (!m_cscsClass.m_customFunctions.TryGetValue(name, out CustomFunction customFunction))
                {
                    return false;
                }
                return true;
            }
        }
    }

    internal class EnumFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<string> properties = Utils.ExtractTokens(script);

            if (properties.Count == 1 && properties[0].Contains("."))
            {
                return UseExistingEnum(properties[0]);
            }

            Variable enumVar = new Variable(Variable.VarType.ENUM);
            for (int i = 0; i < properties.Count; i++)
            {
                enumVar.SetEnumProperty(properties[i], new Variable(i));
            }

            return enumVar;
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

    internal class NewObjectFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string className = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            className = Constants.ConvertName(className);
            script.MoveForwardIf(Constants.START_ARG);
            List<Variable> args = script.GetFunctionArgs();

            CompiledClass csClass = AliceScriptClass.GetClass(className) as CompiledClass;
            if (csClass != null)
            {
                ScriptObject obj = csClass.GetImplementation(args);
                return new Variable(obj);
            }
            CompiledClassAsync csClassAsync = AliceScriptClass.GetClass(className) as CompiledClassAsync;
            if (csClassAsync != null)
            {
                ScriptObject obj = csClassAsync.GetImplementationAsync(args).Result;
                return new Variable(obj);
            }

            AliceScriptClass.ClassInstance instance = new
                AliceScriptClass.ClassInstance(script.CurrentAssign, className, args, script);

            return new Variable(instance);
        }

        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            string className = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            className = Constants.ConvertName(className);
            script.MoveForwardIf(Constants.START_ARG);
            List<Variable> args = await script.GetFunctionArgsAsync();

            CompiledClassAsync csClassAsync = AliceScriptClass.GetClass(className) as CompiledClassAsync;
            if (csClassAsync != null)
            {
                ScriptObject obj = await csClassAsync.GetImplementationAsync(args);
                return new Variable(obj);
            }
            CompiledClass csClass = AliceScriptClass.GetClass(className) as CompiledClass;
            if (csClass != null)
            {
                ScriptObject obj = csClass.GetImplementation(args);
                return new Variable(obj);
            }

            AliceScriptClass.ClassInstance instance = new
                AliceScriptClass.ClassInstance(script.CurrentAssign, className, args, script);

            return new Variable(instance);
        }
    }

    public class ClassCreator : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string className = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            className = Constants.ConvertName(className);
            string[] baseClasses = Utils.GetBaseClasses(script);
            AliceScriptClass newClass = new AliceScriptClass(className, baseClasses);

            script.MoveForwardIf(Constants.START_GROUP, Constants.SPACE);

            newClass.ParentOffset = script.Pointer;
            newClass.ParentScript = script;
            /*string line = */
            script.GetOriginalLine(out _);

            string scriptExpr = Utils.GetBodyBetween(script, Constants.START_GROUP,
                                                     Constants.END_GROUP);
            script.MoveForwardIf(Constants.END_GROUP);

            string body = Utils.ConvertToScript(scriptExpr, out _);

            Variable result = null;
            ParsingScript tempScript = script.GetTempScript(body);
            tempScript.CurrentClass = newClass;
            tempScript.DisableBreakpoints = true;

            // Uncomment if want to step into the class creation code when the debugger is attached (unlikely)
            /*Debugger debugger = script != null && script.Debugger != null ? script.Debugger : Debugger.MainInstance;
            if (debugger != null)
            {
                result = debugger.StepInFunctionIfNeeded(tempScript);
            }*/

            while (tempScript.Pointer < body.Length - 1 &&
                  (result == null || !result.IsReturn))
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }

            return Variable.EmptyInstance;
        }
    }

    public class NamespaceFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            string namespaceName = Utils.GetToken(script, Constants.NEXT_OR_END_ARRAY);
            //Utils.CheckNotEnd(script, m_name);
            Variable result = null;

            ParserFunction.AddNamespace(namespaceName);
            try
            {
                script.MoveForwardIf(Constants.START_GROUP);
                string scriptExpr = Utils.GetBodyBetween(script, Constants.START_GROUP,
                                                         Constants.END_GROUP);
                script.MoveForwardIf(Constants.END_GROUP);

                Dictionary<int, int> char2Line;
                string body = Utils.ConvertToScript(scriptExpr, out char2Line);

                ParsingScript tempScript = script.GetTempScript(body);
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

            return result;
        }
    }

    public class CustomFunction : ParserFunction
    {
        public CustomFunction(string funcName,
                                string body, string[] args, ParsingScript script, object tag = null)
        {
            Name = funcName;
            m_body = body;
            m_tag = tag;
            //正確な変数名の一覧
            List<string> trueArgs = new List<string>();
            //m_args = RealArgs = args;

            bool parms = false;

            for (int i = 0; i < args.Length; i++)
            {
                //変数名
                string arg = args[i];
                //属性等
                List<string> options = new List<string>();
                if (parms)
                {
                    ThrowErrorManerger.OnThrowError("parmsキーワードより後にパラメータを追加することはできません", Exceptions.COULDNT_ADD_PARAMETERS_AFTER_PARMS_KEYWORD, script);
                    break;
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
                        if (option.StartsWith("=") || option.EndsWith("="))
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
                Variable.VarType reqType = Variable.VarType.NONE;
                if (options.Count > 0)
                {
                    parms = (options.Contains("parms"));
                    if (options.Contains("this"))
                    {
                        if (m_this == -1)
                        {
                            m_this = i;
                        }
                        else
                        {
                            ThrowErrorManerger.OnThrowError("this修飾子は一つのメソッドに一つのみ設定可能です", Exceptions.INVAILD_ARGUMENT_FUNCTION);
                        }
                        foreach (string opt in options)
                        {
                            string option = opt.ToLower();
                            if (Constants.TYPE_MODIFER.Contains(option))
                            {
                                if (reqType == Variable.VarType.NONE)
                                {
                                    reqType = Constants.StringToType(option);
                                }
                                else
                                {
                                    ThrowErrorManerger.OnThrowError("複数の型を指定することはできません", Exceptions.WRONG_TYPE_VARIABLE, script);
                                }
                            }
                        }
                    }
                    if (reqType != Variable.VarType.NONE)
                    {
                        m_typArgMap.Add(i, reqType);
                    }
                    int ind = arg.IndexOf('=');
                    if (ind > 0)
                    {

                        trueArgs[i] = arg.Substring(0, ind).Trim();
                        string defValue = ind >= arg.Length - 1 ? "" : arg.Substring(ind + 1).Trim();

                        Variable defVariable = Utils.GetVariableFromString(defValue, script);
                        defVariable.CurrentAssign = m_args[i];
                        defVariable.Index = i;

                        if (defVariable.Type != reqType)
                        {
                            ThrowErrorManerger.OnThrowError("この引数は" + Constants.TypeToString(reqType) + "型である必要があります", Exceptions.WRONG_TYPE_VARIABLE, script);
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
                            argName = argName.TrimStart("parms".ToCharArray());
                            argName = argName.Trim();
                        }
                        trueArgs[i] = argName;

                    }

                    ArgMap[trueArgs[i]] = i;
                }
                m_args = RealArgs = trueArgs.ToArray();
            }
        }

        private int parmsindex = -1;
        public void RegisterArguments(List<Variable> args,
                                      List<KeyValuePair<string, Variable>> args2 = null,Variable current=null)
        {
            if (args == null)
            {
                args = new List<Variable>();
            }
            if (m_this != -1)
            {
                args.Insert(m_this, current);
            }
            int missingArgs = m_args.Length - args.Count;
            bool namedParameters = false;
            for (int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                int argIndex = -1;
                if (m_typArgMap.ContainsKey(i) && m_typArgMap[i] != arg.Type)
                {
                    ThrowErrorManerger.OnThrowError("この引数は" + Constants.TypeToString(m_typArgMap[i]) + "型である必要があります", Exceptions.WRONG_TYPE_VARIABLE);
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
                        throw new ArgumentException("関数におけるすべての引数: [" + m_name +
                         "] はarg=valueの型である必要があります。");
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
                                throw new ArgumentException("関数 [" + m_name + "]に引数 [" + m_args[i] +
                                 "] がありません");
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
                    throw new ArgumentException("関数 [" + m_name + "]に引数 [" + m_args[i] +
                                 "] がありません");
                }
                args.Add(m_defaultArgs[defIndex]);
            }

            if (args2 != null)
            {
                foreach (var entry in args2)
                {
                    var arg = new GetVarFunction(entry.Value);
                    arg.Name = entry.Key;
                    m_VarMap[entry.Key] = arg;
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
                        parmsarg.Tuple.Add(argx);
                    }
                    var arg = new GetVarFunction(parmsarg);
                    arg.Name = m_args[i];
                    m_VarMap[m_args[i]] = arg;
                }
                else
                {
                    var arg = new GetVarFunction(args[i]);
                    arg.Name = m_args[i];
                    m_VarMap[m_args[i]] = arg;
                }
            }
            for (int i = m_args.Length; i < args.Count; i++)
            {
                var arg = new GetVarFunction(args[i]);
                m_VarMap[args[i].ParamName] = arg;
            }

            if (NamespaceData != null)
            {
                var vars = NamespaceData.Variables;
                string prefix = NamespaceData.Name + ".";
                foreach (KeyValuePair<string, ParserFunction> elem in vars)
                {
                    string key = elem.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ?
                        elem.Key.Substring(prefix.Length) : elem.Key;
                    m_VarMap[key] = elem.Value;
                }
            }

        }
        internal List<CustomFunction> Children
        {
            get;set;
        }
        protected override Variable Evaluate(ParsingScript script)
        {

            List<Variable> args = Constants.FUNCT_WITH_SPACE.Contains(m_name) ?
                // Special case of extracting args.
                Utils.GetFunctionArgsAsStrings(script) :
                script.GetFunctionArgs();

            Utils.ExtractParameterNames(args, m_name, script);

            script.MoveBackIf(Constants.START_GROUP);

            m_args ??= new string[0];
            if (args.Count + m_defaultArgs.Count < m_args.Length)
             {
                ThrowErrorManerger.OnThrowError("この関数は、最大で"+(args.Count+m_defaultArgs.Count)+"個の引数を受け取ることができますが、"+m_args.Length+"個の引数が渡されました",Exceptions.TOO_MANY_ARGUREMENTS,script);
                return Variable.EmptyInstance;
             }
            Variable result = Run(args, script);
            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = child.Evaluate(script);
                }
            }
            return result;
        }
        public Variable GetVariable(ParsingScript script,Variable current)
        {
            List<Variable> args = Constants.FUNCT_WITH_SPACE.Contains(m_name) ?
             // Special case of extracting args.
             Utils.GetFunctionArgsAsStrings(script) :
             script.GetFunctionArgs();

            Utils.ExtractParameterNames(args, m_name, script);

            script.MoveBackIf(Constants.START_GROUP);
            //これはメソッドで呼び出される。そのため[this]代入分として1を足す。
            if (args.Count + m_defaultArgs.Count+1 < m_args.Length)
            {
                ThrowErrorManerger.OnThrowError("この関数は、最大で" + (args.Count + m_defaultArgs.Count+1) + "個の引数を受け取ることができますが、" + m_args.Length + "個の引数が渡されました", Exceptions.TOO_MANY_ARGUREMENTS, script);
                return Variable.EmptyInstance;
            }

            Variable result = Run(args,script,null,current);
            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = child.GetVariable(script,current);
                }
            }
            return result;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            List<Variable> args = Constants.FUNCT_WITH_SPACE.Contains(m_name) ?
                // Special case of extracting args.
                Utils.GetFunctionArgsAsStrings(script) :
                await script.GetFunctionArgsAsync();

            Utils.ExtractParameterNames(args, m_name, script);

            script.MoveBackIf(Constants.START_GROUP);

            if (args.Count + m_defaultArgs.Count < m_args.Length)
            {
                ThrowErrorManerger.OnThrowError("この関数は、最大で" + (args.Count + m_defaultArgs.Count) + "個の引数を受け取ることができますが、" + m_args.Length + "個の引数が渡されました", Exceptions.TOO_MANY_ARGUREMENTS, script);
                return Variable.EmptyInstance;
            }

            Variable result = await RunAsync(args, script);
            //このCustomFunctionに子があればそれも実行する
            if (Children != null)
            {
                foreach (CustomFunction child in Children)
                {
                    result = await child.EvaluateAsync(script);
                }
            }
            return result;
        }

        public Variable Run(List<Variable> args = null, ParsingScript script = null,
                            AliceScriptClass.ClassInstance instance = null,Variable current=null)
        {
            List<KeyValuePair<string, Variable>> args2 = instance == null ? null : instance.GetPropList();
            // 1. Add passed arguments as local variables to the Parser.
            RegisterArguments(args, args2,current);

            // 2. Execute the body of the function.
            Variable result = null;
            ParsingScript tempScript = Utils.GetTempScript(m_body,null, m_name, script,
                                                           m_parentScript, m_parentOffset, instance);
            tempScript.Tag = m_tag;
            tempScript.Variables = m_VarMap;


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
            else
            {
                result.IsReturn = false;
            }

            return result;
        }
        public async Task<Variable> RunAsync(List<Variable> args = null, ParsingScript script = null,
                            AliceScriptClass.ClassInstance instance = null)
        {
            List<KeyValuePair<string, Variable>> args2 = instance == null ? null : instance.GetPropList();
            // 1. Add passed arguments as local variables to the Parser.
            RegisterArguments(args, args2);

            // 2. Execute the body of the function.
            Variable result = null;
            ParsingScript tempScript = Utils.GetTempScript(m_body, null, m_name, script,
                                                           m_parentScript, m_parentOffset, instance);
            tempScript.Tag = m_tag;
            tempScript.Variables = m_VarMap;


            while (tempScript.Pointer < m_body.Length - 1 &&
                  (result == null || !result.IsReturn))
            {
                result = await tempScript.ExecuteAsync();
                tempScript.GoToNextStatement();
            }
            

            if (result == null)
            {
                result = Variable.EmptyInstance;
            }
            else
            {
                result.IsReturn = false;
            }

            return result;
        }

        public static Task<Variable> Run(string functionName,
             Variable arg1 = null, Variable arg2 = null, Variable arg3 = null, ParsingScript script = null)
        {
            CustomFunction customFunction = ParserFunction.GetFunction(functionName, null) as CustomFunction;

            if (customFunction == null)
            {
                return null;
            }

            List<Variable> args = new List<Variable>();
            if (arg1 != null)
            {
                args.Add(arg1);
            }
            if (arg2 != null)
            {
                args.Add(arg2);
            }
            if (arg3 != null)
            {
                args.Add(arg3);
            }

            Variable result = customFunction.Run(args, script);
            return Task.FromResult(result);
        }


        public override ParserFunction NewInstance()
        {
            var newInstance = (CustomFunction)this.MemberwiseClone();
            return newInstance;
        }

        public ParsingScript ParentScript { set { m_parentScript = value; } }
        public int ParentOffset { set { m_parentOffset = value; } }
        public string Body { get { return m_body; } }

        public int ArgumentCount { get { return m_args.Length; } }
        public string Argument(int nIndex) { return m_args[nIndex]; }

        public StackLevel NamespaceData { get; set; }
        public bool IsMethod
        {
            get
            {
                return (m_this!=-1);
            }
        }
        public Variable.VarType MethodRequestType
        {
            get
            {
                if (IsMethod&&m_typArgMap.ContainsKey(m_this))
                {
                    return m_typArgMap[m_this];
                }
                else
                {
                    return Variable.VarType.NONE;
                }
            }
        }

        public int DefaultArgsCount
        {
            get
            {
                return m_defaultArgs.Count;
            }
        }

        public string Header
        {
            get
            {
                return Constants.FUNCTION + " " + Constants.GetRealName(Name) + " " +
                       Constants.START_ARG + string.Join(", ", m_args) +
                       Constants.END_ARG + " " + Constants.START_GROUP;
            }
        }

        protected int m_this = -1;
        protected string m_body;
        protected object m_tag;
        protected string[] m_args;
        protected ParsingScript m_parentScript = null;
        protected int m_parentOffset = 0;
        private List<Variable> m_defaultArgs = new List<Variable>();
        private Dictionary<string, ParserFunction> m_VarMap = new Dictionary<string, ParserFunction>();
        private Dictionary<int, int> m_defArgMap = new Dictionary<int, int>();
        private Dictionary<int, Variable.VarType> m_typArgMap = new Dictionary<int, Variable.VarType>();

        public Dictionary<string, int> ArgMap { get; private set; } = new Dictionary<string, int>();
        public string[] RealArgs { get; private set; }
    }

    internal class StringOrNumberFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // 文字列型かどうか確認
            if (Item.Length > 1)
            {
                bool sq = (Item[0] == Constants.QUOTE1 && Item[Item.Length - 1] == Constants.QUOTE1);
                bool dq = (Item[0] == Constants.QUOTE && Item[Item.Length - 1] == Constants.QUOTE);
                if (dq||sq)
                {
                    //文字列型
                    string result = Item.Substring(1, Item.Length - 2);
                    //[\\]は一時的に0x0011(装置制御1)に割り当てられます
                    result = result.Replace("\\\\", "\u0011");
                    result = result.Replace("\\'", "'");
                    //ダブルクォーテーションで囲まれている場合、より多くのエスケープ文字を認識します
                    if (dq)
                    {
                        result = result.Replace("\\\"", "\"");
                        result = result.Replace("\\n", "\n");
                        result = result.Replace("\\0", "\0");
                        result = result.Replace("\\a", "\a");
                        result = result.Replace("\\b", "\b");
                        result = result.Replace("\\f", "\f");
                        result = result.Replace("\\r", "\r");
                        result = result.Replace("\\t", "\t");
                        result = result.Replace("\\v", "\v");
                        //UTF-16文字コードを文字に置き換えます
                        MatchCollection mc = Regex.Matches(result, @"\\u[0-9a-f]{4}");
                        foreach (Match match in mc)
                        {
                            result = result.Replace(match.Value, ConvertUnicodeToChar(match.Value.TrimStart('\\', 'u')));
                        }
                        //UTF-32文字コードを文字に置き換えます
                        mc = Regex.Matches(result, @"\\U[0-9A-F]{8}");
                        foreach (Match match in mc)
                        {
                            result = result.Replace(match.Value, ConvertUnicodeToChar(match.Value.TrimStart('\\', 'U'), false));
                        }
                    }
                    //[\\]を\に置き換えます(装置制御1から[\]に置き換えます)
                    result = result.Replace("\u0011", "\\");
                    return new Variable(result);
                }
            }
            //Nullとして処理
            if (string.IsNullOrEmpty(Item))
            {
                return Variable.EmptyInstance;
            }
            // 数値として処理
            double num = Utils.ConvertToDouble(Item, script);
            return new Variable(num);
        }

        public string Item { private get; set; }

        private static string ConvertUnicodeToChar(string charCode, bool mode = true)
        {
            if (mode)
            {
                int charCode16 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                char c = Convert.ToChar(charCode16);  // 数値(文字コード) -> 文字
                return c.ToString();
            }
            else
            {
                //UTF-32モード
                int charCode32 = Convert.ToInt32(charCode, 16);  // 16進数文字列 -> 数値
                return Char.ConvertFromUtf32(charCode32);
            }

        }
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

    internal class ConstantsFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            return new Variable(m_name);
        }
    }

    internal class IfStatement : FunctionBase
    {
        public IfStatement()
        {
            this.Name = Constants.IF;

        }
        protected override Variable Evaluate(ParsingScript script)
        {
            Variable result = Interpreter.Instance.ProcessIf(script);
            return result;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            Variable result = await Interpreter.Instance.ProcessIfAsync(script);
            return result;
        }
    }

    internal class ForStatement : FunctionBase
    {
        public ForStatement()
        {
            this.Name = Constants.FOR;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessFor(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessForAsync(script);
        }
    }

    internal class ForeachStatement : FunctionBase
    {
        public ForeachStatement()
        {
            this.Name = Constants.FOREACH;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessForeach(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessForeachAsync(script);
        }
    }

    internal class WhileStatement : FunctionBase
    {
        public WhileStatement()
        {
            this.Name = Constants.WHILE;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessWhile(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await Interpreter.Instance.ProcessWhileAsync(script);
        }
    }

    internal class DoWhileStatement : FunctionBase
    {
        public DoWhileStatement()
        {
            this.Name = Constants.DO;
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessDoWhile(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessDoWhile(script);
        }
    }

    internal class SwitchStatement : FunctionBase
    {
        public SwitchStatement()
        {
            this.Name = Constants.SWITCH;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessSwitch(script);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessSwitch(script);
        }
    }

    internal class CaseStatement : FunctionBase
    {
        public CaseStatement()
        {
            this.Name = Constants.CASE;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            return Interpreter.Instance.ProcessCase(script, Name);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return Interpreter.Instance.ProcessCase(script, Name);
        }
    }

    internal class IncludeFile : FunctionBase
    {
        public IncludeFile()
        {
            this.Name = "include";
            this.MinimumArgCounts = 1;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC;
            this.Run += IncludeFile_Run;
        }

        private void IncludeFile_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script == null)
            {
                e.Script = new ParsingScript("");
            }
            ParsingScript tempScript = e.Script.GetIncludeFileScript(e.Args[0].AsString());

            Variable result = null;
            while (tempScript.StillValid())
            {
                result = tempScript.Execute();
                tempScript.GoToNextStatement();
            }
            if (result == null) { result = Variable.EmptyInstance; }
            e.Return = result;
        }

    }

    // Get a value of a variable or of an array element
    public class GetVarFunction : ParserFunction
    {
        public GetVarFunction(Variable value)
        {
            m_value = value;
        }

        protected override Variable Evaluate(ParsingScript script)
        {
            // First check if this element is part of an array:
            if (script.TryPrev() == Constants.START_ARRAY)
            {

                //配列添え字演算子を使用できないケースではじく処理を記述
                switch (m_value.Type)
                {
                    case Variable.VarType.ARRAY:
                        {
                            if (m_value.Tuple == null || m_value.Tuple.Count == 0)
                            {
                                throw new ArgumentException("指定された配列には要素がありません");
                            }
                            break;
                        }

                    case Variable.VarType.DELEGATE:
                        {
                            if (m_value.Delegate == null || m_value.Delegate.Length == 0)
                            {
                                throw new ArgumentException("指定されたデリゲートには要素がありません");
                            }
                            break;
                        }
                    case Variable.VarType.STRING:
                        {
                            if (string.IsNullOrEmpty(m_value.String))
                            {
                                throw new ArgumentException("指定された文字列は空です");
                            }
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("指定された変数で、配列添え字演算子を使用することができません");
                        }
                }

                if (m_arrayIndices == null)
                {
                    string startName = script.Substr(script.Pointer - 1);
                    m_arrayIndices = Utils.GetArrayIndices(script, startName, m_delta, (newStart, newDelta) => { startName = newStart; m_delta = newDelta; });
                }

                script.Forward(m_delta);
                while (script.MoveForwardIf(Constants.END_ARRAY))
                {
                }

                Variable result = Utils.ExtractArrayElement(m_value, m_arrayIndices, script);
                if (script.Prev == '.')
                {
                    script.Backward();
                }

                if (script.TryCurrent() != '.')
                {
                    return result;
                }
                script.Forward();

                m_propName = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
                Variable propValue = result.GetProperty(m_propName, script);
                Utils.CheckNotNull(propValue, m_propName, script);
                return propValue;
            }

            // Now check that this is an object:
            if (!string.IsNullOrWhiteSpace(m_propName))
            {
                string temp = m_propName;
                m_propName = null; // Need this to reset for recursive calls
                Variable propValue = m_value.Type == Variable.VarType.ENUM ?
                                     m_value.GetEnumProperty(temp, script) :
                                     m_value.GetProperty(temp, script);
                Utils.CheckNotNull(propValue, temp, script);
                return EvaluateFunction(propValue, script, m_propName);
            }

            // Otherwise just return the stored value.
            return m_value;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            // First check if this element is part of an array:
            if (script.TryPrev() == Constants.START_ARRAY)
            {
                //配列添え字演算子を使用できないケースではじく処理を記述
                switch (m_value.Type)
                {
                    case Variable.VarType.ARRAY:
                        {
                            if (m_value.Tuple == null || m_value.Tuple.Count == 0)
                            {
                                throw new ArgumentException("指定された配列には要素がありません");
                            }
                            break;
                        }

                    case Variable.VarType.DELEGATE:
                        {
                            if (m_value.Delegate == null || m_value.Delegate.Length == 0)
                            {
                                throw new ArgumentException("指定されたデリゲートには要素がありません");
                            }
                            break;
                        }
                    case Variable.VarType.STRING:
                        {
                            if (string.IsNullOrEmpty(m_value.String))
                            {
                                throw new ArgumentException("指定された文字列は空です");
                            }
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("指定された変数で、配列添え字演算子を使用することができません");
                        }
                }

                if (m_arrayIndices == null)
                {
                    string startName = script.Substr(script.Pointer - 1);
                    m_arrayIndices = await Utils.GetArrayIndicesAsync(script, startName, m_delta, (newStart, newDelta) => { startName = newStart; m_delta = newDelta; });
                }

                script.Forward(m_delta);
                while (script.MoveForwardIf(Constants.END_ARRAY))
                {
                }

                Variable result = Utils.ExtractArrayElement(m_value, m_arrayIndices, script);
                if (script.Prev == '.')
                {
                    script.Backward();
                }
                if (script.TryCurrent() != '.')
                {
                    return result;
                }

                script.Forward();
                m_propName = Utils.GetToken(script, Constants.NEXT_OR_END_ARRAY);
                Variable propValue = await result.GetPropertyAsync(m_propName, script);
                Utils.CheckNotNull(propValue, m_propName, script);
                return propValue;
            }

            // Now check that this is an object:
            if (!string.IsNullOrWhiteSpace(m_propName))
            {
                string temp = m_propName;
                m_propName = null; // Need this to reset for recursive calls

                Variable propValue = m_value.Type == Variable.VarType.ENUM ?
                         m_value.GetEnumProperty(temp, script) :
                         await m_value.GetPropertyAsync(temp, script);
                Utils.CheckNotNull(propValue, temp, script);
                return await EvaluateFunctionAsync(propValue, script, m_propName);
            }

            // Otherwise just return the stored value.
            return m_value;
        }

        public static Variable EvaluateFunction(Variable var, ParsingScript script, string m_propName)
        {
            if (var != null && var.CustomFunctionGet != null)
            {
                List<Variable> args = script.Prev == '(' ? script.GetFunctionArgs() : new List<Variable>();
                if (var.StackVariables != null)
                {
                    args.AddRange(var.StackVariables);
                }
                return var.CustomFunctionGet.Run(args, script);
            }
            if (var != null && !string.IsNullOrWhiteSpace(var.CustomGet))
            {
                return ParsingScript.RunString(var.CustomGet);
            }
            return var;
        }

        public static async Task<Variable> EvaluateFunctionAsync(Variable var, ParsingScript script, string m_propName)
        {
            if (var.CustomFunctionGet != null)
            {
                List<Variable> args = script.Prev == '(' ? await script.GetFunctionArgsAsync() : new List<Variable>();
                if (var.StackVariables != null)
                {
                    args.AddRange(var.StackVariables);
                }
                return await var.CustomFunctionGet.RunAsync(args, script);
            }
            if (!string.IsNullOrWhiteSpace(var.CustomGet))
            {
                return ParsingScript.RunString(var.CustomGet);
            }
            return var;
        }

        public int Delta
        {
            set { m_delta = value; }
        }
        public Variable Value
        {
            get { return m_value; }
        }
        public List<Variable> Indices
        {
            set { m_arrayIndices = value; }
        }
        public string PropertyName
        {
            set { m_propName = value; }
        }

        internal Variable m_value;
        private int m_delta = 0;
        private List<Variable> m_arrayIndices = null;
        private string m_propName;
    }

    internal class IncrementDecrementFunction : ActionFunction, INumericFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            bool prefix = string.IsNullOrWhiteSpace(m_name);
            if (prefix)
            {// If it is a prefix we do not have the variable name yet.
                Name = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            }

            Utils.CheckForValidName(Name, script);

            // Value to be added to the variable:
            int valueDelta = m_action == Constants.INCREMENT ? 1 : -1;
            int returnDelta = prefix ? valueDelta : 0;

            // Check if the variable to be set has the form of x[a][b],
            // meaning that this is an array element.
            double newValue = 0;
            List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

            ParserFunction func = ParserFunction.GetVariable(m_name, script);
            Utils.CheckNotNull(m_name, func, script);

            Variable currentValue = func.GetValue(script);
            currentValue = currentValue.DeepClone();

            if (arrayIndices.Count > 0 || script.TryCurrent() == Constants.START_ARRAY)
            {
                if (prefix)
                {
                    string tmpName = m_name + script.Rest;
                    int delta = 0;
                    arrayIndices = Utils.GetArrayIndices(script, tmpName, delta, (string t, int d) => { tmpName = t; delta = d; });
                    script.Forward(Math.Max(0, delta - tmpName.Length));
                }

                Variable element = Utils.ExtractArrayElement(currentValue, arrayIndices, script);
                script.MoveForwardIf(Constants.END_ARRAY);

                newValue = element.Value + returnDelta;
                element.Value += valueDelta;
            }
            else
            { // A normal variable.
                newValue = currentValue.Value + returnDelta;
                currentValue.Value += valueDelta;
            }

            ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                    new GetVarFunction(currentValue), script);
            return new Variable(newValue);
        }

        override public ParserFunction NewInstance()
        {
            return new IncrementDecrementFunction();
        }
    }

    internal class OperatorAssignFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            // Value to be added to the variable:
            Variable right = Utils.GetItem(script);

            List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

            ParserFunction func = ParserFunction.GetVariable(m_name, script);
            if(!Utils.CheckNotNull(func, m_name, script))
            {
                return Variable.EmptyInstance;
            }

            Variable currentValue = func.GetValue(script);
            currentValue = currentValue.DeepClone();
            Variable left = currentValue;

            if (arrayIndices.Count > 0)
            {// array element
                left = Utils.ExtractArrayElement(currentValue, arrayIndices, script);
                script.MoveForwardIf(Constants.END_ARRAY);
            }
            if (m_action == "??=")
            {
                if (left.IsNull())
                {
                    return right;
                }
                else
                {
                    return left;
                }
            }
            else if (left.Type == Variable.VarType.NUMBER)
            {
                NumberOperator(left, right, m_action);
            }
            else if (left.Type == Variable.VarType.ARRAY)
            {
                ArrayOperator(left, right, m_action, script);
            }
            else if (left.Type == Variable.VarType.DELEGATE)
            {
                DelegateOperator(left, right, m_action, script);
            }
            else if (left.Type == Variable.VarType.OBJECT && left.Object is ObjectBase obj)
            {
                obj.Operator(left, right, m_action, script);
            }
            else
            {
                StringOperator(left, right, m_action);
            }

            if (arrayIndices.Count > 0)
            {// array element
                AssignFunction.ExtendArray(currentValue, arrayIndices, 0, left);
                ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                         new GetVarFunction(currentValue), script);
            }
            else
            {
                ParserFunction.AddGlobalOrLocalVariable(m_name,
                                                         new GetVarFunction(left), script);
            }
            return left;
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
                        if (valueB.Type == Variable.VarType.ARRAY)
                        {
                            valueA.Tuple.AddRange(valueB.Tuple);
                        }
                        else
                        {
                            valueA.Tuple.Add(valueB);
                        }
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

    internal class AssignFunction : ActionFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            return Assign(script, m_name);
        }

        public Variable Assign(ParsingScript script, string varName, bool localIfPossible = false,bool registVar=false,bool registConst=false,ParsingScript baseScript=null,bool isGlobal=false)
        {
            m_name = Constants.GetRealName(varName);
            script.CurrentAssign = m_name;
            Variable varValue = Utils.GetItem(script);

            baseScript ??= script;

            script.MoveBackIfPrevious(Constants.END_ARG);
            varValue.TrySetAsMap();

            if (script.Current == ' ' || script.Prev == ' ')
            {
                Utils.ThrowErrorMsg("[" + script.Rest + "]は無効なトークンです", Exceptions.INVALID_TOKEN,
                                    script, m_name);
            }
            if (registConst)
            {
                //定数定義
                if (!FunctionExists(m_name,script))
                {
                    // Check if the variable to be set has the form of x[a][b]...,
                    // meaning that this is an array element.
                    List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

                    if (arrayIndices.Count == 0)
                    {
                        baseScript.Consts.Add(m_name,new GetVarFunction(varValue));
                        Variable retVar = varValue.DeepClone();
                        retVar.CurrentAssign = m_name;
                        return retVar;
                    }

                    Variable array;

                    ParserFunction pf = ParserFunction.GetVariable(m_name, script);
                    array = pf != null ? (pf.GetValue(script)) : new Variable();

                    ExtendArray(array, arrayIndices, 0, varValue);
                    if (isGlobal)
                    {
                        Constants.CONSTS.Add(m_name,varValue);
                    }
                    else
                    {
                        baseScript.Consts.Add(m_name, new GetVarFunction(varValue));
                    }
                    return array;
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("定数に値を代入することはできません",Exceptions.CANT_ASSIGN_VALUE_TO_CONSTANT,script);
                }
                return Variable.EmptyInstance;
            }
            else
            {
                // First try processing as an object (with a dot notation):
                Variable result = ProcessObject(script, varValue);
                if (result != null)
                {
                    if (script.CurrentClass == null && script.ClassInstance == null)
                    {

                        ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(result), baseScript, localIfPossible,registVar,isGlobal);
                    }
                    return result;
                }

                // Check if the variable to be set has the form of x[a][b]...,
                // meaning that this is an array element.
                List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

                if (arrayIndices.Count == 0)
                {
                    ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(varValue), baseScript, localIfPossible,registVar,isGlobal);
                    Variable retVar = varValue.DeepClone();
                    retVar.CurrentAssign = m_name;
                    return retVar;
                }

                Variable array;

                ParserFunction pf = ParserFunction.GetVariable(m_name, baseScript);
                array = pf != null ? (pf.GetValue(script)) : new Variable();

                ExtendArray(array, arrayIndices, 0, varValue);

                ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(array), baseScript, localIfPossible,registVar,isGlobal);
                return array;
            }
        }

        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            return await AssignAsync(script, m_name);
        }

        public async Task<Variable> AssignAsync(ParsingScript script, string varName, bool localIfPossible = false,bool registVar=false,bool registConst=false,ParsingScript baseScript=null,bool isGlobal=false)
        {
            m_name = Constants.GetRealName(varName);
            script.CurrentAssign = m_name;
            Variable varValue = Utils.GetItem(script);

            baseScript ??= script;

            script.MoveBackIfPrevious(Constants.END_ARG);
            varValue.TrySetAsMap();

            if (script.Current == ' ' || script.Prev == ' ')
            {
                Utils.ThrowErrorMsg("[" + script.Rest + "]は無効なトークンです", Exceptions.INVALID_TOKEN,
                                    script, m_name);
            }
            if (registConst)
            {
                //定数定義
                if (!FunctionExists(m_name, script))
                {
                    // Check if the variable to be set has the form of x[a][b]...,
                    // meaning that this is an array element.
                    List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

                    if (arrayIndices.Count == 0)
                    {
                        baseScript.Consts.Add(m_name, new GetVarFunction(varValue));
                        Variable retVar = varValue.DeepClone();
                        retVar.CurrentAssign = m_name;
                        return retVar;
                    }

                    Variable array;

                    ParserFunction pf = ParserFunction.GetVariable(m_name, script);
                    array = pf != null ? (pf.GetValue(script)) : new Variable();

                    ExtendArray(array, arrayIndices, 0, varValue);
                    if (isGlobal)
                    {
                        Constants.CONSTS.Add(m_name, varValue);
                    }
                    else
                    {
                        baseScript.Consts.Add(m_name, new GetVarFunction(varValue));
                    }
                    return array;
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("定数に値を代入することはできません", Exceptions.CANT_ASSIGN_VALUE_TO_CONSTANT, script);
                }
                return Variable.EmptyInstance;
            }
            else
            {
                // First try processing as an object (with a dot notation):
                Variable result =await ProcessObjectAsync(script, varValue);
                if (result != null)
                {
                    if (script.CurrentClass == null && script.ClassInstance == null)
                    {

                        ParserFunction.AddGlobalOrLocalVariable(m_name, new GetVarFunction(result), baseScript, localIfPossible, registVar, isGlobal);
                    }
                    return result;
                }

                // Check if the variable to be set has the form of x[a][b]...,
                // meaning that this is an array element.
                List<Variable> arrayIndices = Utils.GetArrayIndices(script, m_name, (string name) => { m_name = name; });

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

        private Variable ProcessObject(ParsingScript script, Variable varValue)
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

            int ind = varName.IndexOf('.');
            if (ind <= 0)
            {
                return null;
            }

            Utils.CheckForValidName(varName, script);

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

        private async Task<Variable> ProcessObjectAsync(ParsingScript script, Variable varValue)
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
                await script.ClassInstance.SetProperty(m_name, varValue);
                return varValue.DeepClone();
            }

            int ind = varName.IndexOf('.');
            if (ind <= 0)
            {
                return null;
            }

            Utils.CheckForValidName(varName, script);

            string name = varName.Substring(0, ind);
            string prop = varName.Substring(ind + 1);

            if (ParserFunction.TryAddToNamespace(prop, name, varValue))
            {
                return varValue.DeepClone();
            }

            ParserFunction existing = ParserFunction.GetVariable(name, script);
            Variable baseValue = existing != null ? await existing.GetValueAsync(script) : new Variable(Variable.VarType.ARRAY);
            await baseValue.SetPropertyAsync(prop, varValue, script, name);

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

    internal class AddVariablesToHashFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            string varName = Utils.GetSafeString(args, 0);
            Variable lines = Utils.GetSafeVariable(args, 1);
            int fromLine = Utils.GetSafeInt(args, 2);
            string hash2 = Utils.GetSafeString(args, 3);
            string sepStr = Utils.GetSafeString(args, 4, "\t");
            if (sepStr == "\\t")
            {
                sepStr = "\t";
            }
            char[] sep = sepStr.ToCharArray();

            var function = ParserFunction.GetVariable(varName, script);
            Variable mapVar = function != null ? function.GetValue(script) :
                                        new Variable(Variable.VarType.ARRAY);

            for (int counter = fromLine; counter < lines.Tuple.Count; counter++)
            {
                Variable lineVar = lines.Tuple[counter];
                Variable toAdd = new Variable(counter - fromLine);
                string line = lineVar.AsString();
                var tokens = line.Split(sep);
                string hash = tokens[0];
                mapVar.AddVariableToHash(hash, toAdd);
                if (!string.IsNullOrWhiteSpace(hash2) &&
                    !hash2.Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    mapVar.AddVariableToHash(hash2, toAdd);
                }
            }

            ParserFunction.AddGlobalOrLocalVariable(varName,
                                              new GetVarFunction(mapVar), script);
            return Variable.EmptyInstance;
        }
    }

    internal class AddVariableToHashFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name);

            string varName = Utils.GetSafeString(args, 0);
            Variable toAdd = Utils.GetSafeVariable(args, 1);
            string hash = Utils.GetSafeString(args, 2);

            var function = ParserFunction.GetVariable(varName, script);
            Variable mapVar = function != null ? function.GetValue(script) :
                                        new Variable(Variable.VarType.ARRAY);

            mapVar.AddVariableToHash(hash, toAdd);
            for (int i = 3; i < args.Count; i++)
            {
                string hash2 = Utils.GetSafeString(args, 3);
                if (!string.IsNullOrWhiteSpace(hash2) &&
                    !hash2.Equals(hash, StringComparison.OrdinalIgnoreCase))
                {
                    mapVar.AddVariableToHash(hash2, toAdd);
                }
            }

            ParserFunction.AddGlobalOrLocalVariable(varName,
                                                new GetVarFunction(mapVar), script);

            return Variable.EmptyInstance;
        }
    }

    internal class DefineLocalFunction : ParserFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string varName = Utils.GetSafeString(args, 0);
            Variable currentValue = Utils.GetSafeVariable(args, 1);

            if (currentValue == null)
            {
                currentValue = new Variable("");
            }

            if (script.StackLevel != null)
            {
                ParserFunction.AddLocalVariable(new GetVarFunction(currentValue), script,varName);
            }
            else if (script.CurrentClass != null)
            {
                Utils.ThrowErrorMsg(m_name + "をクラス内で定義することはできません", Exceptions.COULDNT_DEFINE_IN_CLASS,
                                    script, m_name);
            }
            else
            {
                string scopeName = Path.GetFileName(script.Filename);
                ParserFunction.AddLocalScopeVariable(varName, scopeName,
                                                     new GetVarFunction(currentValue));
            }

            return currentValue;
        }
    }

    internal class GetPropertiesFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name, true);

            Variable baseValue = args[0];
            List<Variable> props = baseValue.GetProperties();
            return new Variable(props);
        }
    }

    internal class GetPropertyFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            Variable baseValue = args[0];
            string propName = Utils.GetSafeString(args, 1);

            Variable propValue = baseValue.GetProperty(propName, script);
            Utils.CheckNotNull(propValue, propName, script);

            return new Variable(propValue);
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            List<Variable> args = await script.GetFunctionArgsAsync();
            Utils.CheckArgs(args.Count, 2, m_name, true);

            Variable baseValue = args[0];
            string propName = Utils.GetSafeString(args, 1);

            Variable propValue = await baseValue.GetPropertyAsync(propName, script);
            Utils.CheckNotNull(propValue, propName, script);

            return new Variable(propValue);
        }
        public static Variable GetProperty(ParsingScript script, string sPropertyName)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, "GetProperty", true);

            Variable baseValue = args[0];

            Variable propValue = baseValue.GetProperty(sPropertyName, script);
            Utils.CheckNotNull(propValue, sPropertyName, script);

            return new Variable(propValue);
        }
        public static async Task<Variable> GetPropertyAsync(ParsingScript script, string sPropertyName)
        {
            List<Variable> args = await script.GetFunctionArgsAsync();
            Utils.CheckArgs(args.Count, 1, "GetProperty", true);

            Variable baseValue = args[0];

            Variable propValue = await baseValue.GetPropertyAsync(sPropertyName, script);
            Utils.CheckNotNull(propValue, sPropertyName, script);

            return new Variable(propValue);
        }
    }

    internal class SetPropertyFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 3, m_name, true);

            Variable baseValue = args[0];
            string propName = Utils.GetSafeString(args, 1);
            Variable propValue = Utils.GetSafeVariable(args, 2);

            Variable result = baseValue.SetProperty(propName, propValue, script);

            ParserFunction.AddGlobalOrLocalVariable(baseValue.ParsingToken,
                                                    new GetVarFunction(baseValue), script);
            return result;
        }
        protected override async Task<Variable> EvaluateAsync(ParsingScript script)
        {
            List<Variable> args = await script.GetFunctionArgsAsync();
            Utils.CheckArgs(args.Count, 3, m_name, true);

            Variable baseValue = args[0];
            string propName = Utils.GetSafeString(args, 1);
            Variable propValue = Utils.GetSafeVariable(args, 2);

            Variable result = await baseValue.SetPropertyAsync(propName, propValue, script);

            ParserFunction.AddGlobalOrLocalVariable(baseValue.ParsingToken,
                                                    new GetVarFunction(baseValue), script);
            return result;
        }

        public static Variable SetProperty(ParsingScript script, string sPropertyName)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, "SetProperty", true);

            Variable baseValue = args[0];
            Variable propValue = Utils.GetSafeVariable(args, 1);

            Variable result = baseValue.SetProperty(sPropertyName, propValue, script);

            ParserFunction.AddGlobalOrLocalVariable(baseValue.ParsingToken,
                                                    new GetVarFunction(baseValue), script);
            return result;
        }
        public static async Task<Variable> SetPropertyAsync(ParsingScript script, string sPropertyName)
        {
            List<Variable> args = await script.GetFunctionArgsAsync();
            Utils.CheckArgs(args.Count, 2, "SetProperty", true);

            Variable baseValue = args[0];
            Variable propValue = Utils.GetSafeVariable(args, 1);

            Variable result = await baseValue.SetPropertyAsync(sPropertyName, propValue, script);

            ParserFunction.AddGlobalOrLocalVariable(baseValue.ParsingToken,
                                                    new GetVarFunction(baseValue), script);
            return result;
        }
    }

    internal class CancelFunction : ParserFunction
    {
        public static bool Canceled { get; set; }

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 0, m_name, true);

            bool mode = Utils.GetSafeInt(args, 0, 1) == 1;
            Canceled = mode;

            return new Variable(Canceled);
        }
    }
    public class SingletonFunction : ParserFunction
    {
        private static Dictionary<string, Variable> m_singletons =
           new Dictionary<string, Variable>();

        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 1, m_name);

            string expr = args[0].AsString();
            Dictionary<int, int> char2Line;
            expr = Utils.ConvertToScript(expr, out char2Line);

            Variable result;
            if (m_singletons.TryGetValue(expr, out result))
            {
                return result;
            }

            ParsingScript tempScript = new ParsingScript(expr);
            result = tempScript.Execute();

            m_singletons[expr] = result;

            return result;
        }
    }

    internal class GetColumnFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            Utils.CheckArgs(args.Count, 2, m_name);

            Variable arrayVar = Utils.GetSafeVariable(args, 0);
            int col = Utils.GetSafeInt(args, 1);
            int fromCol = Utils.GetSafeInt(args, 2, 0);

            var tuple = arrayVar.Tuple;

            List<Variable> result = new List<Variable>(tuple.Count);
            for (int i = fromCol; i < tuple.Count; i++)
            {
                Variable current = tuple[i];
                if (current.Tuple == null || current.Tuple.Count <= col)
                {
                    throw new ArgumentException(m_name + ": Index [" + col + "] doesn't exist in column " +
                                                i + "/" + (tuple.Count - 1));
                }
                result.Add(current.Tuple[col]);
            }

            return new Variable(result);
        }
    }

    internal class GetAllKeysFunction : ParserFunction, IArrayFunction
    {
        protected override Variable Evaluate(ParsingScript script)
        {
            Variable varName = Utils.GetItem(script);
            Utils.CheckNotNull(varName, m_name, script);

            List<Variable> results = varName.GetAllKeys();

            return new Variable(results);
        }
    }


}
