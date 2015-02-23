using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class GuildInvitePacketHandler : PacketHandlerBase<GuildInvitePacket>
    {
        public override PacketID ID
        {
            get { return PacketID.GuildInvite; }
        }

        protected override void HandlePacket(Client client, GuildInvitePacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, GuildInvitePacket packet)
        {
            if (player.GuildRank >= 20)
            {
                foreach (Client i in player.Manager.Clients.Values)
                {
                    foreach (var l in player.Manager.Worlds)
                    {
                        if (l.Key != 0)
                        {
                            foreach (var e in l.Value.Players)
                            {
                                if (e.Value.Name == packet.Name)
                                {
                                    if (e.Value.Guild == "")
                                    {
                                        e.Value.Client.SendPacket(new InvitedToGuildPacket
                                        {
                                            Name = player.Client.Account.Name,
                                            Guild = player.Client.Account.Guild.Name
                                        });
                                        i.Player.Invited = true;
                                        player.SendInfo("Guild invite has been sent to " + e.Value.Name);
                                        return;
                                    }
                                    player.SendError("Player is already in a guild!");
                                }
                            }
                        }
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
                    Text = "Members and initiates cannot invite!"
                });
            }
        }
    }
}