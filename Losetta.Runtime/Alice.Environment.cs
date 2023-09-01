namespace AliceScript.NameSpaces
{
    public sealed class Alice_Environment
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

            NameSpaceManager.Add(space);
        }
    }

    internal sealed class Env_CommandLineFunc : FunctionBase
    {
        public Env_CommandLineFunc()
        {
            Name = "env_commandLine";
            Run += Env_CommandLineFunc_Run;
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
            Name = "env_commandLineArgs";
            Run += Env_CommandLineArgsFunc_Run;
        }
        public static List<string> Args { get; set; }
        private void Env_CommandLineArgsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = Args == null ? new Variable(Environment.GetCommandLineArgs()) : new Variable(Args);
        }
    }
    internal sealed class env_currentdirFunc : FunctionBase
    {
        public env_currentdirFunc()
        {
            Name = "env_currentdir";
            Run += Env_currentdirFunc_Run;
        }

        private void Env_currentdirFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.CurrentDirectory);
        }
    }
    internal sealed class env_setExitCodeFunc : FunctionBase
    {
        public env_setExitCodeFunc()
        {
            Name = "env_set_ExitCode";
            MinimumArgCounts = 1;
            Run += Env_setExitCodeFunc_Run;
        }

        private void Env_setExitCodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Environment.ExitCode = e.Args[0].AsInt();
        }
    }
    internal sealed class env_HasShutdownStartedFunc : FunctionBase
    {
        public env_HasShutdownStartedFunc()
        {
            Name = "env_HasShutdownStarted";
            Run += Env_HasShutdownStartedFunc_Run;
        }

        private void Env_HasShutdownStartedFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.HasShutdownStarted);
        }
    }
    internal sealed class env_Is64BitOperatingSystemFunc : FunctionBase
    {
        public env_Is64BitOperatingSystemFunc()
        {
            Name = "env_Is64BitOperatingSystem";
            Run += Env_Is64BitOperatingSystemFunc_Run;
        }

        private void Env_Is64BitOperatingSystemFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Is64BitOperatingSystem);
        }
    }
    internal sealed class env_Is64BitProcessFunc : FunctionBase
    {
        public env_Is64BitProcessFunc()
        {
            Name = "env_is64bitProcess";
            Run += Env_Is64BitProcessFunc_Run;
        }

        private void Env_Is64BitProcessFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Is64BitProcess);
        }
    }
    internal sealed class env_MachineNameFunc : FunctionBase
    {
        public env_MachineNameFunc()
        {
            Name = "env_MachineName";
            Run += Env_MachineNameFunc_Run;
        }

        private void Env_MachineNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.MachineName);
        }
    }
    internal sealed class env_NewLineFunc : FunctionBase
    {
        public env_NewLineFunc()
        {
            Name = "env_NewLine";
            Run += Env_NewLineFunc_Run;
        }

        private void Env_NewLineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.NewLine);
        }
    }
    internal sealed class env_OS_PlatformFunc : FunctionBase
    {
        public env_OS_PlatformFunc()
        {
            Name = "env_os_platform";
            Run += Env_OS_PlatformFunc_Run;
        }

        private void Env_OS_PlatformFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.OSVersion.Platform);
        }
    }
    internal sealed class env_OS_VersionFunc : FunctionBase
    {
        public env_OS_VersionFunc()
        {
            Name = "env_os_version";
            Run += Env_OS_PlatformFunc_Run;
        }

        private void Env_OS_PlatformFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.OSVersion.VersionString);
        }
    }
    internal sealed class env_Process_IdFunc : FunctionBase
    {
        public env_Process_IdFunc()
        {
            Name = "env_process_id";
            Run += Env_Process_IdFunc_Run;
        }

        private void Env_Process_IdFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessId);
        }
    }
    internal sealed class env_Process_PathFunc : FunctionBase
    {
        public env_Process_PathFunc()
        {
            Name = "env_process_path";
            Run += Env_Process_PathFunc_Run;
        }

        private void Env_Process_PathFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessPath);
        }
    }
    internal sealed class env_ProcessorCountFunc : FunctionBase
    {
        public env_ProcessorCountFunc()
        {
            Name = "env_ProcessorCount";
            Run += Env_ProcessorCountFunc_Run;
        }

        private void Env_ProcessorCountFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ProcessorCount);
        }
    }
    internal sealed class env_SystemnDirectoryFunc : FunctionBase
    {
        public env_SystemnDirectoryFunc()
        {
            Name = "env_SystemDir";
            Run += Env_SystemnDirectoryFunc_Run;
        }

        private void Env_SystemnDirectoryFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.SystemDirectory);
        }
    }
    internal sealed class env_SystemPageSizeFunc : FunctionBase
    {
        public env_SystemPageSizeFunc()
        {
            Name = "env_SystemPageSize";
            Run += Env_SystemPageSizeFunc_Run;
        }

        private void Env_SystemPageSizeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.SystemPageSize);
        }
    }
    internal sealed class env_TickCountFunc : FunctionBase
    {
        public env_TickCountFunc()
        {
            Name = "env_TickCount";
            Run += Env_TickCountFunc_Run;
        }

        private void Env_TickCountFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.TickCount64);
        }
    }
    internal sealed class env_UserDomainNameFunc : FunctionBase
    {
        public env_UserDomainNameFunc()
        {
            Name = "env_User_DomainName";
            Run += Env_UserDomainNameFunc_Run;
        }

        private void Env_UserDomainNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.UserDomainName);
        }
    }
    internal sealed class env_UserNameFunc : FunctionBase
    {
        public env_UserNameFunc()
        {
            Name = "env_User_Name";
            Run += Env_UserNameFunc_Run;
        }

        private void Env_UserNameFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.UserName);
        }
    }
    internal sealed class env_WorkingSetFunc : FunctionBase
    {
        public env_WorkingSetFunc()
        {
            Name = "env_workingset";
            Run += Env_WorkingSetFunc_Run;
        }

        private void Env_WorkingSetFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.WorkingSet);
        }
    }
    internal sealed class env_versionFunc : FunctionBase
    {
        public env_versionFunc()
        {
            Name = "env_version";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.Version.ToString());
        }
    }
    internal sealed class env_lang_versionFunc : FunctionBase
    {
        public env_lang_versionFunc()
        {
            Name = "env_lang_version";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.Version.ToString());
        }
    }
    internal sealed class env_lang_nameFunc : FunctionBase
    {
        public env_lang_nameFunc()
        {
            Name = "env_lang_name";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Constants.LANGUAGE);
        }
    }
    internal sealed class env_impl_versionFunc : FunctionBase
    {
        public env_impl_versionFunc()
        {
            Name = "env_impl_version";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationVersion.ToString());
        }
    }
    internal sealed class env_impl_nameFunc : FunctionBase
    {
        public env_impl_nameFunc()
        {
            Name = "env_impl_name";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationName);
        }
    }
    internal sealed class env_impl_locationFunc : FunctionBase
    {
        public env_impl_locationFunc()
        {
            Name = "env_impl_location";
            Run += Env_impl_locationFunc_Run;
        }

        private void Env_impl_locationFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice.ImplementationLocation);
        }
    }
    internal sealed class env_impl_architectureFunc : FunctionBase
    {
        public env_impl_architectureFunc()
        {
            Name = "env_impl_architecture";
            Run += Env_impl_architectureFunc_Run;
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
    internal sealed class env_impl_targetFunc : FunctionBase
    {
        public env_impl_targetFunc()
        {
            Name = "env_impl_target";
            Run += Env_impl_architectureFunc_Run;
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
    internal sealed class env_clr_versionFunc : FunctionBase
    {
        public env_clr_versionFunc()
        {
            Name = "env_clr_version";
            Run += Env_versionFunc_Run;
        }

        private void Env_versionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.Version.ToString());
        }
    }
    internal sealed class env_Expand_EnvironmentVariablesFunc : FunctionBase
    {
        public env_Expand_EnvironmentVariablesFunc()
        {
            Name = "env_expand_environmentVariables";
            MinimumArgCounts = 1;
            Run += Env_Expand_EnvironmentVariablesFunc_Run;
        }

        private void Env_Expand_EnvironmentVariablesFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Environment.ExpandEnvironmentVariables(e.Args[0].AsString() ?? string.Empty));
        }
    }
    internal sealed class env_get_envirnomentVariableFunc : FunctionBase
    {
        public env_get_envirnomentVariableFunc()
        {
            Name = "env_get_environmentvariable";
            Run += Env_get_envirnomentVariable_Run;
            MinimumArgCounts = 1;
        }

        private void Env_get_envirnomentVariable_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 1)
            {
                Utils.CheckNumInRange(e.Args[1], true, 0, 2);
                e.Return = new Variable(Environment.GetEnvironmentVariable(e.Args[0].AsString(), (EnvironmentVariableTarget)e.Args[1].AsInt()));
            }
            else
            {
                e.Return = new Variable(Environment.GetEnvironmentVariable(e.Args[0].AsString()));
            }
        }
    }
    internal sealed class env_set_envirnomentVariableFunc : FunctionBase
    {
        public env_set_envirnomentVariableFunc()
        {
            Name = "env_set_environmentvariable";
            MinimumArgCounts = 2;
            Run += Env_get_envirnomentVariable_Run;
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
