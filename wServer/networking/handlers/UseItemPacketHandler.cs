using wServer.networking.cliPackets;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class UseItemPacketHandler : PacketHandlerBase<UseItemPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.UseItem; }
        }

        protected override void HandlePacket(Client client, UseItemPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, t, packet));
        }

        private void Handle(Player player, RealmTime time, UseItemPacket packet)
        {
            if (player.Owner == null) return;

            player.UseItem(time, packet.Slot.ObjectId, packet.Slot.SlotId, packet.Position);
        }
    }
}