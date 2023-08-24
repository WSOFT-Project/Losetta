using AliceScript.Interop;

namespace AliceScript.NameSpaces
{
    public  class Alice_Runtime : ILibrary
    {
        public string Name => "Alice.Runtime";

        public void Main()
        {
            Alice_Initer.Init();
            Alice_Console.Init();
            Alice_Regex_Initer.Init();
            Alice_Diagnostics_Initer.Init();
            Alice_IO_Intiter.Init();
            Alice_Math_Initer.Init();
            Alice_Net_Initer.Init();
            Alice_Random_Initer.Init();
            Alice_Threading_Initer.Init();
            Alice_Security_Initer.Init();
            Alice_Packaging_Initer.Init();
            Alice_Environment_Initer.Init();
            Alice_Legacy_Initer.Init();
        }
    }
}
