using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace AliceScript
{
    public class DelegateObject
    {
        private List<CustomFunction> m_fucntions = new List<CustomFunction>();

        public List<CustomFunction> Functions
        {
            get
            {
                return m_fucntions;
            }
            set
            {
                m_fucntions = value;
            }
        }
        public CustomFunction Function
        {
            get
            {
                CustomFunction r=null;
                for (int i = 0; i < m_fucntions.Count; i++)
                {
                    if (i == 0)
                    {
                        r= m_fucntions[i];
                        r.Children = new List<CustomFunction>();
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
        public int Length
        {
            get
            {
                return m_fucntions.Count;
            }
        }

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
            foreach(CustomFunction c in d.Functions)
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
            foreach(CustomFunction cf in d.Functions)
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
        public Variable Invoke(List<Variable> args=null,ParsingScript script=null,AliceScriptClass.ClassInstance instance=null)
        {
            Variable last_result = Variable.EmptyInstance;
            foreach(CustomFunction func in m_fucntions)
            {
                last_result=func.Run(args,script,instance);
            }
            return last_result;
        }
        public void BeginInvoke(List<Variable> args=null,ParsingScript script=null,AliceScriptClass.ClassInstance instance = null)
        {
            m_BeginInvokeMessanger mb = new m_BeginInvokeMessanger();
            mb.Delegate = this;
            mb.Args = args;
            mb.Script = script;
            mb.Instance = instance;
            ThreadPool.QueueUserWorkItem(ThreadProc, mb);
        }
        static void ThreadProc(Object stateInfo)
        {
            m_BeginInvokeMessanger mb = (m_BeginInvokeMessanger)stateInfo;
            mb.Delegate.Invoke(mb.Args,mb.Script,mb.Instance);
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
            if (this.Length != d.Length)
                return false;
            
            for (int i = 0; i < d.Length; i++)
            {
                if (this.Functions[i] != d.Functions[i])
                {
                    //一つでも違えば異なる
                    return false;
                }
            }
            return true;
        }
    }
}
