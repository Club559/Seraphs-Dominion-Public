using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm;
using wServer.realm.entities;
using wServer.realm.worlds;
using wServer.realm.worlds.tower;

namespace wServer.networking.handlers
{
    internal class UsePortalPacketHandler : PacketHandlerBase<UsePortalPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.UsePortal; }
        }

        protected override void HandlePacket(Client client, UsePortalPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, UsePortalPacket packet)
        {
            if (player.Party != null && player.Party.Leader != player)
            {
                player.SendInfo("Only the party leader can use portals.");
                return;
            }
            Entity entity = player.Owner.GetEntity(packet.ObjectId);
            if (entity == null) return;
            try
            {
                var eport = (Portal) entity;
                if (!eport.Usable) return;
            }
            catch
            {
            }
            Portal portal = null;
            World world = null;
            if (entity is Portal)
            {
                portal = entity as Portal;
                world = portal.WorldInstance;
            }
            if (entity is TowerPortal)
            {
                int floor = (entity as TowerPortal).Floor;
                if(floor == 0 || floor > Tower.FLOORS)
                    world = player.Manager.GetWorld(World.NEXUS_ID);
                else
                    world = player.Manager.AddWorld(new Tower(floor));
            }
            if (world == null)
            {
                if (portal != null)
                {
                    /*if (player.Party != null && !portal.Descriptor.Party)
                    {
                        player.SendInfo("This portal cannot be used with a party.");
                        return;
                    }*/
                    bool setInstance = true;
                    switch (entity.ObjectType) //handling default case for not found. Add more as implemented
                    {
                        case 0x0703: //portal of cowardice
                        {
                            if (player.Manager.PlayerWorldMapping.ContainsKey(player.AccountId))
                                //may not be valid, realm recycled?
                                world = player.Manager.PlayerWorldMapping[player.AccountId];
                                    //also reconnecting to vault is a little unexpected
                            else
                                world = player.Manager.GetWorld(World.NEXUS_ID);
                        }
                            break;
                        case 0x0712:
                            world = player.Manager.GetWorld(World.VAULT_ID);
                            break;
                        case 0x071c:
                            world = player.Manager.Monitor.GetRandomRealm();
                            break;
                        case 0x071d:
                            world = player.Manager.GetWorld(World.NEXUS_ID);
                            break;
                        case 0x071e:
                            world = player.Manager.AddWorld(new Kitchen());
                            break;
                        case 0x0720:
                            world = player.Manager.GetWorld(World.VAULT_ID);
                            break;
                        case 0x2000:
                            world = player.Manager.AddWorld(new Gauntlet());
                            break;
                        case 0x2001:
                            world = player.Manager.AddWorld(new PVPArena());
                            break;
                        case 0x2002:
                            world = player.Manager.AddWorld(new TestingAndStuff());
                            break;
                        case 0x2003:
                            world = DuelArena.GetBestDuelArena(player);
                            setInstance = false;
                            break;
                        case 0x2004:
                            world = player.Manager.AddWorld(new WineCellar());
                            break;
                            //case 0x071b:
                            //    world = player.Manager.AddWorld(new Abyss()); break;
                        default:
                            player.SendError("Portal Not Implemented!");
                            break;
                            //case 1795
                            /*case 0x0712:
                            world = RealmManager.GetWorld(World.NEXUS_ID); break;*/
                    }
                    if (setInstance)
                        portal.WorldInstance = world;
                }
                else
                {
                    switch (entity.ObjectType)
                    {
                        case 0x072f:
                            world = player.Manager.GuildHallWorld(player.Guild);
                            break;
                        default:
                            player.SendError("Semi-Portal Not Implemented!");
                            break;
                    }
                }
            }

            //used to match up player to last realm they were in, to return them to it. Sometimes is odd, like from Vault back to Vault...
            if (player.Manager.PlayerWorldMapping.ContainsKey(player.AccountId))
            {
                World tempWorld;
                player.Manager.PlayerWorldMapping.TryRemove(player.AccountId, out tempWorld);
            }
            player.Manager.PlayerWorldMapping.TryAdd(player.AccountId, player.Owner);
            player.Client.Reconnect(new ReconnectPacket
            {
                Host = "",
                Port = 2050,
                GameId = world.Id,
                Name = world.Name,
                Key = Empty<byte>.Array,
            });
        }
    }
}