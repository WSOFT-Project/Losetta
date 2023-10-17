using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Objects;
using System.Diagnostics;
using System.Text;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Diagnostics
    {
        public static void Init()
        {
            Alice.RegisterFunctions<DiagnosticsFunctions>();

            NameSpace space = new NameSpace("Alice.Diagnostics");

            space.Add(new StopWatchObject());
            space.Add<ProcessObject>();

            space.Add("ProcessWindowStyle", "System.Diagnostics.ProcessWindowStyle");

            NameSpaceManager.Add(space);
        }
    }
    [AliceNameSpace(Name = "Alice.Diagnostics")]
    internal sealed class DiagnosticsFunctions
    {
        #region プロセス操作
        public static ProcessObject[] Process_GetProcessesByName(string name)
        {
            Process[] ps = Process.GetProcessesByName(name);
            List<ProcessObject> pss = new List<ProcessObject>();
            foreach (Process p in ps)
            {
                ProcessObject po = new ProcessObject();
                po.Process = p;
                pss.Add(po);
            }
            return pss.ToArray();
        }
        public static ProcessObject[] Process_GetProcessesByName(string name, string machineName)
        {
            Process[] ps = Process.GetProcessesByName(name, machineName);
            List<ProcessObject> pss = new List<ProcessObject>();
            foreach (Process p in ps)
            {
                ProcessObject po = new ProcessObject();
                po.Process = p;
                pss.Add(po);
            }
            return pss.ToArray();
        }
        public static ProcessObject Process_GetProcessById(int id)
        {
            ProcessObject po = new ProcessObject();
            po.Process = Process.GetProcessById(id);
            return po;
        }
        public static ProcessObject Process_GetProcessById(int id, string machineName)
        {
            ProcessObject po = new ProcessObject();
            po.Process = Process.GetProcessById(id, machineName);
            return po;
        }
        public static ProcessObject Process_Start(string path, string arguments = "")
        {
            var po = new ProcessObject
            {
                Process = Process.Start(path, arguments)
            };
            return po;
        }
        public static ProcessObject Exec(string fileName, bool waitForExit = true, bool useShell = false)
        {
            var p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.RedirectStandardInput = !useShell;
            p.StartInfo.RedirectStandardOutput = !useShell;
            p.StartInfo.UseShellExecute = useShell;

            p.Start();

            if (waitForExit)
            {
                p.WaitForExit();
            }
            return new ProcessObject()
            {
                Process = p
            };
        }
        public static ProcessObject Exec(string fileName, string arguments, bool waitForExit = true, bool useShell = false)
        {
            var p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.RedirectStandardInput = !useShell;
            p.StartInfo.RedirectStandardOutput = !useShell;
            p.StartInfo.UseShellExecute = useShell;

            p.Start();

            if (waitForExit)
            {
                p.WaitForExit();
            }
            return new ProcessObject()
            {
                Process = p
            };
        }
        #endregion
        #region デバッグ機能
        public static int IndentLevel = 0;
        public static void Debug_Indent()
        {
            IndentLevel++;
        }
        public static void Debug_Unindent()
        {
            if (IndentLevel > 0)
            {
                IndentLevel--;
            }
        }
        public static int Debug_IndentLevel()
        {
            return IndentLevel;
        }
        public static int Debug_IndentLevel(int level)
        {
            IndentLevel = level;
            return IndentLevel;
        }
        public static void Debug_Print()
        {
            AddDebugOutput(string.Empty, true);
        }
        public static void Debug_Print(string text)
        {
            AddDebugOutput(text, true);
        }
        public static void Debug_Print(string format, Variable[] items)
        {
            AddDebugOutput(StringFormatFunction.Format(format, items), true);
        }
        public static void Debug_PrintIf(bool condition)
        {
            if (condition)
            {
                AddDebugOutput(string.Empty, true);
            }
        }
        public static void Debug_PrintIf(bool condition, string text)
        {
            if (condition)
            {
                AddDebugOutput(text, true);
            }
        }
        public static void Debug_PrintIf(bool condition, string format, Variable[] items)
        {
            if (condition)
            {
                AddDebugOutput(StringFormatFunction.Format(format, items), true);
            }
        }
        public static void Debug_Write()
        {
            AddDebugOutput(string.Empty, false);
        }
        public static void Debug_Write(string text)
        {
            AddDebugOutput(text, false);
        }
        public static void Debug_Write(string format, Variable[] items)
        {
            AddDebugOutput(StringFormatFunction.Format(format, items), false);
        }
        public static void Debug_WriteIf(bool condition)
        {
            if (condition)
            {
                AddDebugOutput(string.Empty, false);
            }
        }
        public static void Debug_WriteIf(bool condition, string text)
        {
            if (condition)
            {
                AddDebugOutput(text, false);
            }
        }
        public static void Debug_WriteIf(bool condition, string format, Variable[] items)
        {
            if (condition)
            {
                AddDebugOutput(StringFormatFunction.Format(format, items), false);
            }
        }
        public static void AddDebugOutput(string text,
                                     bool addLine = true, bool addSpace = true, string start = "", string indent = "    ")
        {
            var indents = new StringBuilder();
            for (int i = 0; i < IndentLevel; i++)
            {
                indents.Append(indent);
            }
            string output = indents + text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendDebug(output);
        }
        #endregion
    }

    [AliceObject(Name = "Process")]
    internal sealed class ProcessObject
    {

        internal Process Process = new Process();

        public void Close()
        {
            Process.Close();
        }

        public void CloseMainWindow()
        {
            Process.CloseMainWindow();
        }

        public void Kill()
        {
            Process.Kill();
        }

        public void Kill(bool entireProcessTree)
        {
            Process.Kill(entireProcessTree);
        }

        public void Reflesh()
        {
            Process.Refresh();
        }

        public void Start()
        {
            Process.Start();
        }

        public void WaitForExit()
        {
            Process.WaitForExit();
        }

        public void WaitForExit(int milliseconds)
        {
            Process.WaitForInputIdle(milliseconds);
        }

        public void WaitForInputIdle()
        {
            Process.WaitForExit();
        }

        public void WaitFWaitForInputIdleorExit(int milliseconds)
        {
            Process.WaitForInputIdle(milliseconds);
        }
        public string Read()
        {
            StringBuilder sb = new StringBuilder();
            while (Process.StandardOutput.Peek() >= 0)
            {
                sb.Append((char)Process.StandardOutput.Read());
            }

            return sb.ToString();
        }
        public string ReadToEnd()
        {
            return Process.StandardOutput.ReadToEnd();
        }
        public string ReadLine()
        {
            return Process.StandardOutput.ReadLine();
        }
        public void Write(string input)
        {
            Process.StandardInput.Write(input);
            Process.StandardInput.Flush();
        }
        public void WriteLine(string input)
        {
            Process.StandardInput.WriteLine(input);
            Process.StandardInput.Flush();
        }
        public ProcessStartInfo ProcessStartInfo => Process.StartInfo;
    }


    internal sealed class StopWatchObject : ObjectBase
    {
        public StopWatchObject()
        {
            Name = "stopwatch";
            AddFunction(new STWOFunc(this, 0), "start");
            AddFunction(new STWOFunc(this, 1), "stop");
            AddFunction(new STWOFunc(this, 2), "reset");
            AddFunction(new STWOFunc(this, 3), "restart");
            AddFunction(new ElapsedProperty(stopwatch));
            AddFunction(new ElapsedMillisecondsProperty(stopwatch));
            AddFunction(new ElapsedTicksProperty(stopwatch));
            AddFunction(new FrequencyProperty(stopwatch));
            AddFunction(new IsHighResolutionProperty(stopwatch));
            AddFunction(new IsRunningProperty(stopwatch));
        }


        private Stopwatch stopwatch = new Stopwatch();

        private class ElapsedProperty : ValueFunction
        {
            public ElapsedProperty(Stopwatch stopwatch)
            {
                Name = "elapsed";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Elapsed);
            }
        }

        private class ElapsedMillisecondsProperty : ValueFunction
        {
            public ElapsedMillisecondsProperty(Stopwatch stopwatch)
            {
                Name = "elapsedmilliseconds";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedMilliseconds);
            }
        }

        private class ElapsedTicksProperty : ValueFunction
        {
            public ElapsedTicksProperty(Stopwatch stopwatch)
            {
                Name = "elapsedticks";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedTicks);
            }
        }

        private class IsRunningProperty : ValueFunction
        {
            public IsRunningProperty(Stopwatch stopwatch)
            {
                Name = "isrunning";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsRunning);
            }
        }

        private class IsHighResolutionProperty : ValueFunction
        {
            public IsHighResolutionProperty(Stopwatch stopwatch)
            {
                Name = "ishighresolution";
                CanSet = false;
                HandleEvents = true;
                Getting += IsRunningProperty_Getting;
            }
            private void IsRunningProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsHighResolution);
            }
        }

        private class FrequencyProperty : ValueFunction
        {
            public FrequencyProperty(Stopwatch stopwatch)
            {
                Name = "frequency";
                CanSet = false;
                HandleEvents = true;
                Getting += IsRunningProperty_Getting;
            }
            private void IsRunningProperty_Getting(object sender, ValueFunctionEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Frequency);
            }
        }

        private class STWOFunc : FunctionBase
        {
            public STWOFunc(StopWatchObject sto, int mode)
            {
                Host = sto;
                Mode = mode;
                Run += STWOFunc_Run;
            }

            private void STWOFunc_Run(object sender, FunctionBaseEventArgs e)
            {
                switch (Mode)
                {
                    case 0:
                        {
                            Host.stopwatch.Start();
                            break;
                        }
                    case 1:
                        {
                            Host.stopwatch.Stop();
                            break;
                        }
                    case 2:
                        {
                            Host.stopwatch.Reset();
                            break;
                        }
                    case 3:
                        {
                            Host.stopwatch.Restart();
                            break;
                        }
                }
            }

            private StopWatchObject Host;
            private int Mode = 0;
        }

    }
}
