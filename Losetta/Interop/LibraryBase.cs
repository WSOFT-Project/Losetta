namespace AliceScript.Interop
{

    public interface ILibrary
    {
        string Name { get; }
        void Main();
    }

    /// <summary>
    /// ネイティブプラグインの基礎です。このクラスを継承してネイティブプラグインを作成します。
    /// </summary>
    public class LibraryBase : ILibrary
    {
        public virtual void Main()
        {
        }

        public string Name { get; set; }

    }
}
