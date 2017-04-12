using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NovaDeployC.RecvApp
{
    public class RecvItem
    {
        public Dictionary<string, string> headers;
        public byte[] data;
    }
}
