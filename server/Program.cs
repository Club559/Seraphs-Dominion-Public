using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using db;
using log4net;
using log4net.Config;

namespace server
{
    internal class Program
    {
        private static HttpListener listener;
        private static readonly Thread[] workers = new Thread[5];
        private static readonly Queue<HttpListenerContext> contextQueue = new Queue<HttpListenerContext>();
        private static readonly object queueLock = new object();
        private static readonly ManualResetEvent queueReady = new ManualResetEvent(false);

        internal static SimpleSettings Settings;
        internal static XmlData GameData;

        private static readonly ILog log = LogManager.GetLogger("Server");
        private static bool terminating;

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Name = "Entry";

            using (Settings = new SimpleSettings("server"))
            {
                GameData = new XmlData();
                var port = Settings.GetValue<int>("port", "8080");

                listener = new HttpListener();
                listener.Prefixes.Add("http://*:" + port + "/");
                listener.Start();

                listener.BeginGetContext(ListenerCallback, null);
                for (int i = 0; i < workers.Length; i++)
                {
                    workers[i] = new Thread(Worker) {Name = "Worker " + i};
                    workers[i].Start();
                }
                Console.CancelKeyPress += (sender, e) => e.Cancel = true;
                log.Info("Listening at port " + port + "...");

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

                log.Info("Terminating...");
                terminating = true;
                listener.Stop();
                queueReady.Set();
                GameData.Dispose();
                while (contextQueue.Count > 0)
                    Thread.Sleep(100);
            }
        }

        private static void ListenerCallback(IAsyncResult ar)
        {
            if (!listener.IsListening) return;
            HttpListenerContext context = listener.EndGetContext(ar);
            listener.BeginGetContext(ListenerCallback, null);
            lock (queueLock)
            {
                contextQueue.Enqueue(context);
                queueReady.Set();
            }
        }

        private static void Worker()
        {
            while (queueReady.WaitOne())
            {
                if (terminating) return;
                HttpListenerContext context;
                lock (queueLock)
                {
                    if (contextQueue.Count > 0)
                        context = contextQueue.Dequeue();
                    else
                    {
                        queueReady.Reset();
                        continue;
                    }
                }

                try
                {
                    ProcessRequest(context);
                }
                catch
                {
                }
            }
        }

        private static void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                if (!context.Request.Url.LocalPath.StartsWith("/clientError"))
                    log.InfoFormat("Dispatching request '{0}'@{1}",
                        context.Request.Url.LocalPath, context.Request.RemoteEndPoint);
                RequestHandler handler;

                if (!RequestHandlers.Handlers.TryGetValue(context.Request.Url.LocalPath, out handler))
                {
                    context.Response.StatusCode = 400;
                    context.Response.StatusDescription = "Bad request";
                    using (var wtr = new StreamWriter(context.Response.OutputStream))
                        wtr.Write("<h1>Bad request</h1>");
                }
                else
                    handler.HandleRequest(context);
            }
            catch (Exception e)
            {
                using (var wtr = new StreamWriter(context.Response.OutputStream))
                    wtr.Write("<Error>Internal Server Error</Error>");
                log.Error("Error when dispatching request", e);
            }

            context.Response.Close();
        }
    }
}