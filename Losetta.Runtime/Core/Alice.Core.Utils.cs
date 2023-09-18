namespace AliceScript.NameSpaces.Core
{
    internal partial class CoreFunctions
    {
        public static void Delay()
        {
            Thread.Sleep(-1);
        }
        public static void Delay(int milliSeconds)
        {
            Thread.Sleep(milliSeconds);
        }
        public static void SpinWait(int iterations)
        {
            Thread.SpinWait(iterations);
        }
        public static void Exit()
        {
            Alice.OnExiting();
        }
    }
}
