using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;

namespace AliceScript
{
    public class AlicePackage
    {

        internal ZipArchive archive { get; set; }

        public PackageManifest Manifest { get; set; }

        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                ThrowErrorManerger.OnThrowError("パッケージが見つかりません", Exceptions.FILE_NOT_FOUND);
                return;
            }
            byte[] file = File.ReadAllBytes(path);
            LoadData(file,path);
        }
        public static void LoadData(byte[] data, string filename = "")
        {
            byte[] magic = data.Take(Constants.PACKAGE_MAGIC_NUMBER.Length).ToArray();
            if (magic.SequenceEqual(Constants.PACKAGE_MAGIC_NUMBER))
            {
                LoadEncodingPackage(data, filename);
            }
            else
            {
                LoadArchive(new ZipArchive(new MemoryStream(data)), filename);
            }
        }
        public static PackageManifest GetManifest(string xml)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xml))
                {
                    return null;
                }
                XMLConfig config = new XMLConfig();
                config.XMLText = xml;
                if (!config.Exists("name") && !config.Exists("script"))
                {
                    return null;
                }
                PackageManifest manifest = new PackageManifest();
                manifest.Name = config.Read("name");
                manifest.Version = config.Read("version");
                manifest.Description = config.Read("description");
                manifest.Publisher = config.Read("publisher");

                string sip = config.Read("target");
                if (!string.IsNullOrEmpty(sip) && sip.ToLower() != "any")
                {
                    manifest.Target = new List<string>(sip.Split(','));
                }
                else
                {
                    manifest.Target = null;
                }
                string script = config.Read("script");
                string path = config.ReadAttribute("script", "path");
                if (string.IsNullOrEmpty(script) && !string.IsNullOrEmpty(path))
                {
                    //リダイレクト
                    manifest.ScriptPath = path;
                    manifest.UseInlineScript = false;
                }
                else
                {
                    //インライン
                    manifest.Script = script;
                    manifest.ScriptPath = path;
                    manifest.UseInlineScript = true;
                }
                return manifest;
            }
            catch
            {
                return null;
            }
        }
        private static void LoadArchive(ZipArchive a, string filename = "")
        {
            try
            {
                if (a == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージを展開できません", Exceptions.BAD_PACKAGE);
                    return;
                }
                ZipArchiveEntry e = a.GetEntry(Constants.PACKAGE_MANIFEST_FILENAME);
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ設定ファイル:[manifest.xml]が見つかりません", Exceptions.BAD_PACKAGE);
                }
                else
                {
                    //見つかった時は開く
                    AlicePackage package = new AlicePackage();
                    package.archive = a;
                    string xml = GetEntryScript(e,Constants.PACKAGE_MANIFEST_FILENAME);
                    if (xml == null)
                    {
                        return;
                    }
                    package.Manifest = GetManifest(xml);
                    if (package.Manifest!=null)
                    {
                        if (package.Manifest.Target!=null)
                        {
                            if (!package.Manifest.Target.Contains(Interpreter.Instance.Name))
                            {
                                ThrowErrorManerger.OnThrowError("そのパッケージをこのインタプリタで実行することはできません", Exceptions.NOT_COMPATIBLE_PACKAGES);
                                return;
                            }
                        }
                        string srcname=string.IsNullOrEmpty(package.Manifest.ScriptPath) ? Constants.PACKAGE_MANIFEST_FILENAME: package.Manifest.ScriptPath;
                        if (!package.Manifest.UseInlineScript)
                        {
                            ZipArchiveEntry entry = a.GetEntry(srcname);
                            if (entry == null)
                            {
                                ThrowErrorManerger.OnThrowError("エントリポイント:[" + srcname + "]が見つかりません", Exceptions.BAD_PACKAGE);
                                return;
                            }
                            else
                            {
                                package.Manifest.Script = GetEntryScript(entry, srcname);
                            }
                        }
                        Interpreter.Instance.Process(package.Manifest.Script, filename + "\\" + srcname, true, null, package);
                    }
                    else
                    {
                        ThrowErrorManerger.OnThrowError("パッケージマニフェストファイルが不正です", Exceptions.BAD_PACKAGE);
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError(ex.Message, Exceptions.BAD_PACKAGE);
            }
        }
        public Variable ExecuteEntry(string filename)
        {
            string script = GetEntryScript(archive.GetEntry(filename), filename);
            if (script == null)
            {
                return Variable.EmptyInstance;
            }
            return Interpreter.Instance.Process(script, "main.alice", true, null, this);
        }
        public bool ExistsEntry(string filename)
        {
            return (archive.GetEntry(filename) != null);
        }
        internal static byte[] GetEntryData(ZipArchiveEntry e, string filename)
        {
            try
            {
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                    return null;
                }
                return GetDataFromStream(e.Open());
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:" + ex.Message, Exceptions.BAD_PACKAGE);
                return null;
            }
        }
        internal static string GetEntryScript(ZipArchiveEntry e, string filename)
        {
            try
            {
                if (e == null)
                {
                    ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                    return null;
                }
                string temp = Path.GetTempFileName();
                WriteStreamToExitingFile(temp, e.Open());
                return SafeReader.ReadAllText(temp, out _);
            }
            catch (Exception ex)
            {
                ThrowErrorManerger.OnThrowError("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:" + ex.Message, Exceptions.BAD_PACKAGE);
                return null;
            }
        }
        private static void WriteStreamToExitingFile(string filename, Stream stream)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                stream.CopyTo(fs);
            }
        }
        private static byte[] GetDataFromStream(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.GetBuffer();
            }
        }
        public static void CreateEncodingPackage(string filepath, string outfilepath)
        {
            int i, len;
            byte[] buffer = new byte[4096];
            byte[] data = File.ReadAllBytes(filepath);

            using (FileStream outfs = new FileStream(outfilepath, FileMode.Create, FileAccess.Write))
            {
                using (AesManaged aes = new AesManaged())
                {
                    aes.BlockSize = 128;              // BlockSize = 16bytes
                    aes.KeySize = 128;                // KeySize = 16bytes
                    aes.Mode = CipherMode.CBC;        // CBC mode
                    aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                    // KeyとIV ( Initilization Vector ) は、AesManagedにつくらせる
                    aes.GenerateKey();
                    aes.GenerateIV();

                    //Encryption interface.
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (CryptoStream cse = new CryptoStream(outfs, encryptor, CryptoStreamMode.Write))
                    {
                        outfs.Write(Constants.PACKAGE_MAGIC_NUMBER, 0, Constants.PACKAGE_MAGIC_NUMBER.Length);     // ファイルヘッダを先頭に埋め込む
                        outfs.Write(aes.Key,0,16); //次にKeyをファイルに埋め込む
                        outfs.Write(aes.IV, 0, 16); // 続けてIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            double size = data.LongLength;
                            byte[] sum = BitConverter.GetBytes(size);
                            ds.Write(sum,0,8);//解凍後の実際の長さを書き込む(これを用いて解凍をチェックする)
                            using (MemoryStream fs=new MemoryStream(data))
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
        }
        
        public static void LoadEncodingPackage(byte[] data,string filename="")
        {
            int i, len;
            byte[] buffer = new byte[4096];

            using (MemoryStream outfs = new MemoryStream())
            {
                
                using (MemoryStream fs=new MemoryStream(data))
                {
                    using (AesManaged aes = new AesManaged())
                    {
                        aes.BlockSize = 128;              // BlockSize = 16bytes
                        aes.KeySize = 128;                // KeySize = 16bytes
                        aes.Mode = CipherMode.CBC;        // CBC mode
                        aes.Padding = PaddingMode.PKCS7;    // Padding mode is "PKCS7".

                        int ml = Constants.PACKAGE_MAGIC_NUMBER.Length;
                        byte[] mark = new byte[ml];
                        fs.Read(mark, 0, ml);
                        if (!mark.SequenceEqual(Constants.PACKAGE_MAGIC_NUMBER))
                        {
                            ThrowErrorManerger.OnThrowError("エラー:有効なAlicePackageファイルではありません",Exceptions.BAD_PACKAGE);
                            return;
                        }
                        // Key
                        byte[] key = new byte[16];
                        fs.Read(key,0,16);
                        aes.Key = key;
                        // Initilization Vector
                        byte[] iv = new byte[16];
                        fs.Read(iv, 0, 16);
                        aes.IV = iv;
                        //Decryption interface.
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (CryptoStream cse = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                        {
                            using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Decompress))   //解凍
                            {
                                byte[] sum = new byte[8];
                                ds.Read(sum,0,8);
                                double size = BitConverter.ToDouble(sum);
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                                if (outfs.Length != size)
                                {
                                    ThrowErrorManerger.OnThrowError("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                                    return;
                                }
                            }
                        }
                    }
                }
                try
                {
                    LoadArchive(new ZipArchive(outfs),filename);
                }
                catch
                {
                    ThrowErrorManerger.OnThrowError("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                    return;
                }
            }
        }
    }
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
    }

}
