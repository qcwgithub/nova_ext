using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NovaDeployC.RecvApp
{
    public class HttpRecv
    {
        HttpListener listener = new HttpListener();
       //public static string[] HttpListenerPrefixes = new string[] { "http://*:10000/" };

        Func<RecvItem, string> itemHandler;
        public void Start(Func<RecvItem, string> itemHandler, string addr)
        {
            this.itemHandler = itemHandler;
            //foreach (string prefix in HttpListenerPrefixes)
            {
                //Console.WriteLine("prefix " + prefix);
                listener.Prefixes.Add(addr);
            }

            listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            listener.Start();
            listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
        }

        public List<RecvItem> items = new List<RecvItem>();

        void ListenerCallback(IAsyncResult ar)
        {
            try
            {
                do
                {
                    HttpListener listener = (HttpListener)ar.AsyncState;
                    if (listener == null)
                    {
                        Console.WriteLine("listener is null");
                        break;
                    }

                    HttpListenerContext context = listener.EndGetContext(ar);
                    if (context == null)
                    {
                        Console.WriteLine("context is null");
                        break;
                    }

                    HttpListenerRequest request = context.Request;
                    if (request == null)
                    {
                        Console.WriteLine("request is null");
                        break;
                    }

                    HttpListenerResponse response = context.Response;
                    if (response == null)
                    {
                        Console.WriteLine("response is null");
                        break;
                    }

                    //if (!request.HasEntityBody)
                    //{
                    //    Console.WriteLine("No client data was sent with the request.");
                    //    break;
                    //}

                    System.IO.Stream body = request.InputStream;
                    if (body == null)
                    {
                        Console.WriteLine("body is null");
                        break;
                    }

                    RecvItem item = new RecvItem();
                    if (request.HasEntityBody)
                    {
                        System.IO.BinaryReader reader = new System.IO.BinaryReader(body);
                        item.data = reader.ReadBytes((int)request.ContentLength64);
                    }
                    else
                        item.data = null;

                    item.headers = new Dictionary<string, string>();
                    var headers = request.Headers;                    
                    foreach (var k in headers.AllKeys)
                    {
                        item.headers.Add(k, headers.Get(k));
                    }

                    string r = itemHandler(item);

                    var output = response.OutputStream;
                    var bytes = Encoding.UTF8.GetBytes(r);
                    output.Write(bytes, 0, bytes.Length);
                    output.Close();

                    //lock (items)
                    //{
                    //    items.Add(item);
                    //}
                    //string @do = queryS.Get("do");
                    //switch (@do)
                    //{
                    //    case "unzip":
                    //        {
                    //            string dir = queryS.Get("doParam");

                    //        }
                    //        break;
                    //}

                } while (false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception: " + ex.Message);
            }
            finally
            {
                // 处理一单紧接着下一单
                Console.WriteLine("Wait next command...");
                listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            }
        }
    }
}
