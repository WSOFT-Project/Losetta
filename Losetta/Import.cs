namespace AliceScript
{

    public static class NameSpaceManerger
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>();
        public static void Add(NameSpace space, string name = null)
        {
            if (name == null) { name = space.Name; }
            name = name.ToLower();
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
        public HashSet<FunctionBase> Functions = new HashSet<FunctionBase>();
        public HashSet<ObjectBase> Classes = new HashSet<ObjectBase>();
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public void Add(FunctionBase func)
        {
            func.RelatedNameSpace = Name;
            Functions.Add(func);
        }
        public void Add(ObjectBase obj)
        {
            obj.Namespace = Name;
            Classes.Add(obj);
        }
        public void Add(string name, string val)
        {
            Enums.Add(name, val);
        }

        public int Count => Functions.Count + Classes.Count;
        /// <summary>
        /// 現在の名前空間にもう一方の名前空間をマージします。ただし、列挙体はマージされません。
        /// </summary>
        /// <param name="other">マージする名前空間</param>
        public void Merge(NameSpace other)
        {
            foreach(FunctionBase f in other.Functions)
            {
                this.Functions.Add(f);
            }
            foreach(ObjectBase obj in other.Classes)
            {
                this.Classes.Add(obj);
            }
        }
    }


}
