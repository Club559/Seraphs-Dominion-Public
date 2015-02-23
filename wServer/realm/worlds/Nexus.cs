using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.terrain;
using wServer.realm.worlds.tower;

namespace wServer.realm.worlds
{
    public class Nexus : World
    {
        public Nexus()
        {
            Id = NEXUS_ID;
            Name = "Nexus";
            Background = 2;
            Difficulty = 0;
            SetMusic("world/Nexus", "world/Nexus2", "world/Nexus3");
            AllowNexus = false;
            PvP = false;
        }

        protected override void Init()
        {
            base.FromWorldMap(typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.nexus.wmap"));
        }

        public override void Tick(RealmTime time)
        {
            base.Tick(time); //normal world tick

            CheckDupers();
            UpdatePortals();
        }

        private void CheckDupers()
        {
            foreach (var w in Manager.Worlds)
            {
                foreach (var x in Manager.Worlds)
                {
                    foreach (var y in w.Value.Players)
                    {
                        foreach (var z in x.Value.Players)
                        {
                            if (y.Value.AccountId == z.Value.AccountId && y.Value != z.Value)
                            {
                                y.Value.Client.Disconnect();
                                z.Value.Client.Disconnect();
                            }
                        }
                    }
                }
            }
        }

        private void UpdatePortals()
        {
            foreach (var i in Manager.Monitor.portals)
            {
                foreach (string it in RealmManager.realmNames)
                {
                    if (i.Value.Name.StartsWith(it))
                    {
                        i.Value.Name = it + " (" + i.Key.Players.Count + "/" + RealmManager.MAX_INREALM + ")";
                        i.Value.UpdateCount++;
                        break;
                    }
                }
                if (i.Value.Name.StartsWith("PVP"))
                {
                    i.Value.Name = "PVP Portal" + " (" + i.Key.Players.Count + "/" + "10" + ")";
                    i.Value.UpdateCount++;
                    break;
                }
            }
        }

        public override void TileEvent(Player player, WmapTile tile)
        {
            switch (tile.Region)
            {
                case TileRegion.Hallway:
                    player.Client.SendPacket(new GlobalNotificationPacket
                    {
                        Text = "yellow",
                        Type = 0
                    });
                    break;
                case TileRegion.Hallway_1:
                    if((player.Party != null && player.Party.Leader == player) || player.Party == null)
                    {
                        World tower = Manager.AddWorld(new Tower(1)); 
                        player.Client.Reconnect(new ReconnectPacket
                        {
                            Host = "",
                            Port = 2050,
                            Name = tower.Name,
                            GameId = tower.Id,
                            Key = Empty<byte>.Array
                        });
                    };
                    break;
                case TileRegion.Hallway_2:
                    player.Client.SendPacket(new GlobalNotificationPacket
                    {
                        Text = "green",
                        Type = 0
                    });
                    break;
                case TileRegion.Hallway_3:
                    player.Client.SendPacket(new GlobalNotificationPacket
                    {
                        Text = "purple",
                        Type = 0
                    });
                    break;
                default:
                    break;
            }
        }
    }
}