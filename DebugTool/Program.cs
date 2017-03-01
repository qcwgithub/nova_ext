using Microsoft.Diagnostics.Runtime.Utilities.Pdb;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTool
{
    class Program
    {
        static void ListServerProcesses()
        {
            var pros = Process.GetProcessesByName("Server");

            Console.WriteLine("|tid\t|file name");
            for (int i = 0; i < pros.Length; i++)
            {
                Console.WriteLine(string.Format("|{0}\t|{1}", pros[i].Id, pros[i].MainModule.FileName));
            }
            Console.WriteLine();
        }

        static void PrintProcessStacks(Process pro, string fileName)
        {
            Dictionary<int, string[]> result = StackTraceTool.GetStackTraces(pro);
            StringBuilder sb = new StringBuilder();
            foreach (var kv in result)
            {
                foreach (var st in kv.Value)
                {
                    sb.AppendFormat("{0} {1}", kv.Key, st);
                    sb.AppendLine();
                }
                if (kv.Value.Length>0)
                    sb.AppendLine();
            }
            
            try
            {
                string path = fileName.Replace('/', '\\');
                int index = path.LastIndexOf('\\');
                if (index >= 0)
                {
                    string dir = path.Substring(0, index);
                    Directory.CreateDirectory(dir);
                    //fileName = path.Substring(index + 1);
                }

                File.WriteAllText(fileName, sb.ToString());
                Console.WriteLine("保存成功");
            }
            catch
            {
                Console.WriteLine("写入文件失败");
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("用法");
            Console.WriteLine("l                列出所有服务器进程");
            Console.WriteLine("ps PID FILENAME  打印进程 PID 的所有线程调用栈到文件 FILENAME");
            Console.WriteLine("exit             退出");
        }

        static void Main(string[] args)
        {
            PrintUsage();
            while (true)
            {
                Console.WriteLine();
                Console.Write("输入：");
                string line = Console.ReadLine();
                if (line == "l")
                {
                    ListServerProcesses();
                    continue;
                }
               
                if (line=="exit")
                {
                    break;
                }

                if (line.StartsWith("ps"))
                {
                    int a = line.IndexOf(' ');
                    int b = line.IndexOf(' ', a + 1);
                    if (!(a >=0 && b>a))
                    {
                        Console.WriteLine("输入有误");
                        continue;
                    }
                    string spid = line.Substring(a+1,b-a-1);
                    int pid = 0;
                    if (!int.TryParse(spid, out pid))
                    {
                        Console.WriteLine("输入有误");
                        continue;
                    }
                    string fileName = line.Substring(b + 1, line.Length - b - 1);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        Console.WriteLine("输入有误");
                        continue;
                    }

                    PrintProcessStacks(Process.GetProcessById(pid), fileName);
                    continue;
                }

                Console.WriteLine("输入有误");
                continue;
            }
        }
    }
}
