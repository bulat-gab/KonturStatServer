using System;

namespace Kontur.GameStats.Server
{
    public class Match
    {
        public string server { get; set; }
        public string timestamp { get; set; }

        
        public MatchInfo results { get; set; }
       

        public Match(string dt, string server, MatchInfo results)
        {
            this.timestamp = dt;
            this.results = results;
            this.server = server;
        }
    }
}