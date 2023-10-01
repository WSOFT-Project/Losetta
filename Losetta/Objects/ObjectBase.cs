using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    public class ObjectBase : AliceScriptClass, IComparable, ScriptObject
    {
        public Dictionary<string, FunctionBase> Functions
        {
            get => m_customFunctions;
            set => m_customFunctions = value;
        }
        public Dictionary<string, Variable> Events = new Dictionary<string, Variable>();

        private bool m_handle_operator = false;
        private FunctionBase m_constructor = null;

        public FunctionBase Constructor
        {
            get => m_constructor;
            set => m_constructor = value;
        }

        /// <summary>
        /// Operator関数を上書きするかどうかを表す値
        /// </summary>
        public bool HandleOperator
        {
            get => m_handle_operator;
            set => m_handle_operator = value;
        }

        public ObjectBase(string name = "")
        {
            Name = name;
        }


        public override string ToString()
        {
            var tsf = Functions.Keys.Where(x => x.ToLowerInvariant() == "tostring").FirstOrDefault();
            return tsf != null
                ? Functions[tsf].Evaluate(new List<Variable>(), null, null).AsString()
                : string.IsNullOrEmpty(Namespace) ? Name : Namespace + "." + Name;
        }

        public virtual Variable GetImplementation(List<Variable> args, ParsingScript script)
        {
            if (m_constructor != null)
            {
                var impl = m_constructor.Evaluate(args, script);
                if (impl.Type == Variable.VarType.OBJECT && impl.Object is ObjectBase ob)
                {
                    ob.Namespace = Namespace;
                    return impl;
                }
            }
            var obase = (ObjectBase)this.MemberwiseClone();  //(ObjectBase)Activator.CreateInstance(GetType());
            obase.Namespace = Namespace;
            return new Variable(obase);
        }

        public virtual List<string> GetProperties()
        {
            List<string> v = new List<string>(Functions.Keys);
            v.AddRange(new List<string>(Events.Keys));
            return v;
        }
        internal static bool GETTING = false;
        public static List<Variable> LaskVariable;
        public virtual bool Equals(ObjectBase other)
        {
            return other == this;
        }
        public virtual Variable Operator(Variable left, Variable right, string action, ParsingScript script)
        {
            //継承先によって定義されます
            throw new NotImplementedException();
        }
        public virtual int CompareTo(object other)
        {
            return 0;
        }
        public virtual Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());

            var prop = GetValueFunction(sPropertyName);
            if (prop != null)
            {
                return Task.FromResult(prop.Value);
            }
            else
            {
                if (Functions.ContainsKey(sPropertyName))
                {

                    //issue#1「ObjectBase内の関数で引数が認識されない」に対する対処
                    //原因:先に値検出関数にポインタが移動されているため正常に引数が認識できていない
                    //対処:値検出関数で拾った引数のリストをバックアップし、関数で使用する
                    //ただしこれは、根本的な解決にはなっていない可能性がある
                    GETTING = true;

                    Task<Variable> va = Task.FromResult(Functions[sPropertyName].GetValue(script));
                    GETTING = false;
                    return va;

                }
                else
                {
                    return Events.ContainsKey(sPropertyName)
                        ? Task.FromResult(Events[sPropertyName])
                        : throw new ScriptException("指定されたプロパティまたはメソッドまたはイベントは存在しません。", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script);
                }
            }
        }

        public virtual ValueFunction GetValueFunction(string sPropertyName)
        {
            return Functions.TryGetValue(sPropertyName, out var f) && f is ValueFunction vf ? vf : null;
        }

        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {

            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            var prop = GetValueFunction(sPropertyName);
            if (prop != null)
            {
                prop.Value = argValue;
            }
            else if (Events.ContainsKey(sPropertyName))
            {
                if (argValue.Type == Variable.VarType.DELEGATE && argValue.Delegate != null)
                {
                    Events[sPropertyName] = argValue;
                }
            }
            else
            {
                throw new ScriptException("指定されたプロパティまたはデリゲートは存在しません", Exceptions.COULDNT_FIND_VARIABLE);
            }

            return Task.FromResult(Variable.EmptyInstance);
        }
        public void AddFunction(FunctionBase function, string name = "")
        {
            if (string.IsNullOrEmpty(name)) { name = function.Name; }
            Functions.Add(name, function);
        }
        public void RemoveFunction(string name)
        {
            Functions.Remove(name);
        }
        public void RemoveFunction(FunctionBase function)
        {
            Functions.Remove(function.Name);
        }


    }
    public class ObjectBaseManager
    {
        public static void AddObject(ObjectBase obj)
        {
            if (obj != null)
            {
                ParserFunction.RegisterFunction(obj.Name, new ValueFunction(new Variable(obj)), true);
            }
        }
    }
}
