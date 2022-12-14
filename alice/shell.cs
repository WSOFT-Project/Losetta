using AliceScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace alice
{
    public class Shell
    {
        private static bool allow_print = true;
        private static List<string> print_redirect_files = new List<string>();
        private static bool allow_debug_print = true;
        private static List<string> debug_print_redirect_files = new List<string>();
        private static bool allow_throw = true;
        private static List<string> throw_redirect_files = new List<string>();

        private enum NEXT_CMD
        {
            NONE = 0,
            PREV = -1,
            NEXT = 1,
            TAB = 2
        };
        public static bool canDebug = false;
        [STAThread]
        public static void Do()
        {
            ClearLine();


            //標準出力
            Interpreter.Instance.OnOutput += Print;
            //デバッグ出力
            Interpreter.Instance.OnDebug += Debug_Print;
            //例外のハンドル
            ThrowErrorManerger.HandleError = true;
            //例外出力
            ThrowErrorManerger.ThrowError += ThrowErrorManerger_ThrowError;

            string filename = Path.Combine(AppContext.BaseDirectory, ".alice", "shell");
            if (File.Exists(filename))
            {
                Alice.ExecuteFile(filename);
            }
            RunLoop();
        }

        private static bool mainfile = false;
        private static void ThrowErrorManerger_ThrowError(object sender, ThrowErrorEventArgs e)
        {
            if (e.Message != "")
            {
                string throwmsg = "エラー0x" + ((int)e.ErrorCode).ToString("x3") + ": ";
                if (!string.IsNullOrEmpty(e.Message))
                {
                    throwmsg += e.Message;
                }
                if (e.Script != null)
                {
                    throwmsg += " " + e.Script.OriginalLineNumber + "行 コード:" + e.Script.OriginalLine + " ファイル名:" + e.Script.Filename;
                }
                throwmsg += "\r\n";
                if (allow_throw)
                {
                    AliceScript.Utils.PrintColor(throwmsg, ConsoleColor.Red);
                    DumpLocalVariables(e.Script);
                    DumpGlobalVariables();
                }
                if (throw_redirect_files.Count > 0)
                {
                    foreach (string fn in throw_redirect_files)
                    {
                        File.AppendAllText(fn, throwmsg);
                    }

                }
                s_PrintingCompleted = true;
            }

        }
        // 文字列が表示幅より短ければ、左側と右側に何文字の空白が必要なのかを計算する。
        // 文字列が表示幅より長ければ、何文字目から表示するのかを計算する。
        private static string Centering(string s, int width)
        {
            int space = width - s.Length;
            if (space >= 0)
            {
                return new string(' ', space / 2) + s + new string(' ', space - (space / 2));
            }
            else
            {
                return s.Substring(-space / 2).Remove(width);
            }
        }
        private static void AddDictionaryScriptVariables(ParsingScript script, ref Dictionary<string, ParserFunction> dic)
        {
            foreach (string s in script.Variables.Keys)
            {
                dic.Add(s, script.Variables[s]);
            }
            if (script.ParentScript != null)
            {
                AddDictionaryScriptVariables(script.ParentScript, ref dic);
            }
        }
        public static void DumpLocalVariables(ParsingScript script)
        {
            if (script == null) { return; }
            Dictionary<string, ParserFunction> dic = new Dictionary<string, ParserFunction>();
            AddDictionaryScriptVariables(script, ref dic);
            if (dic.Count <= 0)
            {
                return;
            }
            List<string> names = new List<string>();
            List<string> types = new List<string>();
            List<string> contents = new List<string>();
            int namemax = 4;
            int typemax = 4;
            int contentmax = 7;
            names.Add("Name");
            types.Add("Type");
            contents.Add("Content");
            foreach (string s in dic.Keys)
            {
                if (dic[s] is GetVarFunction vf)
                {
                    names.Add(s);
                    if (s.Length > namemax)
                    {
                        namemax = s.Length;
                    }
                    string type = vf.Value.Type.ToString();
                    types.Add(type);
                    if (type.Length > typemax)
                    {
                        typemax = type.Length;
                    }
                    string content = vf.Value.AsString();
                    contents.Add(content);
                    if (content.Length > contentmax)
                    {
                        contentmax = content.Length;
                    }
                }
            }
            for (int i = 0; i < names.Count; i++)
            {
                string print = "|Local|";
                print += Centering(names[i], namemax + 2) + "|";
                print += Centering(types[i], typemax + 2) + "|";
                print += Centering(contents[i], contentmax + 2) + "|";
                Console.WriteLine(print);
            }
        }
        public static void DumpGlobalVariables()
        {
            Dictionary<string, ParserFunction> dic = ParserFunction.s_variables;
            if (dic.Count <= 0)
            {
                return;
            }
            List<string> names = new List<string>();
            List<string> types = new List<string>();
            List<string> contents = new List<string>();
            int namemax = 4;
            int typemax = 4;
            int contentmax = 7;
            names.Add("Name");
            types.Add("Type");
            contents.Add("Content");
            foreach (string s in dic.Keys)
            {
                try
                {
                    if (dic[s] is GetVarFunction vf)
                    {
                        names.Add(s);
                        if (s.Length > namemax)
                        {
                            namemax = s.Length;
                        }
                        string type = vf.Value.Type.ToString();
                        types.Add(type);
                        if (type.Length > typemax)
                        {
                            typemax = type.Length;
                        }
                        string content = vf.Value.AsString();
                        contents.Add(content);
                        if (content.Length > contentmax)
                        {
                            contentmax = content.Length;
                        }
                    }
                }
                catch { }
            }
            for (int i = 0; i < names.Count; i++)
            {
                string print = "|Global|";
                print += Centering(names[i], namemax + 2) + "|";
                print += Centering(types[i], typemax + 2) + "|";
                print += Centering(contents[i], contentmax + 2) + "|";
                Console.WriteLine(print);
            }
        }
        private static void SplitByLast(string str, string sep, ref string a, ref string b)
        {
            int it = str.LastIndexOfAny(sep.ToCharArray());
            a = it == -1 ? "" : str.Substring(0, it + 1);
            b = it == -1 ? str : str.Substring(it + 1);
        }

        private static string CompleteTab(string script, string init, ref int tabFileIndex,
          ref string start, ref string baseStr, ref string startsWith)
        {
            if (tabFileIndex > 0 && !script.Equals(init))
            {
                // The user has changed something in the input field
                tabFileIndex = 0;
            }
            if (tabFileIndex == 0 || script.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                // The user pressed tab the first time or pressed it on a directory
                string path = "";
                SplitByLast(script, " ", ref start, ref path);
                SplitByLast(path, "/\\", ref baseStr, ref startsWith);
            }

            tabFileIndex++;
            string result = Utils.GetFileEntry(baseStr, tabFileIndex, startsWith);
            result = result.Length == 0 ? startsWith : result;
            return start + baseStr + result;
        }

        private static bool exit = false;
        private static void RunLoop()
        {
            List<string> commands = new List<string>();
            StringBuilder sb = new StringBuilder();
            int cmdPtr = 0;
            int tabFileIndex = 0;
            bool arrowMode = false;
            string start = "", baseCmd = "", startsWith = "", init = "", script;
            string previous = "";

            while (!exit)
            {
                sb.Clear();

                NEXT_CMD nextCmd = NEXT_CMD.NONE;
                script = previous + GetConsoleLine(ref nextCmd, init).Trim();

                if (script.EndsWith(Constants.CONTINUE_LINE.ToString()))
                {
                    previous = script.Remove(script.Length - 1);
                    init = "";
                    continue;
                }

                if (nextCmd == NEXT_CMD.PREV || nextCmd == NEXT_CMD.NEXT)
                {
                    if (arrowMode || nextCmd == NEXT_CMD.NEXT)
                    {
                        cmdPtr += (int)nextCmd;
                    }
                    cmdPtr = cmdPtr < 0 || commands.Count == 0 ?
                             cmdPtr + commands.Count :
                             cmdPtr % commands.Count;
                    init = commands.Count == 0 ? script : commands[cmdPtr];
                    arrowMode = true;
                    continue;
                }
                else if (nextCmd == NEXT_CMD.TAB)
                {
                    init = CompleteTab(script, init, ref tabFileIndex,
                             ref start, ref baseCmd, ref startsWith);
                    continue;
                }

                init = "";
                previous = "";
                tabFileIndex = 0;
                arrowMode = false;

                if (string.IsNullOrWhiteSpace(script))
                {
                    continue;
                }

                if (commands.Count == 0 || !commands[commands.Count - 1].Equals(script))
                {
                    commands.Add(script);
                }
                if (!script.EndsWith(Constants.END_STATEMENT.ToString()))
                {
                    script += Constants.END_STATEMENT;
                }

                ProcessScript(script);
                cmdPtr = commands.Count - 1;
            }
        }

        private static string GetConsoleLine(ref NEXT_CMD cmd, string init = "",
                                             bool enhancedMode = true)
        {
            //string line = init;
            StringBuilder sb = new StringBuilder(init);
            int delta = init.Length - 1;
            string prompt = GetPrompt();
            Console.Write(prompt);
            Console.Write(init);

            if (!enhancedMode)
            {
                return Console.ReadLine();
            }

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.UpArrow)
                {
                    cmd = NEXT_CMD.PREV;
                    ClearLine(prompt, sb.ToString());
                    return sb.ToString();
                }
                if (key.Key == ConsoleKey.DownArrow)
                {
                    cmd = NEXT_CMD.NEXT;
                    ClearLine(prompt, sb.ToString());
                    return sb.ToString();
                }
                if (key.Key == ConsoleKey.RightArrow)
                {
                    delta = Math.Max(-1, Math.Min(++delta, sb.Length - 1));
                    SetCursor(prompt, sb.ToString(), delta + 1);
                    continue;
                }
                if (key.Key == ConsoleKey.LeftArrow)
                {
                    delta = Math.Max(-1, Math.Min(--delta, sb.Length - 1));
                    SetCursor(prompt, sb.ToString(), delta + 1);
                    continue;
                }
                if (key.Key == ConsoleKey.Tab)
                {
                    cmd = NEXT_CMD.TAB;
                    ClearLine(prompt, sb.ToString());
                    return sb.ToString();
                }
                if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.Delete)
                {
                    if (sb.Length > 0)
                    {
                        delta = key.Key == ConsoleKey.Backspace ?
                          Math.Max(-1, Math.Min(--delta, sb.Length - 2)) : delta;
                        if (delta < sb.Length - 1)
                        {
                            sb.Remove(delta + 1, 1);
                        }
                        SetCursor(prompt, sb.ToString(), Math.Max(0, delta + 1));
                    }
                    continue;
                }
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                if (key.KeyChar == Constants.EMPTY)
                {
                    continue;
                }

                ++delta;
                Console.Write(key.KeyChar);
                if (delta < sb.Length)
                {
                    delta = Math.Max(0, Math.Min(delta, sb.Length - 1));
                    sb.Insert(delta, key.KeyChar.ToString());
                }
                else
                {
                    sb.Append(key.KeyChar);
                }
                SetCursor(prompt, sb.ToString(), delta + 1);
            }
        }
        private static ParsingScript CurrentScript = null;
        private static void ProcessScript(string script, string filename = "")
        {
            s_PrintingCompleted = false;
            string errorMsg = null;
            Variable result = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    result = System.Threading.Tasks.Task.Run(() =>
                  Interpreter.Instance.ProcessFileAsync(filename, true)).Result;
                    //Interpreter.Instance.ProcessFile(filename, true)).Result;
                }
                else
                {
                    //Interpreter.Instance.ProcessAsync(script, filename)).Result;
                    if (CurrentScript == null)
                    {
                        CurrentScript = Alice.GetScript(script, filename, true);
                    }
                    else
                    {
                        CurrentScript = CurrentScript.GetTempScript(script);
                    }
                    result = CurrentScript.Process();
                }
            }
            catch (Exception exc)
            {
                errorMsg = exc.InnerException != null ? exc.InnerException.Message : exc.Message;
                ParserFunction.InvalidateStacksAfterLevel(0);
            }

            if (!s_PrintingCompleted)
            {
                string output = Interpreter.Instance.Output;
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine(output);
                }
                else if (result != null)
                {
                    output = result.AsString(false, false);
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine(output);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                Utils.PrintColor(errorMsg + Environment.NewLine, ConsoleColor.Red);
                errorMsg = string.Empty;
            }
        }

        private static string GetPrompt()
        {
            string path = Directory.GetCurrentDirectory();
            return string.Format("{0}>>", path);
        }

        private static void ClearLine(string part1 = "", string part2 = "")
        {
            string spaces = new string(' ', part1.Length + part2.Length + 1);
            Console.Write("\r{0}\r", spaces);
        }

        private static void SetCursor(string prompt, string line, int pos)
        {
            ClearLine(prompt, line);
            Console.Write("{0}{1}\r{2}{3}",
              prompt, line, prompt, line.Substring(0, pos));
        }

        private static void Print(object sender, OutputAvailableEventArgs e)
        {
            if (allow_print)
            {
                Console.Write(e.Output);
            }
            if (print_redirect_files.Count > 0)
            {
                foreach (string fn in print_redirect_files)
                {
                    File.AppendAllText(fn, e.Output);
                }
            }
            s_PrintingCompleted = true;
        }

        private static void Debug_Print(object sender, OutputAvailableEventArgs e)
        {
            if (allow_debug_print)
            {
                Console.Write(e.Output);
            }
            if (debug_print_redirect_files.Count > 0)
            {
                foreach (string fn in debug_print_redirect_files)
                {
                    File.AppendAllText(fn, e.Output);
                }
            }
            s_PrintingCompleted = true;
        }

        private static bool s_PrintingCompleted = false;
    }
}
