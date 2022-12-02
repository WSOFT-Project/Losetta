using System;
using System.Collections.Generic;
using System.Text;
using AliceScript;

namespace alice
{
    internal class shell_dumpFunc:FunctionBase
    {
        public shell_dumpFunc()
        {
            this.Name = "shell_dump";
            this.Run += Shell_dumpFunc_Run;
        }

        private void Shell_dumpFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Shell.DumpLocalVariables(e.Script);
            Shell.DumpGlobalVariables();
        }
    }
    internal class buildpkgFunc : FunctionBase
    {
        public buildpkgFunc()
        {
            this.Name = "buildpkg";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 2;
            this.Run += BuildpkgFunc_Run;
        }

        private void BuildpkgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string result=Program.BuildPackage(e.Args[0].AsString(),e.Args[1].AsString())?"成功":"失敗";
            Interpreter.Instance.AppendOutput(result,true);
        }
    }
    internal class testpkgFunc : FunctionBase
    {
        public testpkgFunc()
        {
            this.Name = "testpkg";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 1;
            this.Run += TestpkgFunc_Run;
        }

        private void TestpkgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string path = Utils.GetSafeString(e.Args,1,System.IO.Path.GetTempFileName());
            if (Program.BuildPackage(e.Args[0].AsString(), path))
            {
                Interpreter.Instance.AppendOutput("ビルド成功...開始しています", true);
                AlicePackage.Load(path);
            }else
            {
                Interpreter.Instance.AppendOutput("ビルド失敗", true);
            }
        }
    }
}
