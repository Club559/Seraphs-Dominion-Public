using wServer.networking.cliPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class MovePacketHandler : PacketHandlerBase<MovePacket>
    {
        public override PacketID ID
        {
            get { return PacketID.Move; }
        }

        protected override void HandlePacket(Client client, MovePacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, MovePacket packet)
        {
            if (player.Owner == null) return;

            player.Flush();
            if (packet.Position.X == -1 || packet.Position.Y == -1) return;

            double newX = player.X;
            double newY = player.Y;
            if (newX != packet.Position.X)
            {
                newX = packet.Position.X;
                player.UpdateCount++;
            }
            if (newY != packet.Position.Y)
            {
                newY = packet.Position.Y;
                player.UpdateCount++;
            }
            player.Move((float) newX, (float) newY);
        }
    }
}