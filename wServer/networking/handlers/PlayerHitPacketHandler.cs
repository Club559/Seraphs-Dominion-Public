using System;
using wServer.networking.cliPackets;
using wServer.realm.entities;

namespace wServer.networking.handlers
{
    internal class PlayerHitPacketHandler : PacketHandlerBase<PlayerHitPacket>
    {
        public override PacketID ID
        {
            get { return PacketID.PlayerHit; }
        }

        protected override void HandlePacket(Client client, PlayerHitPacket packet)
        {
            /*try
            {
                if (client.Player.Owner != null)
                {
                    var enemy = client.Player.Owner.GetEntity(packet.ObjectId);
                    if (enemy == null || enemy is Player)
                        return;
                    Projectile proj;
                    if (
                        client.Player.Owner.Projectiles.TryGetValue(
                            new Tuple<int, byte>(packet.ObjectId, packet.BulletId), out proj))
                    {
                        foreach (ConditionEffect effect in proj.Descriptor.Effects)
                        {
                            client.Player.ApplyConditionEffect(new ConditionEffect
                            {
                                DurationMS = effect.DurationMS,
                                Effect = effect.Effect,
                                Range = effect.Range
                            });
                        }
                        client.Player.Damage(proj.Damage, proj.ProjectileOwner.Self, proj.Descriptor.ArmorPiercing);
                    }
                    else
                        Console.WriteLine("Can't register playerhit." + packet.ObjectId + " - " + packet.BulletId);
                }
            }
            catch
            {
            }*/
        }
    }
}