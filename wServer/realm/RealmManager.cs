using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using db;
using log4net;
using wServer.logic;
using wServer.networking;
using wServer.realm.commands;
using wServer.realm.entities;
using wServer.realm.worlds;

namespace wServer.realm
{
    public struct RealmTime
    {
        public int thisTickCounts;
        public int thisTickTimes;
        public long tickCount;
        public long tickTimes;
    }

    public class TimeEventArgs : EventArgs
    {
        public TimeEventArgs(RealmTime time)
        {
            Time = time;
        }

        public RealmTime Time { get; private set; }
    }

    public enum PendingPriority
    {
        Emergent,
        Destruction,
        Networking,
        Normal,
        Creation,
    }

    public class RealmManager
    {
        public const int MAX_CLIENT = 200;
        public const int MAX_INREALM = 85;
        private static readonly ILog log = LogManager.GetLogger(typeof (RealmManager));

        public static List<string> realmNames = new List<string>
        {
            "Medusa",
            "Beholder",
            "Flayer",
            "Ogre",
            "Cyclops",
            "Sprite",
            "Djinn",
            "Slime",
            "Blob",
            "Demon",
            "Spider",
            "Scorpion",
            "Ghost"
        };

        public static List<string> CurrentRealmNames = new List<string>();

        public readonly ConcurrentDictionary<int, Client> Clients = new ConcurrentDictionary<int, Client>();

        public readonly ConcurrentDictionary<string, GuildHall> GuildHalls =
            new ConcurrentDictionary<string, GuildHall>();

        public readonly ConcurrentDictionary<int, World> Worlds = new ConcurrentDictionary<int, World>();
        public ConcurrentDictionary<int, World> PlayerWorldMapping = new ConcurrentDictionary<int, World>();
        private Thread logic;
        private Thread network;
        private Thread database;
        private Thread save;
        private int nextWorldId;

        public RealmManager(int maxClient, int tps, Database db)
        {
            MaxClient = maxClient;
            TPS = tps;
            Database = db;
        }

        public int MaxClient { get; private set; }
        public int TPS { get; private set; }
        public Database Database { get; private set; }

        public RealmPortalMonitor Monitor { get; private set; }
        public NetworkTicker Network { get; private set; }
        public LogicTicker Logic { get; private set; }
        public DatabaseTicker Data { get; private set; }
        public AutoSave Save { get; private set; }

        public XmlData GameData { get; private set; }
        public BehaviorDb Behaviors { get; private set; }

        public ChatManager Chat { get; private set; }
        public CommandManager Commands { get; private set; }
        public bool Terminating { get; private set; }

        public int TimeOut = 60;
        public int conTimes = 1;

        public bool TryConnect(Client client)
        {
            if (Clients.Count >= MaxClient)
                return false;
            return Clients.TryAdd(client.Account.AccountId, client);
        }

        public bool IsUserOnline(Client client, Account account)
        {
            if (TimeOut > 0)
            {
                this.TimeOut -= conTimes;
                if (conTimes < 5)
                {
                    conTimes++;
                }
            }
            else
            {
                this.TimeOut = 60;
                foreach (Client i in Clients.Values.Where(i => i.Account.Name.EqualsIgnoreCase(account.Name)))
                {
                    i.Disconnect();
                    i.Save();
                }
                return false;
            }

            return Clients.ContainsKey(account.AccountId);
        }

        public void Disconnect(Client client)
        {
            Clients.TryRemove(client.Account.AccountId, out client);
        }

        public World GuildHallWorld(string g)
        {
            if (!GuildHalls.ContainsKey(g))
            {
                var gh = (GuildHall) AddWorld(new GuildHall(g));
                return GuildHalls.GetOrAdd(g, gh);
            }
            return GuildHalls[g];
        }

        public World AddWorld(int id, World world)
        {
            if (world.Manager != null)
                throw new InvalidOperationException("World already added.");
            world.Id = id;
            Worlds[id] = world;
            OnWorldAdded(world);
            return world;
        }

        public World AddWorld(World world)
        {
            if (world.Manager != null)
                throw new InvalidOperationException("World already added.");
            world.Id = Interlocked.Increment(ref nextWorldId);
            Worlds[world.Id] = world;
            OnWorldAdded(world);
            return world;
        }

        public bool RemoveWorld(World world)
        {
            if (world.Manager == null)
                throw new InvalidOperationException("World is not added.");
            World dummy;
            if (Worlds.TryRemove(world.Id, out dummy))
            {
                try
                {
                    OnWorldRemoved(world);
                    world.Dispose();
                    GC.Collect();
                }
                catch (Exception e)
                { log.Fatal(e); }
                return true;
            }
            return false;
        }

        public World GetWorld(int id)
        {
            World ret;
            if (!Worlds.TryGetValue(id, out ret)) return null;
            if (ret.Id == 0) return null;
            return ret;
        }

        public List<Player> GuildMembersOf(string guild)
        {
            return (from i in Worlds
                where i.Key != 0
                from e in i.Value.Players
                where String.Equals(e.Value.Guild, guild, StringComparison.CurrentCultureIgnoreCase)
                select e.Value).ToList();
        }

        public Player FindPlayer(string name)
        {
            if (name.Split(' ').Length > 1)
                name = name.Split(' ')[1];
            return (from i in Worlds
                where i.Key != 0
                from e in i.Value.Players
                where String.Equals(e.Value.Client.Account.Name, name, StringComparison.CurrentCultureIgnoreCase)
                select e.Value).FirstOrDefault();
        }

        public Player FindPlayerRough(string name)
        {
            Player dummy;
            foreach (var i in Worlds)
                if (i.Key != 0)
                    if ((dummy = i.Value.GetUniqueNamedPlayerRough(name)) != null)
                        return dummy;
            return null;
        }

        private void OnWorldAdded(World world)
        {
            world.Manager = this;
            if (world is GameWorld)
                Monitor.WorldAdded(world);
            log.InfoFormat("World {0}({1}) added.", world.Id, world.Name);
        }

        private void OnWorldRemoved(World world)
        {
            world.Manager = null;
            if (world is GameWorld)
                Monitor.WorldRemoved(world);
            log.InfoFormat("World {0}({1}) removed.", world.Id, world.Name);
        }


        public void Initialize()
        {
            log.Info("Initializing Realm Manager...");

            GameData = new XmlData();
            Behaviors = new BehaviorDb(this);

            AddWorld(World.NEXUS_ID, Worlds[0] = new Nexus());
            Monitor = new RealmPortalMonitor(this);

            AddWorld(World.TUT_ID, new Tutorial(true));
            AddWorld(World.NEXUS_LIMBO, new NexusLimbo());
            AddWorld(World.VAULT_ID, new Vault(true));
            AddWorld(World.TEST_ID, new Test());
            AddWorld(World.RAND_REALM, new RandomRealm());
            AddWorld(World.PVP, new PVPArena());
            AddWorld(World.SHOP_ID, new Shop());

            if (Program.Settings.GetValue<bool>("hasRealm"))
                AddWorld(GameWorld.AutoName(1, true));

            Chat = new ChatManager(this);
            Commands = new CommandManager(this);

            UnusualEffects.Init();

            log.Info("Realm Manager initialized.");
        }

        public void Run()
        {
            log.Info("Starting Realm Manager...");

            Network = new NetworkTicker(this);
            Logic = new LogicTicker(this);
            Data = new DatabaseTicker(this);
            Save = new AutoSave(this);
            network = new Thread(Network.TickLoop)
            {
                Name = "Network",
                CurrentCulture = CultureInfo.InvariantCulture
            };
            logic = new Thread(Logic.TickLoop)
            {
                Name = "Logic",
                CurrentCulture = CultureInfo.InvariantCulture
            };
            database = new Thread(Data.TickLoop)
            {
                Name = "Database",
                CurrentCulture = CultureInfo.InvariantCulture
            };
            save = new Thread(Save.TickLoop)
            {
                Name = "Save",
                CurrentCulture = CultureInfo.InvariantCulture
            };
            //Start logic loop first
            logic.Start();
            network.Start();
            database.Start();
            save.Start();

            log.Info("Realm Manager started.");
        }

        public void Stop()
        {
            log.Info("Stopping Realm Manager...");

            Terminating = true;
            GameData.Dispose();
            logic.Join();
            network.Join();
            save.Join();
            database.Join();

            log.Info("Realm Manager stopped.");
        }
    }
}