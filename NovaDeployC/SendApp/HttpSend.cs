using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace NovaDeployC.SendApp
{
    public class HttpSend
    {
        public HttpSend(string url, Dictionary<string, string> messages, byte[] postData)
        {
            Send(url, messages, postData);
        }

        WebClient client;
        int state = 0;
        object m_lock = new object();
        string failedMsg = null;
        string result;

        public bool IsEnd { get { return state != 0; } }
        public bool IsFail 
        {
            get
            {
                lock (m_lock)
                {
                    return state == 2;
                }
            }
        }
        public string FailedMessage
        {
            get
            {
                lock (m_lock)
                {
                    return failedMsg;
                }
            }
        }
        public bool IsSuc 
        { 
            get
            {
                lock (m_lock)
                {
                    return state == 1;
                }
            }
        }
        public string Result
        {
            get
            {
                lock (m_lock)
                {
                    return result;
                }
            }
        }

        void SetFailed(string msg)
        {
            lock (m_lock)
            {
                state = 2;
                failedMsg = msg;
            }
        }

        void SetSuc(string result)
        {
            lock (m_lock)
            {
                state = 1;
                this.result = result;
            }
        }

        void Send(string url, Dictionary<string, string> messages, byte[] postData)
        {
            this.client = new WebClient();
            // http://stackoverflow.com/questions/4932541/c-sharp-webclient-acting-slow-the-first-time
            this.client.Proxy = null;

            this.client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
            this.client.Headers.Add("ContentLength", postData.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // 在 Headers 中加入签名
            foreach (var kv in messages)
            {
                this.client.Headers.Add(kv.Key, kv.Value);
            }

            this.client.UploadDataCompleted += new UploadDataCompletedEventHandler(PostCallback);
            this.client.UploadDataAsync(new Uri(url), "POST", postData);
        }

        void PostCallback(object sender, UploadDataCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                SetFailed("Canceled");
                return;
            }

            if (e.Error != null)
            {
                SetFailed(e.Error.ToString());
                return;
            }

            string str = Encoding.UTF8.GetString(e.Result);
            SetSuc(str);
        }
    }
}
