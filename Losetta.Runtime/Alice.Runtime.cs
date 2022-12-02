using System;
using AliceScript;
using AliceScript.Interop;

namespace AliceScript.NameSpaces
{
    public class Alice_Runtime : ILibrary
    {
        public string Name
        {
            get
            {
                return "Alice.Runtime";
            }
        }

        public void Main()
        {
            Alice_Initer.Init();
            Alice_Console.Init();
            Alice_Interpreter_Initer.Init();
            Alice_Regex_Initer.Init();
            Alice_Diagnostics_Initer.Init();
            Alice_Drawing_Initer.Init();
            Alice_IO_Intiter.Init();
            Alice_Math_Initer.Init();
            Alice_Net_Initer.Init();
            Alice_Random_Initer.Init();
            Alice_Threading_Initer.Init();
            Alice_Security_Initer.Init();
        }
    }
}
