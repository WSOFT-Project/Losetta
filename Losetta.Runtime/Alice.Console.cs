using AliceScript.Binding;
using AliceScript.Functions;
using System.Runtime.Versioning;

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
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_BufferHeight()
        {
            return Console.BufferHeight;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_BufferWidth()
        {
            return Console.BufferWidth;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_CursorLeft()
        {
            return Console.CursorLeft;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_CursorSize()
        {
            return Console.CursorSize;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_CursorTop()
        {
            return Console.CursorTop;
        }
        [SupportedOSPlatform("windows")]
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static bool Console_CursorVisible()
        {
            return Console.CursorVisible;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        [SupportedOSPlatform("windows")]
        public static bool Console_CapsLock()
        {
            return Console.CapsLock;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        [SupportedOSPlatform("windows")]
        public static bool Console_NumberLock()
        {
            return Console.NumberLock;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        [SupportedOSPlatform("windows")]
        public static string Console_Title()
        {
            return Console.Title;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_WindowHeight()
        {
            return Console.WindowHeight;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_WindowWidth()
        {
            return Console.WindowWidth;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_LargestWindowHeight()
        {
            return Console.LargestWindowHeight;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_LargestWindowWidth()
        {
            return Console.LargestWindowWidth;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_WindowLeft()
        {
            return Console.WindowLeft;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_WindowTop()
        {
            return Console.WindowTop;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_BackgroundColor()
        {
            return (int)Console.BackgroundColor;
        }
        [AliceFunction(Attribute = FunctionAttribute.PROPERTY)]
        public static int Console_ForegroundColor()
        {
            return (int)Console.ForegroundColor;
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
