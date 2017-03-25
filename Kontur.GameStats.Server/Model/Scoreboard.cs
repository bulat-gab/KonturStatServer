using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.GameStats.Server
{
    public class Scoreboard
    {
        public string name { get; set; }
        public int frags { get; set; }
        public int kills { get; set; }
        public int deaths { get; set; }

        public Scoreboard(string n, int f, int k, int d)
        {
            name = n;
            frags = f;
            kills = k;
            deaths = d;
        }      
    }
}
