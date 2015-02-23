#region

using System;
using System.Collections.Generic;

#endregion

namespace wServer.realm.entities
{
    internal class MerchantList
    {
        public static Dictionary<int, Tuple<int, CurrencyType>> prices = new Dictionary<int, Tuple<int, CurrencyType>>
        {
            //////////////PRICES ONLY\\\\\\\\\\\\\
                
            //Rings t4
            {0xab8, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Defense
            {0xab7, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Attack
            {0xab9, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Speed
            {0xaba, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Vitality
            {0xabb, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Wisdom
            {0xabc, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Dexterity
            {0xabd, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Health
            {0xabe, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Magic
            
            //Armor t8
            {0xadc, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Robe
            {0xa12, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Heavy
            {0xa0e, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Leather
            
            //weapons t10
            {0xa19, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Dagger
            {0xa82, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Sword
            {0xa1e, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Bow
            {0xa9f, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Staff
            {0xa07, new Tuple<int, CurrencyType>(100, CurrencyType.Fame)}, //Wand
            
            //Abilities t4
            {0xad6, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, // Spell
            {0xa33, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Tome
            {0xa64, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Quiver
            {0xa59, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Cloak
            {0xa6a, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Helm
            {0xa0b, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Shield
            {0xa54, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Seal
            {0xaa7, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Poison
            {0xaae, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Skull
            {0xab5, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Trap
            {0xa45, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Orb
            {0xb1f, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Prism
            {0xb31, new Tuple<int, CurrencyType>(200, CurrencyType.Fame)}, //Scepter

            // Effusions
            {0xaef, new Tuple<int, CurrencyType>(500, CurrencyType.Fame)}, // Defense
            {0xaf0, new Tuple<int, CurrencyType>(400, CurrencyType.Fame)}, // Defense
            {0xaf1, new Tuple<int, CurrencyType>(400, CurrencyType.Fame)}, // Defense
            {0xaf2, new Tuple<int, CurrencyType>(400, CurrencyType.Fame)}, // Defense
            {0xaf3, new Tuple<int, CurrencyType>(400, CurrencyType.Fame)}, // Defense

            //randum stuff
            {0xb3e, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Amulet of Resurrection
            {0xaeb, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Greater Health Potion
            {0xaec, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Greater Magic Potion
            {0x161f, new Tuple<int, CurrencyType>(2000, CurrencyType.Gold)}, //Valentine

            // dyes (For now)
            // Clothing
            {0x1007, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Black
            {0x1009, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Blue
            {0x100b, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Brown
            {0x1010, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Coral
            {0x1012, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Cornsilk
            {0x1015, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Dark Blue
            {0x101f, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Dark Red
            {0x1002, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Aqua
            {0x1004, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Azure
            {0x1033, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Green
            {0x102f, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Ghost White
            {0x1079, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Sienna
            {0x1030, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Gold

            //Accessory
            {0x1107, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Black
            {0x1109, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Blue
            {0x110b, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Brown
            {0x1110, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Coral
            {0x1112, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Cornsilk
            {0x1115, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Dark Blue
            {0x111f, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Dark Red
            {0x1102, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Aqua
            {0x1104, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Azure
            {0x1133, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Green
            {0x112f, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Ghost White
            {0x1179, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Sienna
            {0x1130, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //Gold

            {0xc0a, new Tuple<int, CurrencyType>(20, CurrencyType.Fame)}, //cronus
			
            {0x1500, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1501, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1502, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1503, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1504, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1505, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1506, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1507, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1508, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1509, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150a, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150b, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150c, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150d, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150e, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x150f, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1510, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1511, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1512, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1513, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1514, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1515, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1516, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1517, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1518, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1519, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151a, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151b, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151c, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151d, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151e, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x151f, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1520, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1521, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1522, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1523, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1524, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1525, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1526, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1527, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1528, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1529, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152a, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152b, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152c, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152d, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152e, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x152f, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1530, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1531, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1532, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1533, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1534, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1535, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1536, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1537, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1538, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1539, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153a, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153b, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153c, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153d, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153e, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x153f, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1540, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
            {0x1541, new Tuple<int, CurrencyType>(1000, CurrencyType.Fame)}, //Pets
        };

        public static int[] store1List =
        {
            0x701, 0x705, 0x706, 0x70a, 0x70b, 0x710, 0x71f, 0xc11, 0xc19, 0xc23, 0xc2e,
            0xc3d
        }; //keys

        public static int[] store2List = {0xa33}; // ???

        public static int[] store3List = {0xa19}; // weapons
        public static int[] store4List = {0xadc}; // armour
        public static int[] store5List = {0xaef}; // Consumables
        public static int[] store6List = {0xab8}; // Rings

        public static int[] store7List =
        {
            0x1107, 0x1109, 0x110b, 0x1110, 0x1112, 0x1115, 0x111f, 0x1102, 0x1104, 0x1133,
            0x112f, 0x1179, 0x1130
        }; // Small Dyes

        public static int[] store8List =
        {
            0x1007, 0x1009, 0x100b, 0x1010, 0x1012, 0x1015, 0x101f, 0x1002, 0x1004, 0x1033,
            0x102f, 0x1079, 0x1030
        }; // Large Dyes

        public static int[] store9List =
        {
            0x1500, 0x1501, 0x1502, 0x1503, 0x1504, 0x1505, 0x1506, 0x1507, 0x1508, 0x1509, 0x150A, 0x150B, 0x150C,
            0x150D, 0x150E, 0x150F, 0x1510, 0x1511, 0x1512, 0x1513, 0x1514, 0x1515, 0x1516, 0x1517, 0x1518, 0x1519,
            0x151A, 0x151B, 0x151C, 0x151D, 0x151E, 0x151F, 0x1520, 0x1521, 0x1522, 0x1523, 0x1524, 0x1525, 0x1526,
            0x1527, 0x1528, 0x1529, 0x152A, 0x152B, 0x152C, 0x152D, 0x152E, 0x152F, 0x1530, 0x1531, 0x1532, 0x1533,
            0x1534, 0x1535, 0x1536, 0x1537, 0x1538, 0x1539, 0x153A, 0x153B, 0x153C, 0x153D, 0x153E, 0x153F, 0x1540,
            0x1541
        }; // Pets

        public static Dictionary<string, int[]> shopLists = new Dictionary<string, int[]>();

        public static Random rand = new Random();
    }
}