using AliceScript;
using BenchmarkDotNet.Running;

namespace Losetta.Bench
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<UtilsBench>();
        }
    }
}