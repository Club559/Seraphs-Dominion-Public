using wServer.networking;

namespace wServer.realm.worlds
{
    public class
        PVPArena : World
    {
        public PVPArena()
        {
            Id = PVP;
            Name = "PVP Arena";
            Background = 0;
            Difficulty = 5;
            SetMusic("dungeon/Haunted Cemetary");
            AllowTeleport = false;
            PvP = true;
        }

        protected override void Init()
        {
            base.FromWorldMap(typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.pvp.wmap"));
        }

        public override World GetInstance(Client client)
        {
            return Manager.AddWorld(new PVPArena());
        }
    }
}