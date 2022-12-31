﻿using AliceScript;
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

            if (pa.Flags.Contains("v") || pa.Flags.Contains("version"))
            {
                //バージョン表示
                Console.WriteLine(VersionText);
                return;
            }
            if (pa.Flags.Contains("logo"))
            {
                Console.WriteLine(@"
                 ...(J&aa&JJ.-..
            .._(J9=:~~~~~((JJdMaJJ--.
         .((g#9=7<<?7TMWm-~~~~~~?9e7Ya,.
       .JT=(9~~~~~~(7<~~~?Ya-~~~~~(W,~?9e-
     .J5~~J:~~~~~(C~~~~~~~~~dm-J++JJdp~~(U,.
    (#<~((gVY""77vTTT6+,~((<<~_4/~~~~~d2Tu,?m_
   (@_(Y5F~~~~~(:~~~~~_?6,~~~~~w/~~~~~H~~(Tdp~.
  ~dj8<~(<~~~~~>~~~~~~~~~~7,~:~_4(_~~~Jc~~~_W,~.
 ~(#:~~~J~~(JJTTC<<<<?7C+.~(G_~~Jc~_?1(l~~~~J#p~
.(Mt~~~~(gY:~~_~~~((gMMMMKMNgG_~($~~~~J?3,~~(FJ[_
~djr~~(J51~~~~~(v:jMBBw9UjUJMWp~(l~~~(D~~(O-(t~N_
(F~h~(@~~_>~~(3~~(@#4J<~~~?O(BM[J~~:(Z~~~~~z#~~Jr.
Jt~/a@~~~~~1J~~~Jd#4fc~~~~~?JmMN3~~(f<~~:~_dv/~Jt_
Jr~(Mx~~:~(f~~~(tJMdvZ-_~_(1kWdt~(J=~~1~~(d<~h~d<`
(E~($(G-~~d~~~~J~~dNN7kTQ+aJdM5Jv:~~~~~GJ5~~~J(@~
_J2g>~~?zJF~~~~J~~(6TMNgNNN#57<~~_~~_(J4_~~~~J#~_
 ~dd[~~~~J$1--~J/~~_G_~?71JJJ((JJJvT>~~(l~~:(M<~`
  ~?R_~~~Jr~~~~~b~~~~?&_~~~~~~~~~<~~~~~(>~(d4F~`
   ~dfG-~(E~~~~~/p~~~~~?4J<~~~:~(<~~~:~JJT=(K~`
    ~Ux(TuJp~:~~~/h__((<~~?TTuJJaJJJwT4t~~(B~`
     _?m-~~de777777TJ~~~~~~~~(J~~~~~~(5~(J5_
       _Vm,~?h-~~~~~_TQJ~~:(J:~~~~~(J3(Y3_
         `?ThJdm,~~~~~_(dBN&JJJJJJgH9=_`
            `_~?7TMHVC<~~~~~_(Jd9:~_`
                ``__~?T""""""""TC~_``
");
                Console.WriteLine(VersionText);
                return;
            }
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

            ThrowErrorManerger.ThrowError += ThrowErrorManerger_ThrowError;
            Interpreter.Instance.OnOutput += Instance_OnOutput;
            if (!pa.Flags.Contains("noconfig"))
            {
                string filename = Path.Combine(AppContext.BaseDirectory, "config.alice");
                if (pa.Values.ContainsKey("config"))
                {
                    filename = pa.Values["config"];
                }
                if (File.Exists(filename))
                {
                    Alice.ExecuteFile(filename);
                }
            }
            if (pa.Flags.Contains("s"))
            {
                Alice.Execute(pa.Script);
            }
            else
                if (pa.Flags.Contains("r") || pa.Flags.Contains("run"))
            {
                //実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                ThrowErrorManerger.HandleError = true;
                foreach (string fn in pa.Files)
                {
                    Alice.ExecuteFile(fn, mainfile);
                }
            }
            else if (pa.Flags.Contains("p") || pa.Flags.Contains("pkg") || pa.Flags.Contains("package"))
            {
                //パッケージ実行モード
                bool mainfile = pa.Flags.Contains("mainfile");
                ThrowErrorManerger.HandleError = true;
                foreach (string fn in pa.Files)
                {
                    AlicePackage.Load(Path.GetFileName(fn));
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
                    Alice.ExecuteFile(fn, true);
                }
            }
            else
            {
                ThrowErrorManerger.ThrowError -= ThrowErrorManerger_ThrowError;
                Interpreter.Instance.OnOutput -= Instance_OnOutput;
                Shell.Do(args);
            }
        }
        internal static string VersionText => "AliceScript バージョン " + Alice.Version.ToString() + " (" + Alice.ImplementationName + " v" + Alice.ImplementationVersion.ToString() + " on " + Environment.OSVersion.Platform + ")";
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
