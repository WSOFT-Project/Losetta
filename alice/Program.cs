using System.IO.Compression;
using System.Text;

namespace AliceScript.CLI
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
            if (pa.Values.TryGetValue("runtime", out string v) && v.ToLower() == "disable")
            {
                //最小モードで初期化
                Runtime.InitBasicAPI();
            }
            else
            {
                //ランタイムを初期化
                Runtime.Init();
            }
            //ShellFunctions登録
            ShellFunctions.Init();


            ThrowErrorManager.ThrowError += Shell.ThrowErrorManager_ThrowError;
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

            if (pa.Flags.Contains("e"))
            {
                //単一行評価モード
                Console.Write(Alice.Execute(pa.Script));
                return;
            }
            else
                if (pa.Flags.Contains("r") || pa.Flags.Contains("run"))
            {
                //実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                foreach (string fn in pa.Files)
                {
                    Alice.ExecuteFile(GetScriptPath(fn), mainfile);
                }
            }
            else if (pa.Flags.Contains("p") || pa.Flags.Contains("pkg") || pa.Flags.Contains("package"))
            {
                //パッケージ実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                foreach (string fn in pa.Files)
                {
                    AlicePackage.Load(GetScriptPath(fn));
                }
            }
            else if (pa.Flags.Contains("b") || pa.Flags.Contains("build"))
            {
                //パッケージ生成モード
                if (pa.Values.TryGetValue("out", out string outfile))
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
                ThrowErrorManager.ThrowError -= Shell.ThrowErrorManager_ThrowError;
                Interpreter.Instance.OnOutput -= Instance_OnOutput;
                Shell.Do();
            }
        }
        /// <summary>
        /// デバッグモードかどうかを表す値。
        /// </summary>
        internal static bool IsDebugMode { get; set; }
        private static bool allow_print = true;
        private static List<string> print_redirect_files = new List<string>();
        internal static bool allow_throw = true;
        private static List<string> throw_redirect_files = new List<string>();
        private static string ListToString(List<string> list)
        {
            var sb = new StringBuilder(Constants.START_GROUP);
            bool isFirst = true;
            foreach (string v in list)
            {
                if (isFirst)
                {
                    sb.Append($" {v}");
                    isFirst = false;
                }
                else
                {
                    sb.Append($",{v}");
                }
            }
            sb.Append(Constants.END_GROUP);
            return sb.ToString();
        }
        internal static string GetScriptPath(string path)
        {
            if (Path.IsPathRooted(path) || File.Exists(path))
            {
                return path;
            }
            string fpath = Path.Combine(AppContext.BaseDirectory, ".alice", path);
            return File.Exists(fpath) ? fpath : path;
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

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "version"), Properties.Resources.version, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "install"), Properties.Resources.install, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "shell"), Properties.Resources.shell, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "update"), Properties.Resources.update, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "path"), Properties.Resources.path, Encoding.UTF8);
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

    }
}
