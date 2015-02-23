using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using log4net;

namespace wServer.realm
{
    public class LogicTicker
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (LogicTicker));
        public static RealmTime CurrentTime;
        private readonly ConcurrentQueue<Action<RealmTime>>[] pendings;

        public int MsPT;
        public int TPS;

        public LogicTicker(RealmManager manager)
        {
            Manager = manager;
            pendings = new ConcurrentQueue<Action<RealmTime>>[5];
            for (int i = 0; i < 5; i++)
                pendings[i] = new ConcurrentQueue<Action<RealmTime>>();

            TPS = manager.TPS;
            MsPT = 1000/TPS;
        }

        public RealmManager Manager { get; private set; }

        public void AddPendingAction(Action<RealmTime> callback)
        {
            AddPendingAction(callback, PendingPriority.Normal);
        }

        public void AddPendingAction(Action<RealmTime> callback, PendingPriority priority)
        {
            pendings[(int) priority].Enqueue(callback);
        }

        public void TickLoop()
        {
            log.Info("Logic loop started.");
            var watch = new Stopwatch();
            long dt = 0;
            long count = 0;

            watch.Start();
            var t = new RealmTime();
            long xa = 0;
            do
            {
                if (Manager.Terminating) break;

                long times = dt/MsPT;
                dt -= times*MsPT;
                times++;

                long b = watch.ElapsedMilliseconds;

                count += times;
                if (times > 3)
                    log.Warn("LAGGED!| time:" + times + " dt:" + dt + " count:" + count + " time:" + b + " tps:" +
                             count/(b/1000.0));

                t.tickTimes = b;
                t.tickCount = count;
                t.thisTickCounts = (int) times;
                t.thisTickTimes = (int) (times*MsPT);
                xa += t.thisTickTimes;

                foreach (var i in pendings)
                {
                    Action<RealmTime> callback;
                    while (i.TryDequeue(out callback))
                    {
                        try
                        {
                            callback(t);
                        }
                        catch
                        {
                        }
                    }
                }
                TickWorlds1(t);

                Thread.Sleep(MsPT);
                dt += Math.Max(0, watch.ElapsedMilliseconds - b - MsPT);
            } while (true);
            log.Info("Logic loop stopped.");
        }

        private void TickWorlds1(RealmTime t) //Continous simulation
        {
            CurrentTime = t;
            foreach (World i in Manager.Worlds.Values.Distinct())
                i.Tick(t);
            //if (EnableMonitor)
            //    svrMonitor.Mon.Tick(t);
        }

        private void TickWorlds2(RealmTime t) //Discrete simulation
        {
            long counter = t.thisTickTimes;
            long c = t.tickCount - t.thisTickCounts;
            long x = t.tickTimes - t.thisTickTimes;
            while (counter >= MsPT)
            {
                c++;
                x += MsPT;
                TickWorlds1(new RealmTime
                {
                    thisTickCounts = 1,
                    thisTickTimes = MsPT,
                    tickCount = c,
                    tickTimes = x
                });
                counter -= MsPT;
            }
        }
    }
}