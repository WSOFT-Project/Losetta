using System;
using System.Collections.Generic;
using System.IO;

namespace AliceScript
{
   
    public static class NameSpaceManerger
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>();
        public static void Add(NameSpace space, string name = "")
        {
            if (name == "") { name = space.Name; }
            NameSpaces.Add(name, space);
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
        public void Clear()
        {
            Functions.Clear();
        }
        public event EventHandler<ImportEventArgs> Loading;
        public event EventHandler<ImportEventArgs> UnLoading;
        
        public int Count
        {
            get
            {
                return Functions.Count + Classes.Count;
            }
        }

    }
    public class ImportEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
        public ParsingScript Script { get; set; }
    }

   

}
