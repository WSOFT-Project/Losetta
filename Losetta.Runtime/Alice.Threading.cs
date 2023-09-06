using AliceScript.Functions;
using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Threading
    {
        public static void Init()
        {
            try
            {
                NameSpace space = new NameSpace("Alice.Threading");

                space.Add(new thread_idFunc());
                space.Add(new thread_queueFunc());
                space.Add(new SignalWaitFunction(true));
                space.Add(new SignalWaitFunction(false));
                space.Add(new task_runFunc());

                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }

    internal sealed class thread_idFunc : FunctionBase
    {
        public thread_idFunc()
        {
            Name = "thread_id";
            MinimumArgCounts = 0;
            Run += Thread_idFunc_Run;
        }

        private void Thread_idFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Thread.CurrentThread.ManagedThreadId);
        }
    }

    internal sealed class thread_queueFunc : FunctionBase
    {
        public thread_queueFunc()
        {
            Name = "thread_queue";
            MinimumArgCounts = 1;
            Attribute = FunctionAttribute.CONTROL_FLOW;
            Run += Thread_queueFunc_Run;
        }

        private void Thread_queueFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { throw new ScriptException("引数が不正です", Exceptions.WRONG_TYPE_VARIABLE, e.Script); }
            ThreadQueueStateInfo tqsi = new ThreadQueueStateInfo();
            tqsi.Delegate = e.Args[0].Delegate;
            tqsi.Script = e.Script;
            if (e.Args.Count > 1)
            {
                tqsi.Args = e.Args.GetRange(1, e.Args.Count - 1);
            }
            ThreadPool.QueueUserWorkItem(ThreadProc, tqsi);
            e.Return = Variable.EmptyInstance;
        }

        private static void ThreadProc(object stateInfo)
        {
            ThreadQueueStateInfo tqsi = (ThreadQueueStateInfo)stateInfo;
            tqsi.Delegate.Invoke(tqsi.Args, tqsi.Script);
        }
    }

    internal sealed class ThreadQueueStateInfo
    {
        public List<Variable> Args { get; set; }
        public ParsingScript Script { get; set; }
        public DelegateObject Delegate { get; set; }
    }

    internal sealed class task_runFunc : FunctionBase
    {
        public task_runFunc()
        {
            Name = "task_run";
            MinimumArgCounts = 0;
            Run += Task_runFunc_Run;
        }

        private void Task_runFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { throw new ScriptException("不正な引数です", Exceptions.WRONG_TYPE_VARIABLE, e.Script); }
            else
            {
                List<Variable> args = new List<Variable>();
                if (e.Args.Count > 1)
                {
                    args = e.Args.GetRange(1, e.Args.Count - 1);
                }
                Task.Run(() => { e.Args[0].Delegate.Invoke(args, e.Script); });
            }
        }
    }

    internal sealed class SignalWaitFunction : FunctionBase
    {
        private static AutoResetEvent waitEvent = new AutoResetEvent(false);
        private bool m_isSignal;

        public SignalWaitFunction(bool isSignal)
        {
            m_isSignal = isSignal;
            Attribute = FunctionAttribute.CONTROL_FLOW | FunctionAttribute.FUNCT_WITH_SPACE | FunctionAttribute.LANGUAGE_STRUCTURE;
            Name = isSignal ? "signal" : "signal_wait";
            Run += SignalWaitFunction_Run;
        }

        private void SignalWaitFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool result = m_isSignal ? waitEvent.Set() :
                                      waitEvent.WaitOne();
            e.Return = new Variable(result);
        }

    }



}
