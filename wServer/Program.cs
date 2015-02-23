using System;
using System.Globalization;
using System.IO;
using System.Threading;
using db;
using log4net;
using log4net.Config;
using wServer.networking;
using wServer.realm;

namespace wServer
{
    internal static class Program
    {
        internal static SimpleSettings Settings;

        private static readonly ILog log = LogManager.GetLogger("Server");

        private static void Main(string[] args)
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("log4net.config"));

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.Name = "Entry";

            using (Settings = new SimpleSettings("wServer"))
            {
                var db = new Database(Settings.GetValue("conn"));
                var manager = new RealmManager(
                    Settings.GetValue<int>("maxClient", "100"),
                    Settings.GetValue<int>("tps", "10"),
                    db);

                manager.Initialize();
                manager.Run();

                var server = new Server(manager, 2050);
                var policy = new PolicyServer();

                Console.CancelKeyPress += (sender, e) => e.Cancel = true;

                policy.Start();
                server.Start();
                log.Info("Server initialized.");

                while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;

                log.Info("Terminating...");
                server.Stop();
                policy.Stop();
                manager.Stop();
                db.Dispose();
                log.Info("Server terminated.");
            }
        }
    }
}