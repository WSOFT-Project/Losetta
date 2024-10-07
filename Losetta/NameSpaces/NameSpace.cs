using AliceScript.Functions;
using AliceScript.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AliceScript.NameSpaces
{

    public static class NameSpaceManager
    {
        public readonly static NameSpace TopLevel = new NameSpace(Constants.TOP_NAMESPACE);
        public static Dictionary<string, NameSpace> NameSpaces = new Dictionary<string, NameSpace>(StringComparer.OrdinalIgnoreCase)
        {
            {Constants.TOP_NAMESPACE,new NameSpace() }
        };
        public static void Add(NameSpace space, string name = null)
        {
            TopLevel.Add(space, name);
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
        public static bool TryGetNameSpace(string name, out NameSpace space)
        {
            return TopLevel.TryGetValue(name, out space);
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
    public class NameSpace : FunctionBase
    {
        public NameSpace()
        {
            //Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += NameSpace_Run;
        }
        public NameSpace(string name)
        {
            Name = name;
            //Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += NameSpace_Run;
        }
        private void NameSpace_Run(object sender, FunctionBaseEventArgs e)
        {
            throw new ScriptException($"{Name}は名前空間です。これは、特定のコンテキストでは無効になります。", Exceptions.NONE);
        }
        public Dictionary<string, FunctionBase> Functions = new Dictionary<string, FunctionBase>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, FunctionBase> InternalFunctions = new Dictionary<string, FunctionBase>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> Enums = new Dictionary<string, string>();
        
        public FunctionBase GetFunction(string name)
        {
            if (Functions.TryGetValue(name.ToLowerInvariant(), out var func))
            {
                return func;
            }
            return null;
        }
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
            var func = new ValueFunction(new Variable(new TypeObject(obj)));
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
        public void Add(NameSpace space, string name = null)
        {
            if (name is null) { name = space.Name; }
            name = name.ToLowerInvariant();

            // 名前空間をピリオドごとに分ける
            var ns = name.Split('.').AsSpan();

            Add(space, ns);
        }
        internal void Add(NameSpace space, Span<string> ns)
        {
            string name = ns[0];
            // これが最後じゃない場合
            if (ns.Length > 1)
            {
                // まだないなら作っておく
                if (!Functions.ContainsKey(name))
                {
                    Functions.Add(name, new NameSpace(name));
                }
                // 下の階層に追加
                var func = Functions[name];
                if(func is NameSpace sp)
                {
                    sp.Add(space, ns.Slice(1));
                }
                else
                {
                    throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                }
            }
            else
            {
                // ここに追加すべき場合
                if (Functions.ContainsKey(name))
                {
                    // 既に存在する場合はマージ
                    if(Functions[name] is NameSpace sp)
                    {
                        sp.Merge(space);
                    }
                    else
                    {
                        throw new ScriptException("指定された名前はすでに使用されていて、オーバーライドできません", Exceptions.FUNCTION_IS_ALREADY_DEFINED);
                    }
                }
                else
                {
                    Functions.Add(name, space);
                }
            }
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
        public bool TryGetValue(string name, out NameSpace space)
        {
            name = name.ToLowerInvariant();

            // 名前空間をピリオドごとに分ける
            var ns = name.Split('.').AsSpan();

            return TryGetValue(ns, out space);
        }
        private bool TryGetValue(Span<string> ns, out NameSpace space)
        {
            string name = ns[0];
            if(Functions.TryGetValue(name, out var func) && func is NameSpace sp)
            {
                space = sp;
                if (ns.Length > 1)
                {
                    return space.TryGetValue(ns.Slice(1), out space);
                }
                return true;
            }
            space = null;
            return false;
        }
        
    }


}
