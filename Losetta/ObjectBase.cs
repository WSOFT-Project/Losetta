namespace AliceScript
{
    public class ObjectBase : AliceScriptClass, IComparable, ScriptObject
    {

        public Dictionary<string, PropertyBase> Properties
        {
            get => m_classProperties;
            set => m_classProperties = value;
        }
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
            var tsf = Functions.Keys.Where(x => x.ToLower() == "tostring").FirstOrDefault();
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
            var obase = (ObjectBase)Activator.CreateInstance(GetType());
            obase.Namespace = Namespace;
            return new Variable(obase);
        }

        public virtual List<string> GetProperties()
        {
            List<string> v = new List<string>(Properties.Keys);
            v.AddRange(new List<string>(Functions.Keys));
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
        public virtual int CompareTo(object? other)
        {
            return 0;
        }
        public virtual Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());

            var prop = GetPropertyBase(sPropertyName);
            if (prop != null)
            {
                return Task.FromResult(prop.Property);
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

        public virtual PropertyBase GetPropertyBase(string sPropertyName)
        {
            return Properties.ContainsKey(sPropertyName) ? Properties[sPropertyName] : null;
        }

        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {

            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            var prop = GetPropertyBase(sPropertyName);
            if (prop != null)
            {
                prop.Property = argValue;
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
        public void AddProperty(PropertyBase property)
        {
            Properties.Add(property.Name, property);
        }
        public void RemoveProperty(PropertyBase property)
        {
            Properties.Remove(property.Name);
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


    public class ObjectBaseManerger
    {
        public static void AddObject(ObjectBase obj)
        {
            if (obj != null)
            {
                ParserFunction.RegisterFunction(obj.Name, new GetVarFunction(new Variable(obj)), true);
            }
        }
    }

    public class PropertyBaseEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティの変数の内容
        /// </summary>
        public Variable Value { get; set; }

        /// <summary>
        /// 呼び出し元の変数。これはコアプロパティで使用します。
        /// </summary>
        public Variable Parent { get; set; }
    }
    public delegate void PropertySettingEventHandler(object sender, PropertyBaseEventArgs e);

    public delegate void PropertyGettingEventHandler(object sender, PropertyBaseEventArgs e);

    public class PropertyBase
    {
        /// <summary>
        /// このプロパティの名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// TrueにするとSettingイベントおよびGettingイベントが発生します
        /// </summary>

        public bool HandleEvents { get; set; }
        /// <summary>
        /// プロパティに存在する変数。このプロパティはHandleEventsがTrueの場合には使用されません
        /// </summary>

        public Variable Value { get; set; }

        /// <summary>
        /// プロパティに変数が代入されるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>
        public event PropertySettingEventHandler Setting;
        /// <summary>
        /// プロパティから変数が読みだされるときに発生するイベント。このイベントはHandleEventsがTrueの場合のみ発生します
        /// </summary>

        public event PropertyGettingEventHandler Getting;

        /// <summary>
        /// プロパティはこの型向けのプロパティとして登録されている。これはコアプロパティで使用します。
        /// </summary>
        public Variable.VarType Type { get; set; }

        /// <summary>
        /// SetPropertyが使用可能かを表す値。デフォルトではTrueです。
        /// </summary>
        public bool CanSet
        {
            get => m_CanSet;
            set => m_CanSet = value;
        }
        private bool m_CanSet = true;
        public PropertyBase(Variable value)
        {
            Value = value;
        }
        public PropertyBase()
        {

        }
        public Variable GetProperty(Variable parent = null)
        {
            if (HandleEvents)
            {
                PropertyBaseEventArgs e = new PropertyBaseEventArgs();
                e.Parent = parent;
                e.Value = Value;
                Getting?.Invoke(this, e);
                return e.Value;
            }
            else
            {
                return Value;
            }
        }
        public void SetProperty(Variable value, Variable parent = null)
        {
            if (CanSet)
            {
                if (HandleEvents)
                {
                    PropertyBaseEventArgs e = new PropertyBaseEventArgs();
                    e.Parent = parent;
                    e.Value = value;
                    Setting?.Invoke(this, e);
                }
                else
                {
                    Value = value;
                }
            }
            else
            {
                throw new ScriptException("このプロパティには代入できません", Exceptions.COULDNT_ASSIGN_THIS_PROPERTY);
            }
        }
        public Variable Property
        {
            get => GetProperty();
            set => SetProperty(value);
        }

    }
}
