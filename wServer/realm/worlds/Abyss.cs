//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.IO;
//using wServer.networking;

//namespace wServer.realm.worlds
//{
//    public class Abyss : World
//    {
//        public Abyss()
//        {
//            Id = ABYSS;
//            Name = "Abyss of Demons";
//            Background = 0;
//            AllowTeleport = true;
//        }
//        protected override void Init()
//        {
//            base.FromWorldMap(typeof(RealmManager).Assembly.GetManifestResourceStream("wServer.realm.worlds.abyss.wmap"));
//        }

//        public override World GetInstance(Client Client)
//        {
//            return Manager.AddWorld(new Abyss());
//        }
//    }
//}

