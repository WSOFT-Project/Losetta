using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript
{
    interface INumericFunction { }
    interface IArrayFunction { }
    interface IStringFunction { }

    // Prints passed list of argumentsand
    class PrintFunction : FunctionBase
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
                            if (args != null)
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

    class DataFunction : ParserFunction
    {
        public enum DataMode { ADD, SUBSCRIBE, SEND };

        DataMode m_mode;

        static string s_method;
        static string s_tracking;
        static bool s_updateImmediate = false;

        static StringBuilder s_data = new StringBuilder();

        public DataFunction(DataMode mode = DataMode.ADD)
        {
            m_mode = mode;
        }
        protected override Variable Evaluate(ParsingScript script)
        {
            List<Variable> args = script.GetFunctionArgs();
            string result = "";

            switch (m_mode)
            {
                case DataMode.ADD:
                    Collect(args);
                    break;
                case DataMode.SUBSCRIBE:
                    Subscribe(args);
                    break;
                case DataMode.SEND:
                    result = SendData(s_data.ToString());
                    s_data.Clear();
                    break;
            }

            return new Variable(result);
        }

        public void Subscribe(List<Variable> args)
        {
            s_data.Clear();

            s_method = Utils.GetSafeString(args, 0);
            s_tracking = Utils.GetSafeString(args, 1);
            s_updateImmediate = Utils.GetSafeDouble(args, 2) > 0;
        }

        public void Collect(List<Variable> args)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var arg in args)
            {
                sb.Append(arg.AsString());
            }
            if (s_updateImmediate)
            {
                SendData(sb.ToString());
            }
            else
            {
                s_data.AppendLine(sb.ToString());
            }
        }

        public string SendData(string data)
        {
            if (!string.IsNullOrWhiteSpace(s_method))
            {
                CustomFunction.Run(s_method, new Variable(s_tracking),
                                   new Variable(data));
                return "";
            }
            return data;
        }
    }
    class LockFunction : FunctionBase
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
}
