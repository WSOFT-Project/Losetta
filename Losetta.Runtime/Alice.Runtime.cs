using AliceScript.Interop;
using AliceScript.NameSpaces;

namespace AliceScript
{
    public class Runtime : ILibrary
    {
        public string Name => "Alice.Runtime";

        public void Main()
        {
            Init();
        }
        /// <summary>
        /// Alice.Environmentで使用するコマンドライン引数
        /// </summary>
        public static List<string> Args { get; set; }
        /// <summary>
        /// Alice.Runtimeで使用できるすべてのAPIを読み込みます
        /// </summary>
        public static void Init()
        {
            InitBasicAPI();
            Alice_Console.Init();
            Alice_Regex.Init();
            Alice_Diagnostics.Init();
            Alice_IO.Init();
            Alice_Math.Init();
            Alice_Net.Init();
            Alice_Random.Init();
            Alice_Threading.Init();
            Alice_Security.Init();
            Alice_Packaging.Init();
            Alice_Environment.Init();
            Alice_Reflection.Init();
            Alice_Legacy.Init();
        }
        /// <summary>
        /// 基本的なAPIのみを読み込みます
        /// </summary>
        public static void InitBasicAPI()
        {
            Alice_Initer.Init();
        }
    }
}
