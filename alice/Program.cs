using AliceScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace alice
{
    internal class Program
    {
        /// <summary>
        /// アプリケーションのメインエントリポイントです
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            ParsedArguments pa = new ParsedArguments(args);
            AliceScript.NameSpaces.Env_CommandLineArgsFunc.Args = pa.Args;
            CreateAliceDirectory(false);
            if (pa.Values.ContainsKey("print"))
            {
                if (pa.Values["print"].ToLower() == "off")
                {
                    allow_print = false;
                }
                else
                {
                    print_redirect_files.Add(pa.Values["print"]);
                }
            }
            if (pa.Values.ContainsKey("throw"))
            {
                if (pa.Values["throw"].ToLower() == "off")
                {
                    allow_throw = false;
                }
                else
                {
                    throw_redirect_files.Add(pa.Values["throw"]);
                }
            }
            if (pa.Values.ContainsKey("runtime") && (pa.Values["runtime"].ToLower() == "disable"))
            {
                //ランタイムを初期化しない
            }
            else
            {
                new AliceScript.NameSpaces.Alice_Runtime().Main();
            }
            //ShellFunctions登録
            ShellFunctions.Init();

            ThrowErrorManerger.ThrowError += ThrowErrorManerger_ThrowError;
            Interpreter.Instance.OnOutput += Instance_OnOutput;

            string filename = Path.Combine(AppContext.BaseDirectory, ".alice", "init");
            if (pa.Values.ContainsKey("init"))
            {
                filename = pa.Values["init"];
            }
            if (File.Exists(filename))
            {
                Alice.ExecuteFile(filename);
            }

            if (pa.Flags.Contains("s"))
            {
                string f = Path.Combine(AppContext.BaseDirectory, ".alice", "shell");
                if (File.Exists(f))
                {
                    Alice.ExecuteFile(f);
                }
                Alice.Execute(pa.Script);
                return;
            }
            else
                if (pa.Flags.Contains("r") || pa.Flags.Contains("run"))
            {
                //実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                ThrowErrorManerger.HandleError = true;
                foreach (string fn in pa.Files)
                {
                    Alice.ExecuteFile(GetScriptPath(fn), mainfile);
                }
            }
            else if (pa.Flags.Contains("p") || pa.Flags.Contains("pkg") || pa.Flags.Contains("package"))
            {
                //パッケージ実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                ThrowErrorManerger.HandleError = true;
                foreach (string fn in pa.Files)
                {
                    AlicePackage.Load(GetScriptPath(fn));
                }
            }
            else if (pa.Flags.Contains("b") || pa.Flags.Contains("build"))
            {
                //パッケージ生成モード
                string outfile;
                if (pa.Values.TryGetValue("out", out outfile))
                {
                    int success = 0;
                    int error = 0;
                    int total = 1;
                    foreach (string fn in pa.Files)
                    {
                        if (BuildPackage(fn, outfile, total))
                        {
                            success++;
                        }
                        else
                        {
                            error++;
                        }
                    }
                    Console.WriteLine("ビルド: " + success + " 成功、" + error + " 失敗");
                }
                else
                {
                    Console.WriteLine("有効な出力先を指定してください");
                }
            }
            else if (pa.Files.Count > 0)
            {
                foreach (string fn in pa.Files)
                {
                    Alice.ExecuteFile(GetScriptPath(fn), true);
                }
            }
            else
            {
                ThrowErrorManerger.ThrowError -= ThrowErrorManerger_ThrowError;
                Interpreter.Instance.OnOutput -= Instance_OnOutput;
                Shell.Do();
            }
        }
        private static bool allow_print = true;
        private static List<string> print_redirect_files = new List<string>();
        private static bool allow_throw = true;
        private static List<string> throw_redirect_files = new List<string>();
        private static string ListToString(List<string> list)
        {
            string s = "{";
            bool isFirst = true;
            foreach (string v in list)
            {
                if (isFirst)
                {
                    s += " " + v;
                    isFirst = false;
                }
                else
                {
                    s += "," + v;
                }
            }
            s += " }";
            return s;
        }
        internal static string GetScriptPath(string path)
        {
            if (Path.IsPathRooted(path) || File.Exists(path))
            {
                return path;
            }
            string fpath = Path.Combine(AppContext.BaseDirectory, ".alice", path);
            if (File.Exists(fpath))
            {
                return fpath;
            }
            return path;
        }
        internal static void CreateAliceDirectory(bool force)
        {
            string path = Path.Combine(AppContext.BaseDirectory, ".alice");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                force = true;
            }
            if (force)
            {
                var directoryInfo = new DirectoryInfo(path);
                directoryInfo.Attributes |= System.IO.FileAttributes.Hidden;

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "version"), @"using Alice.Environment;

print(""AliceScript Version {0} ({1} v{2} on {3}-{4})"",env_version(),env_impl_name(),env_impl_version(),env_impl_target(),env_impl_architecture());");

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "install"), @"using Alice.IO;
using Alice.Net;
using Alice.Console;
using Alice.Environment;

var args=env_commandLineArgs();
if((args.Length)>0){
var url=args[0];
var fn=env_impl_location();
if((args.Length)>2){
fn=args[2];
}
var f=path_get_filename(url);
if((args.Length)>1){
f=args[1];
}
fn=(path_combine(fn,"".alice""));
fn=(path_combine(fn,f));
if(file_exists(fn)){
console_write(""{0}には、既にファイルが存在しています。これを上書きしてダウンロードしますか？(Y/N)>>"",fn);
if(Console_ReadKey()!=""y""){
print();
print(""要求はユーザーによってキャンセルされました。"");
exit;
}
print();
}
print(""{0} から {1}へファイルを取得しています..."",url,fn);
web_download_file(url,fn);
print(""完了。"");
}");

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "shell"), @"global using Alice.Shell;
include(""version"");
print(""Copyright (c) WSOFT. All Rights Reserved.\r\n"");");

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "update"), @"// 実行ファイルのファイル名
// ファイル名が異なる場合はこの部分を編集してください。{0}には現在の実行ファイルのディレクトリが代入されます。また、Windowsの場合でも末尾の.exeは不要です
var target_filename = ""{0}alice"";

using Alice.IO;
using Alice.Net;
using Alice.Console;
using Alice.Environment;
using Alice.Diagnostics;

const version_get_api = ""https://api.wsoft.ws/download/detail?id={0}&feature=version"";
const download_url = ""https://download.wsoft.ws/{0}/Download"";

var download_id="""";
var isWin=false;

var platform = env_impl_target();
var arch = env_impl_architecture();

if(platform==""Windows""){
isWin=true;
if(arch==""x64""){
download_id=""WS148"";
}
if(arch==""x86""){
download_id=""WS149"";
}
if(arch==""ARM32""){
download_id=""WS151"";
}
if(arch==""ARM64""){
download_id=""WS150"";
}
}

if(platform==""OSX""){
download_id=""WS147"";
}

if(platform==""Linux""){
if(arch==""x64""){
download_id=""WS144"";
}
if(arch==""ARM32""){
download_id=""WS145"";
}
if(arch==""ARM64""){
download_id=""WS146"";
}
}
if(download_id==""""){
print(""このプラットフォームでは更新はサポートされていません"");
exit;
}

if(isWin){
target_filename+="".exe"";
}
print(""環境の判定: OS:{0},アーキテクチャ:{1}"",platform,arch);
var version_url = version_get_api.format(download_id);
print(""{0} から最新バージョンを取得しています..."",version_url);
var new_version = web_download_text(version_url);
print(""最新バージョン : {0}"",new_version);
var force=env_commandLineArgs().Contains(""force"");
var check=env_commandLineArgs().Contains(""check"");
if(check)
{
     print(""この実装のバージョン : {0}"",env_impl_version());
    if(new_version != env_impl_version())
     {
      print(""AliceScriptの実装の更新があります"");
      }
     exit;
}
if(new_version != env_impl_version() || force){
print(""この実装よりも新しい実装が公開されています。"");
Console_Write(""更新を実行しますか？(Y/N)>>"");
if(Console_ReadKey()==""y""){
print();
var path=target_filename.format(env_impl_location());
var tmp =path+"".old"";
file_delete(tmp);
file_move(path,tmp);

var url = download_url.format(download_id);
print(""{0} から最新バイナリをダウンロードしています..."",url);
web_download_file(url,path);
print(""ダウンロードが完了しました。"");
print(""{0} に更新しました"",new_version);
}else{
print();
print(""更新はユーザーによって取り消されました"");
}
}");
            }
        }
        internal static bool BuildPackage(string fn, string outfilename, int num = 1)
        {
            if (File.GetAttributes(fn).HasFlag(FileAttributes.Directory))
            {
                //パスはディレクトリ
                Console.WriteLine(num + "> ビルド開始: ソース:" + fn + "(ディレクトリ) 出力先: " + outfilename);
                string manifestPath = Path.Combine(fn, Constants.PACKAGE_MANIFEST_FILENAME);
                if (!File.Exists(manifestPath))
                {
                    Console.WriteLine(num + "> エラー: パッケージマニフェストファイルが見つかりません");
                    Console.WriteLine(num + "> パッケージ \"{0}\" のビルドが終了しました --失敗", Path.GetFileName(outfilename));
                    return false;
                }
                Console.WriteLine(num + "> 検出: マニフェストファイル: " + manifestPath);
                PackageManifest manifest = AlicePackage.GetManifest(File.ReadAllText(manifestPath));
                if (manifest == null)
                {
                    Console.WriteLine(num + "> エラー: パッケージマニフェストファイルが正しい形式ではありません");
                    Console.WriteLine(num + "> パッケージ \"{0}\" のビルドが終了しました --失敗", Path.GetFileName(outfilename));
                    return false;
                }
                string srcpath = manifest.UseInlineScript ? "マニフェストに埋め込み" : manifest.ScriptPath;
                Console.WriteLine(num + "> パッケージ名: " + manifest.Name + " エントリポイント: " + srcpath);
                string target = manifest.Target == null ? "Any" : ListToString(manifest.Target);
                Console.WriteLine(num + "> ターゲット: " + target);
                string path = Path.GetTempFileName();
                File.Delete(path);
                Console.WriteLine(num + "> 圧縮: {0} -> {1}", fn, path);
                ZipFile.CreateFromDirectory(fn, path);
                Console.WriteLine(num + "> 変換: {0} -> {1}", path, outfilename);
                try
                {
                    AlicePackage.CreateEncodingPackage(path, outfilename);
                    Console.WriteLine(num + "> パッケージ {0} のビルドが完了しました --成功", manifest.Name);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(num + "> エラー: AlicePackage形式に変換できません");
                    Console.WriteLine(num + "> 詳細: " + ex.Message);
                    Console.WriteLine(num + "> パッケージ {0} のビルドが終了しました --失敗", manifest.Name);
                    return false;
                }
            }
            else
            {
                //パスはファイル
                Console.WriteLine(num + "> ビルド開始: ソース:" + fn + "(Zip書庫) 出力先: " + outfilename);
                try
                {
                    AlicePackage.CreateEncodingPackage(fn, outfilename);
                    Console.WriteLine(num + "> パッケージ {0} のビルドが完了しました --成功", Path.GetFileName(outfilename));
                    return true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine(num + "> エラー: AlicePackage形式に変換できません");
                    Console.WriteLine(num + "> 詳細: " + ex.Message);
                    Console.WriteLine(num + "> パッケージ {0} のビルドが終了しました --失敗", Path.GetFileName(outfilename));
                    return false;
                }
            }
        }
        private static void Instance_OnOutput(object sender, OutputAvailableEventArgs e)
        {
            if (allow_print)
            {
                Console.Write(e.Output);
            }
            if (print_redirect_files.Count > 0)
            {
                foreach (string fn in print_redirect_files)
                {
                    File.AppendAllText(fn, e.Output);
                }
            }
        }
        private static void ThrowErrorManerger_ThrowError(object sender, ThrowErrorEventArgs e)
        {
            if (e.Message != "")
            {
                string throwmsg = "エラー0x" + ((int)e.ErrorCode).ToString("x3") + ": ";
                if (!string.IsNullOrEmpty(e.Message))
                {
                    throwmsg += e.Message;
                }
                if (e.Script != null)
                {
                    throwmsg += " " + e.Script.OriginalLineNumber + "行 コード:" + e.Script.OriginalLine + " ファイル名:" + e.Script.Filename;
                }
                throwmsg += "\r\n";
                if (allow_throw)
                {
                    AliceScript.Utils.PrintColor(throwmsg, ConsoleColor.Red);
                    Shell.DumpLocalVariables(e.Script);
                    Shell.DumpGlobalVariables();
                }
                if (throw_redirect_files.Count > 0)
                {
                    foreach (string fn in throw_redirect_files)
                    {
                        File.AppendAllText(fn, throwmsg);
                    }

                }
            }

        }
    }
}
