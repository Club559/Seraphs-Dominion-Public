using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm;
using wServer.realm.worlds;

namespace wServer.networking.handlers
{
    internal class EscapePacketHandler : PacketHandlerBase<EscapePacket>
    {
        public override PacketID ID
        {
            get { return PacketID.Escape; }
        }

        protected override void HandlePacket(Client client, EscapePacket packet)
        {
            if (client.Player.Owner.Id == World.NEXUS_ID)
            {
                client.Player.SendInfo("You are already in the Nexus!");
            }
            else if (!client.Player.Owner.AllowNexus)
            {
                if (client.Player.Owner is PVPArena && client.Player.Owner.Players.Count == 1)
                {
                    client.Reconnect(new ReconnectPacket
                    {
                        Host = "",
                        Port = 2050,
                        GameId = World.NEXUS_ID,
                        Name = "Nexus",
                        Key = Empty<byte>.Array,
                    });
                    return;
                }
                client.Player.SendInfo("You cannot nexus now!");
            }
            else
            {
                client.Reconnect(new ReconnectPacket
                {
                    Host = "",
                    Port = 2050,
                    GameId = World.NEXUS_ID,
                    Name = "Nexus",
                    Key = Empty<byte>.Array,
                });
            }
        }
    }
}