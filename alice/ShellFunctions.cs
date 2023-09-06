using AliceScript.Functions;
using AliceScript.NameSpaces;
using AliceScript.Packaging;

namespace AliceScript.CLI
{
    internal class ShellFunctions
    {
        public static void Init()
        {
            var space = new NameSpace("Alice.Shell");
            space.Add(new shell_dumpFunc());
            space.Add(new shell_execFunc());
            space.Add(new buildpkgFunc());
            space.Add(new testpkgFunc());
            space.Add(new shell_reinitFunc());

            NameSpaceManager.Add(space);
        }
    }
    internal class shell_dumpFunc : FunctionBase
    {
        public shell_dumpFunc()
        {
            Name = "dump";
            Run += Shell_dumpFunc_Run;
        }

        private void Shell_dumpFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Shell.DumpLocalVariables(e.Script);
            Shell.DumpGlobalVariables();
        }
    }
    internal class shell_reinitFunc : FunctionBase
    {
        public shell_reinitFunc()
        {
            Name = "init";
            Run += Shell_reinitFunc_Run;
        }

        private void Shell_reinitFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Program.CreateAliceDirectory(true);

        }
    }
    internal class shell_execFunc : FunctionBase
    {
        public shell_execFunc()
        {
            Name = "exec";
            Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            Run += Shell_execFunc_Run;
        }

        private void Shell_execFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string file = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            file = file.Trim('"');
            e.Return = Alice.ExecuteFile(Program.GetScriptPath(file));
        }
    }
    internal class buildpkgFunc : FunctionBase
    {
        public buildpkgFunc()
        {
            Name = "buildpkg";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            MinimumArgCounts = 2;
            Run += BuildpkgFunc_Run;
        }

        private void BuildpkgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string result = Program.BuildPackage(e.Args[0].AsString(), e.Args[1].AsString()) ? "成功" : "失敗";
            Interpreter.Instance.AppendOutput(result, true);
        }
    }
    internal class testpkgFunc : FunctionBase
    {
        public testpkgFunc()
        {
            Name = "testpkg";
            Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            MinimumArgCounts = 1;
            Run += TestpkgFunc_Run;
        }

        private void TestpkgFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string path = Utils.GetSafeString(e.Args, 1, System.IO.Path.GetTempFileName());
            if (Program.BuildPackage(e.Args[0].AsString(), path))
            {
                Interpreter.Instance.AppendOutput("ビルド成功...開始しています", true);
                AlicePackage.Load(path, true);
            }
            else
            {
                Interpreter.Instance.AppendOutput("ビルド失敗", true);
            }
        }
    }
}
