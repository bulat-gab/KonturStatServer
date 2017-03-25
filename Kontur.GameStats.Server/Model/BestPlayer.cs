namespace Kontur.GameStats.Server
{
    public class BestPlayer
    {
        public string name { get; set; }
        public double killToDeathRatio { get; set; }

        public BestPlayer(string name, double killToDeathRatio)
        {
            this.name = name;
            this.killToDeathRatio = killToDeathRatio;
        }
    }
}