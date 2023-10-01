using AliceScript.Binding;
using AliceScript.Objects;
using AliceScript.Parsing;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Threading
    {
        public static void Init()
        {
            Alice.RegisterFunctions<ThreadingFunctions>();
        }
    }

    [AliceNameSpace(Name = "Alice.Threading")]
    internal sealed class ThreadingFunctions
    {
        public static int Thread_Id()
        {
            return Thread.CurrentThread.ManagedThreadId;
        }
        public static void Thread_Queue(ParsingScript script, DelegateObject d, params Variable[] args)
        {
            ThreadQueueStateInfo tqsi = new ThreadQueueStateInfo();
            tqsi.Delegate = d;
            tqsi.Script = script;
            tqsi.Args = args.ToList();
            ThreadPool.QueueUserWorkItem(ThreadProc, tqsi);
        }
        private static void ThreadProc(object stateInfo)
        {
            ThreadQueueStateInfo tqsi = (ThreadQueueStateInfo)stateInfo;
            tqsi.Delegate.Invoke(tqsi.Args, tqsi.Script);
        }
        public static void Task_Run(DelegateObject d, ParsingScript script)
        {
            Task.Run(() => d.Invoke(Variable.EmptyInstance, script));
        }
        public static void Task_Run(DelegateObject d, ParsingScript script, params Variable[] args)
        {
            Task.Run(() => d.Invoke(args.ToList(), script));
        }

        private static AutoResetEvent waitEvent = new AutoResetEvent(false);
        public static bool Signal()
        {
            return waitEvent.Set();
        }
        public static bool Signal_Wait()
        {
            return waitEvent.WaitOne();
        }
        public static bool Signal_Wait(int timeout)
        {
            return waitEvent.WaitOne(timeout);
        }
        public static bool Signal_Wait(int timeout, bool waitContext)
        {
            return waitEvent.WaitOne(timeout, waitContext);
        }
    }

    internal sealed class ThreadQueueStateInfo
    {
        public List<Variable> Args { get; set; }
        public ParsingScript Script { get; set; }
        public DelegateObject Delegate { get; set; }
    }
}
