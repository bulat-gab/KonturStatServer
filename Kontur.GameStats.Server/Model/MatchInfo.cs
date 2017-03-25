using Fclp.Internals.Extensions;

namespace Kontur.GameStats.Server
{
    public class MatchInfo
    {

        public string map { get; set; }
        public string gameMode { get; set; }
        public int fragLimit { get; set; }

        public int timeLimit { get; set; }
        public double timeElapsed { get; set; }

        public Scoreboard[] scoreboard { get; set; }

        public MatchInfo(string map, string gameMode, int fragLimit, int timeLimit, double timeElapsed, Scoreboard[] scoreboard)
        {
            this.map = map;
            this.gameMode = gameMode;
            this.fragLimit = fragLimit;
            this.timeLimit = timeLimit;
            this.timeElapsed = timeElapsed;
            this.scoreboard = scoreboard;
        }


        //get ScoreBoard percent for player 'name' in this concreate match
        public double getPercent(string name)
        {
            double percent = 100;
            int playersBelowCurrent = 0, totalPlayers;
            totalPlayers = scoreboard.Length;

            if (totalPlayers == 1)
            {
                return 100;
            }

            for (int i = 0; i < totalPlayers; i++)
            {
                if (scoreboard[i].name.Equals(name))
                    playersBelowCurrent = totalPlayers -1 - i;
            }
            percent = playersBelowCurrent * 1.0 / (totalPlayers - 1) * 100;


            return percent;
        }
    }
}