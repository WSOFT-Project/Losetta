using AliceScript.Parsing;

namespace AliceScript
{
    /// <summary>
    /// AliceScript
    /// </summary>
    public static class Alice
    {
        /// <summary>
        /// AliceScriptのコードを実行します
        /// </summary>
        /// <param name="code">実行したいスクリプト</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Variable Execute(string code, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.Process(code, filename, mainFile);
        }
        /// <summary>
        /// このインタプリタで読み込み可能なファイルを読み込みます
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Variable ExecuteFile(string filename, bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFile(filename, mainFile);
        }
        /// <summary>
        /// このインタプリタで読み込み可能なデータを読み込みます
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Variable ExecuteData(byte[] data, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.ProcessData(data, filename, mainFile);
        }
        /// <summary>
        /// AliceScriptのコードを実行します
        /// </summary>
        /// <param name="code">実行したいスクリプト</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static T Execute<T>(string code, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.Process(code, filename, mainFile).ConvertTo<T>();
        }
        /// <summary>
        /// このインタプリタで読み込み可能なファイルを読み込みます
        /// </summary>
        /// <param name="filename">ファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static T ExecuteFile<T>(string filename, bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFile(filename, mainFile).ConvertTo<T>();
        }
        /// <summary>
        /// このインタプリタで読み込み可能なデータを読み込みます
        /// </summary>
        /// <param name="data">データ</param>
        /// <param name="filename">ファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static T ExecuteData<T>(byte[] data, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.ProcessData(data, filename, mainFile).ConvertTo<T>();
        }
        /// <summary>
        /// AliceScriptのコードを非同期で実行します
        /// </summary>
        /// <param name="code">実行したいスクリプト</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Task<Variable> ExecuteAsync(string code, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.ProcessAsync(code, filename, mainFile);
        }
        /// <summary>
        /// AliceScriptファイルを非同期で実行します
        /// </summary>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>スクリプトから返される戻り値</returns>
        public static Task<Variable> ExecuteFileAsync(string filename, bool mainFile = false)
        {
            return Interpreter.Instance.ProcessFileAsync(filename, mainFile);
        }
        /// <summary>
        /// AliceScriptのコードからスクリプトを生成します
        /// </summary>
        /// <param name="code">生成元のコード</param>
        /// <param name="filename">スクリプトのファイル名</param>
        /// <param name="mainFile">メインファイルとして処理するか否か</param>
        /// <returns>生成されたスクリプト</returns>
        public static ParsingScript GetScript(string code, string filename = "", bool mainFile = false)
        {
            return Interpreter.Instance.GetScript(code, filename, mainFile);
        }
        /// <summary>
        /// クラス内の静的メソッドをAliceScriptで使用できるよう登録します
        /// </summary>
        /// <typeparam name="T">登録するクラス</typeparam>
        /// <param name="name">登録する名前空間の名前。nullにすると自動選択されます。</param>
        public static void RegisterFunctions<T>(string name = null)
        {
            NameSpaces.NameSpaceManager.Add(typeof(T), name);
        }
        /// <summary>
        /// プログラムが終了を求めているときに発生するイベントです
        /// </summary>
        public static event Exiting Exiting;
        /// <summary>
        /// Exitingイベントを発生させます
        /// </summary>
        /// <param name="exitcode">終了の理由を表す終了コード</param>
        public static void OnExiting(int exitcode = 0)
        {
            ExitingEventArgs e = new ExitingEventArgs
            {
                Cancel = false,
                ExitCode = exitcode
            };
            Exiting?.Invoke(null, e);
            if (e.Cancel)
            {
                return;
            }
            else
            {
                Environment.Exit(e.ExitCode);
            }
        }
        /// <summary>
        /// AliceScriptのバージョン
        /// </summary>
        public static Version Version => Constants.VERSION;
        /// <summary>
        /// Losettaのバージョン
        /// </summary>
        public static Version ImplementationVersion
        {
            get
            {
                System.Reflection.Assembly asm =
                    System.Reflection.Assembly.GetExecutingAssembly();
                //バージョンの取得
                return asm.GetName().Version;
            }
        }
        /// <summary>
        /// AliceScriptの実装の名前
        /// </summary>
        public static string ImplementationName => Interpreter.Instance.Name;
        /// <summary>
        /// AliceScriptの実装があるディレクトリの場所
        /// </summary>
        public static string ImplementationLocation => AppContext.BaseDirectory;
        /// <summary>
        /// このAliceScriptが実行されているアプリケーションの名前
        /// </summary>
        public static string AppName
        {
            get; set;
        }
    }
    /// <summary>
    /// スクリプトによってプログラムが終了されようとしているときに呼び出されるデリゲート
    /// </summary>
    /// <param name="sender">オブジェクト</param>
    /// <param name="e">プログラムが終了する理由</param>
    public delegate void Exiting(object sender, ExitingEventArgs e);
    public class ExitingEventArgs : EventArgs
    {
        /// <summary>
        /// キャンセルする場合は、True
        /// </summary>
        public bool Cancel { get; set; }
        /// <summary>
        /// 終了コードを表します
        /// </summary>
        public int ExitCode { get; set; }
    }

}
