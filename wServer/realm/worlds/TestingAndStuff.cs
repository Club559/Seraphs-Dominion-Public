using wServer.networking;

namespace wServer.realm.worlds
{
    public class
        TestingAndStuff : World
    {
        public TestingAndStuff()
        {
            Name = "Testing and Stuff";
            Background = 0;
            SetMusic("dungeon/Haunted Cemetary");
            AllowTeleport = true;
        }

        protected override void Init()
        {
            base.FromWorldMap(typeof (RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.pvp.wmap"));
        }

        public override World GetInstance(Client client)
        {
            return Manager.AddWorld(new TestingAndStuff());
        }
    }
}