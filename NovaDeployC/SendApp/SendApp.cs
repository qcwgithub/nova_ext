using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.SendApp
{
    public class SendApp
    {
        private SendApp() { }

        ComputerConfig cfg;
        Args args;
        Coroutine.CoroutineManager cMgr;

        public static SendApp CreateFromArgs(Args args)
        {
            var app = new SendApp();
            app.args = args;
            app.cfg = NovaDeployC.SendApp.ComputerConfig.CreateFromConfig(args.Get("cfg"));
            //app.cfg = SendApp.ComputerConfig.CreateFromDefaultConfig();
            //string args2 = args[2];
            //{
            //    int _s;
            //    if (int.TryParse(args2, out _s))
            //        app.serverIds = new List<int> { _s };
            //    else if (args2 == "all")
            //        app.serverIds = app.cfg.GetAllServerIDs();
            //    else
            //    {
            //        var arr = args2.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //        app.serverIds = new List<int>();
            //        foreach (var s in arr)
            //            app.serverIds.Add(int.Parse(s));
            //    }
            //}

            app.cMgr = new Coroutine.CoroutineManager();
            return app;
        }


        Dictionary<ComputerConfig.Computer, List<string>> ParseTargetServers(string targetServers)
        {
            var dict = new Dictionary<ComputerConfig.Computer, List<string>>();
            if (targetServers == "all")
            {
                foreach (var computer in cfg.computers)
                    dict.Add(computer, computer.serverNames);
            }
            else
            {
                string[] serverNamesArr = targetServers.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var serverName in serverNamesArr)
                {
                    var computer = cfg.GetComputerByServerName(serverName);
                    if (computer == null)
                        throw new Exception();
                    if (!dict.ContainsKey(computer))
                        dict.Add(computer, new List<string>());
                    if (!dict[computer].Contains(serverName))
                        dict[computer].Add(serverName);
                }
            }
            return dict;
        }

        //#region doModifyText

        //IEnumerator doModifyText(string action)
        //{
        //    Console.WriteLine(action);
        //    Console.WriteLine("{");

        //    string dir = args.Get("dir");
        //    string serverNames = args.Get("serverNames");
        //    var dict = ParseTargetServers(serverNames);

        //    int step = 1;
        //    foreach (var kv in dict)
        //    {
        //        var computer = kv.Key;
        //        Console.Write(string.Format("    {0}) {1} ... ", step++, computer.url));

        //        Dictionary<string, string> headers = new Dictionary<string, string>();
        //        headers.Add("action", @action);
        //        headers.Add("dir", dir);
        //        headers.Add("serverNames", Tool.ObjectToBase64(kv.Value));
        //        HttpSend h = new HttpSend(computer.url, headers, new byte[0]);

        //        while (!h.IsEnd)
        //            yield return null;

        //        if (h.IsFail)
        //        {
        //            Console.WriteLine("失败, message: " + h.FailedMessage);
        //            failed = true;
        //        }
        //        else
        //        {
        //            if (!h.Result.StartsWith("OK"))
        //            {
        //                Console.WriteLine("失败, message: " + h.Result);
        //                failed = true;
        //            }
        //            else
        //            {
        //                Console.WriteLine(h.Result);
        //            }
        //        }

        //        if (failed)
        //        {
        //            Console.WriteLine("}");
        //            yield break;
        //        }
        //    }

        //    Console.WriteLine("}");
        //    yield break;
        //}
        //#endregion

        IEnumerator doCopyFile(string action)
        {
            Console.WriteLine(action);
            Console.WriteLine("{");

            byte[] data = File.ReadAllBytes(args.Get("file"));
            var dict = ParseTargetServers(args.Get("targetServers"));

            int step = 1;
            foreach (var kv in dict)
            {
                var computer = kv.Key;
                Console.Write(string.Format("    {0}) {1} ... ", step++, computer.url));

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("action", action);
                headers.Add("targetServers", Tool.ObjectToBase64(kv.Value));
                headers.Add("targetFile", args.Get("targetFile"));
                HttpSend h = new HttpSend(computer.url, headers, data);

                while (!h.IsEnd)
                    yield return null;

                if (h.IsFail)
                {
                    Console.WriteLine("失败, message: " + h.FailedMessage);
                    setFailed();
                }
                else
                {
                    if (!h.Result.StartsWith("OK"))
                    {
                        Console.WriteLine("失败, message: " + h.Result);
                        setFailed();
                    }
                    else
                    {
                        setSuccess();
                        Console.WriteLine(h.Result);
                    }
                }

                if (failed())
                {
                    Console.WriteLine("}");
                    yield break;
                }
            }

            Console.WriteLine("}");
            yield break;
        }
        IEnumerator doBuildBridgeProj(string action)
        {
            Console.WriteLine(action);
            Console.WriteLine("{");

            var dict = ParseTargetServers(args.Get("targetServers"));

            //// 2
            var cmd = args.Get("cmd");
            var _args = args.Get("args");

            foreach (var kv in dict)
            {
                var computer = kv.Key;
                Console.Write(string.Format("    {0}: 执行命令 {1} {2}... ", computer.url, cmd.Replace('@', ' '), _args.Replace('@', ' ')));

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("action", action);
                headers.Add("cmd", cmd);
                headers.Add("args", _args);
                HttpSend h = new HttpSend(computer.url, headers, new byte[0]);

                while (!h.IsEnd)
                    yield return null;

                if (h.IsFail)
                {
                    Console.WriteLine("失败, message: " + h.FailedMessage);
                    setFailed();
                }
                else
                {
                    //string reBase64 = h.Result;
                    //byte[] reBytes = Convert.FromBase64String(reBase64);
                    //string reUtf8 = Encoding.UTF8.GetString(reBytes);

                    //if (h.Result.Contains(": error"))

                    var r = JsonFx.Json.JsonReader.Deserialize<NovaDeployC.BuildBridgeResult>(h.Result);
                    if (r == null)
                    {
                        Console.WriteLine("失败, r==null");
                        setFailed();
                    }
                    else
                    {
                        if (!r.ok)
                        {
                            Console.WriteLine("失败, !r.ok");
                            setFailed();
                        }
                        byte[] logBytes = Convert.FromBase64String(r.buildLogBase64);
                        File.WriteAllBytes("msbuild.log", logBytes);
                        Console.WriteLine(File.ReadAllText("msbuild.log"));
                        if (r.ok)
                        {
                            File.WriteAllBytes("..\\Bridge\\output\\BridgeProj.js", Convert.FromBase64String(r.javascriptBase64));
                            setSuccess();
                        }
                    }
                }

                if (failed())
                {
                    Console.WriteLine("}");
                    yield break;
                }
            }

            Console.WriteLine("}");
            yield break;
        }

        #region doUploadDir
        IEnumerator doCopyDir(string action)
        {
            Console.WriteLine(action);
            Console.WriteLine("{");

            int step = 1;

            //// 1 压缩目录，得到数据

            string dir = args.Get("dir");
            if (!Directory.Exists(dir))
                throw new Exception("    directory not found: " + dir);
            string zipPath = Tool.tempDir + "\\1.zip";
            Console.Write(string.Format("    {0}) 压缩 {1} -> {2} ... ", step, dir, zipPath));
            yield return new Coroutine.TimeWaiter(300);
            Tool.ZipDir2File(dir, zipPath);
            byte[] data = File.ReadAllBytes(zipPath);
            Console.WriteLine("完成");


            var dict = ParseTargetServers(args.Get("targetServers"));

            //// 2

            step++;

            foreach (var kv in dict)
            {
                var computer = kv.Key;
                Console.Write(string.Format("    {0}) 上传至 {1} ... ", step, computer.url));

                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("action", action);
                headers.Add("dir", dir);
                headers.Add("targetServers", Tool.ObjectToBase64(kv.Value));
                headers.Add("targetDir", args.Get("targetDir"));
                HttpSend h = new HttpSend(computer.url, headers, data);

                while (!h.IsEnd)
                    yield return null;

                if (h.IsFail)
                {
                    Console.WriteLine("失败, message: " + h.FailedMessage);
                    setFailed();
                }
                else
                {
                    if (!h.Result.StartsWith("OK"))
                    {
                        Console.WriteLine("失败, message: " + h.Result);
                        setFailed();
                    }
                    else
                    {
                        setSuccess();
                        Console.WriteLine(h.Result);
                    }
                }

                if (failed())
                {
                    Console.WriteLine("}");
                    yield break;
                }
            }
            Console.WriteLine("}");
            yield break;
        }
        #endregion
        IEnumerator Executer()
        {
            string action = args.Get("action");

            switch (action)
            {
                case "copyFile":
                    {
                        yield return cMgr.Start(doCopyFile(action));
                    }
                    break;
                case "copyDir":
                    {
                        yield return cMgr.Start(doCopyDir(action));
                    }
                    break;

                case "buildBridgeProj":
                    {
                        yield return cMgr.Start(doBuildBridgeProj(action));
                    }
                    break;

                default:
                    throw new Exception("Unknown action="+action);
            }

            Console.WriteLine();
            Console.WriteLine(failed()?"failed":"finished");
        }

        void Loop()
        {
            while (true)
            {
                cMgr.OnTimeElapsed(30);
                System.Threading.Thread.Sleep(30);
                if (state != -1)
                    break;
            }
        }
        int state = -1;

        bool failed() { return state == 1; }
        void setFailed() { state = 1; }
        void setSuccess() { state = 0; }

        public int Start()
        {
            cMgr.Start(Executer());
            Loop();
            return state;
        }
    }
}
