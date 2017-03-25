using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server
{
    public class ServerInfo
    {
        
        
        public string name { get; set; }

        public string[] gameModes { get; set; }

        public ServerInfo(string name, string[] modes)
        {
            this.name = name;
            this.gameModes = modes;
        }

        public override string ToString()
        {
            return "name: " + name + " modes: " + String.Join(", ", gameModes);
        }
    }
}
