using AliceScript.Binding;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Diagnostics
    {
        public static void Init()
        {
            Alice.RegisterFunctions<DiagnosticsFunctions>();

            NameSpace space = new NameSpace("Alice.Diagnostics");

            space.Add<Stopwatch>();
            space.Add<ProcessObject>();

            space.Add("ProcessWindowStyle", "System.Diagnostics.ProcessWindowStyle");

            NameSpaceManager.Add(space);
        }
    }
    [AliceNameSpace(Name = "Alice.Diagnostics")]
    internal sealed class DiagnosticsFunctions
    {
        #region プロセス操作
        public static ProcessObject[] Process_GetProcessesByName(string name)
        {
            Process[] ps = Process.GetProcessesByName(name);
            List<ProcessObject> pss = new List<ProcessObject>();
            foreach (Process p in ps)
            {
                ProcessObject po = new ProcessObject();
                po.Process = p;
                pss.Add(po);
            }
            return pss.ToArray();
        }
        public static ProcessObject[] Process_GetProcessesByName(string name, string machineName)
        {
            Process[] ps = Process.GetProcessesByName(name, machineName);
            List<ProcessObject> pss = new List<ProcessObject>();
            foreach (Process p in ps)
            {
                ProcessObject po = new ProcessObject();
                po.Process = p;
                pss.Add(po);
            }
            return pss.ToArray();
        }
        public static ProcessObject Process_GetProcessById(int id)
        {
            ProcessObject po = new ProcessObject();
            po.Process = Process.GetProcessById(id);
            return po;
        }
        public static ProcessObject Process_GetProcessById(int id, string machineName)
        {
            ProcessObject po = new ProcessObject();
            po.Process = Process.GetProcessById(id, machineName);
            return po;
        }
        public static ProcessObject Process_Start(string path, string arguments = "")
        {
            var po = new ProcessObject
            {
                Process = Process.Start(path, arguments)
            };
            return po;
        }
        public static ProcessObject Exec(string fileName, bool waitForExit = true, bool useShell = false)
        {
            return Exec(fileName, string.Empty, waitForExit, useShell);
        }
        public static ProcessObject Exec(string fileName, string arguments, bool waitForExit = true, bool useShell = false)
        {
            var p = new Process();
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.StartInfo.RedirectStandardInput = !useShell;
            p.StartInfo.RedirectStandardOutput = !useShell;
            p.StartInfo.UseShellExecute = useShell;

            p.Start();

            if (waitForExit)
            {
                p.WaitForExit();
            }
            return new ProcessObject()
            {
                Process = p
            };
        }
        #endregion
        #region デバッグ機能
        public static int Debug_IndentLevel { get; set; }
        public static void Debug_Indent()
        {
            Debug_IndentLevel++;
        }
        public static void Debug_Unindent()
        {
            if (Debug_IndentLevel > 0)
            {
                Debug_IndentLevel--;
            }
        }
        public static void Debug_Print(ParsingScript script)
        {
            AddDebugOutput(string.Empty, true);
        }
        public static void Debug_Print(string text)
        {
            AddDebugOutput(text, true);
        }
        public static void Debug_Print(string format, Variable[] items)
        {
            AddDebugOutput(StringFormatFunction.Format(format, items), true);
        }
        public static void Debug_PrintIf(bool condition)
        {
            if (condition)
            {
                AddDebugOutput(string.Empty, true);
            }
        }
        public static void Debug_PrintIf(bool condition, string text)
        {
            if (condition)
            {
                AddDebugOutput(text, true);
            }
        }
        public static void Debug_PrintIf(bool condition, string format, Variable[] items)
        {
            if (condition)
            {
                AddDebugOutput(StringFormatFunction.Format(format, items), true);
            }
        }
        public static void Debug_Write()
        {
            AddDebugOutput(string.Empty, false);
        }
        public static void Debug_Write(string text)
        {
            AddDebugOutput(text, false);
        }
        public static void Debug_Write(string format, Variable[] items)
        {
            AddDebugOutput(StringFormatFunction.Format(format, items), false);
        }
        public static void Debug_WriteIf(bool condition)
        {
            if (condition)
            {
                AddDebugOutput(string.Empty, false);
            }
        }
        public static void Debug_WriteIf(bool condition, string text)
        {
            if (condition)
            {
                AddDebugOutput(text, false);
            }
        }
        public static void Debug_WriteIf(bool condition, string format, Variable[] items)
        {
            if (condition)
            {
                AddDebugOutput(StringFormatFunction.Format(format, items), false);
            }
        }
        internal static void AddDebugOutput(string text,
                                     bool addLine = true, bool addSpace = true, string start = "", string indent = "    ")
        {
            var indents = new StringBuilder();
            for (int i = 0; i < Debug_IndentLevel; i++)
            {
                indents.Append(indent);
            }
            string output = indents + text + (addLine ? Environment.NewLine : string.Empty);
            Interpreter.Instance.AppendDebug(output);
        }
        #endregion
        #region アサーション関連
        public static void Assert(bool condition)
        {
            if (!condition)
            {
                throw new ScriptException("アサーションが失敗しました", Exceptions.ASSERTION_ERROR);
            }
        }
        public static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new ScriptException(message, Exceptions.ASSERTION_ERROR);
            }
        }
        public static void AssertEqual(Variable expected, Variable actual)
        {
            if (!expected.Equals(actual))
            {
                throw new ScriptException("アサーションが失敗しました", Exceptions.ASSERTION_ERROR);
            }
        }
        public static void AssertEqual(Variable expected, Variable actual, string message)
        {
            if (!expected.Equals(actual))
            {
                throw new ScriptException(message, Exceptions.ASSERTION_ERROR);
            }
        }
        #endregion
    }

    [AliceObject(Name = "Process")]
    internal sealed class ProcessObject
    {

        internal Process Process = new Process();

        public void Close()
        {
            Process.Close();
        }

        public void CloseMainWindow()
        {
            Process.CloseMainWindow();
        }

        public void Kill()
        {
            Process.Kill();
        }

        public void Kill(bool entireProcessTree)
        {
#if NETCOREAPP3_0_OR_GREATER
            Process.Kill(entireProcessTree);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }

        public void Reflesh()
        {
            Process.Refresh();
        }

        public void Start()
        {
            Process.Start();
        }

        public void WaitForExit()
        {
            Process.WaitForExit();
        }

        public void WaitForExit(int milliseconds)
        {
            Process.WaitForInputIdle(milliseconds);
        }

        public void WaitForInputIdle()
        {
            Process.WaitForExit();
        }

        public void WaitFWaitForInputIdleorExit(int milliseconds)
        {
            Process.WaitForInputIdle(milliseconds);
        }
        public string Read()
        {
            StringBuilder sb = new StringBuilder();
            while (Process.StandardOutput.Peek() >= 0)
            {
                sb.Append((char)Process.StandardOutput.Read());
            }

            return sb.ToString();
        }
        public string ReadToEnd()
        {
            return Process.StandardOutput.ReadToEnd();
        }
        public string ReadLine()
        {
            return Process.StandardOutput.ReadLine();
        }
        public void Write(string input)
        {
            Process.StandardInput.Write(input);
            Process.StandardInput.Flush();
        }
        public void WriteLine(string input)
        {
            Process.StandardInput.WriteLine(input);
            Process.StandardInput.Flush();
        }
        public int ExitCode => Process.ExitCode;
        public ProcessStartInfo ProcessStartInfo => Process.StartInfo;
    }
}
