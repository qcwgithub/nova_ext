using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NovaDeployC.Coroutine;

namespace NovaDeployC
{
    class Program
    {
        static int Main(string[] args)
        {
            Args kArgs = new Args(args);
            if (kArgs.Get("type") == "sendApp")
            {
                var app = SendApp.SendApp.CreateFromArgs(kArgs);
                return app.Start();
            }
            else if (kArgs.Get("type") == "recvApp")
            {
                var app = RecvApp.RecvApp.CreateFromArgs(kArgs);
                app.Start();
            }
            else
                throw new Exception("Unknown type="+kArgs.Get("type"));
            //Console.WriteLine("Len=" + args.Length);
            //foreach (var a in args)
            //    Console.WriteLine(a);
            //Console.ReadLine();

            //SendApp.ComputerConfig.SaveLocalSettings("localSettings.txt");
            return 0;
        }
    }
}
