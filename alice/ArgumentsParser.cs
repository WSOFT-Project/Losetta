﻿using System.Text.RegularExpressions;

namespace AliceScript.CLI
{
    public class ParsedArguments
    {
        public static Regex FlagPattern = new Regex("-.*", RegexOptions.Compiled);
        public static Regex ValuePattern = new Regex("-.*=.*", RegexOptions.Compiled);
        public ParsedArguments(string[] args)
        {
            m_init(args);
        }
        public ParsedArguments(string args)
        {
            m_init(args.Split(new string[] { "\r\n" }, StringSplitOptions.None));
        }
        private void m_init(string[] args)
        {
            Script = "";
            bool aarg = false;
            bool src = false;
            foreach (string arg in args)
            {
                var argl = arg.ToLower();
                if(argl=="-h" || argl=="-help" || argl == "?")
                {
                    NeedHelp = true;
                }
                if (argl == "--arg" || argl == "--args")
                {
                    aarg = true;
                    continue;
                }
                if (argl == "-e" || argl == "-execute" || argl == "-evaluate")
                {
                    src = true;
                    Flags.Add("e");
                    continue;
                }
                if (aarg)
                {
                    Args.Add(arg);
                }
                else if (src)
                {
                    Script += arg + AliceScript.Constants.END_STATEMENT;
                }
                else
                {
                    if (arg.StartsWith("-", StringComparison.CurrentCulture))
                    {
                        var mc = FlagPattern.Matches(arg);
                        if (mc.Count > 0)
                        {
                            foreach (Match m in mc)
                            {
                                var mc2 = ValuePattern.Matches(arg);
                                if (mc2.Count > 0)
                                {
                                    foreach (Match m2 in mc2)
                                    {
                                        string v = m2.Value;
                                        v = v.TrimStart('-'); string[] vs = v.Split('=');
                                        Values.Add(vs[0].ToLower(), vs[1]);
                                    }
                                }
                                else
                                {
                                    string v = m.Value;
                                    v = v.TrimStart('-');
                                    Flags.Add(v.ToLower());
                                }
                            }
                        }
                        else
                        {
                            Files.Add(arg);
                        }
                    }
                    else
                    {
                        Files.Add(arg);
                    }
                }
            }
        }
        private Dictionary<string, string> m_values = new Dictionary<string, string>();
        public List<string> Args = new List<string>();
        public Dictionary<string, string> Values
        {
            get => m_values;
            set => m_values = value;
        }
        private List<string> m_flags = new List<string>();
        public List<string> Flags
        {
            get => m_flags;
            set => m_flags = value;
        }
        private List<string> m_files = new List<string>();
        public List<string> Files
        {
            get => m_files;
            set => m_files = value;
        }
        public bool NeedHelp { get; set; }
        public string Script { get; set; }
    }
}
