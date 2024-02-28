using System.Collections.Generic;

namespace AliceScript.Packaging
{
    /// <summary>
    /// AlicePackageの設定
    /// </summary>
    public class PackageManifest
    {
        /// <summary>
        /// パッケージの名前
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// パッケージのバージョン
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// パッケージの説明
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// パッケージの発行者
        /// </summary>
        public string Publisher { get; set; }
        /// <summary>
        /// ターゲット
        /// </summary>
        public List<string> Target { get; set; }
        /// <summary>
        /// インラインスクリプトの場合。それ以外の場合はnull。
        /// </summary>
        public string Script { get; set; }
        /// <summary>
        /// スクリプトファイルのパス
        /// </summary>
        public string ScriptPath { get; set; }
        /// <summary>
        /// インラインスクリプトを使用するかどうか
        /// </summary>
        public bool UseInlineScript { get; set; }
        /// <summary>
        /// ターゲットにするアプリケーション
        /// </summary>
        public List<string> TargetApp { get; set; }
    }
}
