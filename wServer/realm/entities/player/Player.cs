using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using log4net;
using wServer.logic;
using wServer.networking;
using wServer.networking.svrPackets;
using wServer.realm.terrain;
using db;

namespace wServer.realm.entities
{
    internal interface IPlayer
    {
        void Damage(int dmg, Entity chr, bool noDef, bool toSelf = false, float pvpReduction = 0.20f);
        bool IsVisibleToEnemy();
    }

    public partial class Player : Character, IContainer, IPlayer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Player));

        private readonly Client client;
        private FameCounter fames;
        private float hpRegenCounter;
        public bool isNotVisible = false;
        private float mpRegenCounter;
        private bool resurrecting;
        public StatsManager statsMgr;
        private byte[,] tiles;
        public bool usingShuriken = false;
        private int pingSerial;

        public Player(Client client)
            : base(client.Manager, (ushort) client.Character.ObjectType, client.Random)
        {
            this.client = client;
            statsMgr = new StatsManager(this);
            Name = client.Account.Name;
            AccountId = client.Account.AccountId;

            Name = client.Account.Name;
            Level = client.Character.Level;
            Experience = client.Character.Exp;
            ExperienceGoal = GetExpGoal(client.Character.Level);
            Stars = GetStars();
            Texture1 = client.Character.Tex1;
            Texture2 = client.Character.Tex2;
            Effect = client.Character.Effect;
            XmlEffect = "";
            Skin = client.Character.Skin;
            PermaSkin = client.Character.PermaSkin != 0;
            XpBoost = client.Character.XpBoost;
            Credits = client.Account.Credits;
            Souls = client.Account.Souls;
            NameChosen = client.Account.NameChosen;
            CurrentFame = client.Account.Stats.Fame;
            Fame = client.Character.CurrentFame;
            ClassStats state = client.Account.Stats.ClassStates.SingleOrDefault(_ => _.ObjectType == ObjectType);
            FameGoal = GetFameGoal(state != null ? state.BestFame : 0);

            Glowing = -1;
            Manager.Data.AddPendingAction(db =>
            {
                if (db.IsUserInLegends(AccountId))
                    Glowing = 0xFF0000;
                if (client.Account.Admin)
                    Glowing = 0xFF00FF;
            });
            Guild = client.Account.Guild.Name;
            GuildRank = client.Account.Guild.Rank;
            HP = client.Character.HitPoints;
            MP = client.Character.MagicPoints;
            Floors = client.Character.Floors;
            ConditionEffects = 0;
            OxygenBar = 100;

            Party = Party.GetParty(this);
            if(Party != null)
                if (Party.Leader.AccountId == AccountId)
                    Party.Leader = this;
                else
                    Party.Members.Add(this);

            if (HP <= 0)
                HP = client.Character.MaxHitPoints;

            Locked = client.Account.Locked ?? new List<int>();
            Ignored = client.Account.Ignored ?? new List<int>();
            try
            {
                Manager.Data.AddPendingAction(db =>
                {
                    Locked = db.GetLockeds(AccountId);
                    Ignored = db.GetIgnoreds(AccountId);
                });
            }
            catch
            {
            }

            Inventory = new Inventory(this,
                client.Character.Equipment
                    .Select(_ => _ == 0xffff ? null : client.Manager.GameData.Items[_])
                    .ToArray(),
                client.Character.EquipData);
            Inventory.InventoryChanged += (sender, e) => CalculateBoost();
            SlotTypes =
                Utils.FromCommaSepString32(
                    client.Manager.GameData.ObjectTypeToElement[ObjectType].Element("SlotTypes").Value);
            Stats = new[]
            {
                client.Character.MaxHitPoints,
                client.Character.MaxMagicPoints,
                client.Character.Attack,
                client.Character.Defense,
                client.Character.Speed,
                client.Character.HpRegen,
                client.Character.MpRegen,
                client.Character.Dexterity
            };

            Pet = null;
            
            for (int i = 0; i < SlotTypes.Length; i++)
                if (SlotTypes[i] == 0) SlotTypes[i] = 10;

            AddRecipes();
        }

        public Client Client
        {
            get { return client; }
        }

        //Stats
        public int AccountId { get; private set; }

        public Entity Pet { get; set; }

        public int Experience { get; set; }
        public int ExperienceGoal { get; set; }
        public int Level { get; set; }

        public List<int> Locked { get; set; }
        public List<int> Ignored { get; set; }

        public int CurrentFame { get; set; }
        public int Fame { get; set; }
        public int FameGoal { get; set; }
        public int Stars { get; set; }

        public string Guild { get; set; }
        public int GuildRank { get; set; }
        public bool Invited { get; set; }

        public int Credits { get; set; }
        public int Souls { get; set; }
        public bool NameChosen { get; set; }
        public int OxygenBar { get; set; }
        public int Texture1 { get; set; }
        public int Texture2 { get; set; }
        public int Skin { get; set; }
        public bool PermaSkin { get; set; }

        public int Glowing { get; set; }
        public int MP { get; set; }

        public int[] Stats { get; private set; }
        public int[] Boost { get; private set; }

        public int? XpBoost { get; set; }

        public bool PvP { get; set; }
        public int Team { get; set; }
        public bool CanNexus { get; set; }
        public int Floors { get; set; }
        public Party Party { get; set; }

        public string Effect { get; set; }
        public string XmlEffect { get; set; }

        public FameCounter FameCounter
        {
            get { return fames; }
        }

        public int[] SlotTypes { get; private set; }
        public Inventory Inventory { get; private set; }

        public void Damage(int dmg, Entity chr, bool noDef, bool toSelf = false, float pvpReduction = 0.20f)
        {
            try
            {
                if (HasConditionEffect(ConditionEffects.Paused) ||
                    HasConditionEffect(ConditionEffects.Stasis) ||
                    HasConditionEffect(ConditionEffects.Invincible))
                    return;

                dmg = (int) statsMgr.GetDefenseDamage(dmg, noDef);
                if (chr is Player)
                    dmg = Math.Max(1, (int)(dmg * pvpReduction));
                if (!HasConditionEffect(ConditionEffects.Invulnerable))
                    HP -= dmg;
                UpdateCount++;
                Owner.BroadcastPacket(new DamagePacket
                {
                    TargetId = Id,
                    Effects = 0,
                    Damage = (ushort) dmg,
                    Killed = HP <= 0,
                    BulletId = 0,
                    ObjectId = chr != null ? chr.Id : -1
                }, toSelf ? null : this);
                SaveToCharacter();

                string killerName = chr is Player
                    ? chr.Name
                    : chr != null ? (chr.ObjectDesc.DisplayId ?? chr.ObjectDesc.ObjectId) : "Unknown";

                if (HP <= 0)
                    Death(killerName);
            }
            catch (Exception e)
            {
                log.Error("Error while processing playerDamage: ", e);
            }
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            base.ExportStats(stats);
            stats[StatsType.AccountId] = AccountId;

            stats[StatsType.Experience] = Experience - GetLevelExp(Level);
            stats[StatsType.ExperienceGoal] = ExperienceGoal;
            stats[StatsType.Level] = Level;

            stats[StatsType.CurrentFame] = CurrentFame;
            stats[StatsType.Fame] = Fame;
            stats[StatsType.FameGoal] = FameGoal;
            stats[StatsType.Stars] = Stars;

            stats[StatsType.Guild] = Guild;
            stats[StatsType.GuildRank] = GuildRank;

            stats[StatsType.Credits] = Credits;
            stats[StatsType.Souls] = Souls;
            stats[StatsType.NameChosen] = NameChosen ? 1 : 0;
            stats[StatsType.Texture1] = Texture1;
            stats[StatsType.Texture2] = Texture2;
            stats[StatsType.Skin] = Skin;

            stats[StatsType.Glowing] = Glowing;
            stats[StatsType.HP] = HP;
            stats[StatsType.MP] = MP;

            stats[StatsType.InvData0] = (Inventory.Data[0] != null ? Inventory.Data[0].GetJson() : "{}");
            stats[StatsType.InvData1] = (Inventory.Data[1] != null ? Inventory.Data[1].GetJson() : "{}");
            stats[StatsType.InvData2] = (Inventory.Data[2] != null ? Inventory.Data[2].GetJson() : "{}");
            stats[StatsType.InvData3] = (Inventory.Data[3] != null ? Inventory.Data[3].GetJson() : "{}");
            stats[StatsType.InvData4] = (Inventory.Data[4] != null ? Inventory.Data[4].GetJson() : "{}");
            stats[StatsType.InvData5] = (Inventory.Data[5] != null ? Inventory.Data[5].GetJson() : "{}");
            stats[StatsType.InvData6] = (Inventory.Data[6] != null ? Inventory.Data[6].GetJson() : "{}");
            stats[StatsType.InvData7] = (Inventory.Data[7] != null ? Inventory.Data[7].GetJson() : "{}");
            stats[StatsType.InvData8] = (Inventory.Data[8] != null ? Inventory.Data[8].GetJson() : "{}");
            stats[StatsType.InvData9] = (Inventory.Data[9] != null ? Inventory.Data[9].GetJson() : "{}");
            stats[StatsType.InvData10] = (Inventory.Data[10] != null ? Inventory.Data[10].GetJson() : "{}");
            stats[StatsType.InvData11] = (Inventory.Data[11] != null ? Inventory.Data[11].GetJson() : "{}");

            stats[StatsType.Inventory0] = (Inventory[0] != null ? Inventory[0].ObjectType : -1);
            stats[StatsType.Inventory1] = (Inventory[1] != null ? Inventory[1].ObjectType : -1);
            stats[StatsType.Inventory2] = (Inventory[2] != null ? Inventory[2].ObjectType : -1);
            stats[StatsType.Inventory3] = (Inventory[3] != null ? Inventory[3].ObjectType : -1);
            stats[StatsType.Inventory4] = (Inventory[4] != null ? Inventory[4].ObjectType : -1);
            stats[StatsType.Inventory5] = (Inventory[5] != null ? Inventory[5].ObjectType : -1);
            stats[StatsType.Inventory6] = (Inventory[6] != null ? Inventory[6].ObjectType : -1);
            stats[StatsType.Inventory7] = (Inventory[7] != null ? Inventory[7].ObjectType : -1);
            stats[StatsType.Inventory8] = (Inventory[8] != null ? Inventory[8].ObjectType : -1);
            stats[StatsType.Inventory9] = (Inventory[9] != null ? Inventory[9].ObjectType : -1);
            stats[StatsType.Inventory10] = (Inventory[10] != null ? Inventory[10].ObjectType : -1);
            stats[StatsType.Inventory11] = (Inventory[11] != null ? Inventory[11].ObjectType : -1);

            if (Boost == null) CalculateBoost();

            stats[StatsType.MaximumHP] = Stats[0] + Boost[0];
            stats[StatsType.MaximumMP] = Stats[1] + Boost[1];
            stats[StatsType.Attack] = Stats[2] + Boost[2];
            stats[StatsType.Defense] = Stats[3] + Boost[3];
            stats[StatsType.Speed] = Stats[4] + Boost[4];
            stats[StatsType.Vitality] = Stats[5] + Boost[5];
            stats[StatsType.Wisdom] = Stats[6] + Boost[6];
            stats[StatsType.Dexterity] = Stats[7] + Boost[7];

            if (Owner != null && Owner.Name == "Ocean Trench")
                stats[StatsType.OxygenBar] = OxygenBar;

            stats[StatsType.HPBoost] = Boost[0];
            stats[StatsType.MPBoost] = Boost[1];
            stats[StatsType.AttackBonus] = Boost[2];
            stats[StatsType.DefenseBonus] = Boost[3];
            stats[StatsType.SpeedBonus] = Boost[4];
            stats[StatsType.VitalityBonus] = Boost[5];
            stats[StatsType.WisdomBonus] = Boost[6];
            stats[StatsType.DexterityBonus] = Boost[7];

            stats[StatsType.XpBoost] = XpBoost;

            stats[StatsType.PvP] = PvP ? 1 : 0;
            stats[StatsType.Team] = Team;
            stats[StatsType.CanNexus] = CanNexus ? 1 : 0;
            stats[StatsType.Party] = Party != null ? Party.ID : -1;
            stats[StatsType.PartyLeader] = Party != null ? (Party.Leader == this ? 1 : 0) : 0;

            stats[StatsType.Effect] = XmlEffect == "" ? UnusualEffects.GetXML(Effect) : XmlEffect;
        }

        public void SaveToCharacter()
        {
            Char chr = client.Character;
            chr.Exp = Experience;
            chr.Level = Level;
            chr.Tex1 = Texture1;
            chr.Tex2 = Texture2;
            chr.Effect = Effect;
            chr.Skin = Skin;
            chr.PermaSkin = PermaSkin ? 1 : 0;
            chr.Pet = (Pet != null ? Pet.ObjectType : -1);
            chr.CurrentFame = Fame;
            chr.HitPoints = HP;
            chr.MagicPoints = MP;
            chr.Equipment = Inventory.Select(_ => _ == null ? (ushort) 0xffff : _.ObjectType).ToArray();
            chr.EquipData = Inventory.Data;
            chr.MaxHitPoints = Stats[0];
            chr.MaxMagicPoints = Stats[1];
            chr.Attack = Stats[2];
            chr.Defense = Stats[3];
            chr.Speed = Stats[4];
            chr.HpRegen = Stats[5];
            chr.MpRegen = Stats[6];
            chr.Dexterity = Stats[7];
            chr.XpBoost = XpBoost;
            chr.Floors = Floors;
        }

        public void CalculateBoost()
        {
            if (Boost == null)
                Boost = new int[12];
            else
                for (int i = 0; i < Boost.Length; i++) Boost[i] = 0;
            for (int i = 0; i < 4; i++)
            {
                if (Inventory[i] == null) continue;
                foreach (var b in Inventory[i].StatsBoost)
                {
                    switch ((StatsType) b.Key)
                    {
                        case StatsType.MaximumHP:
                            Boost[0] += b.Value;
                            break;
                        case StatsType.MaximumMP:
                            Boost[1] += b.Value;
                            break;
                        case StatsType.Attack:
                            Boost[2] += b.Value;
                            break;
                        case StatsType.Defense:
                            Boost[3] += b.Value;
                            break;
                        case StatsType.Speed:
                            Boost[4] += b.Value;
                            break;
                        case StatsType.Vitality:
                            Boost[5] += b.Value;
                            break;
                        case StatsType.Wisdom:
                            Boost[6] += b.Value;
                            break;
                        case StatsType.Dexterity:
                            Boost[7] += b.Value;
                            break;
                    }
                }
            }
        }


        public override void Init(World owner)
        {
            Owner = owner;
            Move(0.5f, 0.5f);
            tiles = new byte[owner.Map.Width, owner.Map.Height];
            SetNewbiePeriod();
            base.Init(owner);
            tiles = new byte[owner.Map.Width, owner.Map.Height];
            fames = new FameCounter(this);
            if (client.Character.Pet >= 0)
                GivePet((ushort)client.Character.Pet);

            SendAccountList(Locked, Client.LOCKED_LIST_ID);
            SendAccountList(Ignored, Client.IGNORED_LIST_ID);

            CanNexus = Owner.AllowNexus;
            UpdateCount++;

            WorldTimer pingTimer = null;
            owner.Timers.Add(pingTimer = new WorldTimer(PING_PERIOD, (w, t) =>
            {
                if (Client.Stage == ProtocalStage.Ready)
                {
                    Client.SendPacket(new PingPacket {Serial = pingSerial++});
                    pingTimer.Reset();
                    Manager.Logic.AddPendingAction(_ => w.Timers.Add(pingTimer), PendingPriority.Creation);
                }
            }));
        }

        public override void Tick(RealmTime time)
        {
            try
            {
                if (client.Stage == ProtocalStage.Disconnected)
                {
                    Owner.LeaveWorld(this);
                    return;
                }
            }
            catch
            {
            }
            if (!KeepAlive(time)) return;

            if (Boost == null) CalculateBoost();

            CheckTradeTimeout(time);
            TradeTick(time);
            HandleRegen(time);
            HandleQuest(time);
            HandleGround(time);
            HandleEffects(time);
            RegulateParty();
            fames.Tick(time);

            if (usingShuriken)
            {
                if (MP > 0)
                    MP -= 1;
                else
                {
                    usingShuriken = false;
                    ApplyConditionEffect(new ConditionEffect
                    {
                        Effect = ConditionEffectIndex.Speedy,
                        DurationMS = 0
                    });
                }
            }

            try
            {
                SendUpdate(time);
            }
            catch
            {
            }

            if (HP <= -1)
            {
                Death("Unknown");
                return;
            }

            base.Tick(time);
        }

        private void HandleRegen(RealmTime time)
        {
            if (HP == Stats[0] + Boost[0] || !CanHpRegen())
                hpRegenCounter = 0;
            else
            {
                hpRegenCounter += statsMgr.GetHPRegen()*time.thisTickTimes/1000f;
                var regen = (int) hpRegenCounter;
                if (regen > 0)
                {
                    HP = Math.Min(Stats[0] + Boost[0], HP + regen);
                    hpRegenCounter -= regen;
                    UpdateCount++;
                }
            }

            if (MP == Stats[1] + Boost[1] || !CanMpRegen())
                mpRegenCounter = 0;
            else
            {
                mpRegenCounter += statsMgr.GetMPRegen()*time.thisTickTimes/1000f;
                var regen = (int) mpRegenCounter;
                if (regen > 0)
                {
                    MP = Math.Min(Stats[1] + Boost[1], MP + regen);
                    mpRegenCounter -= regen;
                    UpdateCount++;
                }
            }
        }

        public void Teleport(RealmTime time, int objId)
        {
            if (!TPCooledDown())
            {
                SendError("Too soon to teleport again!");
                return;
            }
            SetTPDisabledPeriod();
            if (Pet != null)
                Pet.Move(X, Y);
            Entity obj = Owner.GetEntity(objId);
            if (obj == null) return;
            Move(obj.X, obj.Y);
            fames.Teleport();
            SetNewbiePeriod();
            UpdateCount++;
            Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = Id,
                Position = new Position
                {
                    X = X,
                    Y = Y
                }
            }, null);
            if (!isNotVisible)
                Owner.BroadcastPacket(new ShowEffectPacket
                {
                    EffectType = EffectType.Teleport,
                    TargetId = Id,
                    PosA = new Position
                    {
                        X = X,
                        Y = Y
                    },
                    Color = new ARGB(0xFFFFFFFF)
                }, null);
        }

        public override bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            if (projectile.ProjectileOwner is Player ||
                HasConditionEffect(ConditionEffects.Paused) ||
                HasConditionEffect(ConditionEffects.Stasis) ||
                HasConditionEffect(ConditionEffects.Invincible))
                return false;

            return base.HitByProjectile(projectile, time);
        }

        public void Hit(Projectile proj)
        {
            var dmg = (int) statsMgr.GetDefenseDamage(proj.Damage, proj.Descriptor.ArmorPiercing);
            if (!HasConditionEffect(ConditionEffects.Invulnerable))
                HP -= dmg;
            ApplyConditionEffect(proj.Descriptor.Effects);
            UpdateCount++;
            Owner.BroadcastPacket(new DamagePacket
            {
                TargetId = Id,
                Effects = HasConditionEffect(ConditionEffects.Invincible) ? 0 : proj.ConditionEffects,
                Damage = (ushort) dmg,
                Killed = HP <= 0,
                BulletId = proj.ProjectileId,
                ObjectId = proj.ProjectileOwner.Self.Id
            }, this);

            string killerName = proj.ProjectileOwner is Player
                ? proj.ProjectileOwner.Self.Name
                : (proj.ProjectileOwner.Self.ObjectDesc.DisplayId ?? proj.ProjectileOwner.Self.ObjectDesc.ObjectId);

            if (HP <= 0) Death(killerName);
        }

        private bool CheckResurrection()
        {
            for (int i = 0; i < 4; i++)
            {
                Item item = Inventory[i];
                if (item == null || !item.Resurrects) continue;

                HP = Stats[0] + Stats[0];
                MP = Stats[1] + Stats[1];
                Inventory[i] = null;
                foreach (Player player in Owner.Players.Values)
                    player.SendInfo(string.Format("{0}'s {1} breaks and he disappears", Name, item.ObjectId));

                client.Reconnect(new ReconnectPacket
                {
                    Host = "",
                    Port = 2050,
                    GameId = World.NEXUS_ID,
                    Name = "Nexus",
                    Key = Empty<byte>.Array,
                });
                newbieTime += 1000;

                resurrecting = true;
                return true;
            }
            return false;
        }

        public List<string> GetMaxed()
        {
            List<string> maxed = new List<string>();
            foreach (XElement i in Manager.GameData.ObjectTypeToElement[ObjectType].Elements("LevelIncrease"))
            {
                int limit =
                    int.Parse(Manager.GameData.ObjectTypeToElement[ObjectType].Element(i.Value).Attribute("max").Value);
                int idx = StatsManager.StatsNameToIndex(i.Value);
                if (Stats[idx] >= limit)
                    maxed.Add(i.Value);
            }
            return maxed;
        }

        private void GenerateGravestone()
        {
            int maxed = GetMaxed().Count;
            ushort objType;
            int? time;
            switch (maxed)
            {
                case 8:
                    objType = 0x0735;
                    time = null;
                    break;
                case 7:
                    objType = 0x0734;
                    time = null;
                    break;
                case 6:
                    objType = 0x072b;
                    time = null;
                    break;
                case 5:
                    objType = 0x072a;
                    time = null;
                    break;
                case 4:
                    objType = 0x0729;
                    time = null;
                    break;
                case 3:
                    objType = 0x0728;
                    time = null;
                    break;
                case 2:
                    objType = 0x0727;
                    time = null;
                    break;
                case 1:
                    objType = 0x0726;
                    time = null;
                    break;
                default:
                    if (Level <= 1)
                    {
                        objType = 0x0723;
                        time = 30*1000;
                    }
                    else if (Level < 20)
                    {
                        objType = 0x0724;
                        time = 60*1000;
                    }
                    else
                    {
                        objType = 0x0725;
                        time = 5*60*1000;
                    }
                    break;
            }
            var obj = new StaticObject(Manager, objType, time, true, time != null, false);
            obj.Move(X, Y);
            obj.Name = Name;
            Owner.EnterWorld(obj);
        }

        public void GivePet(ushort petId)
        {
            if (Pet != null)
                Owner.LeaveWorld(Pet);
            Pet = Resolve(Manager, petId);
            Pet.playerOwner = this;
            Pet.Move(X, Y);
            Pet.isPet = true;
            Owner.EnterWorld(Pet);
        }

        public bool CompareName(string name)
        {
            string rn = name.ToLower();
            if (rn.Split(' ')[0].StartsWith("[") || Name.Split(' ').Length == 1)
                if (Name.ToLower().StartsWith(rn)) return true;
                else return false;
            if (Name.Split(' ')[1].ToLower().StartsWith(rn)) return true;
            return false;
        }

        public string GetNameColor()
        {
            if (client.Account.Rank >= 2)
            {
                return "0xE678CC";
            }
            return "0xFF0000";
        }

        public void Death(string killer)
        {
            if (client.Stage == ProtocalStage.Disconnected || resurrecting)
                return;
            switch (Owner.Name)
            {
                case "Editor":
                    return;
                case "Testing and Stuff":
                case "Duel Arena":
                case "PVP Arena":
                    Owner.BroadcastPacket(new TextPacket
                    {
                        BubbleTime = 0,
                        Stars = -1,
                        Name = "",
                        Text = Name + " was eliminated by " + killer
                    }, null);
                    HP = Stats[0] + Stats[0];
                    MP = Stats[1] + Stats[1];
                    client.Reconnect(new ReconnectPacket
                    {
                        Host = "",
                        Port = 2050,
                        GameId = World.NEXUS_ID,
                        Name = "Nexus",
                        Key = Empty<byte>.Array,
                    });
                    return;
                case "Nexus":
                    HP = Stats[0] + Stats[0];
                    MP = Stats[1] + Stats[1];
                    client.Disconnect();
                    return;
            }
            if (CheckResurrection())
                return;
            if (client.Character.Dead)
            {
                client.Disconnect();
                return;
            }

            GenerateGravestone();
            if (killer != "" && killer != "Unknown" && killer != "???")
            {
                Owner.BroadcastPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = Name + " died at Level " + Level + ", killed by " + killer
                }, null);
            }
            /*foreach (Player i in Owner.Players.Values)
            {
                i.SendInfo(Name + " died at Level " + Level + ", killed by " + killer);
            }*/
            Manager.Data.AddPendingAction(db =>
            {

            });
            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                client.Character.Dead = true;
                SaveToCharacter();
                db.SaveCharacter(client.Account, client.Character);
                db.Death(Manager.GameData, client.Account, client.Character, killer);
            }
            client.SendPacket(new DeathPacket
            {
                AccountId = AccountId,
                CharId = client.Character.CharacterId,
                Killer = killer
            });
            Owner.Timers.Add(new WorldTimer(1000, (w, t) => client.Disconnect()));
            Owner.LeaveWorld(this);
        }
    }
}