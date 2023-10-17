using AliceScript.Binding;
using System.Net;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Net
    {
        public static void Init()
        {
            Alice.RegisterFunctions<WebFunctions>();
        }
    }
    [AliceNameSpace(Name = "Alice.Net")]
    internal sealed class WebFunctions
    {
        #region Webアップロード
        public static byte[] Web_Upload_Data(string uri, byte[] data)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadData(uri, data);
        }
        public static byte[] Web_Upload_File(string uri, string filename)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadFile(uri, filename);
        }
        public static string Web_Upload_String(string uri, string text)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadString(uri, text);
        }
        public static byte[] Web_Upload_Data(string uri, string method, byte[] data)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadData(uri, method, data);
        }
        public static byte[] Web_Upload_File(string uri, string method, string filename)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadFile(uri, method, filename);
        }
        public static string Web_Upload_String(string uri, string method, string text)
        {
            WebClient ??= new WebClient();
            return WebClient.UploadString(uri, method, text);
        }
        #endregion
        #region Webダウンロード
        public static byte[] Web_Download_Data(string uri)
        {
            WebClient ??= new WebClient();
            return WebClient.DownloadData(uri);
        }
        public static void Web_Download_File(string uri, string filename)
        {
            WebClient ??= new WebClient();
            WebClient.DownloadFile(uri, filename);
        }
        public static string Web_Download_Text(string uri)
        {
            WebClient ??= new WebClient();
            return WebClient.DownloadString(uri);
        }
        #endregion

        #region DNS解決
        public static string Dns_GetHostName()
        {
            return Dns.GetHostName();
        }
        public static string Dns_GetHostName(string hostNameOrAddress)
        {
            var ip = Dns.GetHostEntry(hostNameOrAddress);
            return ip.HostName;
        }
        public static IEnumerable<string> Dns_GetIPAdress(string hostNameOrAddress)
        {
            var ip = Dns.GetHostEntry(hostNameOrAddress);
            return ip.AddressList.Select(x => x.ToString());
        }
        public static IEnumerable<string> Dns_GetAliases(string hostNameOrAddress)
        {
            var ip = Dns.GetHostEntry(hostNameOrAddress);
            return ip.Aliases;
        }
        #endregion

        #region エンコード・デコード
        public static string Web_UrlDecode(string value)
        {
            return WebUtility.UrlDecode(value);
        }
        public static string Web_UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value);
        }
        public static string Web_HtmlDecode(string value)
        {
            return WebUtility.HtmlDecode(value);
        }
        public static string Web_HtmlEncode(string value)
        {
            return WebUtility.HtmlEncode(value);
        }
        #endregion
        public static bool Web_Send_Ping(string host)
        {
            using var p = new System.Net.NetworkInformation.Ping();
            var reply = p.Send(host);
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
        public static bool Web_Send_Ping(string host, int timeout)
        {
            using var p = new System.Net.NetworkInformation.Ping();
            var reply = p.Send(host, timeout);
            return reply.Status == System.Net.NetworkInformation.IPStatus.Success;
        }
        private static WebClient WebClient { get; set; }
    }
}
