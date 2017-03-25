using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fclp.Internals.Extensions;

namespace Kontur.GameStats.Server
{
    internal class StatServer : IDisposable
    {
        public StatServer()
        {
            listener = new HttpListener();
            DataBase db = new DataBase();
        }
        
        public void Start(string prefix)
        {
            lock (listener)
            {
                if (!isRunning)
                {
                    listener.Prefixes.Clear();
                    listener.Prefixes.Add(prefix);
                    listener.Start();

                    listenerThread = new Thread(Listen)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Highest
                    };
                    listenerThread.Start();
                    
                    isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (listener)
            {
                if (!isRunning)
                    return;

                listener.Stop();

                listenerThread.Abort();
                listenerThread.Join();
                
                isRunning = false;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;

            Stop();

            listener.Close();
        }
        
        private void Listen()
        {
            while (true)
            {
                try
                {
                    
                    if (listener.IsListening)
                    {
                        var context = listener.GetContext();
                        Task.Run(() => HandleContext(context));
                    }
                    else Thread.Sleep(0);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}: {error.Message}\r\n");
                    Console.ResetColor();
                }
            }
        }

        private void HandleContext(HttpListenerContext listenerContext)
        {
            //HttpListenerResponse response = listenerContext.Response;
            HttpListenerRequest request = listenerContext.Request;
            HttpListenerResponse response = listenerContext.Response;

            Console.WriteLine(request.HttpMethod + " " + request.RawUrl);                  

            string url = request.RawUrl;

            //    /servers/<endpoint>/info PUT, GET
            if (Regex.IsMatch(url, @"^\/servers\/[^\s]+\/info$"))
            {
                if (request.HttpMethod == HttpMethod.Put.Method)
                {
                    statApi.PutServerInfo(listenerContext);
                }
                else if (request.HttpMethod == HttpMethod.Get.Method)
                {                 
                    statApi.GetServerInfo(listenerContext);
                }
                else
                {
                    statApi.HandleInvalid(listenerContext);
                }
                return;
            }



            //     /servers/<endpoint>/matches/<timestamp> PUT, GET
            if (Regex.IsMatch(url, @"^\/servers\/[^\/\s]+\/matches\/.*Z$"))
            {
                if (request.HttpMethod == HttpMethod.Put.Method)
                {
                    statApi.PutMatchInfo(listenerContext);
                }
                else if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetMatchInfo(listenerContext);
                }
                else
                {
                    statApi.HandleInvalid(listenerContext);
                }
                return;
            }


            //    /servers/info GET
            if (Regex.IsMatch(url, @"^\/servers\/info$"))
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetAllServersInfo(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }



            //      /servers/<endpoint>/stats GET    
            if (Regex.IsMatch(url, @"^\/servers\/[^\/\s]+\/stats$"))
            {
                 if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetServerStats(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }


            //    /players/<name>/stats GET
            if (Regex.IsMatch(url, @"^\/players\/[^\s]+\/stats$"))
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetPlayerStats(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }


            //  /reports/recent-matches[/<count>] GET
            if (Regex.IsMatch(url, @"^\/reports\/recent-matches(\/[-]?[0-9]*|)"))
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetRecentMatches(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }



            //  /reports/best-players[/<count>] GET
            if (Regex.IsMatch(url, @"^\/reports\/best-players(\/[-]?[0-9]*|)"))
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetBestPlayers(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }



            //  /reports/popular-servers[/<count>] GET
            if (Regex.IsMatch(url, @"^\/reports\/popular-servers(\/[-]?[0-9]*|)"))
            {
                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    statApi.GetPopularServers(listenerContext);
                    return;
                }

                statApi.HandleInvalid(listenerContext);
            }

            statApi.HandleInvalid(listenerContext);
        }



        private readonly HttpListener listener;

        private Thread listenerThread;
        private bool disposed;
        private volatile bool isRunning;
        private readonly StatAPI statApi = new StatAPI();
    }
}