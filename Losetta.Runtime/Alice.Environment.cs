using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Interop;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Environment
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(EnvironmentFunctions));
        }
    }
    [AliceNameSpace(Name = "Alice.Environment")]
    internal static class EnvironmentFunctions
    {
        #region システムの情報
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_NewLine()
        {
            return Environment.NewLine;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static long Env_TickCount64()
        {
            return Environment.TickCount64;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static int Env_ProcessorCount()
        {
            return Environment.ProcessorCount;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static bool Env_HasShutdownStarted()
        {
            return Environment.HasShutdownStarted;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static bool Env_Is64BitOperatingSystem()
        {
            return Environment.Is64BitOperatingSystem;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static bool Env_Is64BitProcess()
        {
            return Environment.Is64BitProcess;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_MachineName()
        {
            return Environment.MachineName;
        }

        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_SystemDirectory()
        {
            return Environment.SystemDirectory;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static int Env_SystemPageSize()
        {
            return Environment.SystemPageSize;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static int Env_TickCount()
        {
            return Environment.TickCount;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_UserName()
        {
            return Environment.UserName;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static long Env_WorkingSet()
        {
            return Environment.WorkingSet;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_OS_Platform()
        {
            return Environment.OSVersion.Platform.ToString();
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_OS_Version()
        {
            return Environment.OSVersion.VersionString;
        }
        #endregion

        #region プロセスの情報
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_CommandLine()
        {
            return Environment.CommandLine;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static int Env_ProcessId()
        {
            return Environment.ProcessId;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_ProcessPath()
        {
            return Environment.ProcessPath;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static bool Env_UserInteractive()
        {
            return Environment.UserInteractive;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Current()
        {
            return Environment.CurrentDirectory;
        }
        #endregion

        #region 言語の情報
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Lang_Version()
        {
            return Alice.Version.ToString();
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Lang_Name()
        {
            return Constants.LANGUAGE;
        }
        #endregion

        #region 実装の情報
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Impl_Version()
        {
            return Alice.ImplementationVersion.ToString();
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Impl_Name()
        {
            return Alice.ImplementationName;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Impl_Location()
        {
            return Alice.ImplementationLocation;
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Impl_Architecture()
        {

#if RELEASE_WIN_X64 || RELEASE_LINUX_X64 || RELEASE_OSX_X64
            return "x64";
#elif RELEASE_WIN_X86
            return "x86";
#elif RELEASE_WIN_ARM || RELEASE_LINUX_ARM
            return "ARM32";
#elif RELEASE_WIN_ARM64 || RELEASE_LINUX_ARM64 || RELEASE_OSX_ARM64
            return "ARM64";
#elif DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Impl_Target()
        {

#if RELEASE_WIN_X64 || RELEASE_WIN_X86 || RELEASE_WIN_ARM || RELEASE_WIN_ARM64
            return "Windows";
#elif RELEASE_OSX_X64 || RELEASE_OSX_ARM64
            return "OSX";
#elif RELEASE_LINUX_X64 || RELEASE_LINUX_ARM || RELEASE_LINUX_ARM64
            return "Linux";
#else
            return "NET";
#endif
        }
        #endregion

        #region 共通言語ランタイムの情報
        [AliceFunction(Attribute = FunctionAttribute.FUNCT_WITH_SPACE_ONC)]
        public static string Env_Clr_Version()
        {
            return Environment.Version.ToString();
        }
        #endregion

        public static string[] Env_CommandLineArgs()
        {
            return Runtime.Args == null ? Environment.GetCommandLineArgs() : Runtime.Args.ToArray();
        }

        public static void env_Exit(int exitCode)
        {
            Environment.Exit(exitCode);
        }
        public static void env_SetExitCode(int exitCode)
        {
            Environment.ExitCode = exitCode;
        }
        public static string env_Expand_EnvironmentVariables(string name)
        {
            return Environment.ExpandEnvironmentVariables(name);
        }
        public static string Env_Get_EnvironmentVariable(string variable)
        {
            return Environment.GetEnvironmentVariable(variable);
        }
        public static string Env_Get_EnvironmentVariable(string? variable, int target)
        {
            return Environment.GetEnvironmentVariable(variable, (EnvironmentVariableTarget)target);
        }
        public static void Env_Set_EnvironmentVariable(string? variable)
        {
            Environment.GetEnvironmentVariable(variable);
        }
        public static void Env_Set_EnvironmentVariable(string? variable, string? value, int target)
        {
            Environment.SetEnvironmentVariable(variable, value, (EnvironmentVariableTarget)target);
        }
    }
}
