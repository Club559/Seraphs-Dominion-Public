using System;
using System.Globalization;
using System.Text;
using db;
using wServer.networking;
using wServer.networking.svrPackets;
using wServer.realm.entities;
using wServer.realm.setpieces;

namespace wServer.realm.commands
{
    internal class SpawnCommand : Command
    {
        public SpawnCommand() : base("spawn", 3) { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            var db = new Database(Program.Settings.GetValue("conn"));
            int index = args.IndexOf(' ');
            int num;
            string name = args;

            if (args.IndexOf(' ') > 0 && int.TryParse(args.Substring(0, args.IndexOf(' ')), out num)) //multi
                name = args.Substring(index + 1);
            else
                num = 1;

            ushort objType;
            if (!player.Manager.GameData.IdToObjectType.TryGetValue(name, out objType) ||
                !player.Manager.GameData.ObjectDescs.ContainsKey(objType))
            {
                player.SendError("Unknown entity!");
                return false;
            }

            for (int i = 0; i < num; i++)
            {
                Entity entity = Entity.Resolve(player.Manager, objType);
                entity.Move(player.X, player.Y);
                player.Owner.EnterWorld(entity);
            }
            if (num > 1)
                if (!args.ToLower().EndsWith("s"))
                    player.SendInfo("Sucessfully spawned " + num + " : " +
                                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                                        args.Substring(index + 1).ToLower() + "s"));
                else
                    player.SendInfo("Sucessfully spawned " + num + " : " +
                                    CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                                        args.Substring(index + 1).ToLower() + "'"));
            else
                player.SendInfo("Sucessfully spawned " + num + " : " +
                                CultureInfo.CurrentCulture.TextInfo.ToTitleCase(args.ToLower()));
            return true;
        }
    }

    internal class ToggleEffCommand : Command
    {
        public ToggleEffCommand() : base("eff", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            ConditionEffectIndex effect;
            if (!Enum.TryParse(args, true, out effect))
            {
                player.SendError("Invalid effect!");
                return false;
            }
            if ((player.ConditionEffects & (ConditionEffects) (1 << (int) effect)) != 0)
            {
                //remove
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = effect,
                    DurationMS = 0
                });
                player.SendInfo("Sucessfully removed effect : " + args);
            }
            else
            {
                //add
                player.ApplyConditionEffect(new ConditionEffect
                {
                    Effect = effect,
                    DurationMS = -1
                });
                player.SendInfo("Sucessfully added effect : " + args);
            }
            return true;
        }
    }

    internal class GiveCommand : Command
    {
        public GiveCommand() : base("give", 2) { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string item = args;
            string data = "";
            if (args.IndexOf("{") >= 0 && args.EndsWith("}"))
            {
                item = args.Remove(args.IndexOf("{")).TrimEnd();
                Console.WriteLine(item);
                data = args.Substring(args.IndexOf("{"));
            }
            ushort objType;
            if (!player.Manager.GameData.IdToObjectType.TryGetValue(item, out objType))
            {
                player.SendError("Unknown item type!");
                return false;
            }
            for (int i = 0; i < player.Inventory.Length; i++)
                if (player.Inventory[i] == null)
                {
                    player.Inventory[i] = player.Manager.GameData.Items[objType];
                    if (data != "")
                        player.Inventory.Data[i] = ItemData.CreateData(data);
                    player.UpdateCount++;
                    return true;
                }
            player.SendError("Not enough space in inventory!");
            return false;
        }
    }

    internal class TpPosCommand : Command
    {
        public TpPosCommand() : base("tpPos", 4) { }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string[] coordinates = args.Split(' ');
            if (coordinates.Length != 2)
            {
                player.SendError("Invalid coordinates!");
                return false;
            }

            int x, y;
            if (!int.TryParse(coordinates[0], out x) ||
                !int.TryParse(coordinates[1], out y))
            {
                player.SendError("Invalid coordinates!");
                return false;
            }

            player.Move(x + 0.5f, y + 0.5f);
            player.SetNewbiePeriod();
            player.UpdateCount++;
            player.Owner.BroadcastPacket(new GotoPacket
            {
                ObjectId = player.Id,
                Position = new Position
                {
                    X = player.X,
                    Y = player.Y
                }
            }, null);
            return true;
        }
    }

    internal class SetpieceCommand : Command
    {
        public SetpieceCommand() : base("setpiece", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            var piece = (ISetPiece) Activator.CreateInstance(Type.GetType(
                "wServer.realm.setpieces." + args, true, true));
            piece.RenderSetPiece(player.Owner, new IntPoint((int) player.X + 1, (int) player.Y + 1));
            return true;
        }
    }

    internal class DebugCommand : Command
    {
        public DebugCommand() : base("debug", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.Owner.EnterWorld(new Locater(player));
            return true;
        }

        private class Locater : Enemy
        {
            private readonly Player player;

            public Locater(Player player)
                : base(player.Manager, 0x0d5d)
            {
                this.player = player;
                Move(player.X, player.Y);
                ApplyConditionEffect(new ConditionEffect
                {
                    Effect = ConditionEffectIndex.Invincible,
                    DurationMS = -1
                });
            }

            public override void Tick(RealmTime time)
            {
                Move(player.X, player.Y);
                UpdateCount++;
                base.Tick(time);
            }
        }
    }

    internal class AllOnlineCommand : Command
    {
        public AllOnlineCommand() : base("online", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            var sb = new StringBuilder("Users online: \r\n");
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Stage == ProtocalStage.Disconnected || i.Player == null || i.Player.Owner == null) continue;
                sb.AppendFormat("{0}#{1}@{2}\r\n",
                    i.Account.Name,
                    i.Player.Owner.Name,
                    i.Socket.RemoteEndPoint);
            }

            player.SendInfo(sb.ToString());
            return true;
        }
    }

    internal class KillAllCommand : Command
    {
        public KillAllCommand() : base("killAll", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            int count = 0;
            foreach (var i in player.Owner.Enemies)
            {
                ObjectDesc desc = i.Value.ObjectDesc;
                if (desc != null &&
                    desc.ObjectId != null &&
                    desc.ObjectId.ContainsIgnoreCase(args))
                {
                    i.Value.Death(time);
                    count++;
                }
            }
            player.SendInfo(string.Format("{0} enemy killed!", count));
            return true;
        }
    }

    internal class KillAllXCommand : Command
    {
        public KillAllXCommand()
            : base("killAllX", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            int count = 0;
            foreach (var i in player.Owner.Enemies)
            {
                ObjectDesc desc = i.Value.ObjectDesc;
                if (desc != null &&
                    desc.ObjectId != null &&
                    desc.ObjectId.ContainsIgnoreCase(args))
                {
                    i.Value.Damage(player, time, i.Value.HP + 1, true, false, null);
                    count++;
                }
            }
            player.SendInfo(string.Format("{0} enemy killed!", count));
            return true;
        }
    }

    internal class KickCommand : Command
    {
        public KickCommand() : base("kick", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Account.Name.EqualsIgnoreCase(args))
                {
                    i.Disconnect();
                    i.Save();
                    player.SendInfo("Player disconnected!");
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class GetQuestCommand : Command
    {
        public GetQuestCommand() : base("getQuest", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.Quest == null)
            {
                player.SendError("Player does not have a quest!");
                return false;
            }
            player.SendInfo("Quest location: (" + player.Quest.X + ", " + player.Quest.Y + ")");
            return true;
        }
    }

    internal class OryxSayCommand : Command
    {
        public OryxSayCommand() : base("oryxSay", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.Manager.Chat.Oryx(player.Owner, args);
            return true;
        }
    }

    internal class AnnounceCommand : Command
    {
        public AnnounceCommand() : base("announce", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.Manager.Chat.Announce(args);
            return true;
        }
    }

    internal class SummonCommand : Command
    {
        public SummonCommand() : base("summon", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (var i in player.Owner.Players)
            {
                if (i.Value.Name.EqualsIgnoreCase(args))
                {
                    i.Value.Teleport(time, player.Id);
                    player.SendInfo("Player summoned!");
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class KillPlayerCommand : Command
    {
        public KillPlayerCommand() : base("killPlayer", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (Client i in player.Manager.Clients.Values)
            {
                if (i.Account.Name.EqualsIgnoreCase(args))
                {
                    i.Player.HP = 0;
                    i.Player.Death("Moderator");
                    player.SendInfo("Player killed!");
                    return true;
                }
            }
            player.SendError(string.Format("Player '{0}' could not be found!", args));
            return false;
        }
    }

    internal class VanishCommand : Command
    {
        public VanishCommand() : base("vanish", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (!player.isNotVisible)
            {
                player.isNotVisible = true;
                player.Owner.PlayersCollision.Remove(player);
                if (player.Pet != null)
                    player.Owner.LeaveWorld(player.Pet);
                player.SendInfo("You're now hidden from all players!");
                return true;
            }
            player.isNotVisible = false;

            player.SendInfo("You're now visible to all players!");
            return true;
        }
    }

    internal class SayCommand : Command
    {
        public SayCommand() : base("say", 1)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = args
                });
            return true;
        }
    }

    internal class SaveCommand : Command
    {
        public SaveCommand() : base("save", 4)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (Client i in player.Manager.Clients.Values)
                i.Save();

            player.SendInfo("Saved all Clients!");
            return true;
        }
    }

    internal class DevChatCommand : Command
    {
        public DevChatCommand() : base("d", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            foreach (Client client in player.Manager.Clients.Values)
                if (client.Account.Rank > 3)
                    client.Player.SendText("@[DEV] - " + player.Name + "", args);
            return true;
        }
    }

    internal class PVPArenaCommand : Command
    {
        public PVPArenaCommand() : base("pvparena", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            Entity prtal = Entity.Resolve(player.Manager, "PVP Portal");
            prtal.Move(player.X, player.Y);
            player.Owner.EnterWorld(prtal);
            World w = player.Manager.GetWorld(player.Owner.Id);
            w.Timers.Add(new WorldTimer(30*1000, (world, t) => //default portal close time * 1000
            {
                try
                {
                    w.LeaveWorld(prtal);
                }
                catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                {
                    Console.Out.WriteLine("Couldn't despawn portal.");
                }
            }));
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "PVP Arena Opened by:" + " " + player.Name
                });
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = "PVP Arena Opened by " + player.Name
                });
            return true;
        }
    }

    internal class DuelArenaCommand : Command
    {
        public DuelArenaCommand() : base("duelarena", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            Entity prtal = Entity.Resolve(player.Manager, "Duel Portal");
            prtal.Move(player.X, player.Y);
            player.Owner.EnterWorld(prtal);
            World w = player.Manager.GetWorld(player.Owner.Id);
            w.Timers.Add(new WorldTimer(30*1000, (world, t) => //default portal close time * 1000
            {
                try
                {
                    w.LeaveWorld(prtal);
                }
                catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                {
                    Console.Out.WriteLine("Couldn't despawn portal.");
                }
            }));
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "Duel Arena Opened by:" + " " + player.Name
                });
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = "Duel Arena Opened by " + player.Name
                });
            return true;
        }
    }

    internal class TestingAndStuffCommand : Command
    {
        public TestingAndStuffCommand() : base("testingandstuff", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            Entity prtal = Entity.Resolve(player.Manager, "Testing and Stuff");
            prtal.Move(player.X, player.Y);
            player.Owner.EnterWorld(prtal);
            World w = player.Manager.GetWorld(player.Owner.Id);
            w.Timers.Add(new WorldTimer(30*1000, (world, t) => //default portal close time * 1000
            {
                try
                {
                    w.LeaveWorld(prtal);
                }
                catch //couldn't remove portal, Owner became null. Should be fixed with RealmManager implementation
                {
                    Console.Out.WriteLine("Couldn't despawn portal.");
                }
            }));
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new TextPacket
                {
                    BubbleTime = 0,
                    Stars = -1,
                    Name = "",
                    Text = "Testing & Stuff Opened by:" + " " + player.Name
                });
            foreach (Client i in player.Manager.Clients.Values)
                i.SendPacket(new NotificationPacket
                {
                    Color = new ARGB(0xff00ff00),
                    ObjectId = player.Id,
                    Text = "Testing & Stuff Opened by " + player.Name
                });
            return true;
        }
    }

    internal class StatCommand : Command
    {
        public StatCommand() : base("stats", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            int index = args.IndexOf(' ');
            int num;
            string stat = args;

            if (args.IndexOf(' ') > 0 && int.TryParse(args.Substring(index), out num))
                stat = args.Substring(0, args.IndexOf(' '));
            else
                num = 1;

            switch (stat)
            {
                case "hp":
                    player.Stats[0] = num;
                    player.UpdateCount++;
                    break;
                case "mp":
                    player.Stats[1] = num;
                    player.UpdateCount++;
                    break;
                case "att":
                    player.Stats[2] = num;
                    player.UpdateCount++;
                    break;
                case "def":
                    player.Stats[3] = num;
                    player.UpdateCount++;
                    break;
                case "spd":
                    player.Stats[4] = num;
                    player.UpdateCount++;
                    break;
                case "vit":
                    player.Stats[5] = num;
                    player.UpdateCount++;
                    break;
                case "wis":
                    player.Stats[6] = num;
                    player.UpdateCount++;
                    break;
                case "dex":
                    player.Stats[7] = num;
                    player.UpdateCount++;
                    break;
                case "lvl":
                    player.Level = num;
                    player.UpdateCount++;
                    break;
                case "all":
                    player.Stats[2] = num;
                    player.Stats[3] = num;
                    player.Stats[4] = num;
                    player.Stats[5] = num;
                    player.Stats[6] = num;
                    player.Stats[7] = num;
                    player.UpdateCount++;
                    break;
                default:
                    player.SendHelp("Usage: /stats <stat name> <amount>");
                    break;
            }
            player.SendInfo("Successfully updated " + stat);
            return true;
        }
    }

    internal class UpdateCommand : Command
    {
        public UpdateCommand() : base("update", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                foreach (var i in player.Owner.Players)
                {
                    Account x = db.GetAccount(i.Value.AccountId);
                    Player usr = i.Value;

                    usr.Name = x.Name;
                    usr.Client.Account.Rank = x.Rank;
                    usr.Client.Account.Tag = x.Tag;
                    usr.Client.Account.Guild.Id = x.Guild.Id;
                    usr.Client.Account.Guild.Fame = x.Guild.Fame;
                    usr.Client.Account.Guild.Rank = x.Guild.Rank;

                    usr.UpdateCount++;
                }
            }
            player.SendInfo("Users Updated.");
            return true;
        }
    }

    internal class TeamCommand : Command
    {
        public TeamCommand() : base("team", 3)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            try
            {
                player.Team = Convert.ToInt32(args);
                player.SendInfo("Updated to team #" + Convert.ToInt32(args));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    internal class BanCommand : Command
    {
        public BanCommand() : base("ban", 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string name = args;
            
            if (name != "")
            {
                using (Database db = new Database(Program.Settings.GetValue("conn")))
                {
                    var cmd = db.CreateQuery();

                    cmd.CommandText = "UPDATE accounts SET banned=1 WHERE name=@name LIMIT 1";
                    cmd.Parameters.AddWithValue("@name", name);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        player.SendInfo("User was successfully banned");
                        foreach (Client i in player.Manager.Clients.Values)
                        {
                            if (i.Account.Name.EqualsIgnoreCase(name))
                            {
                                i.Disconnect();
                            }
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal class MuteCommand : Command
    {
        public MuteCommand() : base ("mute", 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string name = args;

            if (name != "")
            {
                using (Database db = new Database(Program.Settings.GetValue("conn")))
                {
                    var cmd = db.CreateQuery();

                    cmd.CommandText = "UPDATE accounts SET muted=1 WHERE name=@name LIMIT 1";
                    cmd.Parameters.AddWithValue("@name", name);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        player.SendInfo("User was successfully muted");
                        foreach (var i in player.Owner.Players)
                        {
                            Account x = db.GetAccount(i.Value.AccountId);
                            Player usr = i.Value;

                            usr.Client.Account.Muted = x.Muted;

                            usr.UpdateCount++;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal class UnmuteCommand : Command
    {
        public UnmuteCommand() : base("unmute", 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            string name = args;

            if (name != "")
            {
                using (Database db = new Database(Program.Settings.GetValue("conn")))
                {
                    var cmd = db.CreateQuery();

                    cmd.CommandText = "UPDATE accounts SET muted=0 WHERE name=@name LIMIT 1";
                    cmd.Parameters.AddWithValue("@name", name);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        player.SendInfo("User was successfully unmuted");
                        foreach (var i in player.Owner.Players)
                        {
                            Account x = db.GetAccount(i.Value.AccountId);
                            Player usr = i.Value;

                            usr.Client.Account.Muted = x.Muted;

                            usr.UpdateCount++;
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal class AdminBuffCommand : Command
    {
        public AdminBuffCommand() : base("AdminBuff", 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            int index = Convert.ToInt32(args);
            ItemData data = new ItemData()
            {
                NamePrefix = "Admin",
                NameColor = 0xFF1297,
                DmgPercentage = 1000,
                Soulbound = true
            };
            if (player.Inventory.Data[index] == null)
                player.Inventory.Data[index] = data;
            else
            {
                player.Inventory.Data[index].NamePrefix = data.NamePrefix;
                player.Inventory.Data[index].NameColor = data.NameColor;
                player.Inventory.Data[index].DmgPercentage = data.DmgPercentage;
                player.Inventory.Data[index].Soulbound = data.Soulbound;
            }
            player.UpdateCount++;
            return true;
        }
    }

    internal class StrangifyCommand : Command
    {
        public StrangifyCommand()
            : base("Strangify", 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            int index = Convert.ToInt32(args);
            player.SendInfo("Stranged");
            ItemData data = new ItemData
            {
                NamePrefix = "Strange",
                NameColor = 0xE06A2A,
                Strange = true
            };
            if (player.Inventory.Data[index] == null)
                player.Inventory.Data[index] = data;
            else
            {
                player.Inventory.Data[index].NamePrefix = data.NamePrefix;
                player.Inventory.Data[index].NameColor = data.NameColor;
                player.Inventory.Data[index].Strange = data.Strange;
            }
            player.UpdateCount++;
            return true;
        }
    }

    internal class SkinEffectCommand : Command
    {
        public SkinEffectCommand() : base("skinEff", permLevel: 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.XmlEffect = args;
            player.UpdateCount++;
            return true;
        }
    }

    internal class CommandCommand : Command
    {
        public CommandCommand() : base("cmd", permLevel: 6)
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            player.Client.SendPacket(new GetTextInputPacket
            {
                Action = "sendCommand",
                Name = "Type the command"
            });
            return true;
        }
    }
}