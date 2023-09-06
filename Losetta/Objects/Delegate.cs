﻿using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    public class DelegateObject
    {
        private List<CustomFunction> m_fucntions = new List<CustomFunction>();

        public List<CustomFunction> Functions
        {
            get => m_fucntions;
            set => m_fucntions = value;
        }
        public CustomFunction Function
        {
            get
            {
                CustomFunction r = null;
                for (int i = 0; i < m_fucntions.Count; i++)
                {
                    if (i == 0)
                    {
                        r = m_fucntions[i];
                        r.Children = new HashSet<CustomFunction>();
                    }
                    else
                    {
                        r.Children.Add(m_fucntions[i]);
                    }
                }
                return r;
            }
            set
            {
                m_fucntions.Clear();
                m_fucntions.Add(value);
            }
        }
        public int Length => m_fucntions.Count;
        public string Name => m_fucntions.Count == 0 ? string.Empty : m_fucntions[0].Name;
        public DelegateObject()
        {

        }
        public DelegateObject(CustomFunction func)
        {
            m_fucntions.Add(func);
        }
        public DelegateObject(DelegateObject d)
        {
            m_fucntions.AddRange(d.Functions);
        }
        public void Add(CustomFunction func)
        {
            m_fucntions.Add(func);
        }
        public void Add(DelegateObject d)
        {
            m_fucntions.AddRange(d.Functions);
        }
        public bool Remove(CustomFunction func)
        {
            return m_fucntions.Remove(func);
        }
        public bool Remove(DelegateObject d)
        {
            foreach (CustomFunction c in d.Functions)
            {
                if (!Functions.Remove(c))
                {
                    return false;
                }
            }
            return true;
        }
        public bool Contains(CustomFunction func)
        {
            return m_fucntions.Contains(func);
        }
        public bool Contains(DelegateObject d)
        {
            bool r = false;
            foreach (CustomFunction cf in d.Functions)
            {
                if (!m_fucntions.Contains(cf))
                {
                    //一つでも異なればFalse
                    return false;
                }
                else
                {
                    r = true;
                }
            }
            return r;
        }
        public Variable Invoke(List<Variable> args = null, ParsingScript script = null, AliceScriptClass.ClassInstance instance = null)
        {
            Variable last_result = Variable.EmptyInstance;
            foreach (CustomFunction func in m_fucntions)
            {
                last_result = func.ARun(args, script, instance);
            }
            return last_result;
        }
        public void BeginInvoke(List<Variable> args = null, ParsingScript script = null, AliceScriptClass.ClassInstance instance = null)
        {
            m_BeginInvokeMessanger mb = new m_BeginInvokeMessanger();
            mb.Delegate = this;
            mb.Args = args;
            mb.Script = script;
            mb.Instance = instance;
            ThreadPool.QueueUserWorkItem(ThreadProc, mb);
        }

        private static void ThreadProc(object stateInfo)
        {
            m_BeginInvokeMessanger mb = (m_BeginInvokeMessanger)stateInfo;
            mb.Delegate.Invoke(mb.Args, mb.Script, mb.Instance);
        }
        private class m_BeginInvokeMessanger
        {
            public DelegateObject Delegate { get; set; }
            public List<Variable> Args { get; set; }
            public ParsingScript Script { get; set; }
            public AliceScriptClass.ClassInstance Instance { get; set; }
        }
        public bool Equals(DelegateObject d)
        {
            //要素数が異なるときはもちろん異なる
            if (Length != d.Length)
            {
                return false;
            }

            for (int i = 0; i < d.Length; i++)
            {
                if (Functions[i] != d.Functions[i])
                {
                    //一つでも違えば異なる
                    return false;
                }
            }
            return true;
        }
    }
    /// <summary>
    /// C#からAliceScriptにイベントを通知するためのプロパティベースです。
    /// </summary>
    public class EventBase : PropertyBase
    {
        public EventBase()
        {
            CanSet = true;
            Value = new Variable(Variable.VarType.DELEGATE);
        }
        public EventBase(string name)
        {
            Name = name;
            CanSet = true;
            Value = new Variable(Variable.VarType.DELEGATE);
        }
        /// <summary>
        /// イベントを実行します
        /// </summary>
        /// <param name="args">実行の引数</param>
        /// <param name="script">親スクリプト</param>
        /// <param name="instance">実行元のクラスインスタンス</param>
        /// <returns>実行結果</returns>
        public Variable Invoke(List<Variable> args, ParsingScript script, AliceScriptClass.ClassInstance instance = null)
        {
            Variable result = Variable.EmptyInstance;

            if (Value.Type == Variable.VarType.DELEGATE && Value.Delegate != null)
            {
                result = Value.Delegate.Invoke(args, script, instance);
            }

            return result;
        }
    }
}