using System;
using System.IO;
using System.Text;

namespace AliceScript
{
    public static class SafeReader
    {
        public static string ReadAllText(string filename,out string charcode)
        {
            FileInfo file = new FileInfo(filename);
            if (!file.Exists) throw new FileNotFoundException();
            using (FileReader reader = new FileReader(file))
            {
                CharCode c = reader.Read(file);
                charcode = c.Name;
                return reader.Text;
            }
        }
    }
    /// <summary>
    /// ReadJEnc 文字コード種類定義(Rev.20170821)
    /// </summary>
    internal abstract class CharCode
    {   ////////////////////////////////////////////////////////////////////////
        // <CharCode.cs> ReadJEnc 文字コード種類定義(Rev.20170821)
        //  Copyright (C) 2014-2018 hnx8(H.Takahashi)
        //  https://github.com/hnx8/ReadJEnc
        //
        //  Released under the MIT license
        //  http://opensource.org/licenses/mit-license.php
        ////////////////////////////////////////////////////////////////////////

        // Unicode系文字コード

        /// <summary>UTF8(BOMあり)</summary>
        internal static readonly Text UTF8 = new Text("UTF-8", new UTF8Encoding(true, true)); // BOM : 0xEF, 0xBB, 0xBF
        /// <summary>UTF32(BOMありLittleEndian)</summary>
        internal static readonly Text UTF32 = new Text("UTF-32", new UTF32Encoding(false, true, true)); // BOM : 0xFF, 0xFE, 0x00, 0x00
        /// <summary>UTF32(BOMありBigEndian)</summary>
        internal static readonly Text UTF32B = new Text("UTF-32B", new UTF32Encoding(true, true, true)); // BOM : 0x00, 0x00, 0xFE, 0xFF
        /// <summary>UTF16(BOMありLittleEndian)</summary><remarks>Windows標準のUnicode</remarks>
        internal static readonly Text UTF16 = new Text("UTF-16", new UnicodeEncoding(false, true, true)); // BOM : 0xFF, 0xFE
        /// <summary>UTF16(BOMありBigEndian)</summary>
        internal static readonly Text UTF16B = new Text("UTF-16B", new UnicodeEncoding(true, true, true)); // BOM : 0xFE, 0xFF

        /// <summary>UTF16(BOM無しLittleEndian)</summary>
        internal static readonly Text UTF16LE = new Text("UTF-16LE", new UnicodeEncoding(false, false, true));
        /// <summary>UTF16(BOM無しBigEndian)</summary>
        internal static readonly Text UTF16BE = new Text("UTF-16BE", new UnicodeEncoding(true, false, true));
        /// <summary>UTF8(BOM無し)</summary>
        internal static readonly Text UTF8N = new Text("UTF-8N", new UTF8Encoding(false, true));

        // １バイト文字コード

        /// <summary>Ascii</summary><remarks>デコードはUTF8Encodingを流用。Bom情報は転記しない</remarks>
        internal static readonly Text ASCII = new Text("ASCII", 0) { Encoding = UTF8N.GetEncoding() };
        /// <summary>1252 ISO8859 西ヨーロッパ言語</summary>
        internal static readonly Text ANSI = new Text("ANSI1252", 1252);

        // ISO-2022文字コード

        /// <summary>50221 iso-2022-jp 日本語 (JIS-Allow 1 byte Kana) ※MS版</summary>
        internal static readonly Text JIS = new Text("JIS", 50221);
        /// <summary>50222 iso-2022-jp 日本語 (JIS-Allow 1 byte Kana - SO/SI)</summary><remarks>SO/SIによるカナシフトのみのファイルもCP50222とみなす</remarks>
        internal static readonly Text JIS50222 = new Text("JIS50222", 50222);
        /// <summary>50221(MS版JIS) + 20932(JIS補助漢字を無理やりデコード)</summary><remarks>JIS補助漢字はデコードのみ対応、エンコードは未対応</remarks>
        internal static readonly Text JISH = new JisHText("JIS補漢"); // 他クラス定義の50221と20932を利用してデコード
        /// <summary>JISのように見えるがデコード不能な箇所あり、実質非テキストファイル</summary>
        internal static readonly Text JISNG = new Text("JIS破損", -50221);
        /// <summary>50225 iso-2022-kr 韓国語(ISO)</summary><remarks>SO/SIカナシフトファイルの判定ロジックに流れ込まないようにするため定義</remarks>
        internal static readonly Text ISOKR = new Text("ISO-KR", 50225);

        // 日本語文字コード

        /// <summary>932 shift_jis 日本語 (シフト JIS) ※MS独自</summary>
        internal static readonly Text SJIS = new Text("ShiftJIS", 932);
        /// <summary>EUC補助漢字(0x8F)あり ※MS-CP20932を利用し強引にデコードする</summary><remarks>エンコードするとファイルが壊れるので注意</remarks>
        internal static readonly Text EUCH = new EucHText("EUC補漢"); // 20932
        /// <summary>51932 euc-jp 日本語 (EUC) ※MS版</summary>
        internal static readonly Text EUC = new Text("EUCJP", 51932);

#if (!JPONLY)

        // 漢字圏テキスト文字コード各種（日本語判別以外使用しないなら定義省略可）

        /// <summary>950 big5 繁体字中国語 (BIG5)</summary>
        internal static readonly Text BIG5TW = new Text("Big5", 950);
        /// <summary>20000 x-Chinese-CNS 繁体字中国語(EUC-TW)</summary>
        internal static readonly Text EUCTW = new Text("EUC-TW", 20000);

        /// <summary>54936 GB18030 簡体字中国語 (GB2312/GBKの拡張)</summary>
        internal static readonly Text GB18030 = new Text("GB18030", 54936);
        // →EUC-CN(GB2312)はGB18030として取り扱うほうが妥当であるため定義をコメントアウト抹消
        ///// <summary>51936 EUC-CN 簡体字中国語 (=GB2312)</summary>
        //internal static readonly Text EUCCN = new Text("EUC(中)", 51936);

        /// <summary>949 ks_c_5601-1987 韓国語 (UHC=EUC-KRの拡張)</summary>
        internal static readonly Text UHCKR = new Text("UHC", 949);
        // →EUC-KRはUHCとして取り扱うほうが妥当であるため定義をコメントアウト抹消
        ///// <summary>51949 euc-kr 韓国語 (=KSX1001)</summary>
        //internal static readonly Text EUCKR = new Text("EUC(韓)", 51949);


        // ISO8859などのテキスト文字コード自動判別（日本語判別以外使用しないなら定義省略可）

        /// <summary>Windows1250 中央ヨーロッパ言語(チェコ語等) iso-8859-2</summary>
        internal static readonly Text CP1250 = new Text("CP1250", 1250);
        /// <summary>Windows1251 キリル言語(ロシア語等) </summary>
        internal static readonly Text CP1251 = new Text("CP1251", 1251);
        /// <summary>Windows1253 ギリシャ語 iso-8859-7</summary>
        internal static readonly Text CP1253 = new Text("CP1253", 1253);
        /// <summary>Windows1254 トルコ語 iso-8859-9</summary>
        internal static readonly Text CP1254 = new Text("CP1254", 1254);
        /// <summary>Windows1255 ヘブライ語 iso-8859-8</summary>
        internal static readonly Text CP1255 = new Text("CP1255", 1255);
        /// <summary>Windows1256 アラビア語 </summary>
        internal static readonly Text CP1256 = new Text("CP1256", 1256);
        /// <summary>Windows1257 バルト言語 iso-8859-13</summary>
        internal static readonly Text CP1257 = new Text("CP1257", 1257);
        /// <summary>Windows1258 ベトナム語</summary>
        internal static readonly Text CP1258 = new Text("CP1258", 1258);
        /// <summary>TIS-620/Windows874 タイ語 iso-8859-11</summary>
        internal static readonly Text TIS620 = new Text("TIS-620", 874);
#endif

        // 文字コード（ファイル種類）判定メソッド

        /// <summary>引数で指定されたbyte配列がBOMありUTFファイルと判定できる場合、その文字コードを返します。</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="read">バイト配列先頭の読み込み済バイト数（LEASTREADSIZEのバイト数以上読み込んでおくこと）</param>
        /// <returns>BOMから判定できた文字コード種類、合致なしの場合null</returns>
        internal static CharCode GetPreamble(byte[] bytes, int read)
        {   // BOM一致判定
            return GetPreamble(bytes, read,
                UTF8, UTF32, UTF32B, UTF16, UTF16B);
        }

        #region 基本クラス定義--------------------------------------------------
        /// <summary>ファイル文字コード種類名</summary>
        internal readonly string Name;
        /// <summary>先頭バイト識別データ（BOM/マジックナンバー）</summary>
        protected readonly byte[] Bytes = null;
        /// <summary>エンコーディング</summary>
        private Encoding Encoding;
        /// <summary>コードページ番号(Unicode以外のローカルなエンコーディングを使用するものについて設定あり。遅延初期化用の退避変数を兼ねる)</summary>
        internal readonly int CodePage = 0;

        /// <summary>基本コンストラクタ</summary>
        /// <param name="Name">ファイル文字コード種類名を定義する</param>
        /// <param name="CodePage">デコード時に使用するCodePageを指定(正値ならDecoderExceptionFallback、マイナス値ならDecoderReplacementFallBackを設定)</param>
        /// <param name="Bytes">先頭バイト識別データを指定する</param>
        protected CharCode(string Name, int CodePage, byte[] Bytes)
        {
            this.Name = Name;
            this.CodePage = CodePage;
            //GetEncoding(); // Encodingを実際に使用するまで初期化を遅らせる
            this.Bytes = Bytes;
        }
        /// <summary>基本コンストラクタ</summary>
        /// <param name="Name">ファイル文字コード種類名を定義する</param>
        /// <param name="Encoding">デコード時に使用するEncodingを指定する</param>
        /// <param name="Bytes">先頭バイト識別データを指定する</param>
        protected CharCode(string Name, Encoding Encoding, byte[] Bytes)
        {
            this.Name = Name;
            this.Encoding = Encoding;
            this.Bytes = Bytes;
        }

        /// <summary>このファイル文字コード種類のEncodingオブジェクトを取得します。</summary>
        internal Encoding GetEncoding()
        {
            if (this.Encoding == null)
            {   // Encodingオブジェクトがまだ用意されていなければ初期化する
                this.Encoding =
                    (CodePage > 0 ? System.Text.Encoding.GetEncoding(CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback)
                    : CodePage < 0 ? System.Text.Encoding.GetEncoding(-CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback)
                    : null);
            }
            return Encoding;
        }

        /// <summary>引数のバイト配列から文字列を取り出します。失敗時はnullが返ります。</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="len">ファイルサイズ(バイト配列先頭からの先頭からのデコード対象バイト数)</param>
        internal virtual string GetString(byte[] bytes, int len)
        {
            Encoding enc = GetEncoding();
            if (enc == null) { return null; }
            try
            {   // BOMサイズを把握し、BOMを除いた部分を文字列として取り出す
                int bomBytes = (this.Bytes == null ? 0 : this.Bytes.Length);
                return enc.GetString(bytes, bomBytes, len - bomBytes);
            }
            catch (DecoderFallbackException)
            {   // 読み出し失敗(マッピングされていない文字があった場合など)
                return null;
            }
        }

        /// <summary>このファイル文字コード種類の名前を取得します。</summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>判定対象のファイル文字コード種類一覧から、BOM/マジックナンバーが一致するものを探索して返す</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="read">バイト配列先頭の読み込み済バイト数（LEASTREADSIZEのバイト数以上読み込んでおくこと）</param>
        /// <param name="arr">判定対象とするファイル文字コード種類の一覧</param>
        /// <returns>先頭バイトが一致したファイル文字コード種類、合致なしの場合null</returns>
        protected static CharCode GetPreamble(byte[] bytes, int read, params CharCode[] arr)
        {
            foreach (CharCode c in arr)
            {   // 読み込み済バイト配列内容をもとにファイル種類の一致を確認
                byte[] bom = c.Bytes;
                int i = (bom != null ? bom.Length : int.MaxValue); // BOM・マジックナンバー末尾から調べる
                if (read < i) { continue; } // そもそもファイルサイズが小さい場合は不一致
                do
                {   // 全バイト一致ならその文字コードとみなす
                    if (i == 0) { return c; }
                    i--;
                } while (bytes[i] == bom[i]); // BOM・マジックナンバー不一致箇所ありならdo脱出
            }
            return null; // ファイル種類決定できず
        }

        #endregion

        #region テキスト基本クラス定義------------------------------------------
        /// <summary>文字コード種類：テキスト
        /// </summary>
        internal class Text : CharCode
        {
            internal Text(string Name, Encoding Encoding) : base(Name, Encoding, Encoding.GetPreamble()) { }
            internal Text(string Name, int CodePage) : base(Name, CodePage, null) { }
        }
        #endregion

        #region JIS補助漢字対応デコーダ-----------------------------------------
        /// <summary>
        /// EUC補助漢字特殊処理(MS版CP20932の特異なコード体系によりデコードする)
        /// </summary>
        private class EucHText : Text
        {
            internal EucHText(string Name) : base(Name, 20932) { }

            internal override string GetString(byte[] bytes, int len)
            {
                byte[] bytesForCP20932 = new byte[len]; // CP20932でのデコード用にバイト配列を補正
                int cp20932Len = 0;
                int shiftPos = int.MinValue;
                byte b;
                for (int i = 0; i < len; i++)
                {
                    if ((b = bytes[i]) == 0x8F)
                    {   // 3byteの補助漢字を検出、補正箇所を把握(0x8Fは読み捨て、補正後配列には設定しない)
                        shiftPos = i + 2;
                    }
                    else
                    {   // 補助漢字3byte目ならば0x21-0x7Eへシフト(CP20932におけるEUCの2byte目として設定)
                        bytesForCP20932[cp20932Len] = (i == shiftPos ? (byte)(b & 0x7F) : b);
                        cp20932Len++;
                    }
                }
                try
                {   // 補正後配列を用い、CP20932でのデコードを試みる
                    return GetEncoding().GetString(bytesForCP20932, 0, cp20932Len);
                }
                catch (DecoderFallbackException)
                {   // 読み出し失敗(マッピングされていない文字があった場合など)
                    return null;
                }
            }
        }
        /// <summary>
        /// JIS補助漢字特殊処理(MS版CP20932の特異なコード体系によりデコードする)
        /// </summary>
        private class JisHText : Text
        {
            internal JisHText(string Name) : base(Name, 0) { }

            internal override string GetString(byte[] bytes, int len)
            {
                try
                {
                    StringBuilder ret = new StringBuilder(len);
                    int pos = 0;
                    while (pos < len)
                    {   // JIS補助漢字エスケープ以外の範囲を把握
                        int start = pos;
                        while (pos < len)
                        {
                            if (bytes[pos] == 0x1B && pos + 3 < len &&
                                bytes[pos + 1] == 0x24 &&
                                bytes[pos + 2] == 0x28 &&
                                bytes[pos + 3] == 0x44)
                            {   // JIS補助漢字エスケープシーケンスを検出、ループ脱出
                                break;
                            }
                            pos++;
                        }
                        if (start < pos)
                        {   // 通常のCP5022Xでデコードする
                            ret.Append(JIS.GetEncoding().GetString(bytes, start, pos - start));
                        }
                        // JIS補助漢字エスケープ部分の処理
                        if (pos < len)
                        {   // JIS補助漢字エスケープシーケンス除去、補助漢字範囲特定
                            pos = pos + 4;
                            start = pos;
                            while (pos < len && bytes[pos] != 0x1B) { pos++; }
                            if (start < pos)
                            {
                                byte[] bytesForCP20932 = new byte[pos - start];
                                for (int i = 0; i < bytesForCP20932.Length; i++)
                                {   // CP20932のコード体系に合わせ、１バイト目は0xA1-0XFE,２バイト目は0x21-0x7Eとなるようにする
                                    bytesForCP20932[i] = bytes[start + i];
                                    if (i % 2 == 0) { bytesForCP20932[i] |= 0x80; }
                                }
                                // EUC補助漢字のCP20932を用いてデコードする
                                ret.Append(EUCH.GetEncoding().GetString(bytesForCP20932, 0, bytesForCP20932.Length));
                            }
                        }
                    }
                    return ret.ToString();
                }
                catch (DecoderFallbackException)
                {   // 読み出し失敗(マッピングされていない文字があった場合など)
                    return null;
                }
            }
        }
        #endregion
    }
    /// <summary>
    /// ReadJEnc ファイル読み出し＆ファイル文字コード種類自動判別(Rev.20170821)
    /// </summary>
    internal class FileReader : IDisposable
    {   ////////////////////////////////////////////////////////////////////////
        // <FileReader.cs> ReadJEnc File読出＆文字コード自動判別(Rev.20170821)
        //  Copyright (C) 2014-2018 hnx8(H.Takahashi)
        //  https://github.com/hnx8/ReadJEnc
        //
        //  Released under the MIT license
        //  http://opensource.org/licenses/mit-license.php
        ////////////////////////////////////////////////////////////////////////

        // コンストラクタ／デストラクタ==========================================

        /// <summary>単一ファイル読み出し用にファイルを指定して新しいインスタンスを初期化します。</summary>
        /// <param name="file">読み出すファイル（このファイルのサイズどおりに読み出し領域バッファを確保する）</param>
        internal FileReader(FileInfo file)
            : this((file.Length < int.MaxValue ? (int)file.Length : 0)) { }

        /// <summary>複数ファイル連続読み出し用にバッファサイズを指定して新しいインスタンスを初期化します。</summary>
        /// <param name="len">最大読み出しファイルサイズ（領域バッファ確保サイズ）</param>
        internal FileReader(int len)
        {
            Bytes = new byte[len];
        }

        /// <summary>ファイル読み出し用のリソースを解放します。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>ファイル読み出し用のリソースを解放します。</summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {   // 管理（managed）リソースの破棄処理
                Bytes = null;
            }
            // 非管理（unmanaged）リソースの破棄処理：ReadJEncでは特に該当なし
        }

        // 設定カスタマイズ======================================================

        /// <summary>文字コード自動判別対象言語指定（初期状態は日本語ShiftJIS）</summary>
        internal ReadJEnc ReadJEnc = ReadJEnc.JP;

        // ファイル読み出し本体==================================================

        /// <summary>ファイルを読み出してファイル文字コード種類を取得します。</summary>
        /// <param name="file">読み出すファイル</param>
        /// <returns>ファイル文字コード種類の判定結果</returns>
        internal virtual CharCode Read(FileInfo file)
        {
            this.Length = 0;
            text = null;
            try
            {   // 無用なDiskIOを極力行わないよう、オープン前にもファイルサイズチェック
                if (file.Length == 0) { return FileType.EMPTYFILE; } // ■空ファイル
                if (file.Length > Bytes.Length) { return FileType.HUGEFILE; } // ■巨大ファイル
                CharCode c;
                // ファイルを読み込み、ファイル文字コード種類を把握
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {   // オープン後の実際のファイルサイズによるチェック
                    long filesize = stream.Length;
                    if (filesize == 0) { return FileType.EMPTYFILE; } // ■空ファイル
                    if (filesize > Bytes.Length) { return FileType.HUGEFILE; } // ■巨大ファイル
                    if (filesize > 65536)
                    {   // 一定サイズ以上の大きいファイルなら、BOM/マジックナンバー判定に必要な先頭バイトを読み込み、判断
                        this.Length = stream.Read(Bytes, 0, FileType.GetBinaryType_LEASTREADSIZE);
                        c = GetPreamble(filesize);
                        if (c == null || c is CharCode.Text)
                        {   // 残りの読みこみ（ただし非テキストと確定した場合は省略）
                            this.Length += stream.Read(Bytes, this.Length, (int)filesize - this.Length);
                        }
                    }
                    else
                    {   // 大きくないファイルは一括で全バイト読み込み、判断
                        this.Length = stream.Read(Bytes, 0, (int)filesize);
                        c = GetPreamble(filesize);
                    }
                }
                if (c is CharCode.Text)
                {   // BOMありテキストなら文字列を取り出す（取り出せなかったら非テキスト扱い）
                    if ((text = c.GetString(Bytes, Length)) == null) { c = null; }
                }
                else if (c == null)
                {   // ファイル文字コード種類不明なら、全バイト走査して文字コード確定
                    c = ReadJEnc.GetEncoding(Bytes, Length, out text);
                }
                // ここまでで文字コードが決まらなかったらバイナリファイル扱い
                return (c == null ? FileType.GetBinaryType(Bytes, Length) : c);
            }
            catch (System.IO.IOException) { return FileType.READERROR; } // ■読み取りエラー
            catch (System.UnauthorizedAccessException) { return FileType.READERROR; } // ■読み取りエラー
        }
        /// <summary>Readメソッド呼び出し時にファイルから読み出したテキスト文字列内容を取得します。</summary>
        /// <remarks>ファイルからテキストが取り出せなかった場合はnullとなります。</remarks>
        internal string Text { get { return text; } }

        #region 非internal処理----------------------------------------------------
        /// <summary>ファイル内容の読み出し先領域</summary>
        protected byte[] Bytes; // コンストラクタで確保,Disposeで廃棄
        /// <summary>現在読み出し済のファイルサイズ</summary><remarks>非読み出し時は0、分割読み出し時は読込済部分のサイズ</remarks>
        protected int Length = 0;
        /// <summary>ファイルから取り出したテキスト文字列</summary>
        protected String text = null;

        /// <summary>読み込んであるバイト配列のプリアンブル（BOMヘッダ／マジックナンバー）からファイル文字コード種類特定を試みる</summary>
        /// <param name="len">ファイルサイズ(未読込部分も含む。読み込み済サイズはthis.Lengthを参照)</param>
        /// <returns>確定した場合、ファイル文字コード種類。確定できなかった場合null</returns>
        protected virtual CharCode GetPreamble(long len)
        {
            // 【0】ファイル先頭バイトからUTF文字コード（BOMつきUTF）を判定
            CharCode ret = CharCode.GetPreamble(this.Bytes, this.Length);
            // BOMテキストファイルと判定できず＆ファイル先頭にバイナリファイル特徴の0x00が登場している場合、追加チェック
            if (ret == null && Array.IndexOf<byte>(this.Bytes, 0x00, 0, this.Length) >= 0)
            {   // UTF16Nの可能性がなければバイナリファイルとみなす
                if (ReadJEnc.SeemsUTF16N(this.Bytes, (int)len) == null)
                {   // ■バイナリ確定（マジックナンバーからファイル種類を決定）
                    return FileType.GetBinaryType(this.Bytes, this.Length);
                }
            }
            return ret; // ■BOMから特定できた場合はBOMつきUTF（特定できなかった場合はnull）
        }
        #endregion
    }
    /// <summary>
    /// ReadJEnc ファイル種類定義(Rev.20170821)
    /// </summary>
    internal class FileType : CharCode
    {   ////////////////////////////////////////////////////////////////////////
        // <FileType.cs> ReadJEnc ファイル種類定義(Rev.20170821)
        //  Copyright (C) 2014-2018 hnx8(H.Takahashi)
        //  https://github.com/hnx8/ReadJEnc
        //
        //  Released under the MIT license
        //  http://opensource.org/licenses/mit-license.php
        ////////////////////////////////////////////////////////////////////////

        // ※テキストファイルの定義は、CharCode.csを参照のこと

        // 読込対象外ファイル

        /// <summary>読み込み失敗</summary>
        internal static readonly FileType READERROR = new FileType("読込不能");
        /// <summary>空ファイル</summary>
        internal static readonly FileType EMPTYFILE = new FileType("空File");
        /// <summary>巨大ファイル</summary>
        internal static readonly FileType HUGEFILE = new FileType("巨大File");

        // バイナリその他非テキストファイル

        /// <summary>バイナリ</summary>
        internal static readonly Bin BINARY = new Bin("$BINARY", null);

        /// <summary>Javaバイナリ</summary>
        internal static readonly Bin JAVABIN = new Bin(-65001, "$JavaBin", 0xCA, 0xFE, 0xBA, 0xBE); // 内部文字コードはUTF8。HNXgrep用設定
        /// <summary>Windowsバイナリ</summary>
        internal static readonly Bin WINBIN = new Bin("$WinExec", 0x4D, 0x5A);
        /// <summary>Windowsショートカット</summary>
        internal static readonly Bin SHORTCUT = new Bin("$WinLnk", 0x4C, 0x00, 0x00, 0x00, 0x01, 0x14, 0x02, 0x00);
        /// <summary>PDF</summary>
        internal static readonly Bin PDF = new Bin("%PDF", (byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-');

        /// <summary>Zip圧縮</summary>
        internal static readonly Bin ZIP = new ZipBinary("$ZIP", 0x50, 0x4B, 0x03, 0x04);
        /// <summary>GZip圧縮</summary>
        internal static readonly Bin GZIP = new ZipBinary("$GZIP", 0x1F, 0x8B);
        /// <summary>7zip圧縮</summary>
        internal static readonly Bin SEVENZIP = new ZipBinary("$7ZIP", (byte)'7', (byte)'z', 0xBC, 0xAF, 0x27, 0x1C);
        /// <summary>Rar圧縮</summary>
        internal static readonly Bin RAR = new ZipBinary("$RAR", (byte)'R', (byte)'a', (byte)'r', (byte)'!');
        /// <summary>Cab圧縮</summary>
        internal static readonly Bin CABINET = new ZipBinary("$CAB", (byte)'M', (byte)'S', (byte)'C', (byte)'F', 0x00, 0x00, 0x00, 0x00);
        /// <summary>BZip2圧縮</summary>
        internal static readonly Bin BZIP2 = new ZipBinary("$BZIP2", (byte)'B', (byte)'Z', (byte)'h');
        /// <summary>Z(compress)圧縮</summary>
        internal static readonly Bin ZLZW = new ZipBinary("$Z-LZW", 0x1F, 0x9D);
        // TAR/LHA(LZHファイル)については先頭バイトが不定であり判別対応しずらいため対応外とする。

        /// <summary>BMP画像</summary>
        internal static readonly Image BMP = new Image("%BMP", (byte)'B', (byte)'M');
        /// <summary>GIF画像</summary>
        internal static readonly Image GIF = new Image("%GIF", (byte)'G', (byte)'I', (byte)'F', (byte)'8');
        /// <summary>JPEG画像</summary>
        internal static readonly Image JPEG = new Image("%JPEG", 0xFF, 0xD8, 0xFF);
        /// <summary>PNG画像</summary>
        internal static readonly Image PNG = new Image("%PNG", 0x89, (byte)'P', (byte)'N', (byte)'G', 0x0D, 0x0A, 0x1A, 0x0A);
        /// <summary>TIFF画像</summary>
        internal static readonly Image TIFF = new Image("%TIFF", 0x49, 0x49, 0x2A, 0x00); // IE9以降ならimgタグで表示可能
        /// <summary>Windowsアイコン画像</summary><remarks>マジックナンバーのほか追加チェックあり</remarks>
        internal static readonly Image IMGICON = new Image("%ICON", 0x00, 0x00, 0x01, 0x00);


        // ファイル種類判定用定数・判定メソッド

        /// <summary>BOM/マジックナンバー一致判定にあたり、最低限読み込みを済ませておく必要がある先頭バイト数です。</summary>
        internal const int GetBinaryType_LEASTREADSIZE = 32;

        /// <summary>引数で指定されたbyte配列についてバイナリファイルの種類を判定します。</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="read">バイト配列先頭の読み込み済バイト数（LEASTREADSIZEのバイト数以上読み込んでおくこと）</param>
        /// <returns>バイナリファイル種類判定結果（どれにも該当しなければ一般バイナリと判定）</returns>
        internal static CharCode GetBinaryType(byte[] bytes, int read)
        {   // 定義済みマジックナンバーすべてを対象に一致判定
            CharCode ret = GetPreamble(bytes, read,
                BMP, GIF, JPEG, PNG, TIFF, IMGICON,
                JAVABIN, WINBIN, SHORTCUT, PDF,
                ZIP, GZIP, SEVENZIP, RAR, CABINET, BZIP2, ZLZW);
            // ファイル種類に応じた追加判定
            if (ret == IMGICON && (read < 23 || bytes[4] == 0 || bytes[5] != 0)) { ret = null; } // ICONの誤判別防止用（アイコン個数チェック）            
            // 判定できたファイル種類を返す（どれにも該当しなければ一般バイナリと判定）
            return (ret != null ? ret : BINARY);
        }

        #region 継承クラス定義--------------------------------------------------
        private FileType(string Name) : base(Name, 0, null) { }

        /// <summary>ファイル文字コード種類：バイナリ
        /// </summary>
        internal class Bin : CharCode
        {
            internal Bin(string Name, params byte[] Bytes) : base(Name, 0, Bytes) { }
            internal Bin(int Encoding, string Name, params byte[] bytes) : base(Name, Encoding, bytes) { }
        }
        /// <summary>ファイル文字コード種類：Zipバイナリ
        /// </summary>
        internal class ZipBinary : Bin
        {
            internal ZipBinary(string Name, params byte[] Bytes) : base(Name, Bytes) { }
        }
        /// <summary>ファイル文字コード種類：画像
        /// </summary>
        internal class Image : CharCode
        {
            internal Image(string Name, params byte[] Bytes) : base(Name, 0, Bytes) { }
        }
        #endregion
    }
    /// <summary>
    /// ReadJEnc 文字コード自動判別ライブラリ・処理本体(Rev.20180421)
    /// </summary>
    internal class ReadJEnc
    {   ////////////////////////////////////////////////////////////////////////
        // <ReadJEnc.cs> ReadJEnc 文字コード自動判別処理本体(Rev.20180421)
        //  Copyright (C) 2014-2018 hnx8(H.Takahashi)
        //  https://github.com/hnx8/ReadJEnc
        //
        //  Released under the MIT license
        //  http://opensource.org/licenses/mit-license.php
        ////////////////////////////////////////////////////////////////////////

        // 言語別文字コード自動判別オブジェクト==================================

        /// <summary>日本語文字コード判別(SJIS/EUCJP)</summary>
        internal static readonly ReadJEnc JP = new SJIS();

        /// <summary>ANSI文字コード判別(CP1252)</summary>
        internal static readonly ReadJEnc ANSI = new ReadJEnc(CharCode.ANSI, null);

        // En: Kanji zone character code discrimination target language (Definition can be omitted if 
        // it is not used other than Japanese discrimination)
        // Ja: 漢字圏文字コード判別対象言語（日本語判別以外使用しないなら定義省略可）
#if (!JPONLY)
        /// <summary>繁体字中国語文字コード判別(BIG5/EUCTW)</summary>
        internal static readonly ReadJEnc TW = new BIG5TW();

        /// <summary>簡体字中国語文字コード判別(GB18030)</summary>
        internal static readonly ReadJEnc CN = new GB18030();

        /// <summary>ハングル文字コード判別(UHCKR)</summary>
        internal static readonly ReadJEnc KR = new UHCKR();
#endif

#if (!JPONLY) // その他の１バイト系文字コード判別対象言語（日本語判別以外使用しないなら定義省略可）

        // アルファベットと混在させずに使う文字種があればSBCSクラスで、なければReadJEncクラスで、判別解析を行う。
        // ※チェック用定義値は未定義文字コードのポイント通り。具体的には以下のようになる
        //               FEDC BA98 7654 3210         FEDC BA98 7654 3210
        //               ---- ---- ---- ----         ---- ---- ---- ----
        // CP1250 (0x9#) 0000 0001 0000 0001  (0x8#) 0000 0001 0000 1010 
        // CP1251 (0x9#) 0000 0001 0000 0000  (0x8#) 0000 0000 0000 0000 ※0xC0以降を特別扱い
        // CP1253 (0x9#) 1111 0101 0000 0001  (0x8#) 1111 0101 0000 0010 ※0xC1以降を特別扱い、0xAA/D2/FFは無効
        // CP1254 (0x9#) 0110 0000 0000 0001  (0x8#) 1110 0000 0000 0010
        // CP1255 (0x9#) 1111 0100 0000 0001  (0x8#) 1111 0100 0000 0010 ※0xC0以降を特別扱い、0xCA/D9-DF/FB-FC/FFは無効
        // CP1256 (0x9#) 0000 0000 0000 0000  (0x8#) 0000 0000 0000 0000 ※0xC0以降を特別扱い、無効文字なし
        // CP1257 (0x9#) 1001 0101 0000 0001  (0x8#) 0001 0101 0000 1010 （0xA1/A5は妥協する。最終的にデコードエラーとなるはず）
        // CP1258 (0x9#) 0110 0100 0000 0001  (0x8#) 1110 0100 0000 0010
        // TIS620 (0x9#) 1111 1111 0000 0001  (0x8#) 1111 1111 1101 1110 ※OxA1以降を特別扱い、0xDB-DE/FC-FFは無効

        /// <summary>Windows1250(iso-8859-2) 中央ヨーロッパ言語(チェコ語等)判別 </summary>
        internal static readonly ReadJEnc CP1250 = new ReadJEnc(CharCode.CP1250, 0x0101010A);

        /// <summary>Windows1251 キリル言語(ロシア語等)＆ANSI判別 </summary>
        internal static readonly ReadJEnc CP1251 = new SBCS(CharCode.CP1251, 0xC0, 0x01000000);

        /// <summary>Windows1253(iso-8859-7) ギリシャ語＆ANSI判別 </summary>
        internal static readonly ReadJEnc CP1253 = new SBCS(CharCode.CP1253, 0xC1, 0xF501F502, 0x00000400, 0x00040000, 0x80000000);

        /// <summary>Windows1254(iso-8859-9) トルコ語判別 </summary>
        internal static readonly ReadJEnc CP1254 = new ReadJEnc(CharCode.CP1254, 0x6001E002);

        /// <summary>Windows1255(iso-8859-8) ヘブライ語＆ANSI判別 </summary>
        internal static readonly ReadJEnc CP1255 = new SBCS(CharCode.CP1255, 0xC0, 0xF401F402, 0x00000000, 0xFE000400, 0X98000000);

        /// <summary>Windows1256 アラビア語＆ANSI判別 </summary>
        internal static readonly ReadJEnc CP1256 = new SBCS(CharCode.CP1256, 0xC0);

        /// <summary>Windows1257(iso-8859-13) バルト言語判別 </summary>
        internal static readonly ReadJEnc CP1257 = new ReadJEnc(CharCode.CP1257, 0x9501150A);

        /// <summary>Windows1258 ベトナム語判別 </summary>
        internal static readonly ReadJEnc CP1258 = new ReadJEnc(CharCode.CP1258, 0x6401E402);

        /// <summary>TIS620/Windows874(iso-8859-11) タイ語＆ANSI判別 </summary>
        internal static readonly ReadJEnc TIS620 = new SBCS(CharCode.TIS620, 0xA1, 0xFF01FFDE, 0x00000000, 0x78000000, 0xF0000000);
#endif

        #region 基本クラス定義--------------------------------------------------
        /// <summary>バイナリと判定するDEL文字コード、兼、ASCII/非ASCIIの境界文字コード</summary>
        const byte DEL = (byte)0x7F;
        /// <summary>非テキストファイルと判定する制御文字コードの最大値</summary>
        const byte BINARY = (byte)0x03; // 0x01-0x07位の範囲で調整。0x08(BS)はTeraTerm等ログで出る。0x09(TAB)は普通にテキストで使う。0x03くらいにするのがよい。HNXgrep/TresGrepでは0x03を採用

        /// <summary>このインスタンスでおもに判別対象とするデフォルト文字コード</summary>
        internal readonly CharCode CharCode;
        /// <summary>EUCデコードで用いる文字コード（無指定null時はEUCの判別を行わない）</summary>
        protected readonly CharCode EUC = null;
        /// <summary>CP1252系で判別対象とする文字コード（デフォルトはANSI=CP1252）</summary>
        protected readonly CharCode CP125X = CharCode.ANSI;
        /// <summary>CP1252系の判別にあたり、0x80-0x9Fのうち文字コード未定義の箇所をbitで表現した値</summary>
        /// <remarks>
        /// デフォルトの非CP1252チェック用定義値(0x2001A002)＝未定義の81,8D,8F,90,9Dに対応するビットがON
        ///        FEDC BA98 7654 3210         FEDC BA98 7654 3210
        ///        ---- ---- ---- ----         ---- ---- ---- ----
        /// (0x9#) 0010 0000 0000 0001  (0x8#) 1010 0000 0000 0010
        /// </remarks>
        protected readonly uint NODEF = 0x2001A002;

        /// <summary>引数指定文字コード/CP1252/EUCを判別対象とする際のコンストラクタ</summary>
        /// <param name="CharCode">このインスタンスで判別対象とするデフォルト文字コード</param>
        /// <param name="EUC">EUCの文字コード</param>
        protected ReadJEnc(CharCode CharCode, CharCode EUC)
        {
            this.CharCode = CharCode;
            this.EUC = EUC;
        }
        /// <summary>CP1252系文字コードのみを判別対象とする際のコンストラクタ</summary>
        /// <param name="CP125X">CP1252系の判別対象デフォルト文字コード(CP1252以外を指定)</param>
        /// <param name="NODEF">0x80-0x9Fのうち文字コード未定義の箇所をbitで表現した値</param>
        protected ReadJEnc(CharCode CP125X, uint NODEF)
        {
            this.CharCode = CP125X;
            this.CP125X = CP125X;
            this.NODEF = NODEF;
        }
        /// <summary>オブジェクト文字列表現として、自動判別デフォルト文字コードの名前を返す</summary>
        public override string ToString()
        {
            return this.CharCode.Name;
        }
        #endregion

        // 文字コード判別メソッド================================================

        /// <summary>バイト配列を全走査し、文字コードを自動判別する</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="len">ファイルサイズ(バイト配列先頭からのデコード対象バイト数)</param>
        /// <param name="text">out 判別した文字コードにより取り出したテキスト文字列（非テキストならnull）</param>
        /// <returns>文字コード判別結果（非テキストならnull）</returns>
        internal CharCode GetEncoding(byte[] bytes, int len, out string text)
        {
            if (len == 0)
            {   // ■空ファイルにつき非テキストと判断
                text = null;
                return null;
            }

            byte b1 = bytes[0]; // 汎用バイトデータ読み取り変数初期化

            // 【1】7bit文字コードの範囲の走査(ASCII判定/非ASCII文字開始位置把握)、およびUTF16N/JISチェック
            JIS escapeSequenceChecker = null; // JISエスケープシーケンス評価
            int asciiEndPos = 0; // ループ変数、兼、非ASCII文字を初めて検出した位置
            while (b1 < DEL) // 非ASCII文字が出現したらループ脱出：b1にはあらかじめ読み込み済
            {
                if (b1 <= BINARY)
                {   // バイナリ文字検出：先頭２バイトでの検出ならUTF16Nの可能性をチェック、否ならバイナリ確定
                    CharCode ret = (asciiEndPos < 2 ? SeemsUTF16N(bytes, len) : null);
                    if (ret != null && (text = ret.GetString(bytes, len)) != null)
                    {   // UTF16Nデコード成功：非テキスト文字混入チェック
                        int i;
                        for (i = -3; i <= BINARY; i++)
                        {   // 0xFFFD,0xFFFE,0xFFFF,0～BINARY、DELが混入している場合は非テキストとみなす
                            if (text.IndexOf((char)i, 0, text.Length) != -1) { break; }
                        }
                        if (i > BINARY && text.IndexOf((char)DEL, 0, text.Length) == -1)
                        {   // ■UTF16N確定（非テキスト文字混入なし）
                            return ret;
                        }
                    }
                    text = null;
                    return null; // ■バイナリ確定
                }
                if (b1 == 0x1B)
                {   // エスケープシーケンス判定(エスケープコード内容を読み飛ばす)
                    if (escapeSequenceChecker == null) { escapeSequenceChecker = new JIS(bytes, len, asciiEndPos); }
                    asciiEndPos += escapeSequenceChecker.GetEncoding(asciiEndPos);
                }
                // 次の文字へ
                if ((++asciiEndPos) >= len)
                {   // 全文字チェック完了：非ASCII文字未検出、JISもしくはASCII
                    if (escapeSequenceChecker != null)
                    {   // エスケープシーケンスに基づく文字コードが取得できるか確認
                        CharCode ret = escapeSequenceChecker.GetEncoding(out text);
                        if (ret != null)
                        {   // ■エスケープシーケンスに基づく文字コードで確定
                            return ret;
                        }
                    }
                    else if (JIS.hasSOSI(bytes, len))
                    {   // SO,SIによるエスケープを検出した場合は、半角カナJISの可能性を判定
                        if (escapeSequenceChecker == null && (text = CharCode.JIS50222.GetString(bytes, len)) != null)
                        {   // ■エスケープシーケンスなし、半角カナSOSIのみを使用したJISで確定
                            return CharCode.JIS50222;
                        }
                    }
                    // ■ASCII確定（ただしデコード失敗時はバイナリ）
                    return ((text = CharCode.ASCII.GetString(bytes, len)) != null) ? CharCode.ASCII : null;
                }
                b1 = bytes[asciiEndPos];
            }

            // 【2】非ASCII文字を含む範囲の走査、CP1252系/UTF8/EUCチェック、JIS残チェック
            byte b2;
            int cp1252Score = 0; // いずれも、可能性が否定されたらint.MinValueが設定される
            int utfScore = 0;
            int eucScore = (this.EUC == null ? int.MinValue : 0); // EUC検出対象なしなら最初からチェック対象外
            int sjisScore = 0;
            bool existsEUC0x8F = false; // EUC補助漢字を見つけたらtrueを設定
            uint NODEF = this.NODEF; // パフォーマンス改善のためローカル変数におろす

            for (int cp1252Pos = asciiEndPos; cp1252Pos < len;) // cp1252Posの加算はロジック途中で随時実施
            {
                if (b1 == DEL)
                {   // 制御文字0x7F登場なら、ごくわずかなJISの可能性以外全消滅。JISの可能性を消しきれるか判定
                    cp1252Score = int.MinValue;
                    utfScore = int.MinValue;
                    eucScore = int.MinValue;
                    sjisScore = int.MinValue;
                    if (escapeSequenceChecker == null || (cp1252Pos++) >= len || (b1 = bytes[cp1252Pos]) < 0x21 || b1 >= DEL)
                    {   // JISエスケープ未出現 or ファイル末尾で2バイト目なし or 2バイト目が0x21-0x7E範囲外ならJISの可能性も否定
                        text = null;
                        return null; // ■バイナリ確定
                    }
                }
                // CP1252系チェック＆0x80以上の文字範囲の把握(notAsciiStartPos～cp1252Pos)。b1読込済
                int notAsciiStart = cp1252Pos;
                switch (cp1252Score)
                {
                    case int.MinValue: // CP1252系の可能性否定済み、非ASCII文字のスキップのみ実施
                        while (b1 > DEL && (++cp1252Pos) < len) { b1 = bytes[cp1252Pos]; }
                        break;
                    default: // CP1252系可能性あり、定義外文字混入チェック＆ポイント加算
                        while (b1 > DEL)
                        {   // そのバイト値が未定義コードかどうかチェック（bit表現値とかみあうようビットシフトして照合）
                            if (b1 <= 0x9F && (NODEF & (1u << (b1 % 32))) != 0)
                            {   // ビットが立ってる＝未定義コード、可能性消滅
                                cp1252Score = int.MinValue;
                                goto case int.MinValue; // 非ASCII文字スキップへ
                            }
                            if ((++cp1252Pos) >= len) { break; }
                            b1 = bytes[cp1252Pos];
                        }
                        // 非ASCII文字範囲終了、評価ポイント加算
                        // １バイトのみ出現時（SJISよりもCP1252系の可能性が高い）、SJIS漢字1文字目と同評価・SJISカナよりも高評価となるようポイント加算
                        if (cp1252Pos == notAsciiStart + 1) { cp1252Score += 2; }
                        else if (cp1252Pos == notAsciiStart + 2 && (b2 = bytes[cp1252Pos - 1]) >= 0xC0)
                        {   // ２バイトのみ出現時、ダイアクリティカルマーク（発音記号等）つきアルファベットなら配点補正
                            if (b2 == (b2 = bytes[cp1252Pos - 2])) { cp1252Score += 5; } // 同一文字重ねはかなり特徴的(SJISカナより可能性高)
                            else if (b2 >= 0xC0)
                            {   // 続きor直前のASCII文字がアルファベットっぽければ、SJISカナより可能性が高くなるよう補正                               
                                if (b1 > 0x40 || (notAsciiStart > 0 && bytes[notAsciiStart - 1] > 0x40)) { cp1252Score += 5; }
                                else { cp1252Score += 3; } // どちらでもなければ、EUCよりは可能性高とする
                            }
                            else { cp1252Score++; } // 否ならば低めの加算とする
                        }
                        else { cp1252Score++; } // いずれにも該当しなければやや低めの加算とする
                        break;
                }
                // notAsciiStartPos～cp1252Pos範囲のUTF8チェック
                if (utfScore >= 0)
                {
                    bool prevIsKanji = false;
                    for (int utfPos = notAsciiStart; utfPos < cp1252Pos; utfPos++)
                    {
                        b1 = bytes[utfPos]; // ※1バイト目は厳密にチェック、2バイト目以降は（デコード時にチェックアウトできる前提で）冗長なエンコードやサロゲート等を許容している
                        // 1バイト目・２バイト目(ともに0x80以上であることは確認済み)をチェック
                        if (b1 < 0xC2 || (++utfPos) >= cp1252Pos || bytes[utfPos] > 0xBF) { utfScore = int.MinValue; break; } // UTF8可能性消滅
                        else if (b1 < 0xE0)
                        {   // ２バイト文字OK（半角文字とみなして評価）
                            if (prevIsKanji == false) { utfScore += 6; } else { utfScore += 2; prevIsKanji = false; }
                        }
                        // 3バイト目(0x80以上であることは確認済み)をチェック
                        else if ((++utfPos) >= cp1252Pos || bytes[utfPos] > 0xBF) { utfScore = int.MinValue; break; } // UTF8可能性消滅
                        else if (b1 < 0xF0)
                        {   // ３バイト文字OK（全角文字とみなして評価）
                            if (prevIsKanji == true) { utfScore += 8; } else { utfScore += 4; prevIsKanji = true; }
                        }
                        // 4バイト目(0x80以上であることは確認済み)をチェック
                        else if ((++utfPos) >= cp1252Pos || bytes[utfPos] > 0xBF) { utfScore = int.MinValue; break; } // UTF8可能性消滅
                        else if (b1 < 0xF5)
                        {   // ４バイト文字OK（全角文字とみなして評価）
                            if (prevIsKanji == true) { utfScore += 12; } else { utfScore += 6; prevIsKanji = true; }
                        }
                        else { utfScore = int.MinValue; break; } // UTF8可能性消滅(0xF5以降はUTF8未定義)
                    }
                }
                // notAsciiStartPos～cp1252Pos範囲のEUCチェック ※EUCの文字コード範囲はEUC-JP/TW/CN/KRでほぼ共通
                if (eucScore >= 0)
                {   // 前の文字との連続性チェック用定数定義
                    const int PREV_KANA = 1; // 直前文字は半角カナ
                    const int PREV_ZENKAKU = 2; // 直前文字は全角
                    int prevChar = 0; // 前の文字はKANAでもZENKAKUでもない
                    for (int eucPos = notAsciiStart; eucPos < cp1252Pos; eucPos++)
                    {   // １バイト目(0xA1-0xFE,0x8E,0x8F)・２バイト目(１バイト目に応じ範囲が異なる)のチェック
                        b1 = bytes[eucPos];
                        if (b1 == 0xFF || (++eucPos) >= cp1252Pos) { eucScore = int.MinValue; break; } // EUC可能性消滅
                        b2 = bytes[eucPos];
                        if (b1 >= 0xA1)
                        {   // １バイト目＝全角文字指定、２バイト全角文字チェック
                            if (b2 < 0xA1 || b2 == 0xFF) { eucScore = int.MinValue; break; } // EUC可能性消滅
                            // ２バイト文字OK（全角）
                            if (prevChar == PREV_ZENKAKU) { eucScore += 5; } else { eucScore += 2; prevChar = PREV_ZENKAKU; }
                        }
                        else if (b1 == 0x8E)
                        {   // １バイト目＝EUC-JPのかな文字(orEUC-TWの４バイト文字)指定。２バイトの半角カナ文字チェック
                            if (b2 < 0xA1 || b2 > 0xDF) { eucScore = int.MinValue; break; } // EUC可能性消滅
                            // 検出OK,EUC文字数を加算（半角文字）
                            if (prevChar == PREV_KANA) { eucScore += 6; }
#if (!JPONLY)
                            // 漢字圏テキスト文字コードのうちEUC-TWに限り全角文字相当の扱いとする(0x8E,0xA2-0xB0,0xA1-0xFE,0xA1-0xFEの４バイト文字の判定に流用)
                            else if (this.EUC == CharCode.EUCTW) { if (prevChar == PREV_ZENKAKU) { eucScore += 6; } else { eucScore += 2; prevChar = PREV_ZENKAKU; } }
#endif
                            else { eucScore += 2; prevChar = PREV_KANA; }
                        }
                        else if (b1 == 0x8F
                            && b2 >= 0xA1 && b2 < 0xFF
                            && (++eucPos) < cp1252Pos
                            && (b2 = bytes[eucPos]) >= 0xA1 && b2 < 0xFF)
                        {   // 残る可能性は３バイト文字：検出OKならEUC文字数を加算（全角文字、補助漢字）
                            if (prevChar == PREV_ZENKAKU) { eucScore += 8; } else { eucScore += 3; prevChar = PREV_ZENKAKU; }
                            existsEUC0x8F = true; // ※補助漢字有
                        }
                        else { eucScore = int.MinValue; break; } // EUC可能性消滅
                    }
                }

                // ASCII文字範囲の読み飛ばし＆バイナリチェック＆JISチェック、b1に非ASCII文字出現位置のバイト値を格納
                while (cp1252Pos < len && (b1 = bytes[cp1252Pos]) < DEL)
                {
                    if (b1 <= BINARY)
                    {   // ■バイナリ確定
                        text = null;
                        return null;
                    }
                    if (b1 == 0x1B)
                    {   // エスケープシーケンス判定(エスケープコード内容を読み飛ばす)
                        if (escapeSequenceChecker == null) { escapeSequenceChecker = new JIS(bytes, len, cp1252Pos); }
                        cp1252Pos += escapeSequenceChecker.GetEncoding(cp1252Pos);
                    }
                    cp1252Pos++;
                }
            }

            // 【3】SJISなどの各国語文字コードチェック（非ASCII登場位置からチェック開始:ただしDEL検出時などは可能性なし）
            if (sjisScore != int.MinValue)
            {
                sjisScore = GetEncoding(bytes, asciiEndPos, len);
            }

            // 【4】ポイントに応じ文字コードを決定（実際にそのエンコーディングで読み出し成功すればOKとみなす）
            if (escapeSequenceChecker != null)
            {   // JIS系可能性高：エスケープシーケンスに基づく文字コードが取得できるか確認
                CharCode ret = escapeSequenceChecker.GetEncoding(out text);
                if (ret != null) { return ret; } // ■エスケープシーケンスに基づく文字コードで確定
            }
            if (eucScore > 0 && eucScore > sjisScore && eucScore > utfScore)
            {   // EUC可能性高
                if (cp1252Score > eucScore)
                {   // ただし可能性が高ければCP1252系を先にチェック
                    if ((text = this.CP125X.GetString(bytes, len)) != null) { return this.CP125X; } // ■CP1252系で読みこみ成功
                }
                if (existsEUC0x8F && (text = CharCode.EUCH.GetString(bytes, len)) != null) { return CharCode.EUCH; }// ■EUC補助漢字読みこみ成功
                if ((text = this.EUC.GetString(bytes, len)) != null) { return this.EUC; } // ■EUCで読みこみ成功
            }
            if (utfScore > 0 && utfScore >= sjisScore)
            {   // UTF可能性高
                if ((text = CharCode.UTF8N.GetString(bytes, len)) != null) { return CharCode.UTF8N; } // ■UTF-8Nで読みこみ成功
            }
            if (sjisScore >= 0)
            {   // SJISなどの各国語指定に合致したなら、そのコードでの読み出しを試みる(ただし可能性が高ければCP1252系を先にチェック)
                if (cp1252Score > sjisScore && (text = this.CP125X.GetString(bytes, len)) != null) { return this.CP125X; } // ■CP1252系で読みこみ成功
                if ((text = this.CharCode.GetString(bytes, len)) != null) { return this.CharCode; } // ■各国語文字コードで読みこみ成功
            }
            if (cp1252Score > 0)
            {   // CP1252系の可能性のみ残っているのでチェック
                if ((text = this.CP125X.GetString(bytes, len)) != null) { return this.CP125X; } // ■CP1252系で読みこみ成功
            }
            // ■いずれにも該当しなかった場合は、バイナリファイル扱いとする
            text = null;
            return null;
        }

        /// <summary>デフォルト文字コードの判定スコア算出（判定開始位置～ファイル末尾までの範囲を対象）</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="pos">判定開始位置(非ASCII文字コードが初めて登場した位置)</param>
        /// <param name="len">ファイルサイズ(バイト配列先頭からのデコード対象バイト数)</param>
        /// <returns>判定スコア算出結果</returns>
        protected virtual int GetEncoding(byte[] bytes, int pos, int len)
        {   // デフォルト(ANSI)の場合は常に評価なしとする。オーバーライドで算出ロジックを書き換えること
            return int.MinValue;
        }

        /// <summary>BOMなしUTF16の可能性があるか(先頭文字がASCIIか否かをもとに)判定</summary>
        /// <param name="bytes">判定対象のバイト配列</param>
        /// <param name="len">ファイルサイズ(バイト配列先頭からのデコード対象バイト数)</param>
        /// <returns>UTF16Nと思われる場合はその文字コード、否ならnull</returns>
        internal static CharCode SeemsUTF16N(byte[] bytes, int len)
        {
            if (len >= 2 && len % 2 == 0)
            {
                if (bytes[0] == 0x00)
                {
                    if (bytes[1] > BINARY && bytes[1] < DEL && (len == 2 || bytes[2] == 0))
                    {   // ▲UTF16BigEndianの可能性あり
                        return CharCode.UTF16BE;
                    }
                }
                else if (bytes[1] == 0x00)
                {
                    if (bytes[0] > BINARY && bytes[0] < DEL && (len == 2 || bytes[3] == 0))
                    {   // ▲UTF16LittleEndianの可能性あり
                        return CharCode.UTF16LE;
                    }
                }
            }
            return null; // UTF16Nの可能性はないと判断
        }

        #region JIS/SJIS判定処理各種--------------------------------------------
        /// <summary>
        /// JIS判定関連処理クラス
        /// </summary>
        private class JIS
        {
            // ※MS版JISでは、ISO/IEC 2022に準じていない0x7F以上のコード(外字や8bitカナなど)が混入してもOKである。
            //  このため、非ASCII文字登場後もエスケープシーケンスのチェックを行う。
            //  CP5022x(CP50220/CP50221/CP50222)の特異なコード体系については、
            //  http://sourceforge.jp/projects/legacy-encoding/lists/archive/talk-ja/2006-March/000002.html も参照

            // また、エスケープシーケンスを伴わない(文字コードSO/SIのみによる)半角カナ切り替えが含まれている場合も、
            // JISとみなしてデコードするほうが妥当であるため、ASCII文字のみ出現の場合にもSOSI有無チェックを行う。

            /// <summary>SO/SI文字出現チェック</summary>
            /// <param name="bytes">判定対象のバイト配列</param>
            /// <param name="len">ファイルサイズ(バイト配列先頭からのデコード対象バイト数)</param>
            /// <returns>出現していればtrue、否ならfalse</returns>
            internal static bool hasSOSI(byte[] bytes, int len)
            {
                return (Array.IndexOf<byte>(bytes, 0x0E, 0, len) >= 0 && Array.IndexOf<byte>(bytes, 0x0F, 0, len) >= 0);
            }

            #region JIS判定用のインスタンスメンバ／メソッド--------------------
            byte[] bytes;
            int len;
            /// <summary>JIS補助漢字エスケープシーケンス有無(有ならtrue)</summary>
            bool JISH = false;
            /// <summary>ISO-2022-KRエスケープシーケンス有無(有ならtrue)</summary>
            bool ISOKR = false;
            /// <summary>JIS評価値(JISとしてデコードすべきなら正値、否ならマイナス値)</summary>
            int c = 0;

            /// <summary>JISエスケープシーケンス判定オブジェクト初期化（およびISO-2022-KR判定）</summary>
            /// <param name="bytes">判定対象のバイト配列</param>
            /// <param name="len">ファイルサイズ(バイト配列先頭からのデコード対象バイト数)</param>
            /// <param name="pos">エスケープ(0x1B)出現位置</param>
            internal JIS(byte[] bytes, int len, int pos)
            {
                this.bytes = bytes;
                this.len = len;
                // ISO-2022-KR判定特殊ロジック。初出のエスケープシーケンスのみをもとに判定
                this.ISOKR = (pos >= 0 && pos < len - 4
                            && bytes[pos + 1] == '$'
                            && bytes[pos + 2] == ')'
                            && bytes[pos + 3] == 'C');
            }

            /// <summary>JISエスケープシーケンス妥当性チェック</summary>
            /// <param name="pos">エスケープ(0x1B)出現位置</param>
            /// <returns>エスケープシーケンスとして妥当ならpos加算値、そうでなければゼロ</returns>
            internal int GetEncoding(int pos)
            {
                if (pos + 2 < len)
                {   // ESC(0x1B):有効なエスケープシーケンスかチェック
                    c++; // 加算前提
                    switch (bytes[pos + 1])
                    {   // ２バイト目で分岐
                        case 0x24: // ESC$
                            switch (bytes[pos + 2])
                            {   // ３バイト目で分岐
                                case 0x40: // <ESC>$@ : JISエスケープ(78JIS)     - ISO-2022-JP
                                case 0x42: // <ESC>$B : JISエスケープ(83JIS)     - ISO-2022-JP
                                    return 2;
                                case 0x28: // <ESC>$(D: JISエスケープ(90補助漢字)- ISO-2022-JP-1 
                                    // ただしCP5022Xではデコードできないため別途CP20932を用いてデコードすること
                                    if (pos + 3 < len && bytes[pos + 3] == 0x44)
                                    {
                                        JISH = true;
                                        return 3;
                                    }
                                    break;
                            }
                            break;
                        case 0x28: // ESC(
                            switch (bytes[pos + 2])
                            {   // ３バイト目で分岐
                                case 0x42: // <ESC>(B : JISエスケープ(ASCII)    - ISO-2022-JP
                                case 0x48: // <ESC>(H : JISエスケープ(swedish)  - (ISO-2022-JP規定外)
                                case 0x49: // <ESC>(I : JISエスケープ(カタカナ) - (ISO-2022-JP規定外 http://tools.ietf.org/html/rfc1468 だがCP5022Xでデコード可能)
                                case 0x4A: // <ESC>(J : JISエスケープ(ローマ字) - ISO-2022-JP
                                    return 2;
                            }
                            break;
                            // 以下、MS版JIS(CP50220/CP50221/CP50222)でデコードできないエスケープシーケンスは、チェックしない。
                            //    <ESC>&@<ESC>$B  : JISエスケープ(90JIS)   - (ISO-2022-JP規定外。90JISで追加された「凜・熙」の２文字は、CP5022xでは更新シーケンス「<ESC>&@」を付けなくてもデコード可能)
                            //    <ESC>$(O : JISエスケープ(2000JIS#1)      - ISO-2022-JP-3
                            //    <ESC>$(P : JISエスケープ(2000JIS#2)      - ISO-2022-JP-3
                            //    <ESC>$(Q : JISエスケープ(2004JIS#1)      - ISO-2022-JP-4
                            // ほか、多言語対応のエスケープシーケンスもチェック外とする。
                            // エスケープシーケンスの一覧／うちMS版JISで有効なものは、
                            // http://ja.wikipedia.org/wiki/ISO/IEC_2022#.E5.BF.9C.E7.94.A8.E4.BE.8B
                            // http://referencesource.microsoft.com/#mscorlib/system/text/iso2022encoding.cs
                            // http://www.wdic.org/w/WDIC/Microsoft%20Windows%20Codepage%20%3A%2050221 なども参照
                    }
                }
                c -= 4; // 非JIS：ペナルティ的に評価値を減算する
                return 0;
            }

            /// <summary>JISと判定された場合の文字コードを確定する</summary>
            /// <param name="text">out 判別した文字コードにより取り出したテキスト文字列（非テキストならnull）</param>
            /// <returns>カナSO/SI有無やデコード不能箇所有無に応じたJIS文字コード種類</returns>
            internal CharCode GetEncoding(out string text)
            {
                byte[] bytes = this.bytes;
                int len = this.len;
                if (ISOKR && hasSOSI(bytes, len))
                {   // ■KSエスケープシーケンスあり、ISO-2022-KRで確定(半角カナJISではない)
                    text = CharCode.ISOKR.GetString(bytes, len);
                    return (text != null ? CharCode.ISOKR : null);
                }
                if (c <= 0)
                {   // JIS評価値がマイナスないしゼロならばJISではないと判断
                    text = null;
                    return null;
                }

                if (JISH)
                {   // 補助漢字のエスケープシーケンスあり、補助漢字を考慮してデコードする
                    text = CharCode.JISH.GetString(bytes, len);
                    if (text != null)
                    {   // ■デコード成功ならJISH確定
                        return CharCode.JISH;
                    }
                }
                else
                {   // 補助漢字なしJISとしてデコードする
                    text = CharCode.JIS.GetString(bytes, len);
                    if (text != null)
                    {   // ■デコード成功なら、カナのSO/SI切り替えがあればCP50222、なければCP50221とみなす。
                        // (なお、CP50221/CP50222どちらを使ってもデコード結果は同一となる)
                        return hasSOSI(bytes, len) ? CharCode.JIS50222 : CharCode.JIS;
                    }
                }
                // ■デコード不能箇所有、JIS破損ファイルとみなす
                text = CharCode.JISNG.GetString(bytes, len);
                return CharCode.JISNG;
            }
            #endregion
        }

        /// <summary>
        /// SJIS評価クラス
        /// </summary>
        private class SJIS : ReadJEnc
        {   /// <summary>日本語ShiftJIS・EUCJP・CP1252を判別対象とする際のコンストラクタ</summary>
            internal SJIS() : base(CharCode.SJIS, CharCode.EUC) { }

            protected override int GetEncoding(byte[] bytes, int pos, int len)
            {
                int score = 0; // 初期値ゼロからReadJEnc評価を始める
                byte b1 = bytes[pos];
                byte b2;
                while (pos < len)
                {   // 前の文字との連続性チェック用定数定義
                    const int PREV_KANA = 1; // 直前文字は半角カナ
                    const int PREV_ZENKAKU = 2; // 直前文字は全角
                    int prevChar = 0; // 前の文字はKANAでもZENKAKUでもない
                    while (b1 > DEL)
                    {
                        if (b1 >= 0xA1 && b1 <= 0xDF)
                        {   // １バイト半角カナ：OK（連続はEUCやCP1252よりも高配点とする）
                            if (prevChar == PREV_KANA) { score += 3; } else { score += 1; prevChar = PREV_KANA; }
                        }
                        // 非CP932チェック用定数(0x00000061,0xE0009800)：CP932ではデコード不能な未定義文字のビットを１
                        //        FEDC BA98 7654 3210         FEDC BA98 7654 3210
                        //        ---- ---- ---- ----         ---- ---- ---- ----
                        // (0x9#) 0000 0000 0000 0000  (0x8#) 0000 0000 0110 0001 - 80(A0判定でも流用):定義外、85,86:未使用(shift_jis2004などでは使用ありだがCP932ではデコード不能)
                        // (0xF#) 1110 0000 0000 0000  (0xE#) 1001 1000 0000 0000 - FD,FE,FF:定義外、EB,EC,EF:未使用 (F0-F9:外字は許容。HNXgrepなど外字不許容とする場合はビットを立てること)
                        else if (((b1 < 0xE0 ? 0x00000061 : 0xE0009800) & 1u << (b1 % 32)) != 0
                            || (++pos) >= len
                            || (b2 = bytes[pos]) < 0x40 || b2 > 0xFC)
                        {   // １バイト目がおかしい(SJIS定義外/未使用) or ２バイト目把握不能 or ２バイト目SJIS定義外
                            return int.MinValue; // 可能性消滅
                        }
                        else
                        {   // 全角文字数を加算(EUCよりは可能性を低めに見積もっておく)
                            if (prevChar == PREV_ZENKAKU) { score += 4; }
                            else
                            {   // （ただし唐突に0x98以降の第二水準文字が出てきた場合は、UTF-8/EUC/CP1252の可能性が高いのでプラス値なしとする）
                                score += (b1 > 0x98 ? 0 : 2);
                                prevChar = PREV_ZENKAKU;
                            }
                        }
                        // 各国語全コード共通：さらに次の文字へ
                        if ((++pos) >= len) { break; }
                        b1 = bytes[pos];
                    }
                    // 各国語全コード共通：半角文字の範囲を読み飛ばし
                    while (b1 <= DEL && (++pos) < len) { b1 = bytes[pos]; }
                }
                return score;
            }
        }
        #endregion

        #region 各国語文字コード評価クラス--------------------------------------
#if (!JPONLY)
        // 漢字圏テキスト文字コード各種（日本語判別以外使用しないなら定義省略可）
        /// <summary>
        /// BIG5TW評価クラス
        /// </summary>
        private class BIG5TW : ReadJEnc
        {   /// <summary>中国語繁体字・EUC-TW・CP1252を判別対象とする際のコンストラクタ</summary>
            internal BIG5TW() : base(CharCode.BIG5TW, CharCode.EUCTW) { }

            protected override int GetEncoding(byte[] bytes, int pos, int len)
            {   // ※配点未調整（EUC/UTF8はおそらく正しく識別可能、CP1252は一部誤判別する可能性あり）
                int score = 0; // 初期値ゼロからReadJEnc評価を始める
                byte b1 = bytes[pos];
                byte b2;
                while (pos < len)
                {   // 前の文字との連続性チェック用定数定義
                    const int PREV_ZENKAKU = 2; // 直前文字は全角
                    int prevChar = 0; // 前の文字はKANAでもZENKAKUでもない
                    while (b1 > DEL)
                    {
                        if (b1 < 0x81 || b1 > 0xF9 || b1 == 0xC7 || b1 == 0xC8
                            || (++pos) >= len
                            || (b2 = bytes[pos]) < 0x40 || (b2 < 0xA1 && b2 > 0x7E) || b2 > 0xFE)
                        {   // １バイト目がBig5定義外 or ２バイト目把握不能 or ２バイト目がBig5定義外
                            return int.MinValue; // 可能性消滅
                        }
                        else
                        {   // 全角文字数を加算(EUCよりは可能性を低めに見積もっておく)
                            if (prevChar == PREV_ZENKAKU) { score += 4; }
                            else
                            {   // ただし唐突に外字・次常用字(第二水準)が出てきた場合は配点を低めにする
                                score += (b1 < 0xA1 || b1 > 0xC8 ? 1 : 2);
                                prevChar = PREV_ZENKAKU;
                            }
                        }
                        // 各国語全コード共通：さらに次の文字へ
                        if ((++pos) >= len) { break; }
                        b1 = bytes[pos];
                    }
                    // 各国語全コード共通：半角文字の範囲を読み飛ばし
                    while (b1 <= DEL && (++pos) < len) { b1 = bytes[pos]; }
                }
                return score;
            }
        }

        /// <summary>
        /// GB18030評価クラス
        /// </summary>
        private class GB18030 : ReadJEnc
        {   /// <summary>中国語簡体字・CP1252を判別対象とする際のコンストラクタ</summary>
            internal GB18030() : base(CharCode.GB18030, CharCode.GB18030) { }

            protected override int GetEncoding(byte[] bytes, int pos, int len)
            {   // ※配点未調整（EUC/UTF8はおそらく正しく識別可能、CP1252は誤判別の可能性大）
                int score = 0; // 初期値ゼロからReadJEnc評価を始める
                byte b1 = bytes[pos];
                byte b2;
                while (pos < len)
                {   // 前の文字との連続性チェック用定数定義
                    const int PREV_ZENKAKU = 2; // 直前文字は全角
                    int prevChar = 0; // 前の文字はKANAでもZENKAKUでもない
                    while (b1 > DEL)
                    {
                        if (b1 < 0x81 || b1 > 0xFE || (++pos) >= len)
                        {   // １バイト目がおかしい(GB定義外) or ２バイト目把握不能
                            return int.MinValue; // 可能性消滅
                        }
                        else if ((b2 = bytes[pos]) >= 0x40 && b2 <= 0xFE)
                        {   // ２バイト文字：全角文字数を加算(EUCよりは可能性を低めに見積もっておく)
                            if (prevChar == PREV_ZENKAKU) { score += 4; } else { score += 2; prevChar = PREV_ZENKAKU; }
                        }
                        else if (b2 >= 0x30 && b2 <= 0x39
                            && (++pos) < len
                            && (b2 = bytes[pos]) >= 0x81 && b2 <= 0xFE
                            && (++pos) < len
                            && (b2 = bytes[pos]) >= 0x30 && b2 <= 0x39)
                        {   // ４バイト全角文字：全角文字数を加算（かなりGB18030の決め手になる特徴なので、ポイントを高めに）
                            if (prevChar == PREV_ZENKAKU) { score += 16; } else { score += 8; prevChar = PREV_ZENKAKU; }
                        }
                        else
                        {   // GB18030定義外
                            return int.MinValue; // 可能性消滅
                        }
                        // 各国語全コード共通：さらに次の文字へ
                        if ((++pos) >= len) { break; }
                        b1 = bytes[pos];
                    }
                    // 各国語全コード共通：半角文字の範囲を読み飛ばし
                    while (b1 <= DEL && (++pos) < len) { b1 = bytes[pos]; }
                }
                return score;
            }
        }

        /// <summary>
        /// UHCKR評価クラス
        /// </summary>
        private class UHCKR : ReadJEnc
        {   /// <summary>ハングルUHC・CP1252を判別対象とする際のコンストラクタ</summary>
            internal UHCKR() : base(CharCode.UHCKR, CharCode.UHCKR) { }

            protected override int GetEncoding(byte[] bytes, int pos, int len)
            {   // ※配点未調整（EUC/UTF8はおそらく正しく識別可能、CP1252は誤判別の可能性大）
                int score = 0; // 初期値ゼロからReadJEnc評価を始める
                byte b1 = bytes[pos];
                byte b2;
                while (pos < len)
                {   // 前の文字との連続性チェック用定数定義
                    const int PREV_ZENKAKU = 2; // 直前文字は全角
                    int prevChar = 0; // 前の文字はKANAでもZENKAKUでもない
                    while (b1 > DEL)
                    {
                        if (b1 < 0x81 || b1 > 0xFE
                            || (++pos) >= len
                            || (b2 = bytes[pos]) < 0x41 || (b2 < 0x61 && b2 > 0x5A) || (b2 < 0x81 && b2 > 0x7A) || b2 > 0xFE)
                        {   // １バイト目がおかしい(UHC定義外) or ２バイト目把握不能 or ２バイト目がUHC定義外
                            return int.MinValue; // 可能性消滅
                        }
                        else
                        {   // 全角文字数を加算(EUCよりは可能性を低めに見積もっておく)
                            if (prevChar == PREV_ZENKAKU) { score += 4; } else { score += 2; prevChar = PREV_ZENKAKU; }
                        }
                        // 各国語全コード共通：さらに次の文字へ
                        if ((++pos) >= len) { break; }
                        b1 = bytes[pos];
                    }
                    // 各国語全コード共通：半角文字の範囲を読み飛ばし
                    while (b1 <= DEL && (++pos) < len) { b1 = bytes[pos]; }
                }
                return score;
            }
        }

        // ISO8859などのテキスト文字コード自動判別（日本語判別以外使用しないなら定義省略可）
        /// <summary>
        /// １バイト文字コード汎用評価クラス
        /// タイ語ほかアルファベットと混在させない文字種の文字コード判別で共通仕様
        /// </summary>
        private class SBCS : ReadJEnc
        {
            /// <summary>その文字コード体系固有のコードの開始位置</summary>
            private int BOUND;
            /// <summary>0x80-0xFFの文字コード未定義の箇所をbitで表現した値</summary>
            private new readonly uint[] NODEF;

            /// <summary>引数指定文字コードとCP1252を判別対象とする際のコンストラクタ</summary>
            /// <param name="CharCode">このインスタンスで判別対象とするデフォルト文字コード</param>
            /// <param name="BOUND">通常文字／この文字コード固有の文字種範囲の境界</param>
            /// <param name="NODEF">
            /// 文字コード未定義の箇所をbitで表現した値
            /// <remarks>
            /// [0]=0x80-0x9Fの未定義箇所
            /// [1]=0xA0-0xBFの未定義箇所
            /// [2]=0xC0-0xDFの未定義箇所
            /// [3]=0xE0-0xFFの未定義箇所
            /// </remarks></param>
            internal SBCS(CharCode CharCode, int BOUND, params uint[] NODEF)
                : base(CharCode, null)
            {
                this.BOUND = BOUND;
                this.NODEF = NODEF;
            }

            protected override int GetEncoding(byte[] bytes, int pos, int len)
            {   // ※配点未調整（EUC/UTF8はおそらく正しく識別可能、CP1252はひょっとすると誤判別の可能性あり）
                int score = 0; // 初期値ゼロからReadJEnc評価を始める
                byte b1 = bytes[pos];

                // 配列から展開してローカル変数におろす
                uint undefined_0x80_0x9F = (NODEF.Length > 0 ? NODEF[0] : 0);
                uint undefined_0xA0_0xBF = (NODEF.Length > 1 ? NODEF[1] : 0);
                uint undefined_0xC0_0xDF = (NODEF.Length > 2 ? NODEF[2] : 0);
                uint undefined_0xE0_0xFF = (NODEF.Length > 3 ? NODEF[3] : 0);

                while (pos < len)
                {   // 前の文字との連続性チェック用定数定義
                    bool prevOwnChar = false; // 直前文字がその文字コード体系固有のものであればtrue、連続していれば配点高めとする
                    while (b1 > DEL)
                    {   // 未定義文字チェック
                        uint undefinedCheck = (b1 < 0xC0)
                            ? (b1 < 0xA0) ? undefined_0x80_0x9F : undefined_0xA0_0xBF
                            : (b1 < 0xE0) ? undefined_0xC0_0xDF : undefined_0xE0_0xFF;
                        // そのバイト値が未定義コードかどうかチェック（bit表現値とかみあうようビットシフトして照合）
                        if ((undefinedCheck & (1u << (b1 % 32))) != 0)
                        {   // ビットが立ってる＝未定義コード、可能性消滅
                            return int.MinValue;
                        }
                        if (b1 >= BOUND)
                        {   // その文字コード体系における固有文字種：連続はEUCやCP1252よりも高配点とする
                            if (prevOwnChar) { score += 3; } else { score += 1; prevOwnChar = true; }
                        }
                        else
                        {   // 通常のASCII文字と一緒に使われるような文字種：ASCIIとの連続をポイント高めに評価
                            if (prevOwnChar) { score += 1; prevOwnChar = false; } else { score += 2; }
                        }
                        // 各国語全コード共通：さらに次の文字へ
                        if ((++pos) >= len) { break; }
                        b1 = bytes[pos];
                    }
                    // 各国語全コード共通：半角文字の範囲を読み飛ばし
                    while (b1 <= DEL && (++pos) < len) { b1 = bytes[pos]; }
                }
                return score;
            }
        }
#endif
        #endregion
    }
}
