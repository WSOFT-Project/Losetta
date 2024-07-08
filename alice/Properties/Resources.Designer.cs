﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace AliceScript.CLI.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AliceScript.CLI.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   print(&quot;AliceScript言語で記述されたプログラムを実行します。&quot;);
        ///print();
        ///print(&quot;使用法：alice [ファイル名] [オプション] [-e 式] [--args パラメータ]&quot;);
        ///print();
        ///print(&quot;  [ファイル名]           ファイルを実行&quot;);
        ///print();
        ///print(&quot;  -d                     デバッグモードを有効化&quot;);
        ///print(&quot;  -print=off             標準出力を無効化&quot;);
        ///print(&quot;  -throw=off             例外のハンドルを無効化&quot;);
        ///print(&quot;  -runtime=nano          ランタイムを最小モードで初期化&quot;);
        ///print(&quot;  -run                   ファイルをスクリプトとして実行&quot;);
        ///print(&quot;  -run -mainfile         ファイルをスクリプトとしてメインファイルで実行&quot;);
        ///print();
        ///print(&quot;  -e,-execute,-eval [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string help {
            get {
                return ResourceManager.GetString("help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   using Alice.IO;
        ///using Alice.Net;
        ///using Alice.Console;
        ///using Alice.Environment;
        ///
        ///var args=env_commandLineArgs();
        ///if((args.Length)&gt;0){
        ///var url=args[0];
        ///var fn=env_impl_location();
        ///if((args.Length)&gt;2){
        ///fn=args[2];
        ///}
        ///var f=path_get_filename(url);
        ///if((args.Length)&gt;1){
        ///f=args[1];
        ///}
        ///fn=(path_combine(fn,&quot;.alice&quot;));
        ///fn=(path_combine(fn,f));
        ///if(file_exists(fn)){
        ///console_write(&quot;{0}には、既にファイルが存在しています。これを上書きしてダウンロードしますか？(Y/N)&gt;&gt;&quot;,fn);
        ///if(Console_ReadKey()!=&quot;y&quot;){
        ///print();
        ///print(&quot;要求はユーザーによってキャンセルされました。 [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string install {
            get {
                return ResourceManager.GetString("install", resourceCulture);
            }
        }
        
        /// <summary>
        ///   using Alice.Environment;
        ///using Alice.Console;
        ///using Alice.IO;
        ///
        ///print(&quot;AliceScript パス設定ツール&quot;);
        ///
        ///var isMachine = env_commandLineArgs().Contains(&quot;machine&quot;);
        ///var isWin = env_impl_target() == &quot;Windows&quot;;
        ///var pwd=directory_current();
        ///
        ///if(!isWin)
        ///{
        ///	print(&quot;Windows以外の環境では、この機能は使用できません。&quot;);
        ///	exit;
        ///}
        ///
        ///print(&quot;現在の作業ディレクトリへのパスを通します。&quot;);
        ///if(isMachine)
        ///{
        ///	print(&quot;この操作はすべてのユーザーに適用されます。&quot;);
        ///}
        ///
        ///print(&quot;よろしければ\&quot;y\&quot;を、キャンセルする場合はそれ以外のキーを押してください。&quot;);
        ///write(&quot;Y/N&gt;&quot;);
        ///
        ///if(console_readKey() != &quot;y&quot;)
        ///{
        ///	print(&quot;キャンセルさ [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string path {
            get {
                return ResourceManager.GetString("path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   public using Alice.Shell;
        ///include(&quot;version&quot;);
        ///print();
        ///print(&quot;https://a.wsoft.ws/alice&quot;);
        ///print(); に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string shell {
            get {
                return ResourceManager.GetString("shell", resourceCulture);
            }
        }
        
        /// <summary>
        ///   using Alice.IO;
        ///using Alice.Net;
        ///using Alice.Regex;
        ///using Alice.Console;
        ///using Alice.Security;
        ///using Alice.Diagnostics;
        ///using Alice.Environment;
        ///
        ///const string update_get_api = &quot;https://api.wsoft.ws/download/detail?id={0}&quot;;
        ///const download_url = &quot;https://download.wsoft.ws/{0}/Download&quot;;
        ///
        ///// OS名とアーキテクチャ名から、該当するバイナリのダウンロードIDを取得します
        ///// os = OS名。Windows,Linux,OSXのいずれか
        ///// arch = アーキテクチャ名。x64,x86,ARM32,ARM64のいずれか
        ///// return = 該当するバイナリが見つかった場合はID、それ以外の場合は空文字列
        ///string GetDownloadId(string os, string arch) [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string update {
            get {
                return ResourceManager.GetString("update", resourceCulture);
            }
        }
        
        /// <summary>
        ///   using Alice.Environment;
        ///
        ///print($&quot;{env_lang_name()} Version {env_lang_version()} ({env_impl_name()} v{env_impl_version()} on {env_impl_target()}-{env_impl_architecture()})&quot;); に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string version {
            get {
                return ResourceManager.GetString("version", resourceCulture);
            }
        }
    }
}
