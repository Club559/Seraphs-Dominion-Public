using db;
using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class JoinGuildPacketHandler : PacketHandlerBase<JoinGuildPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.JoinGuild; }
        }

        protected override void HandlePacket(Client client, JoinGuildPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, JoinGuildPacket packet)
        {
            player.Manager.Data.AddPendingAction(db =>
            {
                GuildStruct gStruct = db.GetGuild(packet.GuildName);
                if (player.Client.Player.Invited == false)
                {
                    player.SendInfo("You need to be invited to join a guild!");
                }
                if (gStruct != null)
                {
                    Guild g = db.ChangeGuild(player.Client.Account, gStruct.Id, 0, 0, false);
                    if (g != null)
                    {
                        player.Client.Account.Guild = g;
                        player.Guild = g.Name;
                        player.GuildRank = g.Rank;
                        player.UpdateCount++;
                        foreach (Player p in player.Manager.GuildMembersOf(g.Name))
                        {
                            p.Client.SendPacket(new TextPacket
                            {
                                BubbleTime = 0,
                                Stars = -1,
                                Name = "",
                                Recipient = "*Guild*",
                                Text = player.Client.Account.Name + " has joined the guild!"
                            });
                        }
                    }
                }
                else
                {
                    player.SendInfo("Guild doesn't exist!");
                }
            });
        }
    }
}