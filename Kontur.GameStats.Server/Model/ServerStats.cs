namespace Kontur.GameStats.Server
{
    public class ServerStats
    {
        public int totalMatchesPlayed { get; set; }
        public int maximumMatchesPerDay { get; set; }
        public double averageMatchesPerDay { get; set; }
        public int maximumPopulation { get; set; }
        public double averagePopulation { get; set; }
        public string[] top5GameModes { get; set; }
        public string[] top5Maps { get; set; }

        public ServerStats(int totalMatchesPlayed, int maximumMatchesPerDay, 
            double averageMatchesPerDay, int maximumPopulation, double averagePopulation,
            string[] top5GameModes, string[] top5Maps)
        {
            this.totalMatchesPlayed = totalMatchesPlayed;
            this.maximumMatchesPerDay = maximumMatchesPerDay;
            this.averageMatchesPerDay = averageMatchesPerDay;
            this.maximumPopulation = maximumPopulation;
            this.averagePopulation = averagePopulation;
            this.top5GameModes = top5GameModes;
            this.top5Maps = top5Maps;
        }
    }
}