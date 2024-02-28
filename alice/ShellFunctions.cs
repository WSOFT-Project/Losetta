using AliceScript.Binding;
using AliceScript.Functions;
using AliceScript.Packaging;
using AliceScript.Parsing;
using System.IO;

namespace AliceScript.CLI
{
    [AliceNameSpace(Name = "Alice.Shell")]
    internal class ShellFunctions
    {
        public static void Dump(ParsingScript script)
        {
            Shell.DumpLocalVariables(script);
        }
        public static void Init()
        {
            Program.CreateAliceDirectory(true);
        }
        [AliceFunction(Attribute = FunctionAttribute.LANGUAGE_STRUCTURE)]
        public static Variable ExecSh(ParsingScript script)
        {
            string file = Utils.GetToken(script, Constants.TOKEN_SEPARATION);
            file = file.Trim('"');
            return Alice.ExecuteFile(Program.GetScriptPath(file));
        }
        public static void BuildPkg(string fileName, string outFileName)
        {
            string result = Program.BuildPackage(fileName, outFileName) ? "成功" : "失敗";
            Interpreter.Instance.AppendOutput(result, true);
        }
        public static void TestPkg(string fileName)
        {
            TestPkg(fileName, Path.GetTempFileName());
        }
        public static void TestPkg(string fileName, string outFileName)
        {
            if (Program.BuildPackage(fileName, outFileName))
            {
                Interpreter.Instance.AppendOutput("ビルド成功...開始しています", true);
                AlicePackage.Load(outFileName, true);
            }
            else
            {
                Interpreter.Instance.AppendOutput("ビルド失敗", true);
            }
        }
    }
}
