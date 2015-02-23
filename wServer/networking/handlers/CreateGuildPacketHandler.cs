using System;
using db;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class CreateGuildPacketHandler : PacketHandlerBase<CreateGuildPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.CreateGuild; }
        }

        protected override void HandlePacket(Client client, CreateGuildPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, CreateGuildPacket packet)
        {
            try
            {
                string name = packet.Name;
                if (player.Client.Account.Stats.Fame >= 1000)
                {
                    if (name != "")
                    {
                        player.Manager.Data.AddPendingAction(db =>
                        {
                            if (db.GetGuild(name) != null)
                            {
                                player.Client.SendPacket(new CreateGuildResultPacket
                                {
                                    Success = false,
                                    ResultMessage = "Guild already exists!"
                                });
                                return;
                            }
                        });
                        player.Manager.Data.AddPendingAction(db =>
                        {
                            try
                            {
                                if (player.Client.Account.Guild.Name == "")
                                {
                                    if (packet.Name != "")
                                    {
                                        Guild g = db.CreateGuild(player.Client.Account, packet.Name);
                                        player.Client.Account.Guild.Id = g.Id;
                                        player.Client.Account.Guild.Name = g.Name;
                                        player.Client.Account.Guild.Rank = g.Rank;
                                        player.Client.Account.Guild.Fame = g.Fame;
                                        player.Guild = g.Name;
                                        player.GuildRank = g.Rank;
                                        player.Client.SendPacket(new NotificationPacket
                                        {
                                            Color = new ARGB(0xFF00FF00),
                                            ObjectId = player.Id,
                                            Text = "Guild Created"
                                        });
                                        player.SendInfo(g.Name + " has successfully been created");
                                        player.Client.SendPacket(new CreateGuildResultPacket
                                        {
                                            Success = true
                                        });
                                        player.CurrentFame =
                                            player.Client.Account.Stats.Fame =
                                                db.UpdateFame(player.Client.Account, -1000);
                                        player.UpdateCount++;
                                    }
                                    player.Client.SendPacket(new CreateGuildResultPacket
                                    {
                                        Success = false,
                                        ResultMessage = "Guild name cannot be blank!"
                                    });
                                    return;
                                }
                                player.Client.SendPacket(new CreateGuildResultPacket
                                {
                                    Success = false,
                                    ResultMessage = "You cannot create a guild as a guild member!"
                                });
                                return;
                            }
                            catch (Exception e)
                            {
                                player.Client.SendPacket(new CreateGuildResultPacket
                                {
                                    Success = false,
                                    ResultMessage = e.Message
                                });
                            }
                        });
                    }
                    player.Client.SendPacket(new CreateGuildResultPacket
                    {
                        Success = false,
                        ResultMessage = "Name cannot be empty!"
                    });
                }
                else
                {
                    player.Client.SendPacket(new CreateGuildResultPacket
                    {
                        Success = false,
                        ResultMessage = "Not enough fame!"
                    });
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error at line 755 of Player.cs");
                player.Client.SendPacket(new TextPacket
                {
                    Name = "",
                    Stars = -1,
                    BubbleTime = 0,
                    Text = "Error creating guild!"
                });
            }
        }
    }
}