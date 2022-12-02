using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AliceScript
{
    public partial class Utils
    {
        public static string GetFileEntry(string dir, int i, string startsWith)
        {
            List<string> files = new List<string>();
            string[] patterns = { startsWith + "*" };
            GetFiles(dir, patterns, ref files, true, false);

            if (files.Count == 0)
            {
                return "";
            }
            i = i % files.Count;

            string pathname = files[i];
            if (files.Count == 1)
            {
                pathname += Directory.Exists(Path.Combine(dir, pathname)) ?
                            Path.DirectorySeparatorChar.ToString() : " ";
            }
            return pathname;
        }

        public static void GetFiles(string path, string[] patterns, ref List<string> files,
          bool addDirs = true, bool recursive = true)
        {
            SearchOption option = recursive ? SearchOption.AllDirectories :
                                              SearchOption.TopDirectoryOnly;
            if (string.IsNullOrEmpty(path))
            {
                path = Directory.GetCurrentDirectory();
            }

            List<string> dirs = patterns.SelectMany(
              i => Directory.EnumerateDirectories(path, i, option)
            ).ToList<string>();

            List<string> extraFiles = patterns.SelectMany(
              i => Directory.EnumerateFiles(path, i, option)
            ).ToList<string>();

            if (addDirs)
            {
                files.AddRange(dirs);
            }
            files.AddRange(extraFiles);

            if (!recursive)
            {
                files = files.Select(p => Path.GetFileName(p)).ToList<string>();
                files.Sort();
                return;
            }
            /*foreach (string dir in dirs) {
              GetFiles (dir, patterns, addDirs);
            }*/
        }

        public static void PrintColor(string output, ConsoleColor fgcolor)
        {
            ConsoleColor currentForeground = Console.ForegroundColor;
            Console.ForegroundColor = fgcolor;

            Interpreter.Instance.AppendOutput(output);
            //Console.Write(output);

            Console.ForegroundColor = currentForeground;
        }

        public static void GetDir(string dir = "./", bool recursive = true)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string dirPath = Path.Combine(documentsPath, dir);

            var directories = Directory.EnumerateDirectories(dirPath);
            var files = Directory.GetFiles(dirPath);
            foreach (var file in files)
            {
                Console.WriteLine("    " + file);
            }
            foreach (var directory in directories)
            {
                Console.WriteLine(directory);
                if (recursive)
                {
                    GetDir(directory, recursive);
                }
            }
        }
    }
}
