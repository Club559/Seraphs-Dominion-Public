using db;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace wServer.realm
{
    public class DatabaseTicker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(DatabaseTicker));
        private readonly ConcurrentQueue<Action<Database>>[] pendings;

        public DatabaseTicker(RealmManager manager)
        {
            Manager = manager;
            pendings = new ConcurrentQueue<Action<Database>>[5];
            for (int i = 0; i < 5; i++)
                pendings[i] = new ConcurrentQueue<Action<Database>>();
        }

        public RealmManager Manager { get; private set; }

        public void AddPendingAction(Action<Database> callback)
        {
            AddPendingAction(callback, PendingPriority.Networking);
        }

        public void AddPendingAction(Action<Database> callback, PendingPriority priority)
        {
            pendings[(int)priority].Enqueue(callback);
        }

        public void TickLoop()
        {
            log.Info("Database loop started.");
            do
            {
                //First finish every db query
                if (Manager.Terminating)
                {
                    bool empty = true;
                    foreach (var i in pendings)
                    {
                        if (i.Count > 0)
                        {
                            empty = false;
                            break;
                        }
                    }
                    if (empty) break;
                }

                foreach (ConcurrentQueue<Action<Database>> i in pendings)
                {
                    Action<Database> callback;
                    while (i.TryDequeue(out callback))
                    {
                        try
                        {
                            Database db = new Database(Program.Settings.GetValue("conn"));
                            callback(db);
                            db.Dispose();
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex);
                        }
                    }
                }
            } while (true);
            log.Info("Database loop stopped.");
        }
    }
}