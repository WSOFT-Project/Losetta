﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace alice.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("alice.Properties.Resources", typeof(Resources).Assembly);
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
        ///print(); に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string shell {
            get {
                return ResourceManager.GetString("shell", resourceCulture);
            }
        }
        
        /// <summary>
        ///   // 実行ファイルのファイル名
        ///// ファイル名が異なる場合はこの部分を編集してください。{0}には現在の実行ファイルのディレクトリが代入されます。また、Windowsの場合でも末尾の.exeは不要です
        ///var target_filename = &quot;{0}alice&quot;;
        ///
        ///using Alice.IO;
        ///using Alice.Net;
        ///using Alice.Console;
        ///using Alice.Environment;
        ///using Alice.Diagnostics;
        ///
        ///const version_get_api = &quot;https://api.wsoft.ws/download/detail?id={0}&amp;feature=version&quot;;
        ///const download_url = &quot;https://download.wsoft.ws/{0}/Download&quot;;
        ///
        ///var download_id=&quot;&quot;;
        ///var isWin=false;
        ///
        ///var platform= env_impl_target();
        ///var arch= env_impl_architecture [残りの文字列は切り詰められました]&quot;; に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string update {
            get {
                return ResourceManager.GetString("update", resourceCulture);
            }
        }
        
        /// <summary>
        ///   using Alice.Environment;
        ///
        ///print($&quot;{env_lang_name()} Version {env_lang_version()} ({env_impl_name()} v{env_impl_version()} on {env_impl_target()}-{env_impl_architecture()}&quot;); に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string version {
            get {
                return ResourceManager.GetString("version", resourceCulture);
            }
        }
    }
}
