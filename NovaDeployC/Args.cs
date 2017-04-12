using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaDeployC
{
    public class Args
    {
        public class KV { public string k, v; }
        public List<KV> list;
        public Args(string[] args)
        {
            list = new List<KV>();
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                string[] kv = arg.Split("=".ToCharArray());
                list.Add(new KV { k=kv[0], v=kv[1]});
            }
        }
        public string Get(string k)
        {
            foreach (var kv in list)
            {
                if (kv.k == k)
                    return kv.v;
            }
            throw new Exception("k not found:"+k);
        }
    }
}
