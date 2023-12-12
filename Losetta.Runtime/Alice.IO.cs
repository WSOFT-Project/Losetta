using AliceScript.Binding;
using AliceScript.Extra;
using AliceScript.Functions;
using AliceScript.Parsing;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_IO
    {
        public static void Init()
        {
            Alice.RegisterFunctions<FileFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.IO")]
    internal sealed class FileFunctions
    {
        #region ファイル操作
        public static bool File_Exists(string path)
        {
            return File.Exists(path);
        }
        public static void File_Move(string from, string to)
        {
            File.Move(from, to);
        }
        public static void File_Move(string from, string to, bool overwrite)
        {
            File.Move(from, to, overwrite);
        }
        public static void File_Copy(string from, string to)
        {
            File.Copy(from, to);
        }
        public static void File_Copy(string from, string to, bool overwrite)
        {
            File.Copy(from, to, overwrite);
        }
        public static void File_Replace(string from, string to, string backupPath)
        {
            File.Replace(from, to, backupPath);
        }
        public static void File_Replace(string from, string to, string backupPath, bool ignoreMetadataErrors)
        {
            File.Replace(from, to, backupPath, ignoreMetadataErrors);
        }
        public static void File_Delete(string path)
        {
            File.Delete(path);
        }
        public static string File_Read_Text(ParsingScript script, string path, bool fromPackage = false)
        {
            var data = Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
            return SafeReader.ReadAllText(data, out _, out _);
        }
        public static string File_Read_Text(ParsingScript script, string path, string charcode, bool fromPackage = false)
        {
            var data = Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
            Encoding encode = Encoding.GetEncoding(charcode);
            return encode.GetString(data);
        }
        public static string File_Read_Text(ParsingScript script, string path, int charcode, bool fromPackage = false)
        {
            var data = Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
            Encoding encode = Encoding.GetEncoding(charcode);
            return encode.GetString(data);
        }
        public static string File_Read_CharCode(ParsingScript script, string path, bool fromPackage = false)
        {
            var data = Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
            SafeReader.ReadAllText(data, out string charcode,out _);
            return charcode;
        }
        public static int File_Read_CodePage(ParsingScript script, string path, bool fromPackage = false)
        {
            var data = Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
            SafeReader.ReadAllText(data, out _, out int codePage);
            return codePage;
        }
        public static byte[] File_Read_Data(ParsingScript script, string path, bool fromPackage = false)
        {
            return Utils.GetFileFromPackageOrLocal(path, fromPackage, script);
        }
        public static void File_Write_Data(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }
        public static void File_Write_Text(string path, string text)
        {
            File.WriteAllText(path, text);
        }
        public static void File_Write_Text(string path, string text, string charCode)
        {
            File.WriteAllText(path, text, Encoding.GetEncoding(charCode));
        }
        public static void File_Append_Text(string path, string text)
        {
            File.AppendAllText(path, text);
        }
        public static void File_Append_Text(string path, string text, string charCode)
        {
            File.AppendAllText(path, text, Encoding.GetEncoding(charCode));
        }
        public static void File_Encrypt(string path, string outpath, string password)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (FileStream outfs = new FileStream(outpath, FileMode.Create, FileAccess.Write))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, 16);
                    byte[] salt = new byte[16]; // Rfc2898DeriveBytesが内部生成したなソルトを取得
                    salt = deriveBytes.Salt;
                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(16);

                    aes.Key = bufferKey;
                    // IV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(salt, 0, 16);     // salt をファイル先頭に埋め込む
                        outfs.Write(aes.IV, 0, 16); // 次にIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            {
                                while ((len = fs.Read(buffer, 0, 4096)) > 0)
                                {
                                    ds.Write(buffer, 0, len);
                                }
                            }
                        }

                    }

                }
            }
            return;
        }
        public static void File_Decrypt(string path, string outpath, string password)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (FileStream outfs = new FileStream(outpath, FileMode.Create, FileAccess.Write))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
            }
            return;
        }
        #endregion
        #region ディレクトリ操作
        public static void Directory_Create(string path)
        {
            Directory.CreateDirectory(path);
        }
        public static void Directory_Create_SymbolicLink(string path, string pathToTarget)
        {
            Directory.CreateSymbolicLink(path, pathToTarget);
        }
        public static void Directory_Delete(string path)
        {
            Directory.Delete(path);
        }
        public static void Directory_Delete(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }
        public static bool Directory_Exists(string path)
        {
            return Directory.Exists(path);
        }
        public static void Directory_Move(string from, string to)
        {
            Directory.Move(from, to);
        }
        public static string[] Directory_GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }
        public static string[] Directory_GetDirectories(string path, string pattern)
        {
            return Directory.GetDirectories(path, pattern);
        }
        public static string[] Directory_GetDirectories(string path, string pattern, bool searchSubDir)
        {
            return Directory.GetDirectories(path, pattern, searchSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        public static string[] Directory_GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }
        public static string[] Directory_GetFiles(string path, string pattern)
        {
            return Directory.GetFiles(path, pattern);
        }
        public static string[] Directory_GetFiles(string path, string pattern, bool searchSubDir)
        {
            return Directory.GetFiles(path, pattern, searchSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }
        public static string Directory_GetRoot(string path)
        {
            return Directory.GetDirectoryRoot(path);
        }
        public static string[] Directory_GetLogicalDrives()
        {
            return Directory.GetLogicalDrives();
        }
        public static void Directory_Copy(string sourceDirName, string destDirName, bool copySubDirs = true)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(sourceDirName);
            }
            if (sourceDirName.Equals(destDirName, StringComparison.OrdinalIgnoreCase))
            {
                string addPath = Path.GetFileName(sourceDirName);
                destDirName = Path.Combine(destDirName, addPath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                File.Copy(file.FullName, tempPath, true);
            }
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    Directory_Copy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        public static string[] Directory_Grep(string path, string pattern, string filePattern, bool ignoreCase = false)
        {
            Regex textPattern = new Regex(pattern, ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            List<string> result = new List<string>();

            foreach (string file in Directory.GetFiles(path, filePattern))
            {
                try
                {
                    string str = File.ReadAllText(file);
                    if (textPattern.IsMatch(str))
                    {
                        result.Add(str);
                    }
                }
                catch { }
            }

            return result.ToArray();
        }
        #endregion
        #region パス関連
        public static string Path_ChageExtension(string filename, string extension)
        {
            return Path.ChangeExtension(filename, extension);
        }
        public static bool Path_EndsInDirectorySeparator(string path)
        {
            return Path.EndsInDirectorySeparator(path);
        }
        public static string Path_Get_DirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }
        public static string Path_Get_Extension(string path)
        {
            return Path.GetExtension(path);
        }
        public static string Path_Get_FileName(string path)
        {
            return Path.GetFileName(path);
        }
        public static string Path_Get_FileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }
        public static string Path_Get_FullPath(string path)
        {
            return Path.GetFullPath(path);
        }
        public static string Path_Get_RelativePath(string to, string path)
        {
            return Path.GetRelativePath(to, path);
        }
        public static string Path_Get_PathRoot(string path)
        {
            return Path.GetPathRoot(path);
        }
        public static string Path_Get_RandomFileName()
        {
            return Path.GetRandomFileName();
        }
        public static string Path_Get_TempFileName()
        {
            return Path.GetTempFileName();
        }
        public static string Path_Get_TempPath()
        {
            return Path.GetTempPath();
        }
        public static bool Path_Has_Extension(string path)
        {
            return Path.HasExtension(path);
        }
        public static bool Path_IsPathFullyQualified(string path)
        {
            return Path.IsPathFullyQualified(path);
        }
        public static bool Path_IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }
        public static string Path_TrimEndingDirectorySeparator(string path)
        {
            return Path.TrimEndingDirectorySeparator(path);
        }
        public static string Path_Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }
        public static string Path_Join(params string[] paths)
        {
            return Path.Join(paths);
        }
        #endregion
        #region ZIPファイル操作
        public static void Zip_CreateFromDirectory(string path, string to)
        {
            ZipFile.CreateFromDirectory(path, to);
        }
        public static void Zip_ExtractToDirectory(string path, string to)
        {
            ZipFile.ExtractToDirectory(path, to);
        }
        public static void Zip_Append_File(string path, string source, string filename)
        {
            using (ZipArchive a = ZipFile.Open(path, ZipArchiveMode.Update))
            {
                ZipArchiveEntry e = a.CreateEntryFromFile(source, filename);
            }
        }
        #endregion
    }

    internal sealed class FileEncrypter
    {
        internal static byte[] Encrypt(byte[] data, string Password)
        {
            int len;
            byte[] buffer = new byte[4096];
            using (MemoryStream outfs = new MemoryStream())
            {
                using (var aes = Aes.Create())
                {
                    aes.BlockSize = 128;
                    aes.KeySize = 128;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, 16);
                    byte[] salt = new byte[16];
                    salt = deriveBytes.Salt;
                    byte[] bufferKey = deriveBytes.GetBytes(16);
                    aes.Key = bufferKey;
                    // IV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(salt, 0, 16);     // salt をファイル先頭に埋め込む
                        outfs.Write(aes.IV, 0, 16); // 次にIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            using (MemoryStream fs = new MemoryStream(data))
                            {
                                while ((len = fs.Read(buffer, 0, 4096)) > 0)
                                {
                                    ds.Write(buffer, 0, len);
                                }
                                return fs.GetBuffer();
                            }
                        }
                    }
                }
            }
        }

        internal static byte[] Decrypt(byte[] data, string Password)
        {
            int len;
            byte[] buffer = new byte[4096];
            using (MemoryStream outfs = new MemoryStream())
            {
                using (MemoryStream fs = new MemoryStream())
                {
                    using (var aes = Aes.Create())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(Password, salt);
                        byte[] bufferKey = deriveBytes.GetBytes(16);    // 16バイトのsaltを切り出してパスワードに変換
                        aes.Key = bufferKey;

                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
                return outfs.GetBuffer();
            }
        }
    }

    internal sealed class directory_currentdirectoryFunc : FunctionBase
    {
        public directory_currentdirectoryFunc()
        {
            Name = "directory_current";
            MinimumArgCounts = 0;
            Run += File_exists_Run;
        }

        private void File_exists_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count > 0)
            {
                Directory.SetCurrentDirectory(e.Args[0].AsString());
            }
            e.Return = new Variable(e.Script.PWD);
        }
    }


}
