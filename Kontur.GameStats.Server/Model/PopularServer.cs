namespace Kontur.GameStats.Server
{
    public class PopularServer
    {
        public string endpoint { get; set; }
        public string name { get; set; }
        public double averageMatchesPerDay { get; set; }

        public PopularServer(string endpoint, string name, double averageMatchesPerDay)
        {
            this.endpoint = endpoint;
            this.name = name;
            this.averageMatchesPerDay = averageMatchesPerDay;
        }
    }
}