using AliceScript.Functions;
using AliceScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceScript.NameSpaces
{

    public static class NameSpaceManager
    {
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>(StringComparer.OrdinalIgnoreCase)
        {
            {Constants.TOP_NAMESPACE,new NameSpace() }
        };
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
        /// <summary>
        /// 指定した名前の名前空間が存在するかどうかを表す値を取得します。
        /// </summary>
        /// <param name="name">確認したい名前空間の名前</param>
        /// <returns>nameと一致する名前空間が存在する場合はtrue、それ以外の場合はfalse。</returns>
        public static bool Contains(string name)
        {
            return NameSpaces.ContainsKey(name.ToLowerInvariant());
        }
        /// <summary>
        /// 指定した名前の名前空間オブジェクトを取得します
        /// </summary>
        /// <param name="name">取得したい名前空間の名前</param>
        /// <returns>nameと一致する名前空間が存在する場合はそのオブジェクト、存在しない場合はnull。</returns>
        public static NameSpace Get(string name)
        {
            return NameSpaces.TryGetValue(name.ToLowerInvariant(), out var value) ? value : null;
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
        public Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, FunctionBase> InternalFunctions = new Dictionary<string, FunctionBase>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        public void Add(FunctionBase func, bool throwError = false, AccessModifier accessModifier = AccessModifier.PUBLIC)
        {
            func.RelatedNameSpace = Name;
            if (accessModifier == AccessModifier.PUBLIC)
            {
                if (!Functions.TryGetValue(func.Name, out var f) || f.IsVirtual)
                {
                    Functions[func.Name.ToLowerInvariant()] = func;
                    if (f is not null)
                    {
                        f.IsVirtual = true;
                    }
                }
                else if (throwError)
                {
                    throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                }
            }
            else if (accessModifier == AccessModifier.PROTECTED)
            {
                if (!InternalFunctions.TryGetValue(func.Name, out var f) || f.IsVirtual)
                {
                    InternalFunctions[func.Name.ToLowerInvariant()] = func;
                    if (f is not null)
                    {
                        f.IsVirtual = true;
                    }
                }
                else if (throwError)
                {
                    throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                }
            }
        }
        public void Add(ObjectBase obj)
        {
            obj.Namespace = Name;
            var func = new ValueFunction(Variable.From(new TypeObject(obj)));
            func.Name = obj.Name;
            Add(func);
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
            Functions = Functions.Union(other.Functions).ToDictionary(x => x.Key, y => y.Value);
        }
    }


}
