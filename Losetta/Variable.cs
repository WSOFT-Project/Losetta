using AliceScript.Extra;
using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;
using System.Text;
using System.Xml.Linq;

namespace AliceScript
{
    /// <summary>
    /// AliceScriptの変数を表すクラス
    /// </summary>
    public class Variable : ScriptObject, IComparable<Variable>
    {
        [Flags]
        public enum VarType
        {
            NONE = 0b0,
            UNDEFINED = 0b1,
            NUMBER = 0b10,
            STRING = 0b100,
            ARRAY = 0b1000,
            ARRAY_NUM = 0b100000,
            ARRAY_STR = 0b1000000,
            MAP_NUM = 0b10000000,
            MAP_STR = 0b100000000,
            BYTES = 0b1000000000,
            BREAK = 0b10000000000,
            CONTINUE = 0b100000000000,
            OBJECT = 0b1000000000000,
            ENUM = 0b10000000000000,
            VARIABLE = 0b100000000000000,
            CUSTOM = 0b1000000000000000,
            DELEGATE = 0b100000000000000000,
            BOOLEAN = 0b1000000000000000000
        };
        public static Variable True => new Variable(true);
        public static Variable False => new Variable(false);
        public static Variable FromText(string text)
        {
            return new Variable(text);
        }
        public static void AddFunc(FunctionBase fb, string name = null)
        {
            if (name == null)
            {
                name = fb.Name;
            }
            name = name.ToLowerInvariant();
            Functions.Add(name, fb);
        }
        public static void AddProp(PropertyBase pb, string name = null)
        {
            if (name == null)
            {
                name = pb.Name;
            }
            name = name.ToLowerInvariant();
            Properties.Add(name, pb);
        }
        public static void RemoveFunc(FunctionBase fb, string name = "")
        {

            if (name == "")
            {
                name = fb.Name;
            }
            name = name.ToLowerInvariant();
            if (Functions.ContainsKey(name))
            {
                Functions.Remove(name);
            }
        }

        public static Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>();
        public static Dictionary<string, PropertyBase> Properties = new Dictionary<string, PropertyBase>();

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
            return  new Variable(new TypeObject(type));
        }
        public Variable()
        {
        }
        public Variable(VarType type)
        {
            Type = type;
            Activate();
        }
        public Variable(double d)
        {
            m_value = d;
            Type = VarType.NUMBER;
        }
        public Variable(double? d)
        {
            m_value = d;
            Type = VarType.NUMBER;
            Nullable = true;
        }
        public Variable(bool b)
        {
            m_bool = b;
            Type = VarType.BOOLEAN;
        }
        public Variable(bool? b)
        {
            m_bool = b;
            Type = VarType.BOOLEAN;
            Nullable = true;
        }
        public Variable(string s)
        {
            String = s;
        }

        public Variable(CustomFunction func)
        {
            Delegate = new DelegateObject(func);
            Type = VarType.DELEGATE;
        }
        public Variable(byte[] ba)
        {
            ByteArray = ba;
            Type = VarType.BYTES;
        }
        public Variable(IEnumerable<Variable> a)
        {
            Tuple = new VariableCollection();
            foreach (var v in a)
            {
                Tuple.Add(v);
            }
        }
        public Variable(IEnumerable<string> a)
        {
            Tuple = new VariableCollection();
            Tuple.Type = new TypeObject(Variable.VarType.STRING);
            foreach (string s in a)
            {
                Tuple.Add(new Variable(s));
            }
        }
        public Variable(List<double> a)
        {
            Tuple = new VariableCollection();
            Tuple.Type = new TypeObject(Variable.VarType.NUMBER);
            foreach (var i in a)
            {
                Tuple.Add(new Variable(i));
            }
        }
        public Variable(object? o)
        {
            if (o is string s)
            {
                String = s;
                return;
            }
            if (o is bool b)
            {
                Bool = b;
                return;
            }
            if (o is byte[] data)
            {
                ByteArray = data;
                return;
            }
            if (o is double d)
            {
                Value = d;
                return;
            }
            if (o is int i)
            {
                Value = i;
                return;
            }
            if (o is float f)
            {
                Value = f;
                return;
            }
            if (o is long l)
            {
                Value = l;
                return;
            }
            if (o is IEnumerable<Variable> tuple)
            {
                Tuple = new VariableCollection();
                foreach (var v in tuple)
                {
                    Tuple.Add(v);
                }
                return;
            }
            if (o is IEnumerable<object> ary)
            {
                Tuple = new VariableCollection();
                foreach (var v in ary)
                {
                    Tuple.Add(new Variable(v));
                }
                return;
            }
            if (o is DelegateObject m)
            {
                Delegate = m;
                return;
            }
            Object = o;
        }
        public virtual Variable Clone()
        {
            Variable newVar = (Variable)MemberwiseClone();
            return newVar;

        }

        public virtual Variable DeepClone()
        {
            //Variable newVar = new Variable();
            //newVar.Copy(this);
            Variable newVar = (Variable)MemberwiseClone();

            if (m_tuple != null)
            {
                VariableCollection newTuple = new VariableCollection();
                newTuple.Type = m_tuple.Type;
                for (int i = 0; i < m_tuple.Count; i++)
                {
                    newTuple.Add(m_tuple[i].DeepClone());
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
        /// <summary>
        /// 他の変数を使って、この変数に代入します
        /// </summary>
        /// <param name="v">代入する値</param>
        public void Assign(Variable v)
        {
            if (m_type == VarType.VARIABLE)
            {
                //variable型に他の型が代入される前に型チェックをオフにする
                TypeChecked = false;
            }
            if (Readonly)
            {
                throw new ScriptException("readonly属性を持つ変数には、代入できません", Exceptions.CANT_ASSIGN_TO_READ_ONLY);
            }
            if (v.IsNull())
            {
                AssignNull();
                return;
            }
            else if (TypeChecked && m_type != v.Type)
            {
                throw new ScriptException($"`{m_type}`型の変数には`{v.Type}`型の値を代入できません", Exceptions.TYPE_MISMATCH);
            }

            m_bool = v.m_bool;
            m_byteArray = v.m_byteArray;
            m_customFunctionGet = v.m_customFunctionGet;
            m_customFunctionSet = v.m_customFunctionSet;
            m_delegate = v.m_delegate;
            m_dictionary = v.m_dictionary;
            m_enumMap = v.m_enumMap;
            m_keyMappings = v.m_keyMappings;
            m_object = v.m_object;
            m_propertyMap = v.m_propertyMap;
            m_propertyStringMap = v.m_propertyStringMap;
            m_string = v.m_string;
            m_tuple = v.m_tuple;
            m_type = v.m_type;
            m_value = v.m_value;
        }
        public static Variable NewEmpty()
        {
            return new Variable();
        }
        /// <summary>
        /// この変数またはそれが表す値がnullであるかどうかを取得します
        /// </summary>
        /// <returns>nullであればtrue、それ以外の場合はfalse</returns>
        public bool IsNull()
        {
            switch (Type)
            {
                case VarType.NONE: return true;
                default: return false;
                case VarType.ARRAY:
                    return Tuple == null;
                case VarType.DELEGATE:
                    return Delegate == null;
                case VarType.BYTES:
                    return ByteArray == null;
                case VarType.STRING:
                    return String == null;
                case VarType.OBJECT:
                    return Object == null;
                case VarType.BOOLEAN:
                    return m_bool == null;
                case VarType.NUMBER:
                    return m_value == null;
            }
        }

        /// <summary>
        /// この変数をNullに設定します
        /// </summary>
        public void AssignNull()
        {
            if (TypeChecked && !Nullable && IsNull())
            {
                throw new ScriptException($"この変数はnullをとりえません", Exceptions.VARIABLE_IS_NULL);
            }
            m_value = null;
            m_bool = null;
            m_string = null;
            m_object = null;
            m_tuple = null;
            m_byteArray = null;
            m_delegate = null;
            Action = null;
            IsReturn = false;
            //Type = VarType.NONE;
            m_dictionary = null;
            m_keyMappings = null;
            m_propertyMap = new Dictionary<string, Variable>();
            m_propertyStringMap = new Dictionary<string, string>();
            m_tuple = null;
        }
        /// <summary>
        /// 明示的キャスト(as)を実行する時に呼ばれます。この変換は最も広範囲の型変換をサポートします
        /// </summary>
        /// <param name="type">変換したい型</param>
        /// <param name="throwError">変換に失敗した際に例外をスローするか</param>
        /// <returns>変換された型</returns>
        public Variable Convert(VarType type, bool throwError = false)
        {
            if (Type == type)
            {
                return this;
            }
            switch (type)
            {
                case Variable.VarType.ARRAY:
                    {
                        Variable tuple = new Variable(Variable.VarType.ARRAY);
                        tuple.Tuple = new VariableCollection { this };
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
                                    return new Variable(String.Equals(Constants.TRUE, StringComparison.OrdinalIgnoreCase));
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
                                    if (Utils.CanConvertToDouble(String, out double d))
                                    {
                                        return new Variable(d);
                                    }
                                    else if (throwError)
                                    {
                                        throw new ScriptException("文字列 `" + String + "` は有効な数値の形式ではありません", Exceptions.INVALID_NUMERIC_REPRESENTATION);
                                    }
                                    break;
                                }

                        }
                        break;
                    }
                case Variable.VarType.STRING:
                    {
                        return Type == Variable.VarType.BYTES ? new Variable(System.Text.Encoding.Unicode.GetString(ByteArray)) : new Variable(AsString());
                    }
            }
            //変換に失敗または非対応
            return throwError
                ? throw new ScriptException(Constants.TypeToString(Type) + "型を" + Constants.TypeToString(type) + "型に変換することはできません", Exceptions.COULDNT_CONVERT_VARIABLE)
                : Variable.EmptyInstance;
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
            Variable listVar = null;
            string lower = hash.ToLowerInvariant();
            if (m_dictionary.TryGetValue(lower, out int retValue))
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
                results.AddRange(m_tuple);
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
            string lower = hash.ToLowerInvariant();
            if (m_dictionary.TryGetValue(lower, out int retValue))
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

        public int GetArrayIndex(Variable indexVar)
        {
            if (!Constants.CAN_GET_ARRAYELEMENT_VARIABLE_TYPES.Contains(Type))
            {
                //変換不可
                return -1;
            }

            if (indexVar.Type == VarType.NUMBER)
            {
                Utils.CheckNumInRange(indexVar, true, 0);
                return (int)indexVar.Value;
            }

            string hash = indexVar.AsString();
            string lower = hash.ToLowerInvariant();
            int ptr = m_tuple.Count;
            if (m_dictionary.TryGetValue(lower, out ptr) &&
                ptr < m_tuple.Count)
            {
                return ptr;
            }

            int result = -1;
            return !string.IsNullOrWhiteSpace(indexVar.String) &&
                int.TryParse(indexVar.String, out result)
                ? result
                : -1;
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

        public virtual bool AsBool()
        {
            return Type != VarType.BOOLEAN ? throw new ScriptException("型が一致しないか、変換できません。", Exceptions.WRONG_TYPE_VARIABLE) : Bool;
        }

        public virtual int AsInt(bool check = true)
        {
            if (check)
            {
                Utils.CheckNumInRange(this, true);
            }
            return (int)Value;
        }
        public virtual nint AsNInt(bool check = true)
        {
            if (check)
            {
                Utils.CheckNumInRange(this,true,0);
            }
            return (nint)Value;
        }
        public virtual float AsFloat(bool check = true)
        {
            if (check)
            {
                Utils.CheckNumber(this, null);
            }
            return (float)Value;
        }
        public virtual long AsLong(bool check = true)
        {
            if (check)
            {
                Utils.CheckNumber(this, null);
            }
            return (long)Value;
        }
        public virtual double AsDouble(bool check = true)
        {
            if (check)
            {
                Utils.CheckNumber(this, null);
            }
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
        /// <summary>
        /// この変数の種類を表すTypeオブジェクトを返します
        /// </summary>
        /// <returns>この変数の種類を表すTypeオブジェクト</returns>
        public virtual TypeObject AsType()
        {
            if (Tuple != null && Tuple.Type != null)
            {
                var to = new TypeObject(Type);
                to.ArrayType = Tuple.Type;
            }
            return Object != null && Object is AliceScriptClass c ? new TypeObject(c) : new TypeObject(Type);
        }
        public override string ToString()
        {
            return AsString();
        }

        /// <summary>
        /// この変数と指定されたオブジェクトまたはVariableが等価かどうかを評価します
        /// </summary>
        /// <param name="obj">評価する対象のオブジェクト</param>
        /// <returns>二つのオブジェクトが等しければTrue、それ以外の場合はFalse</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return IsNull();
            }
            if (obj is Variable item)
            {
                if (item.Type == VarType.NONE)
                {
                    return IsNull();
                }
                if (item.Type != Type)
                {
                    return false;
                }
                if (item.Type == VarType.NUMBER)
                {
                    return ValueEquals(item.Value);
                }
                if (item.Type == VarType.STRING)
                {
                    return ValueEquals(item.String);
                }
                if (item.Type == VarType.BOOLEAN)
                {
                    return ValueEquals(item.Bool);
                }
                if (item.Type == VarType.ARRAY)
                {
                    return ValueEquals(item.Tuple);
                }
                if (item.Type == VarType.DELEGATE)
                {
                    return ValueEquals(item.Delegate);
                }
                if (item.Type == VarType.BYTES)
                {
                    return ValueEquals(item.ByteArray);
                }
            }
            return ValueEquals(obj);
        }
        /// <summary>
        /// この変数のハッシュ値を求めます
        /// </summary>
        /// <returns>この変数のハッシュ値</returns>
        public override int GetHashCode()
        {
            int subhash = 0;
            switch (Type)
            {
                case VarType.NUMBER:
                    {
                        subhash = Value.GetHashCode();
                        break;
                    }
                case VarType.STRING:
                    {
                        subhash = String.GetHashCode();
                        break;
                    }
                case VarType.BOOLEAN:
                    {
                        subhash = Bool ? 1 : 0;
                        break;
                    }
                case VarType.ARRAY:
                    {
                        subhash = Tuple.GetHashCode();
                        break;
                    }
                case VarType.DELEGATE:
                    {
                        subhash = Delegate.GetHashCode();
                        break;
                    }
                case VarType.BYTES:
                    {
                        subhash = ByteArray.GetHashCode();
                        break;
                    }
                case VarType.OBJECT:
                    {
                        subhash = Object.GetHashCode();
                        break;
                    }
                default:
                    {
                        subhash = 255;
                        break;
                    }
            }
            return (int)Type ^ subhash;
        }

        /// <summary>
        /// この変数と指定されたオブジェクトが等価かどうかを評価します
        /// </summary>
        /// <param name="obj">評価する対象のオブジェクト</param>
        /// <returns>二つのオブジェクトが等しければTrue、それ以外の場合はFalse</returns>
        private bool ValueEquals(object obj)
        {
            return obj is double || obj is int || obj is decimal || obj is float
                ? Value == (double)obj
                : obj is string str
                ? string.Equals(String, str, StringComparison.Ordinal)
                : obj is bool bol
                ? Bool == bol
                : obj is VariableCollection tup
                ? Tuple == tup
                : obj is DelegateObject del
                ? Delegate == del
                : obj is byte[] data
                ? ByteArray == data
                : obj is ObjectBase ob && Object is ObjectBase ob2 ? ob.Equals(ob2) : obj.Equals(Object);
        }
        /// <summary>
        /// この変数と指定された変数を並べ替えるとき、どちらが前に来るかを比較します
        /// </summary>
        /// <param name="other">比較する変数</param>
        /// <returns>より前にくる場合は負の値、後にくる場合は正の値、一致する場合は0</returns>
        public int CompareTo(Variable? other)
        {
            return other == null
                ? 0
                : other.Type != Type
                ? 0
                : other.Type == VarType.NUMBER
                ? Value.CompareTo(other.Value)
                : other.Type == VarType.STRING
                ? String.CompareTo(other.String)
                : other.Type == VarType.BOOLEAN
                ? Bool.CompareTo(other.Bool)
                : other.Type == VarType.OBJECT && Object is ObjectBase ob ? ob.CompareTo(other.Object) : 0;
        }
        /// <summary>
        /// この変数を指定した型に変換します
        /// </summary>
        /// <typeparam name="T">変換先の型</typeparam>
        /// <returns>変換されたオブジェクト</returns>
        public T ConvertTo<T>()
        {
            return (T)ConvertTo(typeof(T));
        }
        /// <summary>
        /// この変数を指定した型に変換します
        /// </summary>
        /// <param name="type">変換先の型</param>
        /// <returns>変換されたオブジェクト</returns>
        /// <exception cref="ScriptException">型の不一致により変換できない場合にスローされる例外</exception>
        public object ConvertTo(Type type)
        {
            return TryConvertTo(type, out object o) ? o : throw new ScriptException("型が一致しないか、変換できません。", Exceptions.WRONG_TYPE_VARIABLE);
        }
        /// <summary>
        /// この変数を指定した型に変換できるか試みます
        /// </summary>
        /// <typeparam name="T">変換先の型</typeparam>
        /// <param name="result">変換されたオブジェクト</param>
        /// <returns>変換に成功した場合はTrue、それ以外の場合はfalse</returns>
        public bool TryConvertTo<T>(out T result)
        {
            bool r = TryConvertTo(typeof(T), out object obj);
            result = (T)obj;
            return r;
        }
        /// <summary>
        /// この変数を指定した型に変換できるか試みます
        /// </summary>
        /// <param name="type">変換先の型</param>
        /// <param name="result">変換されたオブジェクト</param>
        /// <returns>変換に成功した場合はTrue、それ以外の場合はfalse</returns>
        public bool TryConvertTo(Type type, out object result)
        {
            switch (Type)
            {
                case VarType.STRING:
                    {
                        if(type == typeof(string))
                        {
                            result = String;
                            return true;
                        }
                        break;
                    }
                case VarType.BOOLEAN:
                    {
                        if(type == typeof(bool))
                        {
                            result = Bool;
                            return true;
                        }
                        break;
                    }
                case VarType.BYTES:
                    {
                        if(type == typeof(byte[]))
                        {
                            result = ByteArray;
                            return true;
                        }
                        break;
                    }
                case VarType.NUMBER:
                    {
                        if(type == typeof(int))
                        {
                            result = AsInt();
                            return true;
                        }
                        if (type == typeof(nint))
                        {
                            result = AsNInt();
                            return true;
                        }
                        if (type == typeof(float))
                        {
                            result = AsFloat();
                            return true;
                        }
                        if(type == typeof(long))
                        {
                            result = AsLong();
                            return true;
                        }
                        if (type == typeof(double))
                        {
                            result = AsDouble();
                            return true;
                        }
                        break;
                    }
                case VarType.ARRAY:
                    {
                        if(Tuple.Type.Type == VarType.STRING && type == typeof(string[]))
                        {
                            var ary = new List<string>();
                            foreach(var v in Tuple)
                            {
                                ary.Add(v.String);
                            }
                            result = ary.ToArray();
                            return true;
                        }
                        if(type == typeof(VariableCollection))
                        {
                            result = Tuple;
                            return true;
                        }
                        if(type == typeof(Variable[]))
                        {
                            result = Tuple.ToArray();
                            return true;
                        }
                        if(type == typeof(List<Variable>))
                        {
                            result = Tuple.ToList();
                            return true;
                        }
                        break;
                    }
                case VarType.DELEGATE:
                    {
                        if(type == typeof(DelegateObject))
                        {
                            result = AsDelegate();
                            return true;
                        }
                        break;
                    }
                case VarType.OBJECT:
                    {
                        result = System.Convert.ChangeType(Object,type);
                        return true;
                    }
            }
            result = null;
            return false;
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
            switch (Type)
            {
                case VarType.BOOLEAN:
                    return Bool ? Constants.TRUE : Constants.FALSE;
                case VarType.NUMBER:
                    return Value.ToString();
                case VarType.STRING:
                    return m_string;
                case VarType.BYTES:
                    return SafeReader.ReadAllText(m_byteArray, out _);
                case VarType.UNDEFINED:
                    return Constants.UNDEFINED;
                case VarType.ENUM:
                    {
                        var sb = new StringBuilder();
                        sb.Append(Constants.START_GROUP.ToString());
                        sb.Append(Constants.SPACE);
                        foreach (string key in m_propertyMap.Keys)
                        {
                            sb.Append(key);
                            sb.Append(Constants.SPACE);
                        }
                        sb.Append(Constants.END_GROUP.ToString());
                        return sb.ToString();
                    }
                case VarType.OBJECT:
                    {
                        var sb = new StringBuilder();
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
                                sb.Append(prop);
                                sb.Append(value);
                                if (i < allProps.Count - 1)
                                {
                                    sb.Append(", ");
                                }
                            }

                            sb.Append(Constants.END_GROUP.ToString());
                        }
                        return sb.ToString();
                    }
                case VarType.ARRAY:
                    {
                        var sb = new StringBuilder();
                        if (isList)
                        {
                            sb.Append(Constants.START_ARRAY.ToString() +
                                     (sameLine ? "" : Environment.NewLine));
                        }
                        int count = maxCount < 0 ? m_tuple.Count : Math.Min(maxCount, m_tuple.Count);
                        int i = 0;
                        for (; i < count; i++)
                        {
                            Variable arg = m_tuple[i];
                            sb.Append(arg.AsString(isList, sameLine, maxCount));
                            if (i != count - 1)
                            {
                                sb.Append(sameLine ? ", " : Environment.NewLine);
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
                default:
                    {
                        return IsNull() ? null : string.Empty;
                    }
            }
        }

        public void Activate()
        {
            switch (Type)
            {
                case VarType.ARRAY:
                    {
                        if (m_tuple == null)
                        {
                            m_tuple = new VariableCollection();
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
                m_tuple = new VariableCollection();
            }
        }

        public int Count => Type == VarType.ARRAY ? m_tuple.Count :
                       Type == VarType.NONE ? 0 : 1;


        public Variable SetProperty(string propName, Variable value, ParsingScript script, string baseName = "")
        {
            int ind = propName.IndexOf('.', StringComparison.Ordinal);
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
            int ind = propName.IndexOf('.', StringComparison.Ordinal);
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
            string converted = Constants.ConvertName(name);
            if (!m_propertyStringMap.TryGetValue(converted, out string realName))
            {
                realName = name;
            }
            return realName;
        }

        public Variable FinishSetProperty(string propName, Variable value, ParsingScript script, string baseName = "")
        {
            Variable result = Variable.EmptyInstance;

            // Check for an existing custom setter
            if (m_propertyMap.TryGetValue(propName, out result) ||
                m_propertyMap.TryGetValue(GetRealName(propName), out result))
            {
                if (result.Readonly)
                {
                    throw new ScriptException("プロパティ:[" + propName + "]は読み取り専用です", Exceptions.CANT_ASSIGN_TO_READ_ONLY, script);
                }
                if (result.CustomFunctionSet != null)
                {
                    var args = new List<Variable> { value };
                    result.CustomFunctionSet.ARun(args, script);
                    return result;
                }
                if (!string.IsNullOrWhiteSpace(result.CustomSet))
                {
                    return ParsingScript.RunString(result.CustomSet, script);
                }
            }

            if (Object is ObjectBase ob)
            {
                ob.SetProperty(propName, value).Wait();
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
                return propName == Constants.TO_STRING
                    ? ConvertEnumToString(value)
                    : new Variable(m_enumMap != null && m_enumMap.ContainsKey(value.AsInt()));
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
            return m_enumMap != null && m_enumMap.TryGetValue(value.AsInt(), out string result) ? new Variable(result) : Variable.EmptyInstance;
        }

        public Variable GetProperty(string propName, ParsingScript script = null)
        {
            Variable result = Variable.EmptyInstance;

            int ind = propName.IndexOf('.', StringComparison.Ordinal);
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

                        args = script.GetFunctionArgs(null);
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

            int ind = propName.IndexOf('.', StringComparison.Ordinal);
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
                        args = script.GetFunctionArgs(null);
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
            propName = propName.ToLowerInvariant();

            if (m_propertyMap.TryGetValue(propName, out result) ||
                m_propertyMap.TryGetValue(GetRealName(propName), out result))
            {
                return result;
            }
            else if (script != null && Properties.TryGetValue(propName, out var p) && (p.Type.HasFlag(Type) || p.Type == Variable.VarType.NONE))
            {
                return p.GetProperty(this);
            }
            else if (script != null && Functions.TryGetValue(propName, out var f))
            {

                return f.Evaluate(script, this);
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
                allSet.Add(key.ToLowerInvariant());
                all.Add(key);
            }
            foreach (var fn in Functions)
            {
                FunctionBase fb = fn.Value;
                if (fb.RequestType.Match(this))
                {
                    all.Add(fn.Key);
                }
            }

            if (Object is ScriptObject)
            {
                ScriptObject obj = Object as ScriptObject;
                List<string> objProps = obj.GetProperties();
                foreach (string key in objProps)
                {
                    if (allSet.Add(key.ToLowerInvariant()))
                    {
                        all.Add(key);
                    }
                }
            }

            all.Sort();

            if (!allSet.Contains(Constants.OBJECT_TYPE.ToLowerInvariant()))
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
            return Type == VarType.OBJECT && Object != null
                ? Object is ObjectBase ? ((ObjectBase)Object).Name : Object.GetType().ToString()
                : Constants.TypeToString(Type);
        }

        public Variable GetValue(int index)
        {
            if (index >= Count)
            {
                throw new IndexOutOfRangeException("インデックス `" + index + "`は配列の境界 `" + Count + "` 外です。");

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
            VariableCollection newTuple = new VariableCollection();
            newTuple.Type = Tuple.Type;
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
            get => !m_value.HasValue ? throw new ScriptException("変数がnullです", Exceptions.VARIABLE_IS_NULL) : m_value.Value;
            set { m_value = value; Type = VarType.NUMBER; }
        }
        public virtual bool Bool
        {
            get => !m_bool.HasValue ? throw new ScriptException("変数がnullです", Exceptions.VARIABLE_IS_NULL) : m_bool.Value;
            set { m_bool = value; Type = VarType.BOOLEAN; }
        }
        public virtual string String
        {
            get => m_string;
            set { m_string = value; Type = VarType.STRING; }
        }

        public object Object
        {
            get => m_object;
            set { m_object = value; Type = VarType.OBJECT; }
        }
        public DelegateObject Delegate
        {
            get => m_delegate;
            set
            {
                m_delegate = value;
                Type = VarType.DELEGATE;
            }
        }

        public byte[] ByteArray
        {
            get => m_byteArray;
            set { m_byteArray = value; Type = VarType.BYTES; }
        }


        public CustomFunction CustomFunctionGet
        {
            get => m_customFunctionGet;
            set => m_customFunctionGet = value;
        }
        public CustomFunction CustomFunctionSet
        {
            get => m_customFunctionSet;
            set => m_customFunctionSet = value;
        }

        public VariableCollection Tuple
        {
            get => m_tuple;
            set { m_tuple = value; Type = VarType.ARRAY; }
        }

        public string Action { get; set; }
        /// <summary>
        /// この変数の型
        /// </summary>
        public VarType Type
        {
            get => m_type;
            set => m_type = value;
        }
        /// <summary>
        /// これが関数の戻り値などである場合はTrue
        /// </summary>
        public bool IsReturn { get; set; }
        public string ParsingToken { get; set; }
        public int Index { get; set; }
        public string CurrentAssign { get; set; } = "";
        public string ParamName { get; set; } = "";

        /// <summary>
        /// この変数が型検査を受ける場合はtrue
        /// </summary>
        public bool TypeChecked { get; set; }

        /// <summary>
        /// この変数がどのような型でもnullをとりうる場合はtrue
        /// </summary>
        public bool Nullable { get; set; }
        /// <summary>
        /// この変数が読み取り専用の場合はtrue。
        /// </summary>
        public bool Readonly { get; set; }
        public bool Enumerable { get; set; } = true;
        public bool Configurable { get; set; } = true;

        public string CustomGet { get; set; }
        public string CustomSet { get; set; }
        public object Tag { get; set; }
        public HashSet<string> Keywords
        {
            get => m_keywords;
            set => m_keywords = value;
        }

        /// <summary>
        /// この変数が定義された元のスクリプトを表します
        /// </summary>
        public ParsingScript Parent { get; set; }

        public List<Variable> StackVariables { get; set; }

        public static Variable EmptyInstance
        {
            get
            {
                var v = new Variable();
                v.AssignNull();//念のためnullにする
                return v;
            }
        }
        public static Variable Undefined = new Variable(VarType.UNDEFINED);

        internal double? m_value = default;
        internal string m_string = default;
        internal object m_object = null;
        internal bool? m_bool = default;
        internal DelegateObject m_delegate = null;
        internal VarType m_type;
        private CustomFunction m_customFunctionGet;
        private CustomFunction m_customFunctionSet;
        protected VariableCollection m_tuple = null;
        protected byte[] m_byteArray = null;
        private HashSet<string> m_keywords = new HashSet<string>();
        private Dictionary<string, int> m_dictionary = new Dictionary<string, int>();
        private Dictionary<string, string> m_keyMappings = new Dictionary<string, string>();
        private Dictionary<string, string> m_propertyStringMap = new Dictionary<string, string>();
        private Dictionary<string, Variable> m_propertyMap = new Dictionary<string, Variable>();
        private Dictionary<int, string> m_enumMap;
    }

    // ピリオドを使ってプロパティを参照できるオブジェクト
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
