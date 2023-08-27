using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace AliceScript
{
    public class AlicePackage
    {
        public ZipArchive Archive
        {
            get => archive;
            set => archive = value;
        }

        internal ZipArchive archive;

        public PackageManifest Manifest { get; set; }

        public static void Load(string path, bool callFromScrpipt = false)
        {
            if (!File.Exists(path))
            {
                throw new ScriptException("パッケージが見つかりません", Exceptions.FILE_NOT_FOUND);
            }
            byte[] file = File.ReadAllBytes(path);
            LoadData(file, path, callFromScrpipt);
        }
        public static void LoadData(byte[] data, string filename = "", bool callFromScript = false)
        {
            byte[] magic = data.Take(Constants.PACKAGE_MAGIC_NUMBER.Length).ToArray();
            if (magic.SequenceEqual(Constants.PACKAGE_MAGIC_NUMBER))
            {
                LoadEncodingPackage(data, filename, callFromScript);
            }
            else
            {
                LoadArchive(new ZipArchive(new MemoryStream(data)), filename, callFromScript);
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
                manifest.Target = !string.IsNullOrEmpty(sip) && sip.ToLower() != "any" ? new List<string>(sip.Split(',')) : null;
                sip = config.Read("targetapp");
                if (!string.IsNullOrEmpty(sip) && sip.ToLower() != "any")
                {
                    manifest.TargetApp = new List<string>(sip.Split(','));
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
        internal static void LoadArchive(ZipArchive a, string filename = "", bool callFromScript = false)
        {
            if (a == null)
            {
                throw new ScriptException("パッケージを展開できません", Exceptions.BAD_PACKAGE);
            }
            ZipArchiveEntry e = a.GetEntry(Constants.PACKAGE_MANIFEST_FILENAME);
            if (e == null)
            {
                throw new ScriptException("パッケージ設定ファイル:[" + Constants.PACKAGE_MANIFEST_FILENAME + "]が見つかりません", Exceptions.BAD_PACKAGE);
            }
            else
            {
                //見つかった時は開く
                AlicePackage package = new AlicePackage();
                package.archive = a;
                string xml = GetEntryScript(e, Constants.PACKAGE_MANIFEST_FILENAME);
                if (xml == null)
                {
                    return;
                }
                package.Manifest = GetManifest(xml);
                if (package.Manifest != null)
                {
                    if (package.Manifest.Target != null)
                    {
                        if (!package.Manifest.Target.Contains(Interpreter.Instance.Name))
                        {
                            throw new ScriptException("そのパッケージをこのインタプリタで実行することはできません", Exceptions.NOT_COMPATIBLE_PACKAGES);
                        }
                    }
                    if (package.Manifest.TargetApp != null)
                    {
                        if (!package.Manifest.TargetApp.Contains(Alice.AppName))
                        {
                            throw new ScriptException("そのパッケージをこのアプリケーションで実行することはできません", Exceptions.NOT_COMPATIBLE_PACKAGES);
                        }
                    }
                    string srcname = string.IsNullOrEmpty(package.Manifest.ScriptPath) ? Constants.PACKAGE_MANIFEST_FILENAME : package.Manifest.ScriptPath;
                    if (!package.Manifest.UseInlineScript)
                    {
                        ZipArchiveEntry entry = a.GetEntry(srcname);
                        package.Manifest.Script = entry == null
                            ? throw new ScriptException("エントリポイント:[" + srcname + "]が見つかりません", Exceptions.BAD_PACKAGE)
                            : GetEntryScript(entry, srcname);
                    }
                    Interpreter.Instance.Process(package.Manifest.Script, filename + "\\" + srcname, true, null, package);
                }
                else
                {
                    throw new ScriptException("パッケージマニフェストファイルが不正です", Exceptions.BAD_PACKAGE);
                }

            }
        }
        public Variable ExecuteEntry(string filename)
        {
            string script = GetEntryScript(archive.GetEntry(filename), filename);
            return script == null ? Variable.EmptyInstance : Interpreter.Instance.Process(script, "main.alice", true, null, this);
        }
        public bool ExistsEntry(string filename)
        {
            return archive.GetEntry(filename) != null;
        }
        public byte[] GetEntryData(string filename)
        {
            return GetEntryToData(archive.GetEntry(filename), filename);
        }
        public string GetEntryText(string filename)
        {
            return GetEntryScript(archive.GetEntry(filename), filename);
        }
        internal static byte[] GetEntryToData(ZipArchiveEntry e, string filename)
        {
            try
            {
                if (e == null)
                {
                    throw new ScriptException("パッケージ内のファイル[" + filename + "]が見つかりません", Exceptions.FILE_NOT_FOUND);
                }
                var stream = e.Open();
                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                    return bytes;
                }
            }
            catch (Exception ex)
            {
                throw new ScriptException("パッケージ内のファイル[" + filename + "]を読み込めません。詳細:" + ex.Message, Exceptions.BAD_PACKAGE);
            }
        }
        internal static string GetEntryScript(ZipArchiveEntry e, string filename)
        {
            return SafeReader.ReadAllText(GetEntryToData(e, filename), out _);
        }
        internal static byte[] GetByteArrayFromStream(Stream sm)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                sm.CopyTo(ms);
                return ms.ToArray();
            }
        }
        public static void CreateEncodingPackage(string filepath, string outfilepath, byte[] controlCode = null, bool minify = true)
        {
            //Zipファイルを開く
            if (minify)
            {
                using (ZipArchive a = ZipFile.Open(filepath, ZipArchiveMode.Update))
                {
                    Dictionary<string, byte[]> scripts = new Dictionary<string, byte[]>();
                    List<ZipArchiveEntry> deletes = new List<ZipArchiveEntry>();
                    foreach (ZipArchiveEntry entry in a.Entries)
                    {
                        if (entry.Name.EndsWith(".alice", StringComparison.Ordinal))
                        {
                            Stream sw = entry.Open();
                            string script = SafeReader.ReadAllText(GetByteArrayFromStream(sw), out _);
                            int old = script.Length;
                            script = Utils.ConvertToScript(script, out _, out _, out _, entry.FullName);
                            byte[] script_data = Encoding.UTF8.GetBytes(script);
                            string fn = entry.FullName;
                            sw.Close();
                            deletes.Add(entry);
                            scripts.Add(fn, script_data);
                        }
                    }
                    foreach (var e in deletes)
                    {
                        e.Delete();
                    }
                    foreach (var script in scripts)
                    {
                        var ne = a.CreateEntry(script.Key, CompressionLevel.NoCompression);
                        ne.Open().Write(script.Value, 0, script.Value.Length);
                    }
                }
            }

            int i, len;
            byte[] buffer = new byte[4096];
            byte[] data = File.ReadAllBytes(filepath);

            if (controlCode == null)
            {
                controlCode = new byte[16];
            }
            else if (controlCode.Length != 16)
            {
                //制御コードが16バイトに満たない場合は0で埋め、それより大きい場合は切り詰めます
                byte[] newCode = new byte[16];
                for (i = 0; i < newCode.Length; i++)
                {
                    newCode[i] = i < controlCode.Length ? controlCode[i] : (byte)0x00;
                }
                controlCode = newCode;
            }

            using (FileStream outfs = new FileStream(outfilepath, FileMode.Create, FileAccess.Write))
            {
                using (var aes = Aes.Create())
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
                        outfs.Write(controlCode, 0, 16);//制御コードをファイルに埋め込む
                        outfs.Write(aes.Key, 0, 16); //次にKeyをファイルに埋め込む
                        outfs.Write(aes.IV, 0, 16); // 続けてIVもファイルに埋め込む
                        using (DeflateStream ds = new DeflateStream(cse, CompressionMode.Compress)) //圧縮
                        {
                            double size = data.LongLength;
                            byte[] sum = BitConverter.GetBytes(size);
                            ds.Write(sum, 0, 8);//解凍後の実際の長さを書き込む(これを用いて解凍をチェックする)
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
        }

        public static void LoadEncodingPackage(byte[] data, string filename = "", bool callFromScript = false)
        {
            int len;
            byte[] buffer = new byte[4096];

            using (MemoryStream outfs = new MemoryStream())
            {

                using (MemoryStream fs = new MemoryStream(data))
                {
                    using (var aes = Aes.Create())
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
                            throw new ScriptException("エラー:有効なAlicePackageファイルではありません", Exceptions.BAD_PACKAGE);
                        }
                        fs.Seek(16, SeekOrigin.Current);//制御コード分シーク
                        // Key
                        byte[] key = new byte[16];
                        fs.Read(key, 0, 16);
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
                                ds.Read(sum, 0, 8);
                                double size = BitConverter.ToDouble(sum);
                                while ((len = ds.Read(buffer, 0, 4096)) > 0)
                                {
                                    outfs.Write(buffer, 0, len);
                                }
                                if (outfs.Length != size)
                                {
                                    throw new ScriptException("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
                                }
                            }
                        }
                    }
                }
                try
                {
                    new ZipArchive(outfs);
                    LoadArchive(new ZipArchive(outfs), filename, callFromScript);
                }
                catch
                {
                    throw new ScriptException("エラー:AlicePackageが壊れています", Exceptions.BAD_PACKAGE);
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
        /// <summary>
        /// ターゲットにするアプリケーション
        /// </summary>
        public List<string> TargetApp { get; set; }
    }

}