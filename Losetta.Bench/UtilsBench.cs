using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AliceScript;
using BenchmarkDotNet.Attributes;

namespace Losetta.Bench
{
    public class UtilsBench
    {
        const string SCRIPT = "if(true){print(\"Hello,World!\");}";
        [Benchmark]
        public void ConvertToScript()
        {
            Utils.ConvertToScript(SCRIPT,out _,out _);
        }

    }
}
