using System;
using db;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class GuildRemovePacketHandler : PacketHandlerBase<GuildRemovePacket>
    {
        public override PacketID ID
        {
            get { return PacketID.GuildRemove; }
        }

        protected override void HandlePacket(Client client, GuildRemovePacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, GuildRemovePacket packet)
        {
            string pname = packet.Name;
            try
            {
                player.Manager.Data.AddPendingAction(db =>
                {
                    Player p = player.Manager.FindPlayer(pname);
                    if (p != null && p.Guild == player.Guild)
                    {
                        Guild g = db.ChangeGuild(p.Client.Account, p.Client.Account.Guild.Id, p.GuildRank,
                            p.Client.Account.Guild.Fame, true);
                        p.Guild = "";
                        p.GuildRank = 0;
                        p.Client.Account.Guild = g;
                        p.UpdateCount++;
                        if (p != player)
                        {
                            foreach (Player pl in player.Manager.GuildMembersOf(player.Guild))
                                pl.SendGuild(p.Name + " has been kicked from the guild by " + player.Name + ".");
                        }
                        else
                        {
                            foreach (Player pl in player.Manager.GuildMembersOf(player.Guild))
                                pl.SendGuild(player.Name + " has left the guild.");
                        }
                    }
                    else
                    {
                        try
                        {
                            Account other = db.GetAccount(pname);
                            if (other.Guild.Name == player.Guild)
                            {
                                db.ChangeGuild(other, other.Guild.Id, other.Guild.Rank, other.Guild.Fame, true);
                                foreach (Player pl in player.Manager.GuildMembersOf(player.Guild))
                                    pl.SendGuild(pname + " has been kicked from the guild by " + player.Name + ".");
                            }
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
            player.Client.SendPacket(new CreateGuildResultPacket
            {
                Success = true
            });
        }
    }
}