using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliceScript.NameSpaces

{
    public class SingletonFunction : FunctionBase
    {
        private static Dictionary<string, Variable> m_singletons =
           new Dictionary<string, Variable>();

        public SingletonFunction()
        {
            this.Name = Constants.SINGLETON;
            this.MinimumArgCounts = 1;
            this.Run += SingletonFunction_Run;
        }

        private void SingletonFunction_Run(object sender, FunctionBaseEventArgs e)
        {

            string expr = e.Args[0].AsString();
            Dictionary<int, int> char2Line;
            expr = Utils.ConvertToScript(expr, out char2Line);

            Variable result;
            if (m_singletons.TryGetValue(expr, out result))
            {
                e.Return = result;
                return;
            }

            ParsingScript tempScript = new ParsingScript(expr);
            result = tempScript.Execute();

            m_singletons[expr] = result;
            e.Return = result;
        }

    }
    internal class UsingStatement : FunctionBase
    {
        public UsingStatement()
        {
            this.Name = "using";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += UsingStatement_Run;
        }

        private void UsingStatement_Run(object sender, FunctionBaseEventArgs e)
        {
            string file = Utils.GetToken(e.Script, Constants.TOKEN_SEPARATION);
            if (NameSpaceManerger.Contains(file))
            {

                e.Script.UsingNamespaces.Add(NameSpaceManerger.NameSpaces[file]);
            }
            else
            {
                ThrowErrorManerger.OnThrowError("該当する名前空間がありません", Exceptions.NAMESPACE_NOT_FOUND, e.Script);
            }
        }
    }
    internal class ImportFunc : FunctionBase
    {
        public ImportFunc()
        {

            this.FunctionName = "import";
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.MinimumArgCounts = 1;
            this.Run += ImportFunc_Run;
        }
        private void ImportFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            string filename = e.Args[0].AsString();
            if (Utils.GetSafeBool(e.Args, 1))
            {
                if (e.Script.Package != null && e.Script.Package.ExistsEntry(filename))
                {
                    Interop.NetLibraryLoader.LoadLibrary(e.Script.Package.GetEntryData(filename));
                    return;
                }
                if (File.Exists(filename))
                {
                    Interop.NetLibraryLoader.LoadLibrary(filename);
                }
                else
                {
                    ThrowErrorManerger.OnThrowError("ファイルが見つかりません", Exceptions.FILE_NOT_FOUND, e.Script);
                }
            }
            else
            {
                if (e.Script.Package != null && e.Script.Package.ExistsEntry(filename))
                {
                    AlicePackage.LoadData(e.Script.Package.GetEntryData(filename));
                    return;
                }
                AlicePackage.Load(filename);
            }
        }
    }
    internal class DelayFunc : FunctionBase
    {
        public DelayFunc()
        {
            this.Name = "delay";
            this.MinimumArgCounts = 0;
            this.Run += DelayFunc_Run;
        }

        private void DelayFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0 && e.Args[0].Type == Variable.VarType.NUMBER)
            {
                Thread.Sleep((int)e.Args[0].Value);
            }
            else
            {
                Thread.Sleep(-1);
            }
        }
    }

    internal class ExitFunction : FunctionBase
    {
        public ExitFunction()
        {
            this.FunctionName = Constants.EXIT;
            this.MinimumArgCounts = 0;
            this.Run += ExitFunction_Run;
        }

        private void ExitFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                Alice.OnExiting(0);
            }
            else
            {
                Alice.OnExiting(Utils.GetSafeInt(e.Args, 0, 0));
            }
        }
    }

}
