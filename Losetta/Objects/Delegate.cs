using AliceScript.Functions;
using AliceScript.Parsing;

namespace AliceScript.Objects
{
    /// <summary>
    /// デリゲート（匿名関数）
    /// </summary>
    public class DelegateObject
    {
        public List<FunctionBase> m_fucntions = new List<FunctionBase>();
        public List<FunctionBase> Functions
        {
            get => m_fucntions;
            set => m_fucntions = value;
        }
        public FunctionBase Function
        {
            get
            {
                FunctionBase r = null;
                for (int i = 0; i < m_fucntions.Count; i++)
                {
                    if (i == 0)
                    {
                        r = m_fucntions[i];
                        if (r is CustomFunction cf)
                        {
                            cf.Children = new HashSet<CustomFunction>();
                        }
                    }
                    else
                    {
                        if (r is CustomFunction cf && m_fucntions[i] is CustomFunction cf2)
                        {
                            cf.Children.Add(cf2);
                        }
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
        public DelegateObject(FunctionBase func)
        {
            m_fucntions.Add(func);
        }
        public DelegateObject(DelegateObject d)
        {
            m_fucntions.AddRange(d.Functions);
        }
        public void Add(FunctionBase func)
        {
            m_fucntions.Add(func);
        }
        public void Add(DelegateObject d)
        {
            m_fucntions.AddRange(d.Functions);
        }
        public bool Remove(FunctionBase func)
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
        public bool Contains(FunctionBase func)
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
        public Variable Invoke(Variable arg = null, ParsingScript script = null, AliceScriptClass.ClassInstance instance = null)
        {
            var args = new List<Variable>();
            if (arg is not null)
            {
                args.Add(arg);
            }
            return Invoke(args, script, instance);
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
    public class EventBase : ValueFunction
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

            if (Value.Type == Variable.VarType.DELEGATE && Value.Delegate is not null)
            {
                result = Value.Delegate.Invoke(args, script, instance);
            }

            return result;
        }
    }
}
