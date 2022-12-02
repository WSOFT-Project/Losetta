using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AliceScript.NameSpaces
{
    static class Alice_Diagnostics_Initer
    {
        public static void Init()
        {
            //名前空間のメインエントリポイントです。
            NameSpace space = new NameSpace("Alice.Diagnostics");

            space.Add(new Process_GetProcessFunc());
            space.Add(new Process_GetProcessFunc(true));

            space.Add(new StopWatchObject());
            space.Add(new ProcessObject());
            space.Add(new ProcessStartInfoObject());

            space.Add("ProcessWindowStyle", "System.Diagnostics.ProcessWindowStyle");

            NameSpaceManerger.Add(space);
        }
    }
  
    class Process_GetProcessFunc : FunctionBase
    {
        public Process_GetProcessFunc(bool byname = false)
        {
            m_ByName = byname;
            if (m_ByName)
            {
                this.Name = "Process_GetProcessesByName";
            }
            else
            {
                this.Name = "Process_GetProcessById";
            }
            this.MinimumArgCounts = 1;
            this.Run += Process_GetProcessFunc_Run;
        }

        private void Process_GetProcessFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_ByName)
            {
                Process[] ps = Process.GetProcessesByName(e.Args[0].AsString());
                Variable v = new Variable(Variable.VarType.ARRAY);
                foreach(Process p in ps)
                {
                    ProcessObject po = new ProcessObject();
                    po.Process = p;
                    v.Tuple.Add(new Variable(po));
                }
                e.Return = v;
            }
            else
            {
                ProcessObject po = new ProcessObject();
                po.Process = Process.GetProcessById(e.Args[0].AsInt());
                e.Return = new Variable(po);
            }
        }

        private bool m_ByName = false;
    }
    class ProcessObject : ObjectBase
    {
        public ProcessObject()
        {
            this.Name = "Process";

            this.AddProperty(new StartInfoProperty(this));

            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.CloseMainWindow,this));
            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Kill,this));
            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Reflesh,this));
            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.Start, this));
            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.WaitForExit,this));
            this.AddFunction(new ProcessFunction(ProcessFunction.ProcessFunctionMode.WaitForInputIdle, this));

            
        }
        internal Process Process=new Process();
        class StartInfoProperty : PropertyBase
        {
            public StartInfoProperty(ProcessObject host)
            {
                Host = host;
                this.Name = "StartInfo";
                this.HandleEvents = true;
                this.Getting += StartInfoProperty_Getting;
                this.Setting += StartInfoProperty_Setting;
            }

            private void StartInfoProperty_Setting(object sender, PropertySettingEventArgs e)
            {
                if (Host.Process != null)
                {
                    Host.Process.StartInfo = ((ProcessStartInfoObject)e.Value.Object).ps;
                }
            }

            private void StartInfoProperty_Getting(object sender, PropertyGettingEventArgs e)
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
        class ProcessFunction : FunctionBase
        {
            internal enum ProcessFunctionMode
            {
                Start,Kill,WaitForExit,Reflesh, WaitForInputIdle,CloseMainWindow
            }
            public ProcessFunction(ProcessFunctionMode mode,ProcessObject host)
            {
                Mode = mode;
                Host = host;
                this.Name = Mode.ToString();
                this.Run += ProcessFunction_Run;
            }
            ProcessObject Host;
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
    class ProcessStartInfoObject : ObjectBase
    {
        public ProcessStartInfoObject()
        {
            this.Name = "ProcessStartInfo";

            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.Arguments,this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.CreateNoWindow, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.ErrorDialog, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.FileName, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.LoadUserProfile, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardError, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardInput, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.RedirectStandardOutput, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.UseShellExecute, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.Verb, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.WindowStyle, this));
            this.AddProperty(new InfoProperty(InfoProperty.InfoPropertyMode.WorkingDirectory, this));
        }
        internal ProcessStartInfo ps = new ProcessStartInfo();
        class InfoProperty : PropertyBase
        {
            internal enum InfoPropertyMode
            {
                Arguments, CreateNoWindow, ErrorDialog, FileName, LoadUserProfile, RedirectStandardError, RedirectStandardInput, RedirectStandardOutput, UseShellExecute, Verb, WindowStyle, WorkingDirectory
            }
            public InfoProperty(InfoPropertyMode mode,ProcessStartInfoObject host)
            {
                Mode = mode;
                Host = host;
                this.Name = Mode.ToString();
                this.HandleEvents = true;
                this.Getting += InfoProperty_Getting;
                this.Setting += InfoProperty_Setting;
            }
            private ProcessStartInfoObject Host;
            private void InfoProperty_Setting(object sender, PropertySettingEventArgs e)
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
                            Host.ps.CreateNoWindow = (e.Value.AsBool());
                            break;
                        }
                    case InfoPropertyMode.ErrorDialog:
                        {
                            Host.ps.ErrorDialog = (e.Value.AsBool());
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
                            Host.ps.WindowStyle = ((ProcessWindowStyle)e.Value.AsInt());
                            break;
                        }
                    case InfoPropertyMode.WorkingDirectory:
                        {
                            Host.ps.WorkingDirectory = e.Value.AsString();
                            break;
                        }
                }
            }



            private void InfoProperty_Getting(object sender, PropertyGettingEventArgs e)
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

            InfoPropertyMode Mode;
        }

       

    }
    class StopWatchObject : ObjectBase
    {
        public StopWatchObject()
        {
            this.Name = "stopwatch";
            this.AddFunction(new STWOFunc(this, 0), "start");
            this.AddFunction(new STWOFunc(this, 1), "stop");
            this.AddFunction(new STWOFunc(this, 2), "reset");
            this.AddFunction(new STWOFunc(this, 3), "restart");
            this.AddProperty(new ElapsedProperty(stopwatch));
            this.AddProperty(new ElapsedMillisecondsProperty(stopwatch));
            this.AddProperty(new ElapsedTicksProperty(stopwatch));
            this.AddProperty(new FrequencyProperty(stopwatch));
            this.AddProperty(new IsHighResolutionProperty(stopwatch));
            this.AddProperty(new IsRunningProperty(stopwatch));
        }


        private Stopwatch stopwatch = new Stopwatch();

        class ElapsedProperty : PropertyBase
        {
            public ElapsedProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsed";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Elapsed);
            }
        }
        class ElapsedMillisecondsProperty : PropertyBase
        {
            public ElapsedMillisecondsProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsedmilliseconds";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedMilliseconds);
            }
        }
        class ElapsedTicksProperty : PropertyBase
        {
            public ElapsedTicksProperty(Stopwatch stopwatch)
            {
                this.Name = "elapsedticks";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += ElapsedProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void ElapsedProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.ElapsedTicks);
            }
        }
        class IsRunningProperty : PropertyBase
        {
            public IsRunningProperty(Stopwatch stopwatch)
            {
                this.Name = "isrunning";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsRunning);
            }
        }
        class IsHighResolutionProperty : PropertyBase
        {
            public IsHighResolutionProperty(Stopwatch stopwatch)
            {
                this.Name = "ishighresolution";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.IsHighResolution);
            }
        }
        class FrequencyProperty : PropertyBase
        {
            public FrequencyProperty(Stopwatch stopwatch)
            {
                this.Name = "frequency";
                this.CanSet = false;
                this.Stopwatch = stopwatch;
                this.HandleEvents = true;
                this.Getting += IsRunningProperty_Getting;
            }
            private Stopwatch Stopwatch;
            private void IsRunningProperty_Getting(object sender, PropertyGettingEventArgs e)
            {
                e.Value = new Variable(Stopwatch.Frequency);
            }
        }

        class STWOFunc : FunctionBase
        {
            public STWOFunc(StopWatchObject sto, int mode)
            {
                Host = sto;
                Mode = mode;
                this.Run += STWOFunc_Run;
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
