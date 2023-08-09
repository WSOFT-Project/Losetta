namespace AliceScript
{

    public static class NameSpaceManerger
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>();
        public static void Add(NameSpace space, string name = null)
        {
            if (name == null) { name = space.Name; }
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
        public List<ObjectBase> Classes = new List<ObjectBase>();
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public void Add(FunctionBase func)
        {
            Functions.Add(func);
        }
        public void Add(ObjectBase obj)
        {
            obj.Namespace = this.Name;
            Classes.Add(obj);
        }
        public void Add(string name, string val)
        {
            Enums.Add(name, val);
        }
        public void Remove(FunctionBase func)
        {
            Functions.Remove(func);
        }

        public int Count => Functions.Count + Classes.Count;
        /// <summary>
        /// 現在の名前空間にもう一方の名前空間をマージします。ただし、列挙体はマージされません。
        /// </summary>
        /// <param name="other">マージする名前空間</param>
        public void Merge(NameSpace other)
        {
            this.Functions = this.Functions.Union(other.Functions).ToList();
            this.Classes = this.Classes.Union(other.Classes).ToList();
        }
    }


}
