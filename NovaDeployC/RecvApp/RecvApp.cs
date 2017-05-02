using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NovaDeployC.RecvApp
{
    public class RecvApp
    {
        private RecvApp() { }

        Args args;
        public static RecvApp CreateFromArgs(Args args)
        {
            RecvApp app = new RecvApp();
            app.args = args;
            return app;
        }

        HttpRecv httpRecv;
        public void Start()
        {
            httpRecv = new HttpRecv();
            httpRecv.Start(HandleItem, args.Get("addr"));
            Loop();
        }

        public List<RecvItem> items = new List<RecvItem>();
        void CopyItems()
        {
            lock (httpRecv.items)
            {
                if (httpRecv.items.Count > 0)
                {
                    items.AddRange(httpRecv.items);
                    httpRecv.items.Clear();
                }
            }
        }

        string CopyFile(RecvItem item)
        {
            try
            {
                string action = item.headers["action"];
                Console.WriteLine(action);
                Console.WriteLine("{");
                string targetFile = item.headers["targetFile"];

                List<String> targetFiles = new List<string>();
                if (targetFile.Contains("{SERVERNAME}"))
                {
                    string[] targetServers = Tool.Base64ToObject<string[]>(item.headers["targetServers"]);
                    foreach (var serverName in targetServers)
                        targetFiles.Add(targetFile.Replace("{SERVERNAME}", serverName));
                }
                else
                {
                    targetFiles.Add(targetFile);
                }

                foreach (var f in targetFiles)
                {
                    Console.WriteLine("    writeFile "+f);
                    if (f.Contains('\\')||f.Contains('/'))
                        Directory.CreateDirectory(Path.GetDirectoryName(f));
                    File.WriteAllBytes(f, item.data);
                }

                Console.WriteLine("}");
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        string BuildBridgeProj(RecvItem item)
        {
            try
            {
                string action = item.headers["action"];
                string cmd = item.headers["cmd"].Replace('@', ' ');
                string args = item.headers["args"].Replace('@', ' ');


                Console.WriteLine(action);
                Console.WriteLine("{");

                Console.WriteLine("  {0}:{1} {2}", action, cmd, args);


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = cmd;
                psi.Arguments = args;
                psi.UseShellExecute = false;
                psi.CreateNoWindow = false;
                //psi.WorkingDirectory = Directory.GetCurrentDirectory();

                var pro = new Process();
                pro.StartInfo = psi;
                pro.Start();
                pro.WaitForExit();
                int ret = pro.ExitCode;
                pro.Close();

                Console.WriteLine("ExitCode=" + ret);

                NovaDeployC.BuildBridgeResult r = new BuildBridgeResult();
                r.ok = (ret == 0);
                r.buildLogBase64 = Convert.ToBase64String(File.ReadAllBytes("msbuild.log"));
                if (r.ok)
                    r.javascriptBase64 = Convert.ToBase64String(File.ReadAllBytes("..\\Bridge\\output\\BridgeProj.js"));
                else
                    r.javascriptBase64 = "";
                string re = JsonFx.Json.JsonWriter.Serialize(r);

                Console.WriteLine("}");
                return re;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ex.Message;
            }
        }

        string CopyDir(RecvItem item)
        {
            try
            {
                Console.WriteLine(item.headers["action"]);
                Console.WriteLine("{");
                string targetDir = item.headers["targetDir"];
                Console.WriteLine("    targetDir=" + targetDir);

                string zipPath = Tool.SaveToTempFile("", ".zip", item.data);

                List<String> targetDirs = new List<string>();
                if (targetDir.Contains("{SERVERNAME}"))
                {
                    string[] targetServers = Tool.Base64ToObject<string[]>(item.headers["targetServers"]);
                    foreach (var serverName in targetServers)
                        targetDirs.Add(targetDir.Replace("{SERVERNAME}", serverName));
                }
                else
                {
                    targetDirs.Add(targetDir);
                }

                foreach (var f in targetDirs)
                {
                    if (Directory.Exists(f))
                        Directory.Delete(f);
                    Directory.CreateDirectory(f);

                    Console.Write("    解压 " + zipPath + " -> " + f + " ... ");
                    Tool.Unzip(zipPath, f);
                    Console.WriteLine("完成");
                }

                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        //string ModifyText(RecvItem item)
        //{
        //    try
        //    {
        //        string action = item.headers["action"];
        //        Console.WriteLine(action);
        //        Console.WriteLine("{");
        //        string dir = item.headers["dir"];
        //        Console.WriteLine("    dir="+dir);
        //        string[] serverNames = Tool.Base64ToObject<string[]>(item.headers["serverNames"]);
        //        Console.WriteLine("    serverNames=" + string.Join(",",serverNames));
        //        foreach (var serverName in serverNames)
        //        {
        //            switch (action)
        //            {
        //                case "setReady":
        //                    {
        //                        File.WriteAllText(Path.Combine(dir, serverName, "in_g.txt"), "run serverready.txt");
        //                        Console.WriteLine("}");
        //                        return "OK";
        //                    }
        //                    break;
        //                default:
        //                    {
        //                        return "Unknown action=" + action;
        //                    }
        //                    break;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //    return "Failed";
        //}


        string HandleItem(RecvItem item)
        {
            string action = item.headers["action"];
            switch (action)
            {
                case "copyFile":
                    return CopyFile(item);
                case "copyDir":
                    return CopyDir(item);
                case "buildBridgeProj":
                    return BuildBridgeProj(item);
                //case "setReady":
                //    return ModifyText(item);
            }
            return "Unknown action=" + action;
        }

        void HandleItems()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                RecvItem item = items[i];
                HandleItem(item);
                items.RemoveAt(i);
            }
        }

        void Loop()
        {
            while (true)
            {
                //CopyItems();
                //HandleItems();
                System.Threading.Thread.Sleep(500);
            }
        }
    }
}
