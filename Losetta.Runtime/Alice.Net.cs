using AliceScript.Interop;
using System.Net;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Net
    {
        public static void Init()
        {
            NameSpaceManager.Add(typeof(WebFunctions));
        }
        internal static WebClient wc = new WebClient();
    }
    [AliceNameSpace(Name = "Alice.Net")]
    internal static class WebFunctions
    {
        #region Webアップロード
        public static byte[] Web_Upload_Data(string uri, byte[] data)
        {
            return Alice_Net.wc.UploadData(uri, data);
        }
        public static byte[] Web_Upload_File(string uri, string filename)
        {
            return Alice_Net.wc.UploadFile(uri, filename);
        }
        public static string Web_Upload_String(string uri, string text)
        {
            return Alice_Net.wc.UploadString(uri, text);
        }
        public static byte[] Web_Upload_Data(string uri, string? method, byte[] data)
        {
            return Alice_Net.wc.UploadData(uri, method, data);
        }
        public static byte[] Web_Upload_File(string uri, string? method, string filename)
        {
            return Alice_Net.wc.UploadFile(uri, method, filename);
        }
        public static string Web_Upload_String(string uri, string? method, string text)
        {
            return Alice_Net.wc.UploadString(uri, method, text);
        }
        #endregion
        #region Webダウンロード
        public static byte[] Web_Download_Data(string uri)
        {
            return Alice_Net.wc.DownloadData(uri);
        }
        public static void Web_Download_File(string uri, string filename)
        {
            Alice_Net.wc.DownloadFile(uri, filename);
        }
        public static string Web_Download_Text(string uri)
        {
            return Alice_Net.wc.DownloadString(uri);
        }
        #endregion

        #region エンコード・デコード
        public static string Web_UrlDecode(string? value)
        {
            return WebUtility.UrlDecode(value);
        }
        public static string Web_UrlEncode(string? value)
        {
            return WebUtility.UrlEncode(value);
        }
        public static string Web_HtmlDecode(string? value)
        {
            return WebUtility.HtmlDecode(value);
        }
        public static string Web_HtmlEncode(string? value)
        {
            return WebUtility.HtmlEncode(value);
        }
        #endregion
        public static bool Web_Send_Ping(string host)
        {
            using (var p = new System.Net.NetworkInformation.Ping())
            {
                var reply = p.Send(host);
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
        }
        public static bool Web_Send_Ping(string host, int timeout)
        {
            using (var p = new System.Net.NetworkInformation.Ping())
            {
                var reply = p.Send(host, timeout);
                return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
            }
        }
    }
}
