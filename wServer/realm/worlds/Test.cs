using System.IO;
using terrain;

namespace wServer.realm.worlds
{
    public class Test : World
    {
        public Test()
        {
            Id = TEST_ID;
            Name = "Editor";
            Background = 0;
            Difficulty = 5;
            SetMusic("dungeon/Island");
        }

        public void LoadJson(string json)
        {
            SetMusic("");
            FromWorldMap(new MemoryStream(Json2Wmap.Convert(Manager.GameData, json)));
        }
    }
}