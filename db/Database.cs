using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using log4net;
using MySql.Data.MySqlClient;
using Ionic.Zlib;

namespace db
{
    public partial class Database : IDisposable
    {
        private static ILog log = LogManager.GetLogger(typeof (Database));

        private static readonly string[] names =
        {
            "Darq", "Deyst", "Drac", "Drol",
            "Eango", "Eashy", "Eati", "Eendi", "Ehoni",
            "Gharr", "Iatho", "Iawa", "Idrae", "Iri", "Issz", "Itani",
            "Laen", "Lauk", "Lorz",
            "Oalei", "Odaru", "Oeti", "Orothi", "Oshyu",
            "Queq", "Radph", "Rayr", "Ril", "Rilr", "Risrr",
            "Saylt", "Scheev", "Sek", "Serl", "Seus",
            "Tal", "Tiar", "Uoro", "Urake", "Utanu",
            "Vorck", "Vorv", "Yangu", "Yimi", "Zhiar"
        };

        private readonly MySqlConnection con;

        public Database(string connStr)
        {
            con = new MySqlConnection(connStr);
            con.Open();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (con.State == System.Data.ConnectionState.Open)
                {
                    con.Close();
                    con.Dispose();
                }
            }
            //GC.SuppressFinalize(this);//Updated
        }

        public string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public MySqlCommand CreateQuery()
        {
            return con.CreateCommand();
        }

        public static int DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (int) (dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        public string GetAcc(string uuid, string password)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT email WHERE uuid=@uuid AND password=@pass LIMIT 1";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@pass", password);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return "";
                rdr.Read();
                return rdr.GetString("email");
            }
        }

        public void InsertEmail(string uuid, string password, string hash)
        {
            MySqlCommand cmd = CreateQuery();
			
			cmd.CommandText = @"INSERT INTO emails(accId, name, email, accessKey)
VALUES(@accId, @name, @email, @accessKey) 
ON DUPLICATE KEY UPDATE 
accessKey = @accessKey;";
			cmd.Parameters.AddWithValue("@accId", GetAccInfo(uuid, 1));
			cmd.Parameters.AddWithValue("@name", uuid);
			cmd.Parameters.AddWithValue("@email", GetAccInfo(uuid, 3));
			cmd.Parameters.AddWithValue("@accessKey", hash);
			
			cmd.ExecuteNonQuery();
        }

        public string GetAccInfo(string guid, int type)
        {
            string info = "";
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT id, name, email FROM accounts WHERE uuid=@uuid LIMIT 1";
            cmd.Parameters.AddWithValue("@uuid", guid);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return "";
                rdr.Read();
                if (type == 1)
                    info = rdr.GetInt32("id").ToString();
                if (type == 2)
                    info = rdr.GetString("name");
                if (type == 3)
                    info = rdr.GetString("email");
                return info;
            }
        }

        public bool IsUserInLegends(int AccountId)
        {
            //Week
            var cmd = CreateQuery();
            cmd.CommandText = "SELECT * FROM death WHERE (time >= DATE_SUB(NOW(), INTERVAL 1 WEEK)) ORDER BY totalFame DESC LIMIT 10;";
            using (var rdr = cmd.ExecuteReader())
                while (rdr.Read())
                    if (rdr.GetInt32("accId") == AccountId) return true;

            //Month
            cmd = CreateQuery();
            cmd.CommandText = "SELECT * FROM death WHERE (time >= DATE_SUB(NOW(), INTERVAL 1 MONTH)) ORDER BY totalFame DESC LIMIT 10;";
            using (var rdr = cmd.ExecuteReader())
                while (rdr.Read())
                    if (rdr.GetInt32("accId") == AccountId) return true;

            //All Time
            cmd = CreateQuery();
            cmd.CommandText = "SELECT * FROM death WHERE TRUE ORDER BY totalFame DESC LIMIT 10;";
            using (var rdr = cmd.ExecuteReader())
                while (rdr.Read())
                    if (rdr.GetInt32("accId") == AccountId) return true;

            return false;
        }

        public string GetUserEmail(string name)
        {
            string info = "";
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT email FROM accounts WHERE name=@name LIMIT 1";
            cmd.Parameters.AddWithValue("@name", name);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return "";
                rdr.Read();

                info = rdr.GetString("email");

                return info;
            }
        }

        public List<NewsItem> GetNews(XmlData dat, Account acc)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT icon, title, text, link, date FROM news ORDER BY date LIMIT 7;";
            var ret = new List<NewsItem>();
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    ret.Add(new NewsItem
                    {
                        Icon = rdr.GetString("icon"),
                        Title = rdr.GetString("title"),
                        TagLine = rdr.GetString("text"),
                        Link = rdr.GetString("link"),
                        Date = DateTimeToUnixTimestamp(rdr.GetDateTime("date")),
                    });
            }
            if (acc != null)
            {
                cmd.CommandText = @"SELECT charId, characters.charType, level, death.totalFame, time
FROM characters, death
WHERE dead = TRUE AND
characters.accId=@accId AND death.accId=@accId
AND characters.charId=death.chrId;";
                cmd.Parameters.AddWithValue("@accId", acc.AccountId);
                using (MySqlDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                        ret.Add(new NewsItem
                        {
                            Icon = "fame",
                            Title = string.Format("Your {0} died at level {1}",
                                dat.ObjectTypeToId[(ushort) rdr.GetInt32("charType")],
                                rdr.GetInt32("level")),
                            TagLine = string.Format("You earned {0} glorious Fame",
                                rdr.GetInt32("totalFame")),
                            Link = "fame:" + rdr.GetInt32("charId"),
                            Date = DateTimeToUnixTimestamp(rdr.GetDateTime("time")),
                        });
                }
            }
            ret.Sort((a, b) => -Comparer<int>.Default.Compare(a.Date, b.Date));
            return ret.Take(20).ToList();
        }

        public static Account CreateGuestAccount(string uuid)
        {
            return new Account
            {
                Name = names[(uint) uuid.GetHashCode()%names.Length],
                AccountId = 0,
                Admin = false,
                BeginnerPackageTimeLeft = 604800,
                Converted = false,
                Credits = 0,
                Souls = 0,
                Guild = new Guild
                {
                    Name = "",
                    Id = 0,
                    Rank = 0
                },
                NameChosen = false,
                NextCharSlotPrice = 100,
                VerifiedEmail = false,
                Stats = new Stats
                {
                    BestCharFame = 0,
                    ClassStates = new List<ClassStats>(),
                    Fame = 0,
                    TotalFame = 0
                },
                Vault = new VaultData
                {
                    Chests = new List<VaultChest>()
                }
            };
        }

        public Account Verify(string uuid, string password)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText =
                "SELECT id, name, rank, tag, namechosen, verified, guild, guildRank, guildFame, maxCharSlot, locked, ignored, guest, banned, muted, beginnerPackageTimeLeft FROM accounts WHERE uuid=@uuid AND password=SHA1(@password);";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@password", password);
            Account ret;
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return null;
                rdr.Read();
                ret = new Account
                {
                    Name = rdr.GetString("name"),
                    AccountId = rdr.GetInt32("id"),
                    Admin = rdr.GetInt32("rank") >= 2,
                    Rank = rdr.GetInt32("rank"),
                    Tag = rdr.GetString("tag"),
                    Muted = rdr.GetBoolean("muted"),
                    BeginnerPackageTimeLeft = rdr.GetInt32("beginnerPackageTimeLeft"),
                    Converted = false,
                    Guild = new Guild
                    {
                        Id = rdr.GetInt32("guild"),
                        Rank = rdr.GetInt32("guildRank")
                    },
                    NameChosen = rdr.GetBoolean("namechosen"),
                    NextCharSlotPrice = (rdr.GetInt32("maxCharSlot") < 10) ? rdr.GetInt32("maxCharSlot")*100 : 1000,
                    VerifiedEmail = rdr.GetBoolean("verified"),
                    _StarredAccounts = rdr.GetString("locked"),
                    _IgnoredAccounts = rdr.GetString("ignored"),
                    isGuest = rdr.GetBoolean("guest"),
                    isBanned = rdr.GetBoolean("banned")
                };
            }
            ReadStats(ret);
            ret.Guild.Name = GetGuildName(ret.Guild.Id);
            return ret;
        }

        public Account Register(string uuid, string password, string email, bool isGuest)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT COUNT(id) FROM accounts WHERE uuid=@uuid;";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            if ((int) (long) cmd.ExecuteScalar() > 0) return null;

            cmd = CreateQuery();
            cmd.CommandText =
                "INSERT INTO accounts(uuid, password, name, email, guest, maxCharSlot, regTime) VALUES(@uuid, SHA1(@password), @name, @email, @guest, 1, now());";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@name", names[(uint) uuid.GetHashCode()%names.Length]);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@guest", isGuest);
            int v = cmd.ExecuteNonQuery();
            bool ret = v > 0;

            if (ret)
            {
                cmd = CreateQuery();
                cmd.CommandText = "SELECT last_insert_id();";
                int accId = Convert.ToInt32(cmd.ExecuteScalar());

                cmd = CreateQuery();
                cmd.CommandText =
                    "INSERT INTO stats(accId, fame, totalFame, credits, totalCredits, souls, totalSouls) VALUES(@accId, 0, 0, 1000, 1000, 0, 0);";
                cmd.Parameters.AddWithValue("@accId", accId);
                cmd.ExecuteNonQuery();

                cmd = CreateQuery();
                cmd.CommandText =
                    "INSERT INTO vaults(accId, items, itemDatas) VALUES(@accId, '65535, 65535, 65535, 65535, 65535, 65535, 65535, 65535', @datas);";
                cmd.Parameters.AddWithValue("@accId", accId);
                cmd.Parameters.AddWithValue("@datas", ItemDataList.GetJson(new ItemData[8]));
                cmd.ExecuteNonQuery();
            }
            return Verify(uuid, password);
        }

        public bool HasUuid(string uuid)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT COUNT(id) FROM accounts WHERE uuid=@uuid;";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            return (int) (long) cmd.ExecuteScalar() > 0;
        }

        public bool HasEmail(string email)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT COUNT(id) FROM accounts WHERE email=@email;";
            cmd.Parameters.AddWithValue("@email", email);
            return (int) (long) cmd.ExecuteScalar() > 0;
        }

        public Account GetAccount(int id)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText =
                "SELECT id, name, rank, tag, namechosen, verified, guild, guildRank, maxCharSlot, guest, banned, locked, ignored, muted, beginnerPackageTimeLeft FROM accounts WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", id);
            Account ret;
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return null;
                rdr.Read();
                ret = new Account
                {
                    Name = rdr.GetString(UppercaseFirst("name")),
                    AccountId = rdr.GetInt32("id"),
                    Admin = rdr.GetInt32("rank") >= 2,
                    Rank = rdr.GetInt32("rank"),
                    Tag = rdr.GetString("tag"),
                    Muted = rdr.GetBoolean("muted"),
                    BeginnerPackageTimeLeft = rdr.GetInt32("beginnerPackageTimeLeft"),
                    Converted = false,
                    Guild = new Guild
                    {
                        Id = rdr.GetInt32("guild"),
                        Rank = rdr.GetInt32("guildRank")
                    },
                    NameChosen = rdr.GetBoolean("namechosen"),
                    NextCharSlotPrice = (rdr.GetInt32("maxCharSlot") < 10) ? rdr.GetInt32("maxCharSlot")*100 : 1000,
                    VerifiedEmail = rdr.GetBoolean("verified"),
                    _StarredAccounts = rdr.GetString("locked"),
                    _IgnoredAccounts = rdr.GetString("ignored"),
                    isGuest = rdr.GetBoolean("guest"),
                    isBanned = rdr.GetBoolean("banned")
                };
            }
            ReadStats(ret);
            ret.Guild.Name = GetGuildName(ret.Guild.Id);
            return ret;
        }

        public Account GetAccount(string name)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText =
                "SELECT id, name, rank, tag, namechosen, verified, guild, guildRank, maxCharSlot, guest, banned, locked, ignored, muted, beginnerPackageTimeLeft FROM accounts WHERE name=@name;";
            cmd.Parameters.AddWithValue("@name", name);
            Account ret;
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return null;
                rdr.Read();
                ret = new Account
                {
                    Name = rdr.GetString(UppercaseFirst("name")),
                    AccountId = rdr.GetInt32("id"),
                    Admin = rdr.GetInt32("rank") >= 2,
                    Rank = rdr.GetInt32("rank"),
                    Tag = rdr.GetString("tag"),
                    Muted = rdr.GetBoolean("muted"),
                    BeginnerPackageTimeLeft = rdr.GetInt32("beginnerPackageTimeLeft"),
                    Converted = false,
                    Guild = new Guild
                    {
                        Id = rdr.GetInt32("guild"),
                        Rank = rdr.GetInt32("guildRank")
                    },
                    NameChosen = rdr.GetBoolean("namechosen"),
                    NextCharSlotPrice = (rdr.GetInt32("maxCharSlot") < 10) ? rdr.GetInt32("maxCharSlot")*100 : 1000,
                    VerifiedEmail = rdr.GetBoolean("verified"),
                    _StarredAccounts = rdr.GetString("locked"),
                    _IgnoredAccounts = rdr.GetString("ignored"),
                    isGuest = rdr.GetBoolean("guest"),
                    isBanned = rdr.GetBoolean("banned")
                };
            }
            ReadStats(ret);
            ret.Guild.Name = GetGuildName(ret.Guild.Id);
            return ret;
        }

        public int MaxCharSlotPrice(string uuid)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT maxCharSlot FROM accounts WHERE uuid=@uuid";
            cmd.Parameters.AddWithValue("@uuid", uuid);
            var result = (int) cmd.ExecuteScalar();
            if (result < 10)
                return result*100;
            return 1000;
        }

        public int UpdateCredit(Account acc, int amount)
        {
            MySqlCommand cmd = CreateQuery();
            if (amount > 0)
            {
                cmd.CommandText = "UPDATE stats SET totalCredits = totalCredits + @amount WHERE accId=@accId;";
                cmd.Parameters.AddWithValue("@accId", acc.AccountId);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.ExecuteNonQuery();
                cmd = CreateQuery();
            }
            cmd.CommandText = @"UPDATE stats SET credits = credits + (@amount) WHERE accId=@accId;
SELECT credits FROM stats WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@amount", amount);
            return (int) cmd.ExecuteScalar();
        }

        public int UpdateFame(Account acc, int amount)
        {
            MySqlCommand cmd = CreateQuery();
            if (amount > 0)
            {
                cmd.CommandText = "UPDATE stats SET totalFame = totalFame + @amount WHERE accId=@accId;";
                cmd.Parameters.AddWithValue("@accId", acc.AccountId);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.ExecuteNonQuery();
                cmd = CreateQuery();
            }
            cmd.CommandText = @"UPDATE stats SET fame = fame + (@amount) WHERE accId=@accId;
SELECT fame FROM stats WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@amount", amount);
            return (int) cmd.ExecuteScalar();
        }

        public int UpdateSouls(Account acc, int amount)
        {
            MySqlCommand cmd = CreateQuery();
            if (amount > 0)
            {
                cmd.CommandText = "UPDATE stats SET totalSouls = totalSouls + @amount WHERE accId=@accId;";
                cmd.Parameters.AddWithValue("@accId", acc.AccountId);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.ExecuteNonQuery();
                cmd = CreateQuery();
            }
            cmd.CommandText = @"UPDATE stats SET souls = souls + (@amount) WHERE accId=@accId;
SELECT souls FROM stats WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@amount", amount);
            return (int) cmd.ExecuteScalar();
        }

        public void ReadStats(Account acc)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT fame, totalFame, credits, souls FROM stats WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.HasRows)
                {
                    rdr.Read();
                    acc.Credits = rdr.GetInt32("credits");
                    acc.Stats = new Stats
                    {
                        Fame = rdr.GetInt32("fame"),
                        TotalFame = rdr.GetInt32("totalFame")
                    };
                    acc.Souls = rdr.GetInt32("souls");
                }
                else
                {
                    acc.Credits = 0;
                    acc.Stats = new Stats
                    {
                        Fame = 0,
                        TotalFame = 0,
                        BestCharFame = 0,
                        ClassStates = new List<ClassStats>()
                    };
                    acc.Souls = 0;
                }
            }

            acc.Stats.ClassStates = ReadClassStates(acc);
            if (acc.Stats.ClassStates.Count > 0)
                acc.Stats.BestCharFame = acc.Stats.ClassStates.Max(_ => _.BestFame);
            acc.Vault = ReadVault(acc);
        }

        public List<ClassStats> ReadClassStates(Account acc)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT objType, bestLv, bestFame FROM classstats WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            var ret = new List<ClassStats>();
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    ret.Add(new ClassStats
                    {
                        ObjectType = rdr.GetInt32("objType"),
                        BestFame = rdr.GetInt32("bestFame"),
                        BestLevel = rdr.GetInt32("bestLv")
                    });
            }
            return ret;
        }

        public VaultData ReadVault(Account acc)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT chestId, items, itemDatas FROM vaults WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (rdr.HasRows)
                {
                    var ret = new VaultData {Chests = new List<VaultChest>()};
                    while (rdr.Read())
                    {
                        ret.Chests.Add(new VaultChest
                        {
                            ChestId = rdr.GetInt32("chestId"),
                            _Items = rdr.GetString("items"),
                            _Datas = rdr.GetString("itemDatas")
                        });
                    }
                    return ret;
                }
                return new VaultData
                {
                    Chests = new List<VaultChest>()
                };
            }
        }

        public void SaveChest(Account acc, VaultChest chest)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "UPDATE vaults SET items=@items, itemDatas=@itemDatas WHERE accId=@accId AND chestId=@chestId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@chestId", chest.ChestId);
            cmd.Parameters.AddWithValue("@items", chest._Items);
            cmd.Parameters.AddWithValue("@itemDatas", chest._Datas);
            cmd.ExecuteNonQuery();
        }

        public VaultChest CreateChest(Account acc)
        {
            MySqlCommand cmd = CreateQuery();

            cmd.CommandText =
                @"INSERT INTO vaults(accId, items, itemDatas) VALUES(@accId, '65535, 65535, 65535, 65535, 65535, 65535, 65535, 65535', @itemDatas);
SELECT MAX(chestId) FROM vaults WHERE accId = @accId;UPDATE accounts SET vaultCount=vaultCount+1 WHERE id=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@itemDatas", ItemDataList.GetJson(new ItemData[12]));
            return new VaultChest
            {
                ChestId = (int) cmd.ExecuteScalar(),
                _Items = "65535, 65535, 65535, 65535, 65535, 65535, 65535, 65535",
                _Datas = ItemDataList.GetJson(new ItemData[8])
            };
        }

        public void GetCharData(Account acc, Chars chrs)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT IFNULL(MAX(id), 0) + 1 FROM characters WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            chrs.NextCharId = (int) (long) cmd.ExecuteScalar();

            cmd = CreateQuery();
            cmd.CommandText = "SELECT maxCharSlot FROM accounts WHERE id=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            chrs.MaxNumChars = (int) cmd.ExecuteScalar();
        }

        public int GetNextCharID(Account acc)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT IFNULL(MAX(id), 0) + 1 FROM characters WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            var ret = (int) (long) cmd.ExecuteScalar();
            return ret;
        }

        public bool AddLock(int accId, int lockId)
        {
            List<int> x = GetLockeds(accId);
            x.Add(lockId);
            string s = Utils.GetCommaSepString(x.ToArray());
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "UPDATE accounts SET locked=@newlocked WHERE id=@accId";
            cmd.Parameters.AddWithValue("@newlocked", s);
            cmd.Parameters.AddWithValue("@accId", accId);
            if (cmd.ExecuteNonQuery() == 0)
                return false;
            cmd.Dispose();
            return true;

        }

        public bool RemoveLock(int accId, int lockId)
        {
            List<int> x = GetLockeds(accId);
            x.Remove(lockId);
            string s = Utils.GetCommaSepString(x.ToArray());
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "UPDATE accounts SET locked=@newlocked WHERE id=@accId";
            cmd.Parameters.AddWithValue("@newlocked", s);
            cmd.Parameters.AddWithValue("@accId", accId);
            if (cmd.ExecuteNonQuery() == 0)
                return false;
            return true;
        }

        public bool AddIgnore(int accId, int ignoreId)
        {
            List<int> x = GetIgnoreds(accId);
            x.Add(ignoreId);
            string s = Utils.GetCommaSepString(x.ToArray());
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "UPDATE accounts SET ignored=@newignored WHERE id=@accId";
            cmd.Parameters.AddWithValue("@newignored", s);
            cmd.Parameters.AddWithValue("@accId", accId);
            if (cmd.ExecuteNonQuery() == 0)
                return false;
            return true;
        }

        public bool RemoveIgnore(int accId, int ignoreId)
        {
            List<int> x = GetIgnoreds(accId);
            x.Remove(ignoreId);
            string s = Utils.GetCommaSepString(x.ToArray());
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "UPDATE accounts SET ignored=@newignored WHERE id=@accId";
            cmd.Parameters.AddWithValue("@newignored", s);
            cmd.Parameters.AddWithValue("@accId", accId);
            if (cmd.ExecuteNonQuery() == 0)
                return false;
            return true;
        }

        public List<int> GetLockeds(int accId)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT locked FROM accounts WHERE id=@accId";
            cmd.Parameters.AddWithValue("@accid", accId);
            try
            {
                return cmd.ExecuteScalar().ToString().Split(',').Select(int.Parse).ToList();

            }
            catch
            {
                return new List<int>();
            }
        }

        public List<int> GetIgnoreds(int accId)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT ignored FROM accounts WHERE id=@accId";
            cmd.Parameters.AddWithValue("@accid", accId);
            try
            {
                return cmd.ExecuteScalar().ToString().Split(',').Select(int.Parse).ToList();
            }
            catch
            {
                return new List<int>();
            }
        }

        public void LoadCharacters(Account acc, Chars chrs)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT * FROM characters WHERE accId=@accId AND dead = FALSE;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    int[] stats = Utils.FromCommaSepString32(rdr.GetString("stats"));
                    chrs.Characters.Add(new Char
                    {
                        ObjectType = (short) rdr.GetInt32("charType"),
                        CharacterId = rdr.GetInt32("charId"),
                        Level = rdr.GetInt32("level"),
                        Exp = rdr.GetInt32("exp"),
                        CurrentFame = rdr.GetInt32("fame"),
                        _Equipment = rdr.GetString("items"),
                        _EquipData = rdr.GetString("itemDatas"),
                        MaxHitPoints = stats[0],
                        HitPoints = rdr.GetInt32("hp"),
                        MaxMagicPoints = stats[1],
                        MagicPoints = rdr.GetInt32("mp"),
                        Attack = stats[2],
                        Defense = stats[3],
                        Speed = stats[4],
                        HpRegen = stats[5],
                        MpRegen = stats[6],
                        Dexterity = stats[7],
                        Tex1 = rdr.GetInt32("tex1"),
                        Tex2 = rdr.GetInt32("tex2"),
                        Effect = rdr.GetString("effect"),
                        Skin = rdr.GetInt32("skin"),
                        PermaSkin = rdr.GetInt32("permaSkin"),
                        XpBoost = rdr.GetInt32("xpboost"),
                        Dead = false,
                        PCStats = rdr.GetString("fameStats"),
                        Pet = rdr.GetInt32("pet"),
                        Floors = rdr.GetInt32("floors")
                    });
                }
            }
        }

        public static Char CreateCharacter(XmlData dat, ushort type, int chrId)
        {
            XElement cls = dat.ObjectTypeToElement[type];
            if (cls == null) return null;
            return new Char
            {
                ObjectType = type,
                CharacterId = chrId,
                Level = 1,
                Exp = 0,
                CurrentFame = 0,
                _Equipment = cls.Element("Equipment").Value,
                EquipData = new ItemData[12],
                MaxHitPoints = int.Parse(cls.Element("MaxHitPoints").Value),
                HitPoints = int.Parse(cls.Element("MaxHitPoints").Value),
                MaxMagicPoints = int.Parse(cls.Element("MaxMagicPoints").Value),
                MagicPoints = int.Parse(cls.Element("MaxMagicPoints").Value),
                Attack = int.Parse(cls.Element("Attack").Value),
                Defense = int.Parse(cls.Element("Defense").Value),
                Speed = int.Parse(cls.Element("Speed").Value),
                Dexterity = int.Parse(cls.Element("Dexterity").Value),
                HpRegen = int.Parse(cls.Element("HpRegen").Value),
                MpRegen = int.Parse(cls.Element("MpRegen").Value),
                Tex1 = 0,
                Tex2 = 0,
                Effect = "",
                Skin = -1,
                PermaSkin = 0,
                XpBoost = 0,
                Dead = false,
                PCStats = "",
                FameStats = new FameStats(),
                Pet = -1,
                Floors = 0
            };
        }

        public Char LoadCharacter(Account acc, int charId)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = "SELECT * FROM characters WHERE accId=@accId AND charId=@charId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@charId", charId);
            using (MySqlDataReader rdr = cmd.ExecuteReader())
            {
                if (!rdr.HasRows) return null;
                rdr.Read();
                int[] stats = Utils.FromCommaSepString32(rdr.GetString("stats"));
                var ret = new Char
                {
                    ObjectType = (short) rdr.GetInt32("charType"),
                    CharacterId = rdr.GetInt32("charId"),
                    Level = rdr.GetInt32("level"),
                    Exp = rdr.GetInt32("exp"),
                    CurrentFame = rdr.GetInt32("fame"),
                    _Equipment = rdr.GetString("items"),
                    _EquipData = rdr.GetString("itemDatas"),
                    MaxHitPoints = stats[0],
                    HitPoints = rdr.GetInt32("hp"),
                    MaxMagicPoints = stats[1],
                    MagicPoints = rdr.GetInt32("mp"),
                    Attack = stats[2],
                    Defense = stats[3],
                    Speed = stats[4],
                    HpRegen = stats[5],
                    MpRegen = stats[6],
                    Dexterity = stats[7],
                    Tex1 = rdr.GetInt32("tex1"),
                    Tex2 = rdr.GetInt32("tex2"),
                    Effect = rdr.GetString("effect"),
                    Skin = rdr.GetInt32("skin"),
                    PermaSkin = rdr.GetInt32("permaSkin"),
                    XpBoost = rdr.GetInt32("xpboost"),
                    Dead = rdr.GetBoolean("dead"),
                    Pet = rdr.GetInt32("pet"),
                    PCStats = rdr.GetString("fameStats"),
                    Floors = rdr.GetInt32("floors")
                };
                ret.FameStats = new FameStats();
                if (!string.IsNullOrEmpty(ret.PCStats))
                    ret.FameStats.Read(
                        ZlibStream.UncompressBuffer(
                            Convert.FromBase64String(ret.PCStats.Replace('-', '+').Replace('_', '/'))));
                return ret;
            }
        }

        public void SaveCharacter(Account acc, Char chr)
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = @"UPDATE characters SET 
level=@level, 
exp=@exp, 
fame=@fame, 
items=@items, 
itemDatas=@itemDatas,
stats=@stats, 
hp=@hp, 
mp=@mp, 
tex1=@tex1, 
tex2=@tex2, 
effect=@effect,
skin=@skin,
permaSkin=@pskin,
pet=@pet, 
floors=@floors,
xpboost=@xpboost,
fameStats=@fameStats 
WHERE accId=@accId AND charId=@charId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@charId", chr.CharacterId);

            cmd.Parameters.AddWithValue("@level", chr.Level);
            cmd.Parameters.AddWithValue("@exp", chr.Exp);
            cmd.Parameters.AddWithValue("@fame", chr.CurrentFame);
            cmd.Parameters.AddWithValue("@items", chr._Equipment);
            cmd.Parameters.AddWithValue("@itemDatas", chr._EquipData);
            cmd.Parameters.AddWithValue("@stats", Utils.GetCommaSepString(new[]
            {
                chr.MaxHitPoints,
                chr.MaxMagicPoints,
                chr.Attack,
                chr.Defense,
                chr.Speed,
                chr.HpRegen,
                chr.MpRegen,
                chr.Dexterity
            }));
            cmd.Parameters.AddWithValue("@hp", chr.HitPoints);
            cmd.Parameters.AddWithValue("@mp", chr.MagicPoints);
            cmd.Parameters.AddWithValue("@tex1", chr.Tex1);
            cmd.Parameters.AddWithValue("@tex2", chr.Tex2);
            cmd.Parameters.AddWithValue("@effect", chr.Effect);
            cmd.Parameters.AddWithValue("@skin", chr.Skin);
            cmd.Parameters.AddWithValue("@pskin", chr.PermaSkin);
            cmd.Parameters.AddWithValue("@pet", chr.Pet);
            cmd.Parameters.AddWithValue("@floors", chr.Floors);
            cmd.Parameters.AddWithValue("@xpboost", chr.XpBoost);
            chr.PCStats =
                Convert.ToBase64String(ZlibStream.CompressBuffer(chr.FameStats.Write()))
                    .Replace('+', '-')
                    .Replace('/', '_');
            cmd.Parameters.AddWithValue("@fameStats", chr.PCStats);
            cmd.ExecuteNonQuery();

            cmd = CreateQuery();
            cmd.CommandText = @"INSERT INTO classstats(accId, objType, bestLv, bestFame) 
VALUES(@accId, @objType, @bestLv, @bestFame) 
ON DUPLICATE KEY UPDATE 
bestLv = GREATEST(bestLv, @bestLv), 
bestFame = GREATEST(bestFame, @bestFame);";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@objType", chr.ObjectType);
            cmd.Parameters.AddWithValue("@bestLv", chr.Level);
            cmd.Parameters.AddWithValue("@bestFame", chr.CurrentFame);
            cmd.ExecuteNonQuery();
        }

        public void Death(XmlData dat, Account acc, Char chr, string killer) //Save first
        {
            MySqlCommand cmd = CreateQuery();
            cmd.CommandText = @"UPDATE characters SET 
dead=TRUE, 
deathTime=NOW() 
WHERE accId=@accId AND charId=@charId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@charId", chr.CharacterId);
            cmd.ExecuteNonQuery();

            bool firstBorn;
            int finalFame = chr.FameStats.CalculateTotal(dat, acc, chr, chr.CurrentFame, out firstBorn);

            cmd = CreateQuery();
            cmd.CommandText = @"UPDATE stats SET 
fame=fame+@amount, 
totalFame=totalFame+@amount 
WHERE accId=@accId;";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@amount", finalFame);
            cmd.ExecuteNonQuery();

            cmd = CreateQuery();
            cmd.CommandText =
                @"INSERT INTO death(accId, chrId, name, charType, tex1, tex2, skin, items, fame, fameStats, totalFame, firstBorn, killer) 
VALUES(@accId, @chrId, @name, @objType, @tex1, @tex2, @skin, @items, @fame, @fameStats, @totalFame, @firstBorn, @killer);";
            cmd.Parameters.AddWithValue("@accId", acc.AccountId);
            cmd.Parameters.AddWithValue("@chrId", chr.CharacterId);
            cmd.Parameters.AddWithValue("@name", acc.Name);
            cmd.Parameters.AddWithValue("@objType", chr.ObjectType);
            cmd.Parameters.AddWithValue("@tex1", chr.Tex1);
            cmd.Parameters.AddWithValue("@tex2", chr.Tex2);
            cmd.Parameters.AddWithValue("@skin", chr.Skin);
            cmd.Parameters.AddWithValue("@items", chr._Equipment);
            cmd.Parameters.AddWithValue("@fame", chr.CurrentFame);
            cmd.Parameters.AddWithValue("@fameStats", chr.PCStats);
            cmd.Parameters.AddWithValue("@totalFame", finalFame);
            cmd.Parameters.AddWithValue("@firstBorn", firstBorn);
            cmd.Parameters.AddWithValue("@killer", killer);
            cmd.ExecuteNonQuery();
        }
    }
}