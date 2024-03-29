﻿using AliceScript.Functions;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AliceScript.CLI
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

            string filename = Path.Combine(AppContext.BaseDirectory, ".alice", "shell");
            //REPLはデバッグモードに
            Program.IsDebugMode = true;
            if (File.Exists(filename))
            {
                Alice.ExecuteFile(filename);
            }
            RunLoop();
        }

        internal static void ThrowErrorManager_ThrowError(object sender, ThrowErrorEventArgs e)
        {
            if (!Program.allow_throw)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.ErrorCode.ToString() + "(0x" + ((int)e.ErrorCode).ToString("x3") + ")" + (string.IsNullOrEmpty(e.Message) ? string.Empty : ": " + e.Message));
            if (!string.IsNullOrWhiteSpace(e.HelpLink))
            {
                sb.AppendLine("詳細情報: " + e.HelpLink);
            }
            if (Program.IsDebugMode)
            {
                if (e.Script is not null)
                {
                    if (e.Script.StackTrace.Count > 0)
                    {
                        var st = new List<ParsingScript.StackInfo>(e.Script.StackTrace);
                        st.Reverse();
                        sb.AppendLine("スタックトレース");
                        foreach (var ss in st)
                        {
                            sb.Append("  ");
                            sb.AppendLine(ss.ToString());
                        }
                    }
                }
            }
            if (allow_throw)
            {
                PrintColor(sb.ToString(), ConsoleColor.White, ConsoleColor.DarkRed);
                if (Program.IsDebugMode)
                {
                PauseInput:
                    Console.Write("このエラーを無視して続行するには[C]を、終了する場合はそれ以外のキーを入力してください...");
                    switch (Console.ReadKey().Key)
                    {
                        case ConsoleKey.C:
                            {
                                e.Handled = true;
                                break;
                            }
                        case ConsoleKey.D:
                            {
                                Console.WriteLine();
                                DumpLocalVariables(e.Script);
                                goto PauseInput;
                            }
                        case ConsoleKey.W:
                            {
                                Console.WriteLine();
                                Console.Write("評価する式>>>>");
                                string code = Console.ReadLine();
                                Console.WriteLine(e.Script.GetChildScript(code).Process());
                                goto PauseInput;
                            }
                    }
                    Console.WriteLine();
                }
            }
            if (throw_redirect_files.Count > 0)
            {
                foreach (string fn in throw_redirect_files)
                {
                    File.AppendAllText(fn, sb.ToString());
                }

            }
            s_PrintingCompleted = true;

        }
        // 文字列が表示幅より短ければ、左側と右側に何文字の空白が必要なのかを計算する。
        // 文字列が表示幅より長ければ、何文字目から表示するのかを計算する。
        private static string Centering(string s, int width)
        {
            int space = width - s.Length;
            return space >= 0 ? new string(' ', space / 2) + s + new string(' ', space - (space / 2)) : s.Substring(-space / 2).Remove(width);
        }
        private static void AddDictionaryScriptVariables(ParsingScript script, ref Dictionary<string, ParserFunction> dic)
        {
            foreach (string s in script.Variables.Keys)
            {
                dic.Add(s, script.Variables[s]);
            }
            if (script.ParentScript is not null)
            {
                AddDictionaryScriptVariables(script.ParentScript, ref dic);
            }
        }
        public static void DumpLocalVariables(ParsingScript script)
        {
            if (script is null) { return; }
            DumpLocalVariables(script.ParentScript);
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
                if (dic[s] is ValueFunction vf)
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
            if (tabFileIndex == 0 || script.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
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

                if (script.EndsWith(Constants.CONTINUE_LINE.ToString(), StringComparison.Ordinal))
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

                if (commands.Count == 0 || !commands[^1].Equals(script))
                {
                    commands.Add(script);
                }
                if (!script.EndsWith(Constants.END_STATEMENT.ToString(), StringComparison.Ordinal))
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

#if !DEBUG_THROW
            try
#endif
            {
                if (!string.IsNullOrWhiteSpace(filename))
                {
                    result = System.Threading.Tasks.Task.Run(() =>
                  Interpreter.Instance.ProcessFileAsync(filename, true)).Result;
                }
                else
                {
                    CurrentScript = CurrentScript is null ? Alice.GetScript(script, filename, true) : CurrentScript.GetChildScript(script);
                    result = CurrentScript.Process();
                }
            }
#if !DEBUG_THROW
            catch (Exception exc)
            {
                ParsingScript.GetTopLevelScript().OnThrowError(exc);
            }
#endif

            if (!s_PrintingCompleted)
            {
                string output = Interpreter.Instance.Output;
                if (!string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine(output);
                }
                else if (result is not null)
                {
                    output = result.AsString();
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        Console.WriteLine(output);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                PrintColor(errorMsg + Environment.NewLine, ConsoleColor.Red);
                errorMsg = string.Empty;
            }
        }

        private static string GetPrompt()
        {
            string path = Directory.GetCurrentDirectory();
            return $"Alice {path}>>";
        }

        internal static void PrintColor(string output, ConsoleColor fgcolor, ConsoleColor? bgcolor = null)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            ConsoleColor currentBackground = Console.BackgroundColor;

            Console.ForegroundColor = fgcolor;
            if (bgcolor.HasValue)
            {
                Console.BackgroundColor = bgcolor.Value;
            }

            Console.Write(output);

            Console.ForegroundColor = currentForeground;
            Console.BackgroundColor = currentBackground;
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
                PrintColor(e.Output, ConsoleColor.Cyan);
                //Console.Write(e.Output);
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
