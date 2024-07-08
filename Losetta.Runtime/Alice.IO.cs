using AliceScript.Binding;
using AliceScript.Extra;
using AliceScript.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
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
#if NETCOREAPP3_0_OR_GREATER
            File.Move(from, to, overwrite);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
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
            if (fromPackage)
            {
                byte[] data = script?.Package?.GetEntryData(path);
                if (data == null)
                {
                    throw new FileNotFoundException("パッケージ内でファイルが見つかりませんでした", path);
                }
                return SafeReader.ReadAllText(data, out _, out _);
            }
            return SafeReader.ReadAllText(path, out _, out _);
        }
        public static string File_Read_Text(ParsingScript script, string path, string charCode, bool fromPackage = false)
        {
            Encoding encode = Encoding.GetEncoding(charCode); 
            if (fromPackage)
            {
                byte[] data = script?.Package?.GetEntryData(path);
                if (data == null)
                {
                    throw new FileNotFoundException("パッケージ内でファイルが見つかりませんでした", path);
                }
                return encode.GetString(data);
            }
            return File.ReadAllText(path, encode);
        }
        public static string File_Read_Text(ParsingScript script, string path, int codePage, bool fromPackage = false)
        {
            Encoding encode = Encoding.GetEncoding(codePage); 
            if (fromPackage)
            {
                byte[] data = script?.Package?.GetEntryData(path);
                if (data == null)
                {
                    throw new FileNotFoundException("パッケージ内でファイルが見つかりませんでした", path);
                }
                return encode.GetString(data);
            }
            return File.ReadAllText(path, encode);
        }
        public static string File_Read_CharCode(ParsingScript script, string path, bool fromPackage = false)
        {
            string charCode;
            if (fromPackage)
            {
                byte[] data = script?.Package?.GetEntryData(path);
                if (data == null)
                {
                    throw new FileNotFoundException("パッケージ内でファイルが見つかりませんでした", path);
                }
                SafeReader.ReadAllText(data, out charCode, out _);
            }
            else
            {
                SafeReader.ReadAllText(path, out charCode, out _);
            }
            return charCode;
        }
        public static int File_Read_CodePage(ParsingScript script, string path, bool fromPackage = false)
        {
            int codePage;
            if (fromPackage)
            {
                byte[] data = script?.Package?.GetEntryData(path);
                if (data == null)
                {
                    throw new FileNotFoundException("パッケージ内でファイルが見つかりませんでした", path);
                }
                SafeReader.ReadAllText(data, out _, out codePage);
            }
            else
            {
                SafeReader.ReadAllText(path, out _, out codePage);
            }
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
        public static void File_Write_Text(string path, string text, int codePage)
        {
            File.WriteAllText(path, text, Encoding.GetEncoding(codePage));
        }
        public static void File_Append_Text(string path, string text)
        {
            File.AppendAllText(path, text);
        }
        public static void File_Append_Text(string path, string text, string charCode)
        {
            File.AppendAllText(path, text, Encoding.GetEncoding(charCode));
        }
        public static void File_Append_Text(string path, string text, int codePage)
        {
            File.AppendAllText(path, text, Encoding.GetEncoding(codePage));
        }
        public static void File_Write_Encrypt(string path, byte[] data, string password, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (FileStream outfs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                using (Aes aes = Aes.Create())
                {
                    int keyBytes = keySize / 8;
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = keySize;                // KeySize = keySize / 8 bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.ISO10126;    // Padding mode is "ISO10126".

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, 16, iterations, useSHA512 ? HashAlgorithmName.SHA512 : HashAlgorithmName.SHA256);
                    byte[] salt = new byte[16]; // Rfc2898DeriveBytesが内部生成したなソルトを取得
                    salt = deriveBytes.Salt;
                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(keyBytes);

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
                            }
                        }

                    }

                }
            }
            return;
        }
        public static byte[] File_Read_Decrypt(ParsingScript script, string path, string password, bool fromPackage = false, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (MemoryStream outfs = new MemoryStream())
            {
                Stream fs = fromPackage && script.Package is not null && script.Package.ExistsEntry(path)
                    ? new MemoryStream(script.Package.GetEntryData(path))
                    : new FileStream(path, FileMode.Open, FileAccess.Read);
                using (fs)
                {
                    using (Aes aes = Aes.Create())
                    {
                        int keyBytes = keySize / 8;
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = keySize;                // KeySize = keySize / 8 bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.ISO10126;    // Padding mode is "ISO10126".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, useSHA512 ? HashAlgorithmName.SHA512 : HashAlgorithmName.SHA256);
                        byte[] bufferKey = deriveBytes.GetBytes(keyBytes);    // 16バイトのsaltを切り出してパスワードに変換
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
                return outfs.ToArray();
            }
        }
        public static void File_Encrypt(string path, string outpath, string password)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (FileStream outfs = new FileStream(outpath, FileMode.Create, FileAccess.Write))
            {
                using (Aes aes = Aes.Create())
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
                    using (Aes aes = Aes.Create())
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
        public static string Directory_Current()
        {
            return Directory.GetCurrentDirectory();
        }
        public static string Directory_Current(string path)
        {
            Directory.SetCurrentDirectory(path);
            return Directory.GetCurrentDirectory();
        }
        public static void Directory_Create_SymbolicLink(string path, string pathToTarget)
        {
#if NET6_0
            Directory.CreateSymbolicLink(path, pathToTarget);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
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
            return path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith(Path.AltDirectorySeparatorChar);
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
#if NETCOREAPP2_0_OR_GREATER
            return Path.GetRelativePath(to, path);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
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
#if NETCOREAPP2_1_OR_GREATER
            return Path.IsPathFullyQualified(path);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static bool Path_IsPathRooted(string path)
        {
            return Path.IsPathRooted(path);
        }
        public static string Path_TrimEndingDirectorySeparator(string path)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Path.TrimEndingDirectorySeparator(path);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
        }
        public static string Path_Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }
        public static string Path_Join(params string[] paths)
        {
#if NETCOREAPP3_0_OR_GREATER
            return Path.Join(paths);
#else
                throw new ScriptException("この実装では操作がサポートされていません", Exceptions.NOT_IMPLEMENTED);
#endif
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
        internal static byte[] Encrypt(byte[] data, string password, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            int len;
            byte[] buffer = new byte[4096];
            using (MemoryStream outfs = new MemoryStream())
            {
                using (Aes aes = Aes.Create())
                {
                    int keyBytes = keySize / 8;
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = keySize;                // KeySize = keySize / 8 bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.ISO10126;    // Padding mode is "ISO10126".

                    //入力されたパスワードをベースに擬似乱数を新たに生成
                    Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, 16, iterations, useSHA512 ? HashAlgorithmName.SHA512 : HashAlgorithmName.SHA256);
                    byte[] salt = new byte[16]; // Rfc2898DeriveBytesが内部生成したなソルトを取得
                    salt = deriveBytes.Salt;
                    // 生成した擬似乱数から16バイト切り出したデータをパスワードにする
                    byte[] bufferKey = deriveBytes.GetBytes(keyBytes);

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
                            }
                        }

                    }

                }
                return outfs.GetBuffer();
            }
        }

        internal static byte[] Decrypt(byte[] data, string password, int keySize = 128, int iterations = 1024, bool useSHA512 = false)
        {
            int len;
            byte[] buffer = new byte[4096];
            using (MemoryStream outfs = new MemoryStream())
            {
                using (MemoryStream fs = new MemoryStream(data))
                {
                    using (Aes aes = Aes.Create())
                    {
                        int keyBytes = keySize / 8;
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = keySize;                // KeySize = keySize / 8 bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.ISO10126;    // Padding mode is "ISO10126".

                        // salt
                        byte[] salt = new byte[16];
                        fs.Read(salt, 0, 16);

                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;

                        // ivをsaltにしてパスワードを擬似乱数に変換
                        Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, useSHA512 ? HashAlgorithmName.SHA512 : HashAlgorithmName.SHA256);
                        byte[] bufferKey = deriveBytes.GetBytes(keyBytes);    // 16バイトのsaltを切り出してパスワードに変換
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


}
