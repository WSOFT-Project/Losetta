using AliceScript;
using BenchmarkDotNet.Attributes;

namespace Losetta.Bench
{
    public class UtilsBench
    {
        [Benchmark]
        public void RegistFunctionBase()
        {
            NameSpaceManager.Add(typeof(Test));
            Alice.Execute("Test.Pow(2);");
            NameSpaceManager.NameSpaces.Clear();
        }


    }

    internal static class Test
    {
        public static int Pow(int x)
        {
            return x * x;
        }
    }
    internal class PowFunc : FunctionBase
    {
        public PowFunc()
        {
            this.Name = "Pow";
            this.MinimumArgCounts = 1;
            this.MaximumArgCounts = 1;
            this.Run += PowFunc_Run;
        }

        private void PowFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            int x = Utils.GetSafeInt(e.Args, 0);
            e.Return = new Variable(x * x);
        }
    }
}
