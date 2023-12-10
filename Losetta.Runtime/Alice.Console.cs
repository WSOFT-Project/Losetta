using AliceScript.Binding;
using System.Runtime.Versioning;
using System.Text;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Console
    {
        public static void Init()
        {
            Alice.RegisterFunctions<ConsoleFunctions>();

            NameSpace space = new NameSpace("Alice.Console");

            space.Add("ConsoleColor", "System.ConsoleColor");

            NameSpaceManager.Add(space);
        }
    }
    [AliceNameSpace(Name = "Alice.Console")]
    internal sealed class ConsoleFunctions
    {
        public static void Console_SetCursorPosition(int left, int top)
        {
            Console.SetCursorPosition(left, top);
        }
        public static void Console_Clear()
        {
            Console.Clear();
        }
        [SupportedOSPlatform("windows")]
        public static void Console_MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft, int targetTop)
        {
            Console.MoveBufferArea(sourceLeft, sourceTop, sourceWidth, sourceHeight, targetLeft, targetTop);
        }
        #region プロパティ
        public static int Console_BufferHeight
        {
            get => Console.BufferHeight;
            set => Console.BufferHeight = value;
        }
        public static int Console_BufferWidth
        {
            get => Console.BufferWidth;
            set => Console.BufferWidth = value;
        }

        public static int Console_CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public static int Console_CursorSize
        {
            get => Console.CursorSize;
            set => Console.CursorSize = value;
        }

        public static int Console_CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }
        [SupportedOSPlatform("windows")]
        public static bool Console_CursorVisible
        {
            get => Console.CursorVisible;
            set => Console.CursorVisible = value;
        }

        [SupportedOSPlatform("windows")]
        public static bool Console_CapsLock => Console.CapsLock;

        [SupportedOSPlatform("windows")]
        public static bool Console_NumberLock => Console.NumberLock;

        [SupportedOSPlatform("windows")]
        public static string Console_Title
        {
            get => Console.Title;
            set => Console.Title = value;
        }

        public static int Console_WindowHeight
        {
            get => Console.WindowHeight;
            set => Console.WindowHeight = value;
        }

        public static int Console_WindowWidth
        {
            get => Console.WindowWidth;
            set => Console.WindowWidth = value;
        }
        public static string Console_InputEncoding
        {
            get => Console.InputEncoding.EncodingName;
            set => Console.InputEncoding = Encoding.GetEncoding(value);
        }
        public static string Console_OutputEncoding
        {
            get => Console.OutputEncoding.EncodingName;
            set => Console.OutputEncoding = Encoding.GetEncoding(value);
        }

        public static int Console_LargestWindowHeight => Console.LargestWindowHeight;
        public static int Console_LargestWindowWidth => Console.LargestWindowWidth;
        public static int Console_WindowLeft
        {
            get => Console.WindowLeft;
            set => Console.WindowLeft = value;
        }
        public static int Console_WindowTop
        {
            get => Console.WindowTop;
            set => Console.WindowTop = value;
        }
        public static int Console_BackgroundColor
        {
            get => (int)Console.BackgroundColor;
            set => Console.BackgroundColor = (ConsoleColor)value;
        }
        public static int Console_ForegroundColor
        {
            get => (int)Console.ForegroundColor;
            set => Console.ForegroundColor = (ConsoleColor)value;
        }
        #endregion
        #region コンソール入力
        public static int Console_Read()
        {
            return Console.Read();
        }
        public static string Console_ReadKey()
        {
            return Console.ReadKey().KeyChar.ToString();
        }
        public static string Console_ReadKey(bool intercept)
        {
            return Console.ReadKey(intercept).KeyChar.ToString();
        }
        public static string Console_ReadLine()
        {
            return Console.ReadLine();
        }
        #endregion
        #region コンソール出力
        public static void Console_Write(Variable v)
        {
            Console.Write(v.AsString());
        }
        public static void Console_WriteLine(Variable v)
        {
            Console.WriteLine(v.AsString());
        }
        public static void Console_Beep()
        {
            Console.Beep();
        }

        [SupportedOSPlatform("windows")]
        public static void Console_Beep(int frequency, int duration)
        {
            Console.Beep(frequency, duration);
        }
        #endregion
        #region コンソールウインドウ関連
        [SupportedOSPlatform("windows")]
        public static void Console_SetTitle(string title)
        {
            Console.Title = title;
        }
        [SupportedOSPlatform("windows")]
        public static void Console_SetWindowSize(int width, int height)
        {
            Console.SetWindowSize(width, height);
        }
        [SupportedOSPlatform("windows")]
        public static void Console_SetWindowPosition(int left, int top)
        {
            Console.SetWindowPosition(left, top);
        }
        [SupportedOSPlatform("windows")]
        public static void Console_SetBufferSize(int width, int height)
        {
            Console.SetBufferSize(width, height);
        }
        #endregion
        #region コンソール色関連
        public static void Console_ResetColor()
        {
            Console.ResetColor();
        }
        public static void Console_SetBackgroundColor(int color)
        {
            Console.BackgroundColor = (ConsoleColor)color;
        }
        public static void Console_SetForegroundColor(int color)
        {
            Console.ForegroundColor = (ConsoleColor)color;
        }
        #endregion
    }
}
