using AliceScript.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript
{
    public class UnSupportedPlatformFunction : FunctionBase, ICallingHandleAttribute
    {
        public UnSupportedPlatformFunction()
        {
            Name = Constants.ANNOTATION_FUNCTION_REFIX + "UnSupportedPlatform";
            Run += PInvokeFlagFunction_Run;
        }
        private void PInvokeFlagFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            Target = e.Args[0].ToString().ToLowerInvariant();
        }
        public void PreCall(FunctionBase function, FunctionBaseEventArgs args)
        {
            if(Target == GetPlatformId())
            {
                throw new ScriptException("このプラットフォームでは操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
            }
        }
        public void PostCall(FunctionBase function, FunctionBaseEventArgs args)
        {
            
        }
        public string GetPlatformId()
        {
            if(OperatingSystem.IsAndroid())
            {
                return "android";
            }
            else if (OperatingSystem.IsBrowser())
            {
                return "browser";
            }
            else if (OperatingSystem.IsFreeBSD())
            {
                return "freebsd";
            }
            else if(OperatingSystem.IsIOS())
            {
                return "ios";
            }
            else if(OperatingSystem.IsLinux())
            {
                return "linux";
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                return "mac-catalyst";
            }
            else if(OperatingSystem.IsMacOS())
            {
                return "osx";
            }
            else if(OperatingSystem.IsTvOS())
            {
                return "tvos";
            }
            else if (OperatingSystem.IsWatchOS())
            {
                return "watchos";
            }
            else if (OperatingSystem.IsWindows())
            {
                return "windows";
            }
            return "";
        }
        public string Target { get; set; }
    }
}
