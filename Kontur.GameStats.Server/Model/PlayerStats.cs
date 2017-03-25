using System;

namespace Kontur.GameStats.Server
{
    public class PlayerStats
    {
        public int totalMatchesPlayed { get; set; }
        public int totalMatchesWon { get; set; }
        public string favoriteServer { get; set; }
        public int uniqueServers { get; set; }
        public string favoriteGameMode { get; set; }

        public double averageScoreboardPercent { get; set; }
        public int maximumMatchesPerDay { get; set; }
        public double averageMatchesPerDay { get; set; }

        public string lastMatchPlayed { get; set; }

        public double killToDeathRatio { get; set; }

        public PlayerStats(int totalMatchesPlayed, int totalMatchesWon, string favoriteServer, 
            int uniqueServers, string favoriteGameMode, double averageScoreboardPercent, 
            int maximumMatchesPerDay, double averageMatchesPerDay, string lastMatchPlayed, double killToDeathRatio)
        {
            this.totalMatchesPlayed = totalMatchesPlayed;
            this.totalMatchesWon = totalMatchesWon;
            this.favoriteServer = favoriteServer;
            this.uniqueServers = uniqueServers;
            this.favoriteGameMode = favoriteGameMode;
            this.averageScoreboardPercent = averageScoreboardPercent;
            this.maximumMatchesPerDay = maximumMatchesPerDay;
            this.averageMatchesPerDay = averageMatchesPerDay;
            this.lastMatchPlayed = lastMatchPlayed;
            this.killToDeathRatio = killToDeathRatio;
        }
    }
}