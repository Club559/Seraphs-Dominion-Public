using db;
using wServer.networking;

namespace wServer.realm.worlds
{
    public class GuildHall : World
    {
        public GuildHall(string guild)
        {
            Id = GUILD_ID;
            Guild = guild;
            Name = "Guild Hall";
            Background = 0;
            Difficulty = 0;
            SetMusic("Guild Hall");
            AllowTeleport = true;
        }

        public string Guild { get; set; }
        public int guildLevel { get; set; }

        protected override void Init()
        {
            switch (Level())
            {
                case 0:
                    base.FromWorldMap(
                        typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.ghall0.wmap"));
                    break;
                case 1:
                    base.FromWorldMap(
                        typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.ghall1.wmap"));
                    break;
                case 2:
                    base.FromWorldMap(
                        typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.ghall2.wmap"));
                    break;
                case 3:
                    base.FromWorldMap(
                        typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.ghall3.wmap"));
                    break;
            }
        }

        public override World GetInstance(Client client)
        {
            return Manager.AddWorld(new GuildHall(Guild));
        }

        private int level { get; set; }
        public int Level()
        {
            Manager.Data.AddPendingAction(db =>
            {
                int id = db.GetGuildId(Guild);
                this.level = db.GetGuildLevel(id);
            });
            return level;
        }
    }
}