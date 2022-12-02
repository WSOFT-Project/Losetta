using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{

    public class Variable : ScriptObject
    {
        public enum VarType
        {
            NONE, UNDEFINED, NUMBER, STRING, ARRAY,
            ARRAY_NUM, ARRAY_STR, MAP_NUM, MAP_STR, BYTES,
            BREAK, CONTINUE, OBJECT, ENUM, VARIABLE, CUSTOM, POINTER, DELEGATE, BOOLEAN, TYPE
        };

        public static Variable True
        {
            get
            {
                return new Variable(true);
            }
        }
        public static Variable False
        {
            get
            {
                return new Variable(false);
            }
        }
        public static Variable FromText(string text)
        {
            return new Variable(text);
        }
        public static void AddFunc(FunctionBase fb, string name = "")
        {
            if (name == "")
            {
                name = fb.FunctionName;
            }
            name = name.ToLower();
            Functions.Add(name, fb);
        }
        public static void RemoveFunc(FunctionBase fb, string name = "")
        {

            if (name == "")
            {
                name = fb.FunctionName;
            }
            name = name.ToLower();
            if (Functions.ContainsKey(name))
            {
                Functions.Remove(name);
            }
        }

        public static Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>();

        List<string> ScriptObject.GetProperties()
        {
            List<string> v = Functions.Keys.ToList();


            return v;
        }
        public static bool GETTING = false;
        public static List<Variable> LaskVariable;

        Task<Variable> ScriptObject.GetProperty(string sPropertyName, List<Variable> args, ParsingScript script)
        {


            sPropertyName = Variable.GetActualPropertyName(sPropertyName, ((ScriptObject)this).GetProperties());


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
                return Task.FromResult(Variable.EmptyInstance);
            }

        }
        public virtual Task<Variable> SetProperty(string sPropertyName, Variable argValue)
        {

            sPropertyName = Variable.GetActualPropertyName(sPropertyName, ((ScriptObject)this).GetProperties());


            return Task.FromResult(Variable.EmptyInstance);
        }
        public static Variable AsType(VarType type)
        {
            var r = new Variable(VarType.TYPE);
            r.VariableType = type;
            return r;
        }
        public Variable()
        {
            Reset();
        }
        public Variable(VarType type)
        {
            Type = type;
            Activate();
        }
        public Variable(double d)
        {
            Value = d;
        }
        public Variable(bool b)
        {
            Bool = b;
            Type = VarType.BOOLEAN;
        }
        public Variable(string s)
        {
            String = s;
        }

        public Variable(CustomFunction func)
        {
            this.Delegate = new DelegateObject(func);
            this.Type = VarType.DELEGATE;
        }
        public Variable(byte[] ba)
        {
            ByteArray = ba;
            Type = VarType.BYTES;
        }
        public Variable(List<Variable> a)
        {
            this.Tuple = a;
        }
        public Variable(List<string> a)
        {
            List<Variable> tuple = new List<Variable>(a.Count);
            for (int i = 0; i < a.Count; i++)
            {
                tuple.Add(new Variable(a[i]));
            }
            this.Tuple = tuple;
        }
        public Variable(string[] a)
        {
            List<Variable> tuple = new List<Variable>();
            foreach (string s in a)
            {
                tuple.Add(new Variable(s));
            }
            this.Tuple = tuple;
        }
        public Variable(List<double> a)
        {
            List<Variable> tuple = new List<Variable>(a.Count);
            for (int i = 0; i < a.Count; i++)
            {
                tuple.Add(new Variable(a[i]));
            }
            this.Tuple = tuple;
        }
        public Variable(Dictionary<string, string> a)
        {
            List<Variable> tuple = new List<Variable>(a.Count);
            foreach (string key in a.Keys)
            {
                string lower = key.ToLower();
                m_keyMappings[lower] = key;
                m_dictionary[lower] = tuple.Count;
                tuple.Add(new Variable(a[key]));
            }
            this.Tuple = tuple;
        }
        public Variable(Dictionary<string, double> a)
        {
            List<Variable> tuple = new List<Variable>(a.Count);
            foreach (string key in a.Keys)
            {
                string lower = key.ToLower();
                m_keyMappings[lower] = key;
                m_dictionary[lower] = tuple.Count;
                tuple.Add(new Variable(a[key]));
            }
            this.Tuple = tuple;
        }

        public Variable(object o)
        {
            Object = o;
        }


        public virtual Variable Clone()
        {
            Variable newVar = (Variable)this.MemberwiseClone();
            return newVar;

        }

        public virtual Variable DeepClone()
        {
            //Variable newVar = new Variable();
            //newVar.Copy(this);
            Variable newVar = (Variable)this.MemberwiseClone();

            if (m_tuple != null)
            {
                List<Variable> newTuple = new List<Variable>();
                foreach (var item in m_tuple)
                {
                    newTuple.Add(item.DeepClone());
                }

                newVar.Tuple = newTuple;

                newVar.m_dictionary = new Dictionary<string, int>(m_dictionary);
                newVar.m_keyMappings = new Dictionary<string, string>(m_keyMappings);
                newVar.m_propertyStringMap = new Dictionary<string, string>(m_propertyStringMap);
                newVar.m_propertyMap = new Dictionary<string, Variable>(m_propertyMap);
                newVar.m_enumMap = m_enumMap == null ? null : new Dictionary<int, string>(m_enumMap);
            }
            return newVar;
        }

        public static Variable NewEmpty()
        {
            return new Variable();
        }
        public static Variable ConvertToVariable(object obj)
        {
            if (obj == null)
            {
                return Variable.EmptyInstance;
            }
            if (obj is Variable)
            {
                return (Variable)obj;
            }
            if (obj is string || obj is char)
            {
                return new Variable(System.Convert.ToString(obj));
            }
            if (obj is double || obj is float || obj is int || obj is long)
            {
                return new Variable(System.Convert.ToDouble(obj));
            }
            if (obj is bool)
            {
                return new Variable(((bool)obj));
            }
            if (obj is byte[])
            {
                return new Variable(((byte[])obj));
            }
            if (obj is List<string>)
            {
                return new Variable(((List<string>)obj));
            }
            if (obj is List<double>)
            {
                return new Variable(((List<double>)obj));
            }

            return new Variable(obj);
        }

        public void Reset()
        {
            m_value = Double.NaN;
            m_bool = false;
            m_string = null;
            m_object = null;
            m_tuple = null;
            m_byteArray = null;
            Action = null;
            IsReturn = false;
            Type = VarType.NONE;
            m_dictionary.Clear();
            m_keyMappings.Clear();
            m_propertyMap.Clear();
            m_propertyStringMap.Clear();
        }
        public bool IsNull()
        {
            switch (Type)
            {
                case VarType.NONE: return true;
                default:return false;
                case VarType.ARRAY:
                    {
                        return (Tuple==null);
                    }
                case VarType.DELEGATE:
                    {
                        return (Delegate==null);
                    }
                case VarType.BYTES:
                    {
                        return (ByteArray==null);
                    }
                case VarType.STRING:
                    {
                        return (String==null);
                    }
                case VarType.POINTER:
                    {
                        return (Pointer==null);
                    }
                case VarType.OBJECT:
                    {
                        return (Object==null);
                    }
            }
        }
        public bool Equals(Variable other)
        {
            if (Type != other.Type)
            {
                return false;
            }

            if (Type == VarType.NUMBER && Value == other.Value)
            {
                return true;
            }
            bool stringsEqual = String.Equals(this.String, other.String, StringComparison.Ordinal);
            if (Type == VarType.STRING && stringsEqual)
            {
                return true;
            }
            if (Type == VarType.OBJECT)
            {
                return Object == other.Object;
            }
            if (Type == VarType.BYTES)
            {
                return ByteArray == other.ByteArray;
            }
            if (Type == VarType.BOOLEAN)
            {
                return Bool == other.Bool;
            }
            if (Type == VarType.DELEGATE)
            {
                return Delegate.Equals(other.Delegate);
            }
            if (Type == VarType.BOOLEAN)
            {
                return Bool == other.Bool;
            }
            if (Type == VarType.ARRAY)
            {
                return EqualsArray(Tuple, other.Tuple);
            }
            if (Type == VarType.TYPE)
            {
                return VariableType == other.VariableType;
            }
            if (Type == VarType.NONE)
            {
                return other.Type == VarType.NONE;
            }
            if (Double.IsNaN(Value) != Double.IsNaN(other.Value) ||
              (!Double.IsNaN(Value) && Value != other.Value))
            {
                return false;
            }
            if (!String.Equals(this.Action, other.Action, StringComparison.Ordinal))
            {
                return false;
            }
            if ((this.Tuple == null) != (other.Tuple == null))
            {
                return false;
            }
            if (this.Tuple != null && !this.Tuple.Equals(other.Tuple))
            {
                return false;
            }
            if (!m_propertyMap.Equals(other.m_propertyMap))
            {
                return false;
            }
            if (!stringsEqual)
            {
                return false;
            }
            return AsString() == other.AsString();
        }
        /// <summary>
        /// 明示的キャスト(as)を実行する時に呼ばれます。この変換は最も広範囲の型変換をサポートします
        /// </summary>
        /// <param name="type">変換したい型</param>
        /// <param name="throwError">変換に失敗した際に例外をスローするか</param>
        /// <returns>変換された型</returns>
        public Variable Convert(VarType type, bool throwError = false)
        {
            switch (type)
            {
                case Variable.VarType.ARRAY:
                    {
                        Variable tuple = new Variable(Variable.VarType.ARRAY);
                        tuple.Tuple = new List<Variable> { this };
                        return tuple;
                    }
                case Variable.VarType.BOOLEAN:
                    {
                        switch (Type)
                        {
                            case Variable.VarType.NUMBER:
                                {
                                    return new Variable(Value == 1.0);
                                }
                            case Variable.VarType.BYTES:
                                {
                                    return new Variable(BitConverter.ToBoolean(ByteArray));
                                }
                            case Variable.VarType.STRING:
                                {
                                    return new Variable(String.ToLower() == Constants.TRUE);
                                }
                        }
                        break;
                    }
                case Variable.VarType.BYTES:
                    {
                        switch (Type)
                        {
                            case Variable.VarType.BOOLEAN:
                                {
                                    return new Variable(BitConverter.GetBytes(Bool));
                                }
                            case Variable.VarType.NUMBER:
                                {
                                    return new Variable(BitConverter.GetBytes(Value));
                                }
                            case Variable.VarType.STRING:
                                {
                                    return new Variable(System.Text.Encoding.Unicode.GetBytes(AsString()));
                                }
                        }
                        break;
                    }
                case Variable.VarType.NUMBER:
                    {
                        switch (Type)
                        {
                            case Variable.VarType.BOOLEAN:
                                {
                                    double d = 0.0;
                                    if (Bool)
                                    {
                                        d = 1.0;
                                    }
                                    return new Variable(d);
                                }
                            case Variable.VarType.BYTES:
                                {
                                    return new Variable(BitConverter.ToDouble(ByteArray));
                                }
                            case Variable.VarType.STRING:
                                {
                                    double d = 0.0;
                                    if (double.TryParse(String, out d))
                                    {
                                        return new Variable(d);
                                    }
                                    else
                                    {
                                        ThrowErrorManerger.OnThrowError("引数である" + String + "は有効な数値の形式ではありません", Exceptions.INVALID_NUMERIC_REPRESENTATION);
                                    }
                                    break;
                                }

                        }
                        break;
                    }
                case Variable.VarType.STRING:
                    {
                        if (Type == Variable.VarType.BYTES)
                        {
                            return new Variable(System.Text.Encoding.Unicode.GetString(ByteArray));
                        }
                        else
                        {
                            return new Variable(AsString());
                        }
                    }
                case VarType.TYPE:
                    {
                        VarType ptype;
                        if (Constants.TryParseType(AsString(), out ptype))
                        {
                            return Variable.AsType(ptype);
                        }
                        break;
                    }
            }
            //変換に失敗または非対応
            if (throwError)
            {
                ThrowErrorManerger.OnThrowError(Constants.TypeToString(Type) + "型を" + Constants.TypeToString(type) + "型に変換することはできません", Exceptions.COULDNT_CONVERT_VARIABLE);
            }
            return Variable.EmptyInstance;
        }

        private bool EqualsArray(List<Variable> ary1, List<Variable> ary2)
        {
            //結果を格納する変数
            bool isEqual = true;

            if (object.ReferenceEquals(ary1, ary2))
            {
                //同一のインスタンスの時は、同じとする
                isEqual = true;
            }
            else if (ary1 == null || ary2 == null
                || ary1.Count != ary2.Count)
            {
                //どちらかがNULLか、要素数が異なる時は、同じではない
                isEqual = false;
            }
            else
            {
                //1つ1つの要素が等しいかを調べる
                for (int i = 0; i < ary1.Count; i++)
                {
                    //ary1の要素のEqualsメソッドで、ary2の要素と等しいか調べる
                    if (!ary1[i].Equals(ary2[i]))
                    {
                        //1つでも等しくない要素があれば、同じではない
                        isEqual = false;
                        break;
                    }
                }
            }
            return isEqual;
        }

        public void AddVariableToHash(string hash, Variable newVar)
        {
            int retValue = 0;
            Variable listVar = null;
            string lower = hash.ToLower();
            if (m_dictionary.TryGetValue(lower, out retValue))
            {
                // already exists, change the value:
                listVar = m_tuple[retValue];
            }
            else
            {
                listVar = new Variable(VarType.ARRAY);
                m_tuple.Add(listVar);

                m_keyMappings[lower] = hash;
                m_dictionary[lower] = m_tuple.Count - 1;
            }

            listVar.AddVariable(newVar);
        }

        public List<Variable> GetAllKeys()
        {
            List<Variable> results = new List<Variable>();
            var keys = m_keyMappings.Values;
            foreach (var key in keys)
            {
                results.Add(new Variable(key));
            }

            if (results.Count == 0 && m_tuple != null)
            {
                results = m_tuple;
            }

            return results;
        }

        public List<string> GetKeys()
        {
            List<string> results = new List<string>();
            var keys = m_keyMappings.Values;
            foreach (var key in keys)
            {
                results.Add(key);
            }
            return results;
        }

        public int SetHashVariable(string hash, Variable var)
        {
            SetAsArray();
            int retValue;
            string lower = hash.ToLower();
            if (m_dictionary.TryGetValue(lower, out retValue))
            {
                // already exists, change the value:
                m_tuple[retValue] = var;
                return retValue;
            }

            m_tuple.Add(var);
            m_keyMappings[lower] = hash;
            m_dictionary[lower] = m_tuple.Count - 1;

            return m_tuple.Count - 1;
        }

        public void TrySetAsMap()
        {
            if (m_tuple == null || m_tuple.Count < 1 ||
                m_dictionary.Count > 0 || m_keyMappings.Count > 0 ||
                m_tuple[0].m_dictionary.Count == 0)
            {
                return;
            }

            for (int i = 0; i < m_tuple.Count; i++)
            {
                var current = m_tuple[i];
                if (current.m_tuple == null || current.m_dictionary.Count == 0)
                {
                    continue;
                }

                var key = current.m_dictionary.First().Key;
                m_keyMappings[key] = current.m_keyMappings[key];
                m_dictionary[key] = i;

                current.m_dictionary.Clear();
                m_tuple[i] = current.m_tuple[0];
            }
        }

        public int RemoveItem(string item)
        {
            string lower = item.ToLower();
            if (m_dictionary.Count > 0)
            {
                int index = 0;
                if (!m_dictionary.TryGetValue(lower, out index))
                {
                    return 0;
                }

                m_tuple.RemoveAt(index);
                m_keyMappings.Remove(lower);
                m_dictionary.Remove(lower);

                // "Rehash" the dictionary so that it points correctly to the indices after removed.
                foreach (var key in m_dictionary.Keys.ToList())
                {
                    int value = m_dictionary[key];
                    if (value > index)
                    {
                        m_dictionary[key] = value - 1;
                    }
                }

                return 1;
            }

            int removed = m_tuple.RemoveAll(p => p.AsString() == item);
            return removed;
        }

        public int GetArrayIndex(Variable indexVar)
        {
            if (!Constants.CAN_GET_ARRAYELEMENT_VARIABLE_TYPES.Contains(this.Type))
            {
                //変換不可
                return -1;
            }

            if (indexVar.Type == VarType.NUMBER)
            {
                Utils.CheckNonNegativeInt(indexVar, null);
                return (int)indexVar.Value;
            }

            string hash = indexVar.AsString();
            string lower = hash.ToLower();
            int ptr = m_tuple.Count;
            if (m_dictionary.TryGetValue(lower, out ptr) &&
                ptr < m_tuple.Count)
            {
                return ptr;
            }

            int result = -1;
            if (!String.IsNullOrWhiteSpace(indexVar.String) &&
                Int32.TryParse(indexVar.String, out result))
            {
                return result;
            }

            return -1;
        }

        public void AddVariable(Variable v, int index = -1)
        {
            SetAsArray();
            if (index < 0 || m_tuple.Count <= index)
            {
                m_tuple.Add(v);
            }
            else
            {
                m_tuple.Insert(index, v);
            }
        }
        public int FindIndex(Variable val)
        {
            if (this.Type != VarType.ARRAY)
            {
                return -1;
            }
            return m_tuple.FindIndex(item => item == val);
        }

        public virtual bool AsBool()
        {
            return m_bool;
        }

        public virtual int AsInt()
        {
            return (int)Value;
        }
        public virtual float AsFloat()
        {
            return (float)Value;
        }
        public virtual long AsLong()
        {
            return (long)Value;
        }
        public virtual double AsDouble()
        {
            return Value;
        }

        public virtual DelegateObject AsDelegate()
        {
            return m_delegate;
        }

        public virtual byte[] AsByteArray()
        {
            return m_byteArray;
        }
        public virtual VarType AsType()
        {
            if (Type == VarType.TYPE)
            {
                return VariableType;
            }
            return Type;
        }
        public override string ToString()
        {
            return AsString();
        }

        public object AsObject()
        {
            switch (Type)
            {
                case VarType.BOOLEAN: return AsBool();
                case VarType.NUMBER: return AsDouble();
                case VarType.OBJECT: return Object;
                case VarType.ARRAY:
                case VarType.ARRAY_NUM:
                case VarType.ARRAY_STR:
                    var list = new List<object>();
                    for (int i = 0; i < m_tuple.Count; i++)
                    {
                        list.Add(m_tuple[i].AsObject());
                    }
                    return list;
            }
            return AsString();
        }


        public virtual string AsString(bool isList = true,
                                       bool sameLine = true,
                                       int maxCount = -1)
        {
            if (Type == VarType.BOOLEAN)
            {
                if (m_bool)
                {
                    return Constants.TRUE;
                }
                else
                {
                    return Constants.FALSE;
                }
            }
            if (Type == VarType.NUMBER)
            {
                return Value.ToString();
            }
            if (Type == VarType.STRING)
            {
                return m_string == null ? "" : m_string;
            }
            if (Type == VarType.OBJECT)
            {
                return ObjectToString();
            }
            if (Type == VarType.BYTES)
            {
                return Encoding.Unicode.GetString(m_byteArray, 0, m_byteArray.Length);
            }
            if (Type == VarType.TYPE)
            {
                return Constants.TypeToString(VariableType);
            }


            StringBuilder sb = new StringBuilder();
            if (Type == VarType.ENUM)
            {
                sb.Append(Constants.START_GROUP.ToString() + " ");
                foreach (string key in m_propertyMap.Keys)
                {
                    sb.Append(key + " ");
                }
                sb.Append(Constants.END_GROUP.ToString());
                return sb.ToString();
            }

            if (Type == VarType.UNDEFINED)
            {
                return Constants.UNDEFINED;
            }
            if (Type == VarType.NONE || m_tuple == null)
            {
                return string.Empty;
            }

            if (isList)
            {
                sb.Append(Constants.START_ARRAY.ToString() +
                         (sameLine ? "" : Environment.NewLine));
            }

            int count = maxCount < 0 ? m_tuple.Count : Math.Min(maxCount, m_tuple.Count);
            int i = 0;
            if (m_dictionary.Count > 0)
            {
                count = maxCount < 0 ? m_dictionary.Count : Math.Min(maxCount, m_dictionary.Count);
                foreach (KeyValuePair<string, int> entry in m_dictionary)
                {
                    if (entry.Value >= 0 && entry.Value < m_tuple.Count)
                    {
                        string value = m_tuple[entry.Value].AsString(isList, sameLine, maxCount);
                        string realKey = entry.Key;
                        m_keyMappings.TryGetValue(entry.Key.ToLower(), out realKey);

                        sb.Append("\"" + realKey + "\" : " + value);
                        if (i++ < count - 1)
                        {
                            sb.Append(sameLine ? ", " : Environment.NewLine);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("インデックス: [" + entry.Value +
                                  "] は配列の長さ[" + m_tuple.Count + "]を超えています", Exceptions.INDEX_OUT_OF_RANGE);
                    }
                }
            }
            else
            {
                for (; i < count; i++)
                {
                    Variable arg = m_tuple[i];
                    sb.Append(arg.AsString(isList, sameLine, maxCount));
                    if (i != count - 1)
                    {
                        sb.Append(sameLine ? ", " : Environment.NewLine);
                    }
                }
            }
            if (count < m_tuple.Count)
            {
                sb.Append(" ...");
            }
            if (isList)
            {
                sb.Append(Constants.END_ARRAY.ToString() +
                         (sameLine ? "" : Environment.NewLine));
            }

            return sb.ToString();
        }

        private string ObjectToString()
        {
            StringBuilder sb = new StringBuilder();
            if (m_object != null)
            {
                sb.Append(m_object.ToString());
            }
            else
            {
                sb.Append((m_object != null ? (m_object.ToString() + " ") : "") +
                           Constants.START_ARRAY.ToString());

                List<string> allProps = GetAllProperties();
                for (int i = 0; i < allProps.Count; i++)
                {
                    string prop = allProps[i];
                    if (prop.Equals(Constants.OBJECT_PROPERTIES, StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append(prop);
                        continue;
                    }
                    Variable propValue = GetProperty(prop);
                    string value = "";
                    if (propValue != null && propValue != Variable.EmptyInstance)
                    {
                        value = propValue.AsString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            if (propValue.Type == VarType.STRING &&
                               !prop.Equals(Constants.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase))
                            {
                                value = "\"" + value + "\"";
                            }
                            value = ": " + value;
                        }
                    }
                    sb.Append(prop + value);
                    if (i < allProps.Count - 1)
                    {
                        sb.Append(", ");
                    }
                }

                sb.Append(Constants.END_GROUP.ToString());
            }
            return sb.ToString();
        }
        public void Activate()
        {
            switch (Type)
            {
                case VarType.ARRAY:
                    {
                        if (m_tuple == null)
                        {
                            m_tuple = new List<Variable>();
                        }
                        break;
                    }
                case VarType.DELEGATE:
                    {
                        if (m_delegate == null)
                        {
                            m_delegate = new DelegateObject();
                        }
                        break;
                    }
            }
        }
        public void SetAsArray()
        {
            Type = VarType.ARRAY;
            if (m_tuple == null)
            {
                m_tuple = new List<Variable>();
            }
        }

        public int Count
        {
            get
            {
                return Type == VarType.ARRAY ? m_tuple.Count :
                       Type == VarType.NONE ? 0 : 1;
            }
        }


        public Variable SetProperty(string propName, Variable value, ParsingScript script, string baseName = "")
        {
            int ind = propName.IndexOf('.');
            if (ind > 0)
            { // The case a.b.c = ... is dealt here recursively
                string varName = propName.Substring(0, ind);
                string actualPropName = propName.Substring(ind + 1);
                Variable property = GetProperty(varName);
                Utils.CheckNotNull(property, varName, script);
                return property.SetProperty(actualPropName, value, script, baseName);
            }
            return FinishSetProperty(propName, value, script, baseName);
        }

        public async Task<Variable> SetPropertyAsync(string propName, Variable value, ParsingScript script, string baseName = "")
        {
            int ind = propName.IndexOf('.');
            if (ind > 0)
            { // The case a.b.c = ... is dealt here recursively
                string varName = propName.Substring(0, ind);
                string actualPropName = propName.Substring(ind + 1);
                Variable property = await GetPropertyAsync(varName);
                Utils.CheckNotNull(property, varName, script);
                Variable result = await property.SetPropertyAsync(actualPropName, value, script, baseName);
                return result;
            }
            return FinishSetProperty(propName, value, script, baseName);
        }

        private string GetRealName(string name)
        {
            string realName;
            string converted = Constants.ConvertName(name);
            if (!m_propertyStringMap.TryGetValue(converted, out realName))
            {
                realName = name;
            }
            return realName;
        }

        public Variable FinishSetProperty(string propName, Variable value, ParsingScript script, string baseName = "")
        {
            Variable result = Variable.EmptyInstance;

            // Check for an existing custom setter
            if ((m_propertyMap.TryGetValue(propName, out result) ||
                m_propertyMap.TryGetValue(GetRealName(propName), out result)))
            {
                if (!result.Writable)
                {
                    Utils.ThrowErrorMsg("プロパティ:[" + propName + "]は読み取り専用です", Exceptions.PROPERTY_IS_READ_ONLY,
                        script, propName);
                }
                if (result.CustomFunctionSet != null)
                {
                    var args = new List<Variable> { value };
                    result.CustomFunctionSet.Run(args, script);
                    return result;
                }
                if (!string.IsNullOrWhiteSpace(result.CustomSet))
                {
                    return ParsingScript.RunString(result.CustomSet);
                }
            }

            m_propertyMap[propName] = value;

            string converted = Constants.ConvertName(propName);
            m_propertyStringMap[converted] = propName;

            Type = VarType.OBJECT;

            if (Object is ScriptObject)
            {
                ScriptObject obj = Object as ScriptObject;
                result = obj.SetProperty(propName, value).Result;
            }
            return result;
        }

        public void SetEnumProperty(string propName, Variable value, string baseName = "")
        {
            m_propertyMap[propName] = value;

            string converted = Constants.ConvertName(propName);
            m_propertyStringMap[converted] = propName;

            if (m_enumMap == null)
            {
                m_enumMap = new Dictionary<int, string>();
            }
            m_enumMap[value.AsInt()] = propName;
        }

        public Variable GetEnumProperty(string propName, ParsingScript script, string baseName = "")
        {
            propName = Constants.ConvertName(propName);
            if (script.Prev == Constants.START_ARG)
            {
                Variable value = Utils.GetItem(script);
                if (propName == Constants.TO_STRING)
                {
                    return ConvertEnumToString(value);
                }
                else
                {
                    return new Variable(m_enumMap != null && m_enumMap.ContainsKey(value.AsInt()));
                }
            }

            string[] tokens = propName.Split('.');
            if (tokens.Length > 1)
            {
                propName = tokens[0];
            }

            string match = GetActualPropertyName(propName, GetAllProperties(), baseName, this);

            Variable result = GetCoreProperty(match, script);

            if (tokens.Length > 1)
            {
                result = ConvertEnumToString(result);
                if (tokens.Length > 2)
                {
                    string rest = string.Join(".", tokens, 2, tokens.Length - 2);
                    result = result.GetProperty(rest, script);
                }
            }

            return result;
        }

        public Variable ConvertEnumToString(Variable value)
        {
            string result = "";
            if (m_enumMap != null && m_enumMap.TryGetValue(value.AsInt(), out result))
            {
                return new Variable(result);
            }
            return Variable.EmptyInstance;
        }

        public Variable GetProperty(string propName, ParsingScript script = null)
        {
            Variable result = Variable.EmptyInstance;

            int ind = propName.IndexOf('.');
            if (ind > 0)
            { //x=a.b.cの場合はここで再帰的に処理
                string varName = propName.Substring(0, ind);
                string actualPropName = propName.Substring(ind + 1);
                Variable property = GetProperty(varName, script);
                result = string.IsNullOrEmpty(actualPropName) ? property :
                               property.GetProperty(actualPropName, script);
                return result;
            }

            if (Object is ScriptObject)
            {
                ScriptObject obj = Object as ScriptObject;
                string match = GetActualPropertyName(propName, obj.GetProperties());
                if (!string.IsNullOrWhiteSpace(match))
                {
                    List<Variable> args = null;
                    if (script != null &&
                       (script.Pointer == 0 || script.Prev == Constants.START_ARG))
                    {

                        args = script.GetFunctionArgs();
                        ObjectBase.LaskVariable = args;
                    }
                    else if (script != null)
                    {
                        args = new List<Variable>();
                    }
                    var task = obj.GetProperty(match, args, script);
                    result = task != null ? task.Result : null;
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return GetCoreProperty(propName, script);
        }

        public async Task<Variable> GetPropertyAsync(string propName, ParsingScript script = null)
        {
            Variable result = Variable.EmptyInstance;

            int ind = propName.IndexOf('.');
            if (ind > 0)
            { // The case x = a.b.c ... is dealt here recursively
                string varName = propName.Substring(0, ind);
                string actualPropName = propName.Substring(ind + 1);
                Variable property = await GetPropertyAsync(varName, script);
                result = string.IsNullOrEmpty(actualPropName) ? property :
                               await property.GetPropertyAsync(actualPropName, script);
                return result;
            }

            if (Object is ScriptObject)
            {
                ScriptObject obj = Object as ScriptObject;
                string match = GetActualPropertyName(propName, obj.GetProperties());
                if (!string.IsNullOrWhiteSpace(match))
                {
                    List<Variable> args = null;
                    if (script != null &&
                       (script.Pointer == 0 || script.Prev == Constants.START_ARG))
                    {
                        args = await script.GetFunctionArgsAsync();
                    }
                    else if (script != null)
                    {
                        args = new List<Variable>();
                    }
                    result = await obj.GetProperty(match, args, script);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return GetCoreProperty(propName, script);
        }

        private Variable GetCoreProperty(string propName, ParsingScript script = null)
        {
            Variable result = Variable.EmptyInstance;

            if (m_propertyMap.TryGetValue(propName, out result) ||
                m_propertyMap.TryGetValue(GetRealName(propName), out result))
            {
                return result;
            }
            else if (script != null && Functions.ContainsKey(propName.ToLower()))
            {

                return Functions[propName.ToLower()].Evaluate(script, this);
            }

            return result;
        }

       

        public List<Variable> GetProperties()
        {
            List<string> all = GetAllProperties();
            List<Variable> allVars = new List<Variable>(all.Count);
            foreach (string key in all)
            {
                allVars.Add(new Variable(key));
            }

            return allVars;
        }

        public List<string> GetAllProperties()
        {
            HashSet<string> allSet = new HashSet<string>();
            List<string> all = new List<string>();

            foreach (string key in m_propertyMap.Keys)
            {
                allSet.Add(key.ToLower());
                all.Add(key);
            }
            foreach(string name in Functions.Keys)
            {
                FunctionBase fb = Functions[name];
                if (fb.RequestType == VarType.NONE || fb.RequestType == Type)
                {
                    all.Add(name);
                }
            }

            if (Object is ScriptObject)
            {
                ScriptObject obj = Object as ScriptObject;
                List<string> objProps = obj.GetProperties();
                foreach (string key in objProps)
                {
                    if (allSet.Add(key.ToLower()))
                    {
                        all.Add(key);
                    }
                }
            }

            all.Sort();

            if (!allSet.Contains(Constants.OBJECT_TYPE.ToLower()))
            {
                all.Add(Constants.OBJECT_TYPE);
            }

            return all;
        }

        public int GetSize()
        {
            return GetLength();
        }

        public int GetLength()
        {
            int len = 0;
            switch (Type)
            {
                case VarType.ARRAY:
                    {
                        len = Tuple.Count;
                        break;
                    }
                case VarType.DELEGATE:
                    {
                        len = Delegate.Length;
                        break;
                    }
                case VarType.BYTES:
                    {
                        len = ByteArray.Length;
                        break;
                    }
                case VarType.STRING:
                    {
                        len = String.Length;
                        break;
                    }
            }
            return len;
        }

        public virtual string GetTypeString()
        {
            if (Type == VarType.OBJECT && Object != null)
            {
                if (Object is ObjectBase)
                {
                    return ((ObjectBase)Object).Name;
                }
                else
                {
                    return Object.GetType().ToString();
                }
            }
            return Constants.TypeToString(Type);
        }

        public Variable GetValue(int index)
        {
            if (index >= Count)
            {
                throw new ArgumentException("There are only [" + Count +
                                             "] but " + index + " requested.");

            }
            if (Type == VarType.ARRAY)
            {
                return m_tuple[index];
            }
            else if (Type == VarType.BYTES)
            {
                return new Variable(ByteArray[index]);
            }
            else if (Type == VarType.DELEGATE)
            {
                return new Variable(Delegate.Functions[index]);
            }
            else if (Type == VarType.STRING)
            {
                return new Variable(String[index].ToString());
            }
            return this;
        }

        public static string GetActualPropertyName(string propName, List<string> properties,
                                                   string baseName = "", Variable root = null)
        {
            string match = properties.FirstOrDefault(element => element.Equals(propName,
                                   StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(match))
            {
                match = "";
                if (root != null)
                {
                    string objName = !string.IsNullOrWhiteSpace(baseName) ? baseName + "." : "";
                    if (string.IsNullOrWhiteSpace(objName))
                    {
                        AliceScriptClass.ClassInstance obj = root.m_object as AliceScriptClass.ClassInstance;
                        objName = obj != null ? obj.InstanceName + "." : "";
                    }
                    match = Constants.GetRealName(objName + propName);
                    match = match.Substring(objName.Length);
                }
            }
            return match;
        }

        public void Sort()
        {
            if (Tuple == null || Tuple.Count <= 1)
            {
                return;
            }

            List<double> numbers = new List<double>();
            List<string> strings = new List<string>();
            for (int i = 0; i < Tuple.Count; i++)
            {
                Variable arg = Tuple[i];
                if (arg.Tuple != null)
                {
                    arg.Sort();
                }
                else if (arg.Type == VarType.NUMBER)
                {
                    numbers.Add(arg.AsDouble());
                }
                else
                {
                    strings.Add(arg.AsString());
                }
            }
            List<Variable> newTuple = new List<Variable>(Tuple.Count);
            numbers.Sort();
            strings.Sort();

            for (int i = 0; i < numbers.Count; i++)
            {
                newTuple.Add(new Variable(numbers[i]));
            }
            for (int i = 0; i < strings.Count; i++)
            {
                newTuple.Add(new Variable(strings[i]));
            }
            Tuple = newTuple;
        }



        public virtual double Value
        {
            get { return m_value; }
            set { m_value = value; Type = VarType.NUMBER; }
        }
        public virtual bool Bool
        {
            get { return m_bool; }
            set { m_bool = value; }
        }
        public virtual string String
        {
            get { return m_string; }
            set { m_string = value; Type = VarType.STRING; }
        }

        public object Object
        {
            get { return m_object; }
            set { m_object = value; Type = VarType.OBJECT; }
        }
        public DelegateObject Delegate
        {
            get { return m_delegate; }
            set
            {
                m_delegate = value;
                Type = VarType.DELEGATE;
            }
        }

        public byte[] ByteArray
        {
            get { return m_byteArray; }
            set { m_byteArray = value; Type = VarType.BYTES; }
        }

        public string Pointer
        {
            get;
            set;
        } = null;

        public CustomFunction CustomFunctionGet
        {
            get { return m_customFunctionGet; }
            set { m_customFunctionGet = value; }
        }
        public CustomFunction CustomFunctionSet
        {
            get { return m_customFunctionSet; }
            set { m_customFunctionSet = value; }
        }

        public List<Variable> Tuple
        {
            get { return m_tuple; }
            set { m_tuple = value; Type = VarType.ARRAY; }
        }

        public string Action { get; set; }
        public VarType Type
        {
            get
            {
                return m_type;
            }
            set
            {
                m_type = value;
            }
        }
        /// <summary>
        /// タイプ型の表すタイプです。変数の型ではないことに注意してください
        /// </summary>
        public VarType VariableType
        {
            get;
            set;
        }
        public bool IsReturn { get; set; }
        public string ParsingToken { get; set; }
        public int Index { get; set; }
        public string CurrentAssign { get; set; } = "";
        public string ParamName { get; set; } = "";

        public bool Writable { get; set; } = true;
        public bool Enumerable { get; set; } = true;
        public bool Configurable { get; set; } = true;

        public string CustomGet { get; set; }
        public string CustomSet { get; set; }

        public List<Variable> StackVariables { get; set; }

        public static Variable EmptyInstance = new Variable();
        public static Variable Undefined = new Variable(VarType.UNDEFINED);

        public virtual Variable Default()
        {
            return EmptyInstance;
        }

        protected double m_value;
        protected string m_string;
        protected object m_object;
        protected bool m_bool;
        protected DateTime m_datetime;
        protected DelegateObject m_delegate;
        protected VarType m_type;
        private CustomFunction m_customFunctionGet;
        private CustomFunction m_customFunctionSet;
        protected List<Variable> m_tuple;
        protected byte[] m_byteArray;
        private Dictionary<string, int> m_dictionary = new Dictionary<string, int>();
        private Dictionary<string, string> m_keyMappings = new Dictionary<string, string>();
        private Dictionary<string, string> m_propertyStringMap = new Dictionary<string, string>();
        private Dictionary<string, Variable> m_propertyMap = new Dictionary<string, Variable>();
        private Dictionary<int, string> m_enumMap;

        //Dictionary<string, Func<ParsingScript, Variable, string, Variable>> m_properties = new Dictionary<string, Func<ParsingScript, Variable, string, Variable>>();
    }

    // A Variable supporting "dot-notation" must have an object implementing this interface.
    public interface ScriptObject
    {
        // SetProperty is triggered by the following scripting call: "a.name = value;"
        Task<Variable> SetProperty(string name, Variable value);

        // GetProperty is triggered by the following scripting call: "x = a.name;"
        // If args are null, it is triggered by object.ToString() function"
        // If args are not empty, it is triggered by a function call: "y = a.name(arg1, arg2, ...);"
        Task<Variable> GetProperty(string name, List<Variable> args = null, ParsingScript script = null);

        // Returns all of the properties that this object implements. Only these properties will be processed
        // by SetProperty() and GetProperty() methods above.
        List<string> GetProperties();
    }
}
