#region

using db;
using wServer.networking.svrPackets;

#endregion

namespace wServer.realm.entities
{
    public class GuildMerchant : SellableObject
    {
        public const int UP1 = 0x0736;
        public const int UP1C = 10000;
        public const int UP2 = 0x0737;
        public const int UP2C = 100000;
        public const int UP3 = 0x0738;
        public const int UP3C = 250000;
        public bool UseFame = true;
        public int nextLevel = 0;

        public GuildMerchant(RealmManager manager, ushort objType)
            : base(manager, objType)
        {
            RankReq = 0;
            Currency = CurrencyType.GuildFame;
            switch (objType)
            {
                case UP1:
                    Price = UP1C;
                    nextLevel = 1;
                    break;
                case UP2:
                    Price = UP2C;
                    nextLevel = 2;
                    break;
                case UP3:
                    Price = UP3C;
                    nextLevel = 3;
                    break;
            }
        }

        public override void Buy(Player player)
        {
            player.Manager.Data.AddPendingAction(db =>
            {
                if (db.GetGuild(db.GetGuildId(player.Guild)).GuildFame >= Price)
                {
                    player.Client.SendPacket(new BuyResultPacket
                    {
                        Message = "Upgrade successful! Please leave the Guild Hall to have it upgraded",
                        Result = 0
                    });
                }
                else
                {
                    player.SendHelp("FUCK");
                    player.Client.SendPacket(new BuyResultPacket
                    {
                        Message = "Not enough Guild Fame!",
                        Result = 9
                    });
                }
            });
            base.Buy(player);
        }
    }
}