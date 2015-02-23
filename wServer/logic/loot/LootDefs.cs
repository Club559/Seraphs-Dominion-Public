using db;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using wServer.networking.svrPackets;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic.loot
{
    public interface ILootDef
    {
        void Populate(RealmManager manager, Enemy enemy, Tuple<Player, int> playerDat,
            Random rand, IList<LootDef> lootDefs);
    }

    public class ItemLoot : ILootDef
    {
        private readonly string item;
        private readonly double probability;

        public ItemLoot(string item, double probability)
        {
            this.item = item;
            this.probability = probability;
        }

        public void Populate(RealmManager manager, Enemy enemy, Tuple<Player, int> playerDat,
            Random rand, IList<LootDef> lootDefs)
        {
            if (playerDat != null) return;
            XmlData dat = manager.GameData;
            lootDefs.Add(new LootDef(dat.Items[dat.IdToObjectType[item]], probability));
        }
    }

    public enum ItemType
    {
        Weapon,
        Ability,
        Armor,
        Ring,
        Potion
    }

    public class TierLoot : ILootDef
    {
        public static readonly int[] WeaponT = {1, 2, 3, 8, 17, 24};
        public static readonly int[] AbilityT = {4, 5, 11, 12, 13, 15, 16, 18, 19, 20, 21, 22, 23, 25};
        public static readonly int[] ArmorT = {6, 7, 14};
        public static readonly int[] RingT = {9};
        public static readonly int[] PotionT = {10};
        private readonly double probability;

        private readonly byte tier;
        private readonly int[] types;

        public TierLoot(byte tier, ItemType type, double probability)
        {
            this.tier = tier;
            switch (type)
            {
                case ItemType.Weapon:
                    types = WeaponT;
                    break;
                case ItemType.Ability:
                    types = AbilityT;
                    break;
                case ItemType.Armor:
                    types = ArmorT;
                    break;
                case ItemType.Ring:
                    types = RingT;
                    break;
                case ItemType.Potion:
                    types = PotionT;
                    break;
                default:
                    throw new NotSupportedException(type.ToString());
            }
            this.probability = probability;
        }

        public void Populate(RealmManager manager, Enemy enemy, Tuple<Player, int> playerDat,
            Random rand, IList<LootDef> lootDefs)
        {
            if (playerDat != null) return;
            Item[] candidates = manager.GameData.Items
                .Where(item => Array.IndexOf(types, item.Value.SlotType) != -1)
                .Where(item => item.Value.Tier == tier)
                .Select(item => item.Value)
                .ToArray();
            foreach (Item i in candidates)
                lootDefs.Add(new LootDef(i, probability/candidates.Length));
        }
    }

    public class Threshold : ILootDef
    {
        private readonly ILootDef[] children;
        private readonly double threshold;

        public Threshold(double threshold, params ILootDef[] children)
        {
            this.threshold = threshold;
            this.children = children;
        }

        public void Populate(RealmManager manager, Enemy enemy, Tuple<Player, int> playerDat,
            Random rand, IList<LootDef> lootDefs)
        {
            if (playerDat != null && playerDat.Item2/(double) enemy.ObjectDesc.MaxHP >= threshold)
            {
                foreach (ILootDef i in children)
                    i.Populate(manager, enemy, null, rand, lootDefs);
            }
        }
    }

    public class SoulLoot : ILootDef
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SoulLoot));

        private readonly int maxSouls;
        private readonly int minSouls;

        public SoulLoot(int minSouls, int maxSouls)
        {
            this.maxSouls = maxSouls;
            this.minSouls = minSouls;
        }

        public void Populate(RealmManager manager, Enemy enemy, Tuple<Player, int> playerDat,
            Random rand, IList<LootDef> lootDefs)
        {
            if (playerDat == null)
                return;
            for (int i = 0; i < 3; i++ )
                playerDat.Item1.Owner.BroadcastPacket(new ShowEffectPacket()
                {
                    EffectType = EffectType.Flow,
                    Color = new ARGB(0xFF9999FF),
                    TargetId = playerDat.Item1.Id,
                    PosA = new Position() { X = enemy.X, Y = enemy.Y }
                }, null);
            manager.Data.AddPendingAction(db =>
            {
                playerDat.Item1.Souls =
                    playerDat.Item1.Client.Account.Souls =
                        db.UpdateSouls(playerDat.Item1.Client.Account,
                            rand.Next(minSouls, maxSouls + 1));
            });
            playerDat.Item1.UpdateCount++;
        }
    }
}