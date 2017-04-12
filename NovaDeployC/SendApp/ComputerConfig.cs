using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC.SendApp
{
    // 所有放置服务器的电脑的配置
    public class ComputerConfig
    {
        public static ComputerConfig CreateFromConfig(string fileName)
        {
            string text = File.ReadAllText(fileName);
            ComputerConfig cfg = JsonFx.Json.JsonReader.Deserialize<ComputerConfig>(text);
            cfg.Organize();
            return cfg;
        }
        public static ComputerConfig CreateFromDefaultConfig()
        {
            ComputerConfig cfg = CreateDefaultSettings();
            cfg.Organize();
            return cfg;
        }

        void Organize()
        {
            //serverId2Computer = new Dictionary<int, Computer>();
            //foreach (var computer in computers)
            //{
            //    foreach (var server in computer.servers)
            //    {
            //        foreach (var s in server)
            //        {
            //            serverId2Computer[s] = computer;
            //        }
            //    }
            //}

            serverName2Computer = new Dictionary<string, Computer>();
            foreach (var computer in computers)
            {
                foreach (var server in computer.serverNames)
                {
                    serverName2Computer[server] = computer;
                }
            }
        }

        static string CustomSeriaze(ComputerConfig cfg)
        {
            StringBuilder output = new StringBuilder();
            var setting = new JsonFx.Json.JsonWriterSettings();

            // 1
            //setting.PrettyPrint = true;
            // 2
            setting.Tab = "  ";
            setting.NewLine = "\r\n";

            using (JsonFx.Json.JsonWriter writer = new JsonFx.Json.JsonWriter(output, setting))
            {
                writer.Write(cfg);
            }
            return output.ToString();
        }

        public static void Save(ComputerConfig cfg, string fileName)
        {
            string text = CustomSeriaze(cfg);
            File.WriteAllText(fileName, text);
        }

        //public List<int> GetAllServerIDs()
        //{
        //    return serverId2Computer.Keys.ToList();
        //}

        //public Computer GetComputerByServerId(int serverId)
        //{
        //    return serverId2Computer[serverId];
        //}
        public Computer GetComputerByServerName(string serverName)
        {
            return serverName2Computer[serverName];
        }

        static ComputerConfig CreateDefaultSettings()
        {
            var cfg = new ComputerConfig();

            cfg.computers = new List<Computer>
            {
#region 默认设置
                new Computer
                {
                    name = "zbwdl.1",
                    url = "119.23.125.77",
                    cntr = true,
                    //servers = new List<int[]> {new int[]{1}, new int[]{2}},
                    serverNames = new List<string>{"srv1","srv2"},
                },
                new Computer
                {
                    name = "zbwdl.2",
                    url = "119.23.129.80",
                    cntr = false,
                    //servers = new List<int[]> {new int[]{3}, new int[]{4}},
                    serverNames = new List<string>{"srv3","srv4"},
                },
                new Computer
                {
                    name = "zbwdl.3",
                    url = "120.76.221.27",
                    cntr = false,
                    //servers = new List<int[]> {new int[]{5}, new int[]{6}},
                    serverNames = new List<string>{"srv5","srv6"},
                },
                new Computer
                {
                    name = "zbwdl.4",
                    url = "119.23.129.124",
                    cntr = false,
                    //servers = new List<int[]> {new int[]{7}, new int[]{8}, new int[]{9}, new int[]{10}},
                    serverNames = new List<string>{"srv7", "srv8", "srv9", "srv10"},
                },
                new Computer
                {
                    name = "zbwdl.5",
                    url = "119.23.130.211",
                    cntr = false,
                    //servers = new List<int[]> {new int[]{11}, new int[]{12}, new int[]{13}, new int[]{14}},
                    serverNames = new List<string>{"srv11", "srv12", "srv13", "srv14"},
                },
                new Computer
                {
                    name = "zbwdl.6",
                    url = "119.23.130.145",
                    cntr = false,
                    //servers = new List<int[]> {new int[]{15}, new int[]{16}, new int[]{17}, new int[]{18}},
                    serverNames = new List<string>{"srv15", "srv16", "srv17", "srv18"},
                },
#endregion
            };

            return cfg;
        }

        public static void SaveDefauleSettings(string fileName)
        {
            var cfg = CreateDefaultSettings();

            Save(cfg, fileName);
        }
        public static void SaveLocalSettings(string fileName)
        {
            var cfg = new ComputerConfig();
            cfg.computers = new List<Computer>
            {
#region 默认设置
                new Computer
                {
                    name = "localhost",
                    url = "127.0.0.1",
                    cntr = true,
                    //servers = new List<int[]> {new int[]{1}},
                    serverNames = new List<string>{"srv1"},
                },
#endregion
            };
            Save(cfg, fileName);
        }

        // 一台电脑
        public class Computer
        {
            public string name;
            // ip
            public string url;

            // 是否生成中心服文件夹
            public bool cntr;

            // 服务器ID有哪些
            // int[]里如果有多个表示合服
            //public List<int[]> servers;
            public List<string> serverNames;
        }

        // 所有的电脑
        public List<Computer> computers;
        //Dictionary<int, Computer> serverId2Computer;
        Dictionary<string, Computer> serverName2Computer;
    }
}
