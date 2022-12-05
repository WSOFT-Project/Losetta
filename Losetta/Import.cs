﻿using System;
using System.Collections.Generic;
using System.IO;

namespace AliceScript
{
    internal class Import
    {

    }
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
        public static void Load(string name, ParsingScript script)
        {
            NameSpaces[name].Load(script);
        }
        public static void UnLoad(string name, ParsingScript script)
        {
            NameSpaces[name].UnLoad(script);
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
        public virtual void Load(ParsingScript script)
        {
            int ecount = 0;
            ImportEventArgs e = new ImportEventArgs();
            e.Cancel = false;
            e.Script = script;
            Loading?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Add(func,func.Name,script);
                }
                catch { ecount++; }
            }
            foreach (ObjectBase obj in Classes)
            {
                try
                {
                    ClassManerger.Add(obj,script);
                }
                catch { ecount++; }
            }
            foreach (string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.RegisterEnum(s, Enums[s],script);
                }
                catch { ecount++; }
            }

            if (ecount != 0) { throw new Exception("名前空間のロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
        public virtual void UnLoad(ParsingScript script)
        {
            int ecount = 0;
            ImportEventArgs e = new ImportEventArgs();
            e.Cancel = false;
            e.Script = script;
            UnLoading?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }
            foreach (FunctionBase func in Functions)
            {
                try
                {
                    FunctionBaseManerger.Remove(func,func.Name,script);
                }
                catch { ecount++; }
            }
            foreach (ObjectBase obj in Classes)
            {
                try
                {
                    ClassManerger.Remove(obj,script);
                }
                catch { ecount++; }
            }
            foreach (string s in Enums.Keys)
            {
                try
                {
                    FunctionBase.UnregisterScriptFunction(s,script);
                }
                catch { ecount++; }
            }
            if (ecount != 0) { throw new Exception("名前空間のアンロード中に" + ecount + "件の例外が発生しました。これらの例外は捕捉されませんでした"); }
        }
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
