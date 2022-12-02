using AliceScript;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces
{
    static class Alice_Threading_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Threading");

            space.Add(new thread_idFunc());
            space.Add(new thread_queueFunc());
            space.Add(new SignalWaitFunction(true));
            space.Add(new SignalWaitFunction(false));
            space.Add(new task_runFunc());

            NameSpaceManerger.Add(space);
        }
    }
    class thread_idFunc : FunctionBase
    {
        public thread_idFunc()
        {
            this.Name = "thread_id";
            this.MinimumArgCounts = 0;
            this.Run += Thread_idFunc_Run;
        }

        private void Thread_idFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Thread.CurrentThread.ManagedThreadId);
        }
    }
    class thread_queueFunc : FunctionBase
    {
        public thread_queueFunc()
        {
            this.Name = "thread_queue";
            this.MinimumArgCounts = 1;
            this.Attribute = FunctionAttribute.CONTROL_FLOW;
            this.Run += Thread_queueFunc_Run;
        }

        private void Thread_queueFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { ThrowErrorManerger.OnThrowError("不正な引数です",Exceptions.WRONG_TYPE_VARIABLE,e.Script); }
            ThreadQueueStateInfo tqsi = new ThreadQueueStateInfo();
            tqsi.Delegate = e.Args[0].Delegate;
            tqsi.Script = e.Script;
            if (e.Args.Count > 1)
            {
                tqsi.Args = e.Args.GetRange(1, e.Args.Count - 1);
            }
            ThreadPool.QueueUserWorkItem(ThreadProc,tqsi);
            e.Return = Variable.EmptyInstance;
        }
        static void ThreadProc(Object stateInfo)
        {
            ThreadQueueStateInfo tqsi = (ThreadQueueStateInfo)stateInfo;
            tqsi.Delegate.Invoke(tqsi.Args,tqsi.Script);
        }
    }
    class ThreadQueueStateInfo
    {
        public List<Variable> Args { get; set; }
        public ParsingScript Script { get; set; }
        public DelegateObject Delegate { get; set; }
    }
    class task_runFunc : FunctionBase
    {
        public task_runFunc()
        {
            this.Name = "task_run";
            this.MinimumArgCounts = 0;
            this.Run += Task_runFunc_Run;
        }

        private void Task_runFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args[0].Type != Variable.VarType.DELEGATE) { ThrowErrorManerger.OnThrowError("不正な引数です",Exceptions.WRONG_TYPE_VARIABLE,e.Script); }
            List<Variable> args = new List<Variable>();
            if (e.Args.Count > 1)
            {
                args = e.Args.GetRange(1, e.Args.Count - 1);
            }
            Task.Run(()=> { e.Args[0].Delegate.Invoke(args,e.Script); });
        }
    }
    class SignalWaitFunction : FunctionBase
    {
        static AutoResetEvent waitEvent = new AutoResetEvent(false);
        bool m_isSignal;

        public SignalWaitFunction(bool isSignal)
        {
            m_isSignal = isSignal;
            this.Attribute = FunctionAttribute.CONTROL_FLOW | FunctionAttribute.FUNCT_WITH_SPACE|FunctionAttribute.LANGUAGE_STRUCTURE;
            if (isSignal)
            {
                this.Name = "signal";
            }
            else
            {
                this.Name = "signal_wait";
            }
            this.Run += SignalWaitFunction_Run;
        }

        private void SignalWaitFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            bool result = m_isSignal ? waitEvent.Set() :
                                      waitEvent.WaitOne();
            e.Return=new Variable(result);
        }

    }



}
