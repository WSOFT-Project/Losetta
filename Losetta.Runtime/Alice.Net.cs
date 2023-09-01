using System.Net;

namespace AliceScript.NameSpaces
{
    public sealed class Alice_Net
    {
        public static void Init()
        {
            try
            {
                wc = new WebClient();

                NameSpace space = new NameSpace("Alice.Net");

                space.Add(new web_urldecodeFunc());
                space.Add(new web_urlencodeFunc());
                space.Add(new web_htmldecodeFunc());
                space.Add(new web_htmlencodeFunc());
                space.Add(new web_upload_dataFunc());
                space.Add(new web_upload_fileFunc());
                space.Add(new web_upload_textFunc());
                space.Add(new web_download_dataFunc());
                space.Add(new web_download_fileFunc());
                space.Add(new web_download_textFunc());
                space.Add(new web_send_pingFunc());


                NameSpaceManager.Add(space);
            }
            catch { }
        }
        internal static WebClient wc;
    }

    internal sealed class web_upload_dataFunc : FunctionBase
    {
        public web_upload_dataFunc()
        {
            Name = "web_upload_data";
            MinimumArgCounts = 2;
            Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Net.wc.UploadData(e.Args[0].AsString(), e.Args[1].AsByteArray()));
            }
            else if (e.Args.Count >= 3)
            {
                e.Return = new Variable(Alice_Net.wc.UploadData(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsByteArray()));
            }
        }
    }

    internal sealed class web_upload_fileFunc : FunctionBase
    {
        public web_upload_fileFunc()
        {
            Name = "web_upload_file";
            MinimumArgCounts = 2;
            Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Net.wc.UploadFile(e.Args[0].AsString(), e.Args[1].AsString()));
            }
            else if (e.Args.Count >= 3)
            {
                e.Return = new Variable(Alice_Net.wc.UploadFile(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString()));
            }
        }
    }

    internal sealed class web_upload_textFunc : FunctionBase
    {
        public web_upload_textFunc()
        {
            Name = "web_upload_text";
            MinimumArgCounts = 2;
            Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Net.wc.UploadString(e.Args[0].AsString(), e.Args[1].AsString()));
            }
            else if (e.Args.Count >= 3)
            {
                e.Return = new Variable(Alice_Net.wc.UploadString(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString()));
            }
        }
    }

    internal sealed class web_download_dataFunc : FunctionBase
    {
        public web_download_dataFunc()
        {
            Name = "web_download_data";
            MinimumArgCounts = 1;
            Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Net.wc.DownloadData(e.Args[0].AsString()));
        }
    }

    internal sealed class web_download_fileFunc : FunctionBase
    {
        public web_download_fileFunc()
        {
            Name = "web_download_file";
            MinimumArgCounts = 2;
            Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            Alice_Net.wc.DownloadFile(e.Args[0].AsString(), e.Args[1].AsString());
        }
    }

    internal sealed class web_download_textFunc : FunctionBase
    {
        public web_download_textFunc()
        {
            Name = "web_download_text";
            MinimumArgCounts = 1;
            Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Net.wc.DownloadString(e.Args[0].AsString()));
        }
    }

    internal sealed class web_htmldecodeFunc : FunctionBase
    {
        public web_htmldecodeFunc()
        {
            Name = "web_htmldecode";
            MinimumArgCounts = 1;
            Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.HtmlDecode(e.Args[0].AsString()));
        }
    }

    internal sealed class web_htmlencodeFunc : FunctionBase
    {
        public web_htmlencodeFunc()
        {
            Name = "web_htmlencode";
            MinimumArgCounts = 1;
            Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.HtmlEncode(e.Args[0].AsString()));
        }
    }

    internal sealed class web_urldecodeFunc : FunctionBase
    {
        public web_urldecodeFunc()
        {
            Name = "web_urldecode";
            MinimumArgCounts = 1;
            Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.UrlDecode(e.Args[0].AsString()));
        }
    }

    internal sealed class web_urlencodeFunc : FunctionBase
    {
        public web_urlencodeFunc()
        {
            Name = "web_urlencode";
            MinimumArgCounts = 1;
            Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.UrlEncode(e.Args[0].AsString()));
        }
    }

    internal sealed class web_send_pingFunc : FunctionBase
    {
        public web_send_pingFunc()
        {
            Name = "web_send_ping";
            MinimumArgCounts = 1;
            Run += W_pingFunc_Run;

        }

        private void W_pingFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            //Pingオブジェクトの作成
            System.Net.NetworkInformation.Ping p =
                new System.Net.NetworkInformation.Ping();

            System.Net.NetworkInformation.PingReply reply = p.Send(e.Args[0].AsString(), Utils.GetSafeInt(e.Args, 1, 5000));

            //結果を取得
            e.Return = reply.Status == System.Net.NetworkInformation.IPStatus.Success ? Variable.True : Variable.False;

            p.Dispose();
        }
    }
}
