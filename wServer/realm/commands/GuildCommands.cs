using System;
using wServer.networking.svrPackets;
using wServer.realm.entities;

namespace wServer.realm.commands
{
    internal class GuildChatCommand : Command
    {
        public GuildChatCommand() : base("guild")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.Guild != "")
                try
                {
                    string saytext = string.Join(" ", args);
                    foreach (var w in player.Manager.Worlds)
                    {
                        World world = w.Value;
                        if (w.Key != 0) // 0 is limbo??
                            foreach (var i in world.Players)
                                if (i.Value.Guild == player.Guild)
                                    if (saytext.Equals(" ") || saytext == "" || saytext == null)
                                    {
                                        player.SendHelp("Usage: /guild <text>");
                                        return false;
                                    }
                                    else
                                    {
                                        player.Manager.Chat.SayGuild(player, saytext.ToSafeText());
                                        return true;
                                    }
                    }
                }
                catch(Exception e)
                {
                    player.SendError(e.ToString());
                    player.SendInfo("Cannot guild chat!");
                    return false;
                }
            else
                player.SendInfo("You need to be in a guild to use guild chat!");
            return false;
        }
    }

    internal class GChatCommand : Command
    {
        public GChatCommand() : base("g")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.Guild != "")
                try
                {
                    string saytext = string.Join(" ", args);
                    foreach (var w in player.Manager.Worlds)
                    {
                        World world = w.Value;
                        if (w.Key != 0) // 0 is limbo??
                            foreach (var i in world.Players)
                                if (i.Value.Guild == player.Guild)
                                    if (saytext.Equals(" ") || saytext == "" || saytext == null)
                                    {
                                        player.SendHelp("Usage: /g <text>");
                                        return false;
                                    }
                                    else
                                    {
                                        player.Manager.Chat.SayGuild(player, saytext.ToSafeText());
                                        return true;
                                    }
                    }
                }
                catch (Exception e)
                {
                    player.SendError(e.ToString());
                    player.SendInfo("Cannot guild chat!");
                    return false;
                }
            else
                player.SendInfo("You need to be in a guild to use guild chat!");
            return false;
        }
    }

    internal class GuildInviteCommand : Command
    {
        public GuildInviteCommand() : base("invite")
        {
        }

        protected override bool Process(Player player, RealmTime time, string args)
        {
            if (player.GuildRank >= 20)
                foreach (var i in player.Manager.Worlds)
                    if (i.Key != 0)
                        foreach (var e in i.Value.Players)
                            if (e.Value.Client.Account.Name.ToLower() == args.ToLower())
                                if (e.Value.Client.Account.Guild.Name == "")
                                {
                                    player.SendInfo(e.Value.Client.Account.Name + " has been invited to your guild!");
                                    e.Value.Client.SendPacket(new InvitedToGuildPacket
                                    {
                                        Name = player.Client.Account.Name,
                                        Guild = player.Client.Account.Guild.Name
                                    });
                                    return true;
                                }
                                else
                                {
                                    player.SendError(e.Value.Client.Account.Name + " is already in a guild!");
                                    return false;
                                }
                            else
                                player.SendInfo("Members and initiates cannot invite!");
            return false;
        }
    }

    //class GuildJoinCommand : Command
    //{
    //    public GuildJoinCommand() : base("join") { }

    //    protected override bool Process(Player player, RealmTime time, string args)
    //    {
    //        if (player.Guild == null || player.Guild == "")
    //            foreach (var i in player.Manager.Worlds)
    //                if (i.Key != 0)
    //                    foreach (var e in i.Value.Players)
    //                        if (e.Value.Client.Account.Name.ToLower() == args.ToLower())
    //                            if (e.Value.Client.Account.Guild.Name == "")
    //                            {
    //                                player.SendInfo(e.Value.Client.Account.Name + " has been invited to your guild!");
    //                                player.Client.SendPacket(new JoinGuildPacket
    //                                {
    //                                    GuildName = args
    //                                });
    //                                return true;
    //                            }
    //                            else
    //                            {
    //                                player.SendError(e.Value.Client.Account.Name + " is already in a guild!");
    //                                return false;
    //                            }
    //                        else
    //                            player.SendInfo("Members and initiates cannot invite!");
    //        return false;
    //    }
    //}
}