using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Kontur.GameStats.Server;

namespace Kontur.GameStats.Server
{
    class DataBase
    {
        private const string CONNECTION_PARAMS = "Data Source=MyDB.db;Version=3;New=False;Compress=True;";    

        public void PutServerInfo(Server server)
        {
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                string sql = $"insert or replace into Servers values('{server.endpoint}', '{server.info.name}', '{String.Join(",", server.info.gameModes)}');";
                var cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }         
        }
        
        public ServerInfo GetServerInfo(string url)
        {         
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                ServerInfo serverInfo;             
                string sql = $"select name, gameModes from Servers where endpoint = '{url}';";                                                 

                var cmd = new SQLiteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {                    
                    string name = reader["name"].ToString();
                    string[] modes = reader["gameModes"].ToString().Trim().Split(',');
                    serverInfo = new ServerInfo(name, modes);
                }
                else
                {
                    serverInfo = null;
                }

                return serverInfo;
            }                                
        }

        public bool PutMatchInfo(Match m)
        {            
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                string sqlCheck = $"select count(*) from Servers where endpoint = '{m.server}'";
                var cmd = new SQLiteCommand(sqlCheck, conn);
                int exists = int.Parse(cmd.ExecuteScalar().ToString());

                // no such server in database
                if (exists == 0)
                {
                    return false;
                }

                string sqlInsertMatches = $"insert into Matches values ('{m.server}', '{m.timestamp}', " +
                                   $"'{m.results.map}', '{m.results.gameMode}', '{m.results.fragLimit}', " +
                                   $"'{m.results.timeLimit}', '{m.results.timeElapsed}');";

                cmd = new SQLiteCommand(sqlInsertMatches, conn);
                cmd.ExecuteNonQuery();

                string sql_prefix = "insert into Scoreboard values";
                StringBuilder sqlInsertScoreboard = new StringBuilder();

                   
                foreach (var tuple in m.results.scoreboard)
                {
                    double percent = m.results.getPercent(tuple.name);
                    sqlInsertScoreboard.Append($"('{m.server}', '{m.timestamp}', ");
                    sqlInsertScoreboard.Append($"'{tuple.name}', '{tuple.frags}', '{tuple.kills}', '{tuple.deaths}', '{percent}'),");

                }
                sqlInsertScoreboard.Remove(sqlInsertScoreboard.Length - 1, 1);
                sqlInsertScoreboard.Append(';');

                cmd = new SQLiteCommand(sql_prefix + sqlInsertScoreboard, conn);
                cmd.ExecuteNonQuery();

                return true;
            }
        }

        public MatchInfo GetMatchInfo(string url, string dt)
        {
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();

                MatchInfo info = null;
                List<Scoreboard> scoreboardList = new List<Scoreboard>();

                string sql = $"select * from Matches where endpoint = '{url}' AND timeStamp = '{dt}'";
                var cmd = new SQLiteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string sqlGetScoreboard =
                        $"select * from Scoreboard where endpoint = '{url}' AND timeStamp = '{dt}'";
                    cmd = new SQLiteCommand(sqlGetScoreboard, conn);
                    var scReader = cmd.ExecuteReader();
                    while (scReader.Read())
                    {
                        string n = scReader["name"].ToString();
                        int f = int.Parse(scReader["frags"].ToString());
                        int k = int.Parse(scReader["kills"].ToString());
                        int d = int.Parse(scReader["deaths"].ToString());

                        scoreboardList.Add(new Scoreboard(n, f, k, d));
                    }
                    Console.WriteLine(scoreboardList.Count);

                    string map = reader["map"].ToString();
                    string mode = reader["gameMode"].ToString();
                    int fragLim = int.Parse(reader["fragLimit"].ToString());
                    int timeLim = int.Parse(reader["timeLimit"].ToString());
                    double timeElapsed = double.Parse(reader["timeElapsed"].ToString());


                    info = new MatchInfo(map, mode, fragLim, timeLim, timeElapsed, scoreboardList.ToArray());

                }
                return info;
            }
        }

        public List<Server> GetAllServersInfo()
        {
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                
                string sql = $"select * from Servers ;";
                var cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = cmd.ExecuteReader();

                List<Server> list = new List<Server>();
                              
                while (reader.Read())
                {
                    string addr = reader["endpoint"].ToString();
                    string name = reader["name"].ToString();
                    string[] modes = reader["gameModes"].ToString().Trim().Split(',');
                    Server server = new Server(addr, new ServerInfo(name, modes));
                  
                    list.Add(server);
                }

                return list;
            }                    
        }
        
        public ServerStats GetServerStats(string url)
        {
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                

                string sqlCheckServerExists = $"select count(*) from Servers where endpoint = '{url}'";
                var cmd = new SQLiteCommand(sqlCheckServerExists, conn);
                if(0 == int.Parse(cmd.ExecuteScalar().ToString()))
                {
                    // no such server
                    return null;
                }

                int totalMatches, maxPerDay, maxPopulation;
                double avgPerDay, avgPopulation;
                List<string> top5GameModes = new List<string>();
                List<string> top5Maps = new List<string>();




                string sqlTotalMatchesPlayed = $"select count(*) from Matches where endpoint = '{url}'";
                cmd = new SQLiteCommand(sqlTotalMatchesPlayed, conn);
                totalMatches =  int.Parse(cmd.ExecuteScalar().ToString());




                string sqlMaxPerDay = $"select count(*) from Matches where Matches.endpoint = '{url}'" +
                                      $" group by date(Matches.timeStamp) " +
                                      $"order by(count(*)) DESC " +
                                      $"Limit 1";
                cmd = new SQLiteCommand(sqlMaxPerDay, conn);
                maxPerDay = int.Parse(cmd.ExecuteScalar().ToString());




                string sqlAvgPerDay = $"Select avg(n) from " +
                                      $"(select count(*) as n" +
                                      $" from Matches where Matches.endpoint = '{url}'" +
                                      $" group by date(Matches.timeStamp) " +
                                      $"order by(count(*)) DESC)";

                cmd = new SQLiteCommand(sqlAvgPerDay, conn);
                avgPerDay = double.Parse(cmd.ExecuteScalar().ToString());





                string sqlMaxPopulation = $"select count(*) from Scoreboard where endpoint = '{url}' " +
                                          $"group by timeStamp " +
                                          $"order by count(*) desc " +
                                          $"Limit 1";

                cmd = new SQLiteCommand(sqlMaxPopulation, conn);
                maxPopulation = int.Parse(cmd.ExecuteScalar().ToString());




                string sqlAvgPopulation = $"select avg(n) from " +
                                          $"(select count(*) as n " +
                                          $"from Scoreboard where endpoint = '{url}' " +
                                          $"group by timeStamp " +
                                          $"order by count(*) desc)";

                cmd = new SQLiteCommand(sqlAvgPopulation, conn);
                avgPopulation = double.Parse(cmd.ExecuteScalar().ToString());



                string sqlTop5Modes = $"select gameMode from Matches where endpoint = '{url}' " +
                                      $"group by gameMode " +
                                      $"order by count(*) DESC " +
                                      $"Limit 5";

                cmd = new SQLiteCommand(sqlTop5Modes, conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    top5GameModes.Add(reader["gameMode"].ToString());
                }




                string sqlTop5Maps = $"select map from Matches where endpoint = '{url}' " +
                                     $"group by map " +
                                     $"order by count(*) DESC " +
                                     $"Limit 5";
                cmd = new SQLiteCommand(sqlTop5Maps, conn);
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    top5Maps.Add(reader["map"].ToString());
                }

                return new ServerStats(totalMatches, maxPerDay, avgPerDay, maxPopulation, avgPopulation, top5GameModes.ToArray(), top5Maps.ToArray());
            }
        }

        public PlayerStats GetPlayerStats(string name)
        {
            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();
                string sql = $"select count(*) from Scoreboard where name = '{name}' COLLATE NOCASE";
                var cmd = new SQLiteCommand(sql, conn);
                int totalMatches = int.Parse(cmd.ExecuteScalar().ToString());
                if (0 == totalMatches)
                {
                    /*  no such player/no matches found */
                    return null;
                }


                int totalWon, uniqueServers, maxPerDay;
                double avgScoreboard, avgPerDay, KD;
                string favServer, favMode;
                string lastMatch;



                string sqlTotalWon = $"select count(*) from Scoreboard s, Matches m" +
                                     $" where s.name = '{name}' COLLATE NOCASE AND s.endpoint = m.endpoint" +
                                     $" AND s.timeStamp = m.timeStamp AND s.frags = m.fragLimit";
                cmd = new SQLiteCommand(sqlTotalWon, conn);
                totalWon = int.Parse(cmd.ExecuteScalar().ToString());





                string sqlFavServer = $"select endpoint from Scoreboard s where s.name = '{name}' COLLATE NOCASE " +
                                      $"group by endpoint " +
                                      $"order by count(*) DESC " +
                                      $"Limit 1";
                cmd = new SQLiteCommand(sqlFavServer, conn);
                favServer = cmd.ExecuteScalar().ToString();


                string sqlUniqueServers = $"select count(distinct endpoint) from Scoreboard s where s.name = '{name}' COLLATE NOCASE";
                cmd = new SQLiteCommand(sqlUniqueServers, conn);
                uniqueServers = int.Parse(cmd.ExecuteScalar().ToString()) ;



                string sqlFavMode = $"select Matches.gameMode from Scoreboard inner join Matches " +
                                    $"on Scoreboard.endpoint = Matches.endpoint and Scoreboard.timeStamp = Matches.timeStamp " +
                                    $"where Scoreboard.name = '{name}' COLLATE NOCASE " +
                                    $"group by Matches.gameMode " +
                                    $"order by count(*) DESC " +
                                    $"Limit 1";
                cmd = new SQLiteCommand(sqlFavMode, conn);
                favMode = cmd.ExecuteScalar().ToString();




                string sqlAvgScoreboard = $"select avg(percent) from Scoreboard where name = '{name}' COLLATE NOCASE";
                cmd = new SQLiteCommand(sqlAvgScoreboard, conn);
                avgScoreboard = double.Parse(cmd.ExecuteScalar().ToString());
                


                string sqlMaxPerDay = $"select count(*) from Scoreboard s where  s.name = '{name}' COLLATE NOCASE " +                                    
                                      $"group by date(s.timeStamp) " +
                                      $"order by count(*) DESC " +
                                      $"Limit 1";
                cmd = new SQLiteCommand(sqlMaxPerDay, conn);
                maxPerDay = int.Parse(cmd.ExecuteScalar().ToString());



                string sqlAvgPerDay = $"select avg(n) from ( " +
                                            $"select count(*) as n " +
                                            $"from Scoreboard s where s.name = '{name}' COLLATE NOCASE " +
                                            $"group by date(s.timeStamp) " +
                                            $"order by count(*) DESC )";
                cmd = new SQLiteCommand(sqlAvgPerDay, conn);
                avgPerDay = double.Parse(cmd.ExecuteScalar().ToString());




                string sqlLastMatch = $"select max(timeStamp) from Scoreboard where name = '{name}' COLLATE NOCASE";
                cmd = new SQLiteCommand(sqlLastMatch, conn);
                lastMatch = cmd.ExecuteScalar().ToString();


                string sqlKD = $"select sum(kills)*1.0 / sum(deaths) " +
                               $"from Scoreboard where name = '{name}' COLLATE NOCASE";
                cmd = new SQLiteCommand(sqlKD, conn);
                KD = double.Parse(cmd.ExecuteScalar().ToString());

                return  new PlayerStats(totalMatches, totalWon, favServer, uniqueServers, favMode, avgScoreboard, maxPerDay, avgPerDay, lastMatch, KD);
            }
        }

        public List<Match> GetRecentMatches(int count)
        {
            List<Match> listMatches = new List<Match>();

            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();



                string sqlMatches = $"select * from Matches m " +
                                    $"order by datetime(m.timeStamp) DESC " +
                                    $"Limit {count};";

                var cmdMatches = new SQLiteCommand(sqlMatches, conn);
                var readerMatches = cmdMatches.ExecuteReader();

                while (readerMatches.Read())
                {
                    string server = readerMatches["endpoint"].ToString();
                    string timeStamp = readerMatches["timeStamp"].ToString();

                    DateTime dt = DateTime.Parse(timeStamp);
                    timeStamp = dt.ToString("s") + "Z";
                    

                    string map = readerMatches["map"].ToString();
                    string gameMode = readerMatches["gameMode"].ToString();
                    int fragLim = int.Parse(readerMatches["fragLimit"].ToString());
                    int timeLim = int.Parse(readerMatches["timeLimit"].ToString());
                    double timeElapsed = double.Parse(readerMatches["timeElapsed"].ToString());


                    string sqlScoreboard = $"select name, frags, kills, deaths from Scoreboard s " +
                                           $"where s.endpoint = '{server}' and " +
                                           $"s.timeStamp = '{timeStamp}'";

                    var cmd = new SQLiteCommand(sqlScoreboard, conn);
                    var readerSc = cmd.ExecuteReader();
                    List<Scoreboard> scoreboards = new List<Scoreboard>();
                    while (readerSc.Read())
                    {
                        scoreboards.Add(new Scoreboard(readerSc["name"].ToString(), 
                                                        int.Parse(readerSc["frags"].ToString()),
                                                        int.Parse(readerSc["kills"].ToString()), 
                                                        int.Parse(readerSc["deaths"].ToString())));
                    }

                    listMatches.Add(new Match(timeStamp, server, 
                        new MatchInfo(map, gameMode, fragLim, timeLim, timeElapsed, scoreboards.ToArray())));

                }

                return listMatches;
            }
          
        }

        public List<BestPlayer> GetBestPlayers(int count)
        {
            List<BestPlayer> list = new List<BestPlayer>();

            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();

                string sql = $"select name, sum(kills)*1.0 / sum(deaths) as KD " +
                             $"from Scoreboard s " +
                             $"where name IN (select name from Scoreboard s " +
                                            $"group by name " +
                                            $"having sum(deaths) > 0 and count(*) > 1) " +
                             $"group by name " +
                             $"order by KD DESC " +
                             $"Limit {count}";
                var cmd = new SQLiteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new BestPlayer(reader["name"].ToString(), double.Parse(reader["KD"].ToString())));
                }

                return list;
            }

           
        }

        public List<PopularServer> GetPopularServers(int count)
        {
            List<PopularServer> list = new List<PopularServer>();

            using (var conn = new SQLiteConnection(CONNECTION_PARAMS))
            {
                conn.Open();

                string sql = $"select endpoint as endp, name, avg(n) as avgPerDay " +
                             $"from (select *, count(*) as n " +
                                          $"from Matches m inner join Servers s " +
                                          $"on m.endpoint = s.endpoint " +
                                          $" group by m.endpoint, date(m.timeStamp) ) " +
                             $"group by endpoint " +
                             $"order by avgPerDay DESC";
                var cmd = new SQLiteCommand(sql, conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(new PopularServer(reader["endp"].ToString(), reader["name"].ToString(), 
                        double.Parse(reader["avgPerDay"].ToString())));
                }
            }

            return list;
        }
    }
}
