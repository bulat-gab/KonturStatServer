using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server
{
    public class Server
    {

        public string endpoint { get; set; }
        public ServerInfo info { get; set; }
        

        public Server() { }

        public Server(string endpoint, ServerInfo info)
        {
            this.info = info;
            this.endpoint = endpoint;
        }

        public override string ToString()
        {
            return endpoint + " " + info;
        }
    }
}
