using AliceScript;
using System.IO.Compression;
using System.Text;

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


            ThrowErrorManerger.ThrowError += Shell.ThrowErrorManerger_ThrowError;
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
                ThrowErrorManerger.ThrowError -= Shell.ThrowErrorManerger_ThrowError;
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

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "version"), Properties.Resources.version, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "install"), Properties.Resources.install, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "shell"), Properties.Resources.shell, Encoding.UTF8);

                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, ".alice", "update"), Properties.Resources.update, Encoding.UTF8);
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
