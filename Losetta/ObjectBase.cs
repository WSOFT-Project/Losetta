using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    public class ObjectBase : ScriptObject
    {
        /// <summary>
        /// このオブジェクトの名前
        /// </summary>
        public string Name { get; set; }

        public Dictionary<string, PropertyBase> Properties = new Dictionary<string, PropertyBase>();
        public Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>();
        public Dictionary<string, Variable> Events = new Dictionary<string, Variable>();

        public ObjectBase(string name = "")
        {
            Name = name;
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

        public virtual Variable Operator(Variable left, Variable right, string action, ParsingScript script)
        {
            //継承先によって定義されます
            throw new NotImplementedException();
        }
        public virtual Task<Variable> GetProperty(string sPropertyName, List<Variable> args = null, ParsingScript script = null)
        {
            sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
            if (Properties.ContainsKey(sPropertyName))
            {
               return Task.FromResult(Properties[sPropertyName].Property);
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
                else if (Events.ContainsKey(sPropertyName))
                {
                    return Task.FromResult(Events[sPropertyName]);
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("指定されたプロパティまたはメソッドまたはイベントは存在しません。", Exceptions.PROPERTY_OR_METHOD_NOT_FOUND, script);
                    return Task.FromResult(Variable.EmptyInstance);
                }
            }
        }

        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {
           
                sPropertyName = Variable.GetActualPropertyName(sPropertyName, GetProperties());
                if (Properties.ContainsKey(sPropertyName))
                {
                    Properties[sPropertyName].Property = argValue;
                }
                else if (Events.ContainsKey(sPropertyName))
                {
                    if (argValue.Type==Variable.VarType.DELEGATE && argValue.Delegate != null)
                    {
                        Events[sPropertyName] = argValue;
                    }
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("指定されたプロパティまたはデリゲートは存在しません",Exceptions.COULDNT_FIND_VARIABLE);
                }
            
            return Task.FromResult(Variable.EmptyInstance);
        }
      public void AddProperty(PropertyBase property)
        {
            this.Properties.Add(property.Name,property);
        }
        public void RemoveProperty(PropertyBase property)
        {
            this.Properties.Remove(property.Name);
        }
        public void AddFunction(FunctionBase function,string name="")
        {
            if (string.IsNullOrEmpty(name)) { name = function.Name; }
            this.Functions.Add(name,function);
        }
        public void RemoveFunction(string name)
        {
            this.Functions.Remove(name);
        }
        public void RemoveFunction(FunctionBase function)
        {
            this.Functions.Remove(function.Name);
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

    public class PropertySettingEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティに代入されようとしている変数の内容
        /// </summary>
        public Variable Value { get; set; }

    }
    public class PropertyGettingEventArgs : EventArgs
    {
        /// <summary>
        /// プロパティの変数の内容
        /// </summary>
        public Variable Value { get; set; }
    }
    public delegate void PropertySettingEventHandler(object sender, PropertySettingEventArgs e);

    public delegate void PropertyGettingEventHandler(object sender, PropertyGettingEventArgs e);

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
        /// SetPropertyが使用可能かを表す値。デフォルトではTrueです。
        /// </summary>
        public bool CanSet
        {
            get
            {
                return m_CanSet;
            }
            set
            {
                m_CanSet = value;
            }
        }
        private bool m_CanSet = true;
        public Variable Property
        {
            get
            {
                if (HandleEvents)
                {
                    PropertyGettingEventArgs e = new PropertyGettingEventArgs();
                    e.Value = Value;
                    Getting?.Invoke(this, e);
                    return e.Value;
                }
                else
                {
                    return Value;
                }
            }
            set
            {
                if (CanSet)
                {
                    if (HandleEvents)
                    {
                        PropertySettingEventArgs e = new PropertySettingEventArgs();
                        e.Value = value;
                        Setting?.Invoke(this, e);
                    }
                    else
                    {
                        Value = value;
                    }
                }else
                {
                    ThrowErrorManerger.OnThrowError("このプロパティに代入できません",Exceptions.COULDNT_ASSIGN_THIS_PROPERTY);
                }
            }
        }

    }
}
