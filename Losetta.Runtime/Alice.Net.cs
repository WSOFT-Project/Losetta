using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace AliceScript.NameSpaces
{
   static class Alice_Net_Initer
    {
        public static void Init()
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


            NameSpaceManerger.Add(space);
        }
        internal static WebClient wc;
    }
    class web_upload_dataFunc : FunctionBase
    {
        public web_upload_dataFunc()
        {
            this.Name = "web_upload_data";
            this.MinimumArgCounts = 2;
            this.Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
              e.Return=new Variable(  Alice_Net_Initer.wc.UploadData(e.Args[0].AsString(),e.Args[1].AsByteArray()));
            }else if (e.Args.Count >= 3)
            {
               e.Return=new Variable( Alice_Net_Initer.wc.UploadData(e.Args[0].AsString(), e.Args[1].AsString(),e.Args[2].AsByteArray()));
            }
        }
    }
    class web_upload_fileFunc : FunctionBase
    {
        public web_upload_fileFunc()
        {
            this.Name = "web_upload_file";
            this.MinimumArgCounts = 2;
            this.Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Net_Initer.wc.UploadFile(e.Args[0].AsString(), e.Args[1].AsString()));
            }
            else if (e.Args.Count >= 3)
            {
                e.Return = new Variable(Alice_Net_Initer.wc.UploadFile(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString()));
            }
        }
    }
    class web_upload_textFunc : FunctionBase
    {
        public web_upload_textFunc()
        {
            this.Name = "web_upload_text";
            this.MinimumArgCounts = 2;
            this.Run += Web_upload_data_Run;
        }

        private void Web_upload_data_Run(object sender, FunctionBaseEventArgs e)
        {
            if (e.Args.Count == 2)
            {
                e.Return = new Variable(Alice_Net_Initer.wc.UploadString(e.Args[0].AsString(), e.Args[1].AsString()));
            }
            else if (e.Args.Count >= 3)
            {
                e.Return = new Variable(Alice_Net_Initer.wc.UploadString(e.Args[0].AsString(), e.Args[1].AsString(), e.Args[2].AsString()));
            }
        }
    }
    class web_download_dataFunc : FunctionBase
    {
        public web_download_dataFunc()
        {
            this.Name = "web_download_data";
            this.MinimumArgCounts = 1;
            this.Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Net_Initer.wc.DownloadData(e.Args[0].AsString()));
        }
    }
    class web_download_fileFunc : FunctionBase
    {
        public web_download_fileFunc()
        {
            this.Name = "web_download_file";
            this.MinimumArgCounts = 2;
            this.Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            Alice_Net_Initer.wc.DownloadFile(e.Args[0].AsString(),e.Args[1].AsString());
        }
    }
    class web_download_textFunc : FunctionBase
    {
        public web_download_textFunc()
        {
            this.Name = "web_download_text";
            this.MinimumArgCounts = 1;
            this.Run += Web_download_data_Run;
        }

        private void Web_download_data_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(Alice_Net_Initer.wc.DownloadData(e.Args[0].AsString()));
        }
    }
    class web_htmldecodeFunc : FunctionBase
    {
        public web_htmldecodeFunc()
        {
            this.Name = "web_htmldecode";
            this.MinimumArgCounts = 1;
            this.Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.HtmlDecode(e.Args[0].AsString()));
        }
    }
    class web_htmlencodeFunc : FunctionBase
    {
        public web_htmlencodeFunc()
        {
            this.Name = "web_htmlencode";
            this.MinimumArgCounts = 1;
            this.Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.HtmlEncode(e.Args[0].AsString()));
        }
    }
    class web_urldecodeFunc : FunctionBase
    {
        public web_urldecodeFunc()
        {
            this.Name = "web_urldecode";
            this.MinimumArgCounts = 1;
            this.Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.UrlDecode(e.Args[0].AsString()));
        }
    }
    class web_urlencodeFunc : FunctionBase
    {
        public web_urlencodeFunc()
        {
            this.Name = "web_urlencode";
            this.MinimumArgCounts = 1;
            this.Run += Web_htmldecodeFunc_Run;
        }

        private void Web_htmldecodeFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            e.Return = new Variable(WebUtility.UrlEncode(e.Args[0].AsString()));
        }
    }
    class web_send_pingFunc : FunctionBase
    {
        public web_send_pingFunc()
        {
            FunctionName = "web_send_ping";
            MinimumArgCounts = 1;
            Run += W_pingFunc_Run;

        }

        private void W_pingFunc_Run(object sender, FunctionBaseEventArgs e)
        {
            //Pingオブジェクトの作成
            System.Net.NetworkInformation.Ping p =
                new System.Net.NetworkInformation.Ping();

            System.Net.NetworkInformation.PingReply reply = p.Send(e.Args[0].AsString(),Utils.GetSafeInt(e.Args,1,5000));

            //結果を取得
            if (reply.Status == System.Net.NetworkInformation.IPStatus.Success)
            {
                e.Return = Variable.True;
            }
            else
            {
                e.Return = Variable.False;
            }

            p.Dispose();
        }
    }
}
