namespace AliceScript.Interop
{
    /// <summary>
    /// AliceScriptのプラグインが継承するインターフェイス
    /// </summary>
    public interface ILibrary
    {
        /// <summary>
        /// プラグインの名前
        /// </summary>
        string Name { get; }
        /// <summary>
        /// プラグインのエントリポイント
        /// </summary>
        void Main();
    }

    /// <summary>
    /// ネイティブプラグインの基礎です。このクラスを継承してネイティブプラグインを作成します。
    /// </summary>
    public class LibraryBase : ILibrary
    {
        /// <summary>
        /// プラグインのエントリポイント
        /// </summary>
        public virtual void Main()
        {
        }

        /// <summary>
        /// プラグインの名前
        /// </summary>
        public string Name { get; set; }

    }
}
