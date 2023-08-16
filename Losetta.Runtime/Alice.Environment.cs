namespace AliceScript.NameSpaces
{
    internal static class Alice_Environment_Initer
    {
        public static void Init()
        {
            NameSpace space = new NameSpace("Alice.Environment");

            space.Add(new Env_CommandLineArgsFunc());
            space.Add(new Env_CommandLineFunc());
            space.Add(new env_clr_versionFunc());
            space.Add(new env_currentdirFunc());
            space.Add(new env_Expand_EnvironmentVariablesFunc());
            space.Add(new env_get_envirnomentVariableFunc());
            space.Add(new env_set_envirnomentVariableFunc());
            space.Add(new env_HasShutdownStartedFunc());
            space.Add(new env_lang_nameFunc());
            space.Add(new env_lang_versionFunc());
            space.Add(new env_impl_nameFunc());
            space.Add(new env_impl_versionFunc());
            space.Add(new env_impl_locationFunc());
            space.Add(new env_impl_architectureFunc());
            space.Add(new env_impl_targetFunc());
            space.Add(new env_Is64BitOperatingSystemFunc());
            space.Add(new env_Is64BitProcessFunc());
            space.Add(new env_MachineNameFunc());
            space.Add(new env_NewLineFunc());
            space.Add(new env_OS_PlatformFunc());
            space.Add(new env_OS_VersionFunc());
            space.Add(new env_ProcessorCountFunc());
            space.Add(new env_Process_IdFunc());
            space.Add(new env_Process_PathFunc());
            space.Add(new env_setExitCodeFunc());
            space.Add(new env_SystemnDirectoryFunc());
            space.Add(new env_SystemPageSizeFunc());
            space.Add(new env_TickCountFunc());
            space.Add(new env_UserDomainNameFunc());
            space.Add(new env_UserNameFunc());
            space.Add(new env_versionFunc());
            space.Add(new env_WorkingSetFunc());

            NameSpaceManerger.Add(space);
        }
    }

    internal class Env_CommandLineFunc : FunctionBase
    {
        public Env_CommandLineFunc()
        {
            this.Name = "env_commandLine";
            this.Run += Env_CommandLineFunc_Run;
        }

        private void Env_CommandLineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.CommandLine);
        }
    }
    public class Env_CommandLineArgsFunc : FunctionBase
    {
        public Env_CommandLineArgsFunc()
        {
            this.Name = "env_commandLineArgs";
            this.Run += Env_CommandLineArgsFunc_Run;
        }
        public static List<string> Args { get; set; }
        private void Env_CommandLineArgsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (Args == null)
            {
                e.Return = new Variable(Environment.GetCommandLineArgs());
            }
            else
            {
                e.Return = new Variable(Args);
            }
        }
    }
    internal class env_currentdirFunc : FunctionBase
    {
        public env_currentdirFunc()
        {
            this.Name = "env_currentdir";
            this.Run += Env_currentdirFunc_Run;
        }

        private void Env_currentdirFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.CurrentDirectory);
        }
    }
    internal class env_setExitCodeFunc : FunctionBase
    {
        public env_setExitCodeFunc()
        {
            this.Name = "env_set_ExitCode";
            this.MinimumArgCounts = 1;
            this.Run += Env_setExitCodeFunc_Run;
        }

        private void Env_setExitCodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Environment.ExitCode = e.Args[0].AsInt();
        }
    }
    internal class env_HasShutdownStartedFunc : FunctionBase
    {
        public env_HasShutdownStartedFunc()
        {
            this.Name = "env_HasShutdownStarted";
            this.Run += Env_HasShutdownStartedFunc_Run;
        }

        private void Env_HasShutdownStartedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.HasShutdownStarted);
        }
    }
    internal class env_Is64BitOperatingSystemFunc : FunctionBase
    {
        public env_Is64BitOperatingSystemFunc()
        {
            this.Name = "env_Is64BitOperatingSystem";
            this.Run += Env_Is64BitOperatingSystemFunc_Run;
        }

        private void Env_Is64BitOperatingSystemFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Is64BitOperatingSystem);
        }
    }
    internal class env_Is64BitProcessFunc : FunctionBase
    {
        public env_Is64BitProcessFunc()
        {
            this.Name = "env_is64bitProcess";
            this.Run += Env_Is64BitProcessFunc_Run;
        }

        private void Env_Is64BitProcessFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Is64BitProcess);
        }
    }
    internal class env_MachineNameFunc : FunctionBase
    {
        public env_MachineNameFunc()
        {
            this.Name = "env_MachineName";
            this.Run += Env_MachineNameFunc_Run;
        }

        private void Env_MachineNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.MachineName);
        }
    }
    internal class env_NewLineFunc : FunctionBase
    {
        public env_NewLineFunc()
        {
            this.Name = "env_NewLine";
            this.Run += Env_NewLineFunc_Run;
        }

        private void Env_NewLineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.NewLine);
        }
    }
    internal class env_OS_PlatformFunc : FunctionBase
    {
        public env_OS_PlatformFunc()
        {
            this.Name = "env_os_platform";
            this.Run += Env_OS_PlatformFunc_Run;
        }

        private void Env_OS_PlatformFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.OSVersion.Platform);
        }
    }
    internal class env_OS_VersionFunc : FunctionBase
    {
        public env_OS_VersionFunc()
        {
            this.Name = "env_os_version";
            this.Run += Env_OS_PlatformFunc_Run;
        }

        private void Env_OS_PlatformFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.OSVersion.VersionString);
        }
    }
    internal class env_Process_IdFunc : FunctionBase
    {
        public env_Process_IdFunc()
        {
            this.Name = "env_process_id";
            this.Run += Env_Process_IdFunc_Run;
        }

        private void Env_Process_IdFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessId);
        }
    }
    internal class env_Process_PathFunc : FunctionBase
    {
        public env_Process_PathFunc()
        {
            this.Name = "env_process_path";
            this.Run += Env_Process_PathFunc_Run;
        }

        private void Env_Process_PathFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessPath);
        }
    }
    internal class env_ProcessorCountFunc : FunctionBase
    {
        public env_ProcessorCountFunc()
        {
            this.Name = "env_ProcessorCount";
            this.Run += Env_ProcessorCountFunc_Run;
        }

        private void Env_ProcessorCountFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessorCount);
        }
    }
    internal class env_SystemnDirectoryFunc : FunctionBase
    {
        public env_SystemnDirectoryFunc()
        {
            this.Name = "env_SystemDir";
            this.Run += Env_SystemnDirectoryFunc_Run;
        }

        private void Env_SystemnDirectoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.SystemDirectory);
        }
    }
    internal class env_SystemPageSizeFunc : FunctionBase
    {
        public env_SystemPageSizeFunc()
        {
            this.Name = "env_SystemPageSize";
            this.Run += Env_SystemPageSizeFunc_Run;
        }

        private void Env_SystemPageSizeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.SystemPageSize);
        }
    }
    internal class env_TickCountFunc : FunctionBase
    {
        public env_TickCountFunc()
        {
            this.Name = "env_TickCount";
            this.Run += Env_TickCountFunc_Run;
        }

        private void Env_TickCountFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.TickCount64);
        }
    }
    internal class env_UserDomainNameFunc : FunctionBase
    {
        public env_UserDomainNameFunc()
        {
            this.Name = "env_User_DomainName";
            this.Run += Env_UserDomainNameFunc_Run;
        }

        private void Env_UserDomainNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.UserDomainName);
        }
    }
    internal class env_UserNameFunc : FunctionBase
    {
        public env_UserNameFunc()
        {
            this.Name = "env_User_Name";
            this.Run += Env_UserNameFunc_Run;
        }

        private void Env_UserNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.UserName);
        }
    }
    internal class env_WorkingSetFunc : FunctionBase
    {
        public env_WorkingSetFunc()
        {
            this.Name = "env_workingset";
            this.Run += Env_WorkingSetFunc_Run;
        }

        private void Env_WorkingSetFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.WorkingSet);
        }
    }
    internal class env_versionFunc : FunctionBase
    {
        public env_versionFunc()
        {
            this.Name = "env_version";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.Version.ToString());
        }
    }
    internal class env_lang_versionFunc : FunctionBase
    {
        public env_lang_versionFunc()
        {
            this.Name = "env_lang_version";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.Version.ToString());
        }
    }
    internal class env_lang_nameFunc : FunctionBase
    {
        public env_lang_nameFunc()
        {
            this.Name = "env_lang_name";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Constants.LANGUAGE);
        }
    }
    internal class env_impl_versionFunc : FunctionBase
    {
        public env_impl_versionFunc()
        {
            this.Name = "env_impl_version";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationVersion.ToString());
        }
    }
    internal class env_impl_nameFunc : FunctionBase
    {
        public env_impl_nameFunc()
        {
            this.Name = "env_impl_name";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationName);
        }
    }
    internal class env_impl_locationFunc : FunctionBase
    {
        public env_impl_locationFunc()
        {
            this.Name = "env_impl_location";
            this.Run += Env_impl_locationFunc_Run;
        }

        private void Env_impl_locationFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationLocation);
        }
    }
    internal class env_impl_architectureFunc : FunctionBase
    {
        public env_impl_architectureFunc()
        {
            this.Name = "env_impl_architecture";
            this.Run += Env_impl_architectureFunc_Run;
        }

        private void Env_impl_architectureFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string arch_name = string.Empty;

#if RELEASE_WIN_X64 || RELEASE_LINUX_X64 || RELEASE_OSX_X64
            arch_name = "x64";
#elif RELEASE_WIN_X86
            arch_name = "x86";
#elif RELEASE_WIN_ARM || RELEASE_LINUX_ARM
            arch_name = "ARM32";
#elif RELEASE_WIN_ARM64 || RELEASE_LINUX_ARM64 || RELEASE_OSX_ARM64
            arch_name = "ARM64";
#elif DEBUG
            arch_name = "Debug";
#else
            arch_name = "Release";
#endif

            e.Return = new Variable(arch_name);
        }
    }
    internal class env_impl_targetFunc : FunctionBase
    {
        public env_impl_targetFunc()
        {
            this.Name = "env_impl_target";
            this.Run += Env_impl_architectureFunc_Run;
        }

        private void Env_impl_architectureFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string arch_name = string.Empty;

#if RELEASE_WIN_X64 || RELEASE_WIN_X86 || RELEASE_WIN_ARM || RELEASE_WIN_ARM64
            arch_name = "Windows";
#elif RELEASE_OSX_X64 || RELEASE_OSX_ARM64
            arch_name = "OSX";
#elif RELEASE_LINUX_X64 || RELEASE_LINUX_ARM || RELEASE_LINUX_ARM64
            arch_name = "Linux";
#else
            arch_name = "NET";
#endif

            e.Return = new Variable(arch_name);
        }
    }
    internal class env_clr_versionFunc : FunctionBase
    {
        public env_clr_versionFunc()
        {
            this.Name = "env_clr_version";
            this.Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Version.ToString());
        }
    }
    internal class env_Expand_EnvironmentVariablesFunc : FunctionBase
    {
        public env_Expand_EnvironmentVariablesFunc()
        {
            this.Name = "env_expand_environmentVariables";
            this.MinimumArgCounts = 1;
            this.Run += Env_Expand_EnvironmentVariablesFunc_Run;
        }

        private void Env_Expand_EnvironmentVariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ExpandEnvironmentVariables(e.Args[0].AsString() ?? string.Empty));
        }
    }
    internal class env_get_envirnomentVariableFunc : FunctionBase
    {
        public env_get_envirnomentVariableFunc()
        {
            this.Name = "env_get_environmentvariable";
            this.Run += Env_get_envirnomentVariable_Run;
            this.MinimumArgCounts = 1;
        }

        private void Env_get_envirnomentVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                Utils.CheckNumInRange(e.Args[1],true,0,2);
                e.Return = new Variable(Environment.GetEnvironmentVariable(e.Args[0].AsString(), (EnvironmentVariableTarget)e.Args[1].AsInt()));
            }
            else
            {
                e.Return = new Variable(Environment.GetEnvironmentVariable(e.Args[0].AsString()));
            }
        }
    }
    internal class env_set_envirnomentVariableFunc : FunctionBase
    {
        public env_set_envirnomentVariableFunc()
        {
            this.Name = "env_set_environmentvariable";
            this.MinimumArgCounts = 2;
            this.Run += Env_get_envirnomentVariable_Run;
        }

        private void Env_get_envirnomentVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 2)
            {
                Utils.CheckNumInRange(e.Args[2], true, 0, 2);
                Environment.SetEnvironmentVariable(e.Args[0].AsString(), e.Args[1].AsString(), (EnvironmentVariableTarget)e.Args[2].AsInt());
            }
            else
            {
                Environment.SetEnvironmentVariable(e.Args[0].AsString(), e.Args[1].AsString());
            }
        }
    }
}
