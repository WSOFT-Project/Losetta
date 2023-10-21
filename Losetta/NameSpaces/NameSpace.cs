using AliceScript.Functions;
using AliceScript.Objects;

namespace AliceScript.NameSpaces
{

    public static class NameSpaceManager
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>();
        public static void Add(NameSpace space, string name = null)
        {
            if (name is null) { name = space.Name; }
            name = name.ToLowerInvariant();
            if (NameSpaces.ContainsKey(name))
            {
                //既に存在する場合はマージ
                NameSpaces[name].Merge(space);
            }
            else
            {
                NameSpaces.Add(name, space);
            }
        }
        public static void Add(Type type, string name = null)
        {
            Add(Utils.BindToNameSpace(type), name);
        }
        public static void AddObj(Type type, string name = null)
        {
            var obj = Utils.CreateBindObject(type);
            if (name is null) { name = obj.Namespace; }
            if (string.IsNullOrEmpty(name)) { name = Constants.TOP_NAMESPACE; }
            if (!NameSpaces.ContainsKey(name))
            {
                Add(new NameSpace(name));
            }
            NameSpaces[name].Add(obj);
        }
        public static bool Contains(NameSpace name)
        {
            return NameSpaces.ContainsValue(name);
        }
        public static bool Contains(string name)
        {
            return NameSpaces.ContainsKey(name);
        }
    }
    public class NameSpace
    {
        public NameSpace()
        {

        }
        public NameSpace(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
        public List<FunctionBase> Functions = new List<FunctionBase>();
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public void Add(FunctionBase func)
        {
            func.RelatedNameSpace = Name;
            Functions.Add(func);
        }
        public void Add(ObjectBase obj)
        {
            obj.Namespace = Name;
            var func = new ValueFunction(new Variable(new TypeObject(obj)));
            func.Name = obj.Name;
            Functions.Add(func);
        }
        public void Add<T>()
        {
            Add(Utils.CreateBindObject(typeof(T)));
        }
        public void Add(string name, string val)
        {
            Enums.Add(name, val);
        }

        public int Count => Functions.Count;
        /// <summary>
        /// 現在の名前空間にもう一方の名前空間をマージします。ただし、列挙体はマージされません。
        /// </summary>
        /// <param name="other">マージする名前空間</param>
        public void Merge(NameSpace other)
        {
            Functions = Functions.Union(other.Functions).ToList();
        }
    }


}
