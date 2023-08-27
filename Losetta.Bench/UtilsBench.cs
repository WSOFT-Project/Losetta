using AliceScript;
using BenchmarkDotNet.Attributes;

namespace Losetta.Bench
{
    public class UtilsBench
    {
        private const string SCRIPT = "if(true){print(\"Hello,World!\");}";
        [Benchmark]
        public void ConvertToScript()
        {
            Utils.ConvertToScript(SCRIPT, out _, out _, out _);
        }

    }
}
