﻿namespace AliceScript.NameSpaces
{
    public sealed class Alice_Console
    {
        public static void Init()
        {
            try
            {
                //名前空間のメインエントリポイントです。
                NameSpace space = new NameSpace("Alice.Console");

                space.Add(new Console_BeepFunc());
                space.Add(new Console_ClearFunc());
                space.Add(new Console_MoveBufferAreaFunc());
                space.Add(new Console_ReadsFunc(0));
                space.Add(new Console_ReadsFunc(1));
                space.Add(new Console_ReadsFunc(2));
                space.Add(new Console_WriteLineFunc());
                space.Add(new Console_WriteLineFunc(false));
                space.Add(new Console_GetCursorPositionFunc(0));
                space.Add(new Console_GetCursorPositionFunc(1));
                space.Add(new Console_GetCursorPositionFunc(2));
                space.Add(new Console_GetCursorPositionFunc(3));
                space.Add(new Console_GetBufferSizenFunc(0));
                space.Add(new Console_GetBufferSizenFunc(1));
                space.Add(new Console_SetBufferSizeFunc());
                space.Add(new Console_SetCursorPositionFunc());
                space.Add(new Console_SetWindowPositionFunc());
                space.Add(new Console_SetWindowSizeFunc());
                space.Add(new Console_GetWindowFunc(0));
                space.Add(new Console_GetWindowFunc(1));
                space.Add(new Console_GetWindowFunc(2));
                space.Add(new Console_GetWindowFunc(3));
                space.Add(new Console_GetWindowFunc(4));
                space.Add(new Console_GetWindowFunc(5));
                space.Add(new Console_GetColorFunc());
                space.Add(new Console_GetColorFunc(false));
                space.Add(new Console_SetColorFunc());
                space.Add(new Console_SetColorFunc(false));
                space.Add(new Console_ResetColorFunc());

                space.Add("ConsoleColor", "System.ConsoleColor");

                NameSpaceManager.Add(space);
            }
            catch { }
        }
    }

    internal sealed class Console_ResetColorFunc : FunctionBase
    {
        public Console_ResetColorFunc()
        {
            Name = "Console_ResetColor";
            Run += Console_ResetColorFunc_Run;
        }

        private void Console_ResetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.ResetColor();
        }
    }

    internal sealed class Console_GetColorFunc : FunctionBase
    {
        public Console_GetColorFunc(bool bgcolor = true)
        {
            m_BGColor = bgcolor;
            Name = m_BGColor ? "Console_GetBackgroundColor" : "Console_GetForegroundColor";
            Run += Console_GetColorFunc_Run;
        }

        private void Console_GetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = m_BGColor ? new Variable((int)Console.BackgroundColor) : new Variable((int)Console.ForegroundColor);
        }

        private bool m_BGColor = true;
    }

    internal sealed class Console_SetColorFunc : FunctionBase
    {
        public Console_SetColorFunc(bool bgcolor = true)
        {
            m_BGColor = bgcolor;
            Name = m_BGColor ? "Console_SetBackgroundColor" : "Console_SetForegroundColor";
            Run += Console_GetColorFunc_Run;
        }

        private void Console_GetColorFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (m_BGColor)
            {
                Console.BackgroundColor = (ConsoleColor)e.Args[0].AsInt();
            }
            else
            {
                Console.ForegroundColor = (ConsoleColor)e.Args[0].AsInt();
            }
        }

        private bool m_BGColor = true;
    }

    internal sealed class Console_GetCursorPositionFunc : FunctionBase
    {
        public Console_GetCursorPositionFunc(int mode = 0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        Name = "Console_GetCursorLeft";
                        break;
                    }
                case 1:
                    {
                        Name = "Console_GetCursorTop";
                        break;
                    }
                case 2:
                    {
                        Name = "Console_GetCursorSize";
                        break;
                    }
                case 3:
                    {
                        Name = "Console_CursorVisible";
                        break;
                    }
            }
            Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.CursorLeft);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.CursorTop);
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.CursorSize);
                        break;
                    }
                case 3:
                    {
                        if (Console.CursorVisible)
                        {
                        }
                        Console.CursorVisible = Utils.GetSafeBool(e.Args, 1);
                        e.Return = new Variable(Console.CursorVisible);
                        break;
                    }
            }
        }
    }

    internal sealed class Console_GetWindowFunc : FunctionBase
    {
        public Console_GetWindowFunc(int mode = 0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        Name = "Console_GetWindowHeight";
                        break;
                    }
                case 1:
                    {
                        Name = "Console_GetWindowWidth";
                        break;
                    }
                case 2:
                    {
                        Name = "Console_GetWIndowLeft";
                        break;
                    }
                case 3:
                    {
                        Name = "Console_GetWindowTop";
                        break;
                    }
                case 4:
                    {
                        Name = "Console_GetLargestWindowHeight";
                        break;
                    }
                case 5:
                    {
                        Name = "COnsole_GetLargestWindowWidth";
                        break;
                    }
            }
            Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.WindowHeight);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.WindowWidth);
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.WindowLeft);
                        break;
                    }
                case 3:
                    {
                        e.Return = new Variable(Console.WindowTop);
                        break;
                    }
                case 4:
                    {
                        e.Return = new Variable(Console.LargestWindowHeight);
                        break;
                    }
                case 5:
                    {
                        e.Return = new Variable(Console.LargestWindowWidth);
                        break;
                    }
            }
        }
    }

    internal sealed class Console_GetBufferSizenFunc : FunctionBase
    {
        public Console_GetBufferSizenFunc(int mode = 0)
        {
            m_mode = mode;
            switch (m_mode)
            {
                case 0:
                    {
                        Name = "Console_GetBufferHeight";
                        break;
                    }
                case 1:
                    {
                        Name = "Console_GetBufferWidth";
                        break;
                    }

            }
            Run += Console_GetCursorPositionFunc_Run;
        }
        private int m_mode = 0;
        private void Console_GetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.BufferHeight);
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.BufferWidth);
                        break;
                    }

            }
        }
    }

    internal sealed class Console_SetCursorPositionFunc : FunctionBase
    {
        public Console_SetCursorPositionFunc()
        {
            Name = "Console_SetCursorPosition";
            MinimumArgCounts = 2;
            Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetCursorPosition(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }

    internal sealed class Console_SetBufferSizeFunc : FunctionBase
    {
        public Console_SetBufferSizeFunc()
        {
            Name = "Console_SetBufferSize";
            MinimumArgCounts = 2;
            Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetBufferSize(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }

    internal sealed class Console_SetWindowSizeFunc : FunctionBase
    {
        public Console_SetWindowSizeFunc()
        {
            Name = "Console_SetWindowSize";
            MinimumArgCounts = 2;
            Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetWindowSize(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }

    internal sealed class Console_SetWindowPositionFunc : FunctionBase
    {
        public Console_SetWindowPositionFunc()
        {
            Name = "Console_SetWindowPosition";
            MinimumArgCounts = 2;
            Run += Console_SetCursorPositionFunc_Run;
        }

        private void Console_SetCursorPositionFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.SetWindowPosition(e.Args[0].AsInt(), e.Args[1].AsInt());
        }
    }

    internal sealed class Console_BeepFunc : FunctionBase
    {
        public Console_BeepFunc()
        {
            Name = "Console_Beep";
            Run += Console_BeepFunc_Run;
        }

        private void Console_BeepFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count >= 2)
            {
                Console.Beep(e.Args[0].AsInt(), e.Args[1].AsInt());
            }
            else
            {
                Console.Beep();
            }
        }
    }

    internal sealed class Console_ClearFunc : FunctionBase
    {
        public Console_ClearFunc()
        {
            Name = "Console_Clear";
            Run += Console_ClearFunc_Run;
        }

        private void Console_ClearFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.Clear();
        }
    }

    internal sealed class Console_MoveBufferAreaFunc : FunctionBase
    {
        public Console_MoveBufferAreaFunc()
        {
            Name = "Console_MoveBufferArea";
            MinimumArgCounts = 6;
            Run += Console_MoveBufferAreaFunc_Run;
        }

        private void Console_MoveBufferAreaFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            Console.MoveBufferArea(e.Args[0].AsInt(), e.Args[1].AsInt(), e.Args[2].AsInt(), e.Args[3].AsInt(), e.Args[4].AsInt(), e.Args[5].AsInt());
        }
    }

    internal sealed class Console_ReadsFunc : FunctionBase
    {
        public Console_ReadsFunc(int mode = 0)
        {
            m_Mode = mode;
            Run += Console_ReadsFunc_Run;
            switch (m_Mode)
            {
                case 0:
                    {
                        Name = "Console_Read";
                        break;
                    }
                case 1:
                    {
                        Name = "Console_ReadKey";
                        break;
                    }
                case 2:
                    {
                        Name = "Console_ReadLine";
                        break;
                    }
            }
        }

        private void Console_ReadsFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            switch (m_Mode)
            {
                case 0:
                    {
                        e.Return = new Variable(Console.Read());
                        break;
                    }
                case 1:
                    {
                        e.Return = new Variable(Console.ReadKey().KeyChar.ToString());
                        break;
                    }
                case 2:
                    {
                        e.Return = new Variable(Console.ReadLine());
                        break;
                    }
            }
        }

        private int m_Mode = 0;
    }

    internal sealed class Console_WriteLineFunc : FunctionBase
    {
        public Console_WriteLineFunc(bool wline = true)
        {
            m_WLine = wline;
            Run += Console_WriteLineFunc_Run;
            Name = m_WLine ? "Console_WriteLine" : "Console_Write";
        }

        private void Console_WriteLineFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count < 1)
            {
                if (m_WLine)
                {
                    Console.WriteLine();
                }
            }
            else if (e.Args.Count == 1)
            {
                if (m_WLine)
                {
                    Console.WriteLine(e.Args[0].AsString());
                }
                else
                {
                    Console.Write(e.Args[0].AsString());
                }
            }
            else
            {
                string text = e.Args[0].AsString();
                e.Args.RemoveAt(0);
                text = StringFormatFunction.Format(text, e.Args);
                if (m_WLine)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    Console.Write(text);
                }
            }
        }

        private bool m_WLine = false;
    }
}
