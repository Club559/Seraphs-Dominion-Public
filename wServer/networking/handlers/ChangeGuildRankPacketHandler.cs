using System;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class ChangeGuildRankPacketHandler : PacketHandlerBase<ChangeGuildRankPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.ChangeGuildRank; }
        }

        protected override void HandlePacket(Client client, ChangeGuildRankPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, ChangeGuildRankPacket packet)
        {
            string status = "";
            string pname = packet.Name;
            int rank = packet.GuildRank;
            if (player.GuildRank >= 20)
            {
                Player other = player.Manager.FindPlayer(pname);
                if (other != null && other.Guild == player.Guild)
                {
                    string rankname = player.ResolveRankName(other.GuildRank);
                    string rankname2 = player.ResolveRankName(rank);

                    status = rank > other.GuildRank ? "promoted" : "demoted";

                    other.GuildRank = rank;
                    other.Client.Account.Guild.Rank = rank;
                    player.Manager.Data.AddPendingAction(db => db.ChangeGuild(other.Client.Account, other.Client.Account.Guild.Id, other.GuildRank,
                        other.Client.Account.Guild.Fame, false));
                    other.UpdateCount++;
                    foreach (Player p in player.Manager.GuildMembersOf(player.Guild))
                    {
                        p.Client.SendPacket(new TextPacket
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "",
                            Recipient = "*Guild*",
                            Text = other.Client.Account.Name + " has been " + status + " to " + rankname2 + "."
                        });
                    }
                }
                else
                {
                    try
                    {
                        player.Manager.Data.AddPendingAction(db =>
                        {
                            Account acc = db.GetAccount(pname);
                            if (acc.Guild.Name == player.Guild)
                            {
                                string rankname = player.ResolveRankName(acc.Guild.Rank);
                                string rankname2 = player.ResolveRankName(rank);
                                db.ChangeGuild(acc, acc.Guild.Id, rank, acc.Guild.Fame, false);

                                if (rank > acc.Guild.Rank)
                                    status = "promoted";
                                else
                                    status = "demoted";

                                foreach (Player p in player.Manager.GuildMembersOf(player.Guild))
                                {
                                    p.Client.SendPacket(new TextPacket
                                    {
                                        BubbleTime = 0,
                                        Stars = -1,
                                        Name = "",
                                        Recipient = "*Guild*",
                                        Text = acc.Name + " has been " + status + " to " + rankname2 + "."
                                    });
                                }
                            }
                            else
                            {
                                player.Client.SendPacket(new TextPacket
                                {
                                    BubbleTime = 0,
                                    Stars = -1,
                                    Name = "*Error*",
                                    Text = "You can only change a player in your guild."
                                });
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        player.Client.SendPacket(new TextPacket
                        {
                            BubbleTime = 0,
                            Stars = -1,
                            Name = "*Error*",
                            Text = e.Message
                        });
                    }
                }
            }
            else
            {
                player.Client.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "Members and initiates cannot promote!"
                });
            }
            player.Client.SendPacket(new CreateGuildResultPacket
            {
                Success = true,
                ResultMessage = ""
            });
        }
    }
}