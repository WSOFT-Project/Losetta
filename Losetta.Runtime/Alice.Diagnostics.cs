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
            space.Add(new ProcessObject());
            space.Add(new ProcessStartInfoObject());

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
            var po = new ProcessObject();
            po.Process = Process.Start(path, arguments);
            return po;
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

    internal sealed class ProcessObject : ObjectBase
    {
        public ProcessObject()
        {
            Name = "Process";

            AddProperty(new StartInfoProperty(this));

            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.CloseMainWindow, this));
            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Kill, this));
            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Reflesh, this));
            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Start, this));
            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.WaitForExit, this));
            AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.WaitForInputIdle, this));


        }
        internal Process Process = new Process();

        private class StartInfoProperty : PropertyBase
        {
            public StartInfoProperty(ProcessObject host)
            {
                Host = host;
                Name = "StartInfo";
                HandleEvents = true;
                Getting += StartInfoProperty_Getting;
                Setting += StartInfoProperty_Setting;
            }

            private void StartInfoProperty_Setting(object sender, PropertyBaseEventArgs e)
            {
                if (Host.Process != null)
                {
                    Host.Process.StartInfo = ((ProcessStartInfoObject)e.Value.Object).ps;
                }
            }

            private void StartInfoProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                if (Host.Process != null)
                {
                    ProcessStartInfoObject psio = new ProcessStartInfoObject();
                    psio.ps = Host.Process.StartInfo;
                    e.Value = new Variable(psio);
                }
            }

            private ProcessObject Host;
        }

        private class ProcessFunction : FunctionBase
        {
            internal enum ProcessFunctionMode
            {
                Start, Kill, WaitForExit, Reflesh, WaitForInputIdle, CloseMainWindow
            }
            public ProcessFunction(ProcessFunctionMode mode, ProcessObject host)
            {
                Mode = mode;
                Host = host;
                Name = Mode.ToString();
                Run += ProcessFunction_Run;
            }

            private ProcessObject Host;
            private void ProcessFunction_Run(object sender, FunctionBaseEventArgs e)
            {
                switch (Mode)
                {
                    case ProcessFunctionMode.Start:
                        {
                            Host.Process.Start();
                            break;
                        }
                    case ProcessFunctionMode.Kill:
                        {
                            Host.Process.Kill();
                            break;
                        }
                    case ProcessFunctionMode.Reflesh:
                        {
                            Host.Process.Refresh();
                            break;
                        }
                    case ProcessFunctionMode.WaitForExit:
                        {
                            Host.Process.WaitForExit();
                            break;
                        }
                    case ProcessFunctionMode.WaitForInputIdle:
                        {
                            Host.Process.WaitForInputIdle();
                            break;
                        }
                    case ProcessFunctionMode.CloseMainWindow:
                        {
                            Host.Process.CloseMainWindow();
                            break;
                        }
                }
            }

            private ProcessFunctionMode Mode;
        }
    }

    internal sealed class ProcessStartInfoObject : ObjectBase
    {
        public ProcessStartInfoObject()
        {
            Name = "ProcessStartInfo";

            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.Arguments, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.CreateNoWindow, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.ErrorDialog, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.FileName, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.LoadUserProfile, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardError, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardInput, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardOutput, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.UseShellExecute, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.Verb, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.WindowStyle, this));
            AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.WorkingDirectory, this));
        }
        internal ProcessStartInfo ps = new ProcessStartInfo();

        private class InfoProperty : PropertyBase
        {
            internal enum InfoPropertyMode
            {
                Arguments, CreateNoWindow, ErrorDialog, FileName, LoadUserProfile, RedirectStandardError, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, Verb, WindowStyle, WorkingDirectory
            }
            public InfoProperty(InfoPropertyMode mode, ProcessStartInfoObject host)
            {
                Mode = mode;
                Host = host;
                Name = Mode.ToString();
                HandleEvents = true;
                Getting += InfoProperty_Getting;
                Setting += InfoProperty_Setting;
            }
            private ProcessStartInfoObject Host;
            private void InfoProperty_Setting(object sender, PropertyBaseEventArgs e)
            {
                switch (Mode)
                {
                    case InfoPropertyMode.Arguments:
                        {
                            Host.ps.Arguments = e.Value.AsString();
                            break;
                        }
                    case InfoPropertyMode.CreateNoWindow:
                        {
                            Host.ps.CreateNoWindow = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.ErrorDialog:
                        {
                            Host.ps.ErrorDialog = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.FileName:
                        {
                            Host.ps.FileName = e.Value.AsString();
                            break;
                        }
                    case InfoPropertyMode.LoadUserProfile:
                        {
                            Host.ps.LoadUserProfile = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardError:
                        {
                            Host.ps.RedirectStandardError = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardInput:
                        {
                            Host.ps.RedirectStandardInput = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardOutput:
                        {
                            Host.ps.RedirectStandardOutput = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.UseShellExecute:
                        {
                            Host.ps.UseShellExecute = e.Value.AsBool();
                            break;
                        }
                    case InfoPropertyMode.Verb:
                        {
                            Host.ps.Verb = e.Value.AsString();
                            break;
                        }
                    case InfoPropertyMode.WindowStyle:
                        {
                            Host.ps.WindowStyle = (ProcessWindowStyle)e.Value.AsInt();
                            break;
                        }
                    case InfoPropertyMode.WorkingDirectory:
                        {
                            Host.ps.WorkingDirectory = e.Value.AsString();
                            break;
                        }
                }
            }



            private void InfoProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                switch (Mode)
                {
                    case InfoPropertyMode.Arguments:
                        {
                            e.Value = new Variable(Host.ps.Arguments);
                            break;
                        }
                    case InfoPropertyMode.CreateNoWindow:
                        {
                            e.Value = new Variable(Host.ps.CreateNoWindow);
                            break;
                        }
                    case InfoPropertyMode.ErrorDialog:
                        {
                            e.Value = new Variable(Host.ps.ErrorDialog);
                            break;
                        }
                    case InfoPropertyMode.FileName:
                        {
                            e.Value = new Variable(Host.ps.FileName);
                            break;
                        }
                    case InfoPropertyMode.LoadUserProfile:
                        {
                            e.Value = new Variable(Host.ps.LoadUserProfile);
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardError:
                        {
                            e.Value = new Variable(Host.ps.RedirectStandardError);
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardInput:
                        {
                            e.Value = new Variable(Host.ps.RedirectStandardInput);
                            break;
                        }
                    case InfoPropertyMode.RedirectStandardOutput:
                        {
                            e.Value = new Variable(Host.ps.RedirectStandardOutput);
                            break;
                        }
                    case InfoPropertyMode.UseShellExecute:
                        {
                            e.Value = new Variable(Host.ps.UseShellExecute);
                            break;
                        }
                    case InfoPropertyMode.Verb:
                        {
                            e.Value = new Variable(Host.ps.Verb);
                            break;
                        }
                    case InfoPropertyMode.WindowStyle:
                        {
                            e.Value = new Variable((int)Host.ps.WindowStyle);
                            break;
                        }
                    case InfoPropertyMode.WorkingDirectory:
                        {
                            e.Value = new Variable(Host.ps.WorkingDirectory);
                            break;
                        }
                }
            }

            private InfoPropertyMode Mode;
        }



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
            AddProperty(new ElapsedProperty(stopwatch));
            AddProperty(new ElapsedMillisecondsProperty(stopwatch));
            AddProperty(new ElapsedTicksProperty(stopwatch));
            AddProperty(new FrequencyProperty(stopwatch));
            AddProperty(new IsHighResolutionProperty(stopwatch));
            AddProperty(new IsRunningProperty(stopwatch));
        }


        private Stopwatch stopwatch = new Stopwatch();

        private class ElapsedProperty : PropertyBase
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
            private void ElapsedProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Elapsed);
            }
        }

        private class ElapsedMillisecondsProperty : PropertyBase
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
            private void ElapsedProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedMilliseconds);
            }
        }

        private class ElapsedTicksProperty : PropertyBase
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
            private void ElapsedProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedTicks);
            }
        }

        private class IsRunningProperty : PropertyBase
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
            private void IsRunningProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsRunning);
            }
        }

        private class IsHighResolutionProperty : PropertyBase
        {
            public IsHighResolutionProperty(Stopwatch stopwatch)
            {
                Name = "ishighresolution";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyBaseEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsHighResolution);
            }
        }

        private class FrequencyProperty : PropertyBase
        {
            public FrequencyProperty(Stopwatch stopwatch)
            {
                Name = "frequency";
                CanSet = false;
                Stopwatch = stopwatch;
                HandleEvents = true;
                Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyBaseEventArgs e)
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
