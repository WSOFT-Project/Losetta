using BenchmarkDotNet.Running;

namespace Losetta.Bench
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<UtilsBench>();
        }
    }
}