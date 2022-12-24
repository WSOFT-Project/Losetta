using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

            this.Name = "import";
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
            this.Name = Constants.EXIT;
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

    internal class GetStringFunction : FunctionBase
    {
        public GetStringFunction()
        {
            this.Name = "GetString";
            this.Attribute = FunctionAttribute.LANGUAGE_STRUCTURE;
            this.Run += GetStringFunction_Run;
        }

        private void GetStringFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Script != null)
            {
                e.Return = new Variable(Utils.GetBodyBetween(e.Script));
            }
        }
    }
    internal class LockFunction : FunctionBase
    {
        public LockFunction()
        {
            this.Name = Constants.LOCK;
            this.MinimumArgCounts = 1;
            this.Run += LockFunction_Run;
        }
        private void LockFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string body = Utils.GetBodyBetween(e.Script, Constants.START_GROUP,
                                                       Constants.END_GROUP);
            ParsingScript parsingScript = e.Script.GetTempScript(body);
            lock (e.Args[0])
            {
                parsingScript.ExecuteAll();
            }
        }
    }

    // Prints passed list of argumentsand
    internal class PrintFunction : FunctionBase
    {
        public PrintFunction(bool isWrite = false)
        {
            if (isWrite)
            {
                this.Name = "write";
                m_write = true;
            }
            else
            {
                this.Name = "print";
            }

            //AliceScript925から、Print関数は引数を持つ必要がなくなりました。
            //this.MinimumArgCounts = 1;
            this.Attribute = FunctionAttribute.FUNCT_WITH_SPACE;
            this.Run += PrintFunction_Run;
        }
        private bool m_write;
        private void PrintFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 0)
            {
                AddOutput("", e.Script, !m_write);
            }
            else if (e.Args.Count == 1)
            {
                AddOutput(e.Args[0].AsString(), e.Script, !m_write);
            }
            else
            {
                string format = e.Args[0].AsString();
                e.Args.RemoveAt(0);
                AddOutput(StringFormatFunction.Format(format, e.Args), e.Script, !m_write);
            }
        }

        public static void AddOutput(string text, ParsingScript script = null,
                                     bool addLine = true, bool addSpace = true, string start = "")
        {

            string output = text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendOutput(output);

        }
    }
    public class ReadFunction : FunctionBase
    {
        public ReadFunction()
        {
            this.Name = "read";
            this.Run += ReadFunction_Run;
        }

        private void ReadFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Interpreter.Instance.ReadInput());
        }
    }
    public class StringFormatFunction : FunctionBase
    {
        public StringFormatFunction()
        {
            this.Name = "string_format";
            this.MinimumArgCounts = 1;
            this.Run += StringFormatFunction_Run;
        }

        private void StringFormatFunction_Run(object sender, FunctionBaseEventArgs e)
        {
            string format = e.Args[0].AsString();
            e.Args.RemoveAt(0);
            e.Return = new Variable(Format(format, e.Args));
        }

        public static string Format(string format, List<Variable> args)
        {
            string text = format;
            MatchCollection mc = Regex.Matches(format, @"{[0-9]+:?[a-z,A-Z]*}");
            foreach (Match match in mc)
            {
                int mn = -1;
                string indstr = match.Value.TrimStart('{').TrimEnd('}');
                bool selectSubFormat = false;
                string subFormat = "";
                if (indstr.Contains(":"))
                {
                    string[] vs = indstr.Split(':');
                    indstr = vs[0];
                    if (!string.IsNullOrEmpty(vs[1]))
                    {
                        selectSubFormat = true;
                        subFormat = vs[1];
                    }
                }
                if (int.TryParse(indstr, out mn))
                {
                    if (args.Count > mn)
                    {
                        if (selectSubFormat)
                        {
                            switch (args[mn].Type)
                            {
                                case Variable.VarType.NUMBER:
                                    {
                                        switch (subFormat.ToLower())
                                        {
                                            case "c":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("c"));
                                                    break;
                                                }
                                            case "d":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("d"));
                                                    break;
                                                }
                                            case "e":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("e"));
                                                    break;
                                                }
                                            case "f":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("f"));
                                                    break;
                                                }
                                            case "g":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("g"));
                                                    break;
                                                }
                                            case "n":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("n"));
                                                    break;
                                                }
                                            case "p":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("p"));
                                                    break;
                                                }
                                            case "r":
                                                {
                                                    text = text.Replace(match.Value, args[mn].Value.ToString("r"));
                                                    break;
                                                }
                                            case "x":
                                                {
                                                    text = text.Replace(match.Value, ((int)args[mn].Value).ToString("x"));
                                                    break;
                                                }
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            if (args != null && args[mn] != null)
                            {
                                text = text.Replace(match.Value, args[mn].AsString());
                            }
                        }
                    }
                    else
                    {
                        //範囲外のためスキップ
                        continue;
                    }
                }
                else
                {
                    //数字ではないためスキップ
                    continue;
                }

            }
            return text;
        }
    }

}
