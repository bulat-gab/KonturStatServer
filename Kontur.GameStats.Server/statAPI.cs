using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Fclp.Internals.Extensions;
using Newtonsoft.Json;
using System.Web;

namespace Kontur.GameStats.Server
{
    class StatAPI
    {
        private readonly DataBase db = new DataBase();

        private string getUrl(HttpListenerContext context)
        {
            string[] arr = context.Request.RawUrl.Split(new []{ '/' }, StringSplitOptions.RemoveEmptyEntries);              
            return arr[1];
        }

        private string getTimeStamp(HttpListenerContext context)
        {
            string[] arr = context.Request.RawUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return arr[3];  
        }

        private string GetPlayerName(HttpListenerContext context)
        {
            string[] arr = context.Request.RawUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            return HttpUtility.UrlDecode(arr[1]); 
        }

        private int GetCount(HttpListenerContext context)
        {
            string[] arr = context.Request.RawUrl.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int count = 0;

            if (arr.Length == 3)
            {
                count = int.Parse(arr[2]);
            }
            else
            {
                count = 5;
            }

            if (count > 50)
                count = 50;

            if (count < 0)
                count = 0;

            return count;
        }

        public void HandleInvalid(HttpListenerContext listenerContext)
        {
            listenerContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
            {
                writer.Write("<h1>Bad Request</h1>");
            }
            listenerContext.Response.Close();
        }

        public void PutServerInfo(HttpListenerContext listenerContext)
        {           
            try
            {
                var content = new StreamReader(listenerContext.Request.InputStream);
                string url = getUrl(listenerContext);
                ServerInfo serverInfo = JsonConvert.DeserializeObject<ServerInfo>(content.ReadToEnd());

                Server argument = new Server(url, serverInfo);              

                db.PutServerInfo(argument);
                listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;               
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, e.Message);
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetServerInfo(HttpListenerContext listenerContext)
        {                    
            try
            {
                string url = getUrl(listenerContext);
                ServerInfo results = db.GetServerInfo(url);

                if (results == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;                    
                }
                else
                {
                    string jsonServer = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(jsonServer);
                    }
                }
                

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int) HttpStatusCode.InternalServerError;                
            }
            finally
            {
                listenerContext.Response.Close();
            }
            
        }

        public void PutMatchInfo(HttpListenerContext listenerContext)
        {
            try
            {
                var content = new StreamReader(listenerContext.Request.InputStream);

                MatchInfo matchInfo = JsonConvert.DeserializeObject<MatchInfo>(content.ReadToEnd());
                string url = getUrl(listenerContext);
                string timeStamp = getTimeStamp(listenerContext);

                Match argument = new Match(timeStamp, url, matchInfo);

                if (db.PutMatchInfo(argument) == false)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                }
                else
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, e.Message);
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetMatchInfo(HttpListenerContext listenerContext)
        {
            try
            {
                string url = getUrl(listenerContext);
                string timestamp = getTimeStamp(listenerContext);

                MatchInfo results = db.GetMatchInfo(url, timestamp);

                if (results == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
                else
                {
                    string jsonMatches = JsonConvert.SerializeObject(results);

                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(jsonMatches);
                    }
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0:dd.MM.yyyy HH:mm:ss}: {1}\r\n", DateTime.Now, e.Message);
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetAllServersInfo(HttpListenerContext listenerContext)
        {
            try
            {
                List<Server> results = db.GetAllServersInfo();

                if (results.IsNullOrEmpty())
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
                

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetServerStats(HttpListenerContext listenerContext)
        {
            try
            {
                string url = getUrl(listenerContext);
                ServerStats results = db.GetServerStats(url);

                if (results == null)
                {
                    listenerContext.Response.StatusCode = (int) HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetPlayerStats(HttpListenerContext listenerContext)
        {
            try
            {
                string name = GetPlayerName(listenerContext);
                PlayerStats results = db.GetPlayerStats(name);

                if (results == null)
                {                    
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetRecentMatches(HttpListenerContext listenerContext)
        {
            try
            {
                int count = GetCount(listenerContext);

                if (count == 0)
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write("[]");
                    }
                    listenerContext.Response.Close();
                    return;
                }


                List<Match> results = db.GetRecentMatches(count);

                if (results.IsNullOrEmpty())
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetBestPlayers(HttpListenerContext listenerContext)
        {
            try
            {
                int count = GetCount(listenerContext);

                if (count == 0)
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write("[]");
                    }
                    listenerContext.Response.Close();
                    return;
                }

                List<BestPlayer> results = db.GetBestPlayers(count);

                if (results.IsNullOrEmpty())
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }

        public void GetPopularServers(HttpListenerContext listenerContext)
        {
            try
            {
                int count = GetCount(listenerContext);

                if (count == 0)
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write("[]");
                    }
                    listenerContext.Response.Close();
                    return;
                }


                List<PopularServer> results = db.GetPopularServers(count);

                if (results.IsNullOrEmpty())
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                else
                {
                    string json = JsonConvert.SerializeObject(results);
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.ContentType = "application/json";
                    using (var writer = new StreamWriter(listenerContext.Response.OutputStream))
                    {
                        writer.Write(json);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {e.Message}\r\n");
                Console.ResetColor();
                listenerContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            finally
            {
                listenerContext.Response.Close();
            }
        }
    }
}  