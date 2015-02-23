using wServer.networking.cliPackets;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class PlayerShootPacketHandler : PacketHandlerBase<PlayerShootPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.PlayerShoot; }
        }

        protected override void HandlePacket(Client client, PlayerShootPacket packet)
        {
            client.Manager.Logic.AddPendingAction(t => Handle(client.Player, packet));
        }

        private void Handle(Player player, PlayerShootPacket packet)
        {
            if (player.Owner == null) return;

            Item item = player.Manager.GameData.Items[packet.ContainerType];
            ProjectileDesc prjDesc = item.Projectiles[0]; //Assume only one
            var dmg = (int) player.statsMgr.GetAttackDamage(prjDesc.MinDamage, prjDesc.MaxDamage, (player.Inventory.Data[0] != null ? player.Inventory.Data[0].DmgPercentage : 0));
            Projectile prj = player.PlayerShootProjectile(
                packet.BulletId, prjDesc, item.ObjectType,
                packet.Time, packet.Position, packet.Angle, dmg, player.Inventory.Data[0], 0);
            player.Owner.EnterWorld(prj);
            player.BroadcastSync(new AllyShootPacket
            {
                OwnerId = player.Id,
                Angle = packet.Angle,
                ContainerType = packet.ContainerType,
                BulletId = packet.BulletId,
                Damage = dmg
            }, p => p != player && player.Dist(p) < 25);
            player.FameCounter.Shoot(prj);
        }
    }
}