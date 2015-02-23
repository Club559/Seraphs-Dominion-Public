using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using db;
using MySql.Data.MySqlClient;

namespace server.@char
{
    internal class list : RequestHandler
    {
        private Lazy<List<ServerItem>> svrList;

        public list()
        {
            svrList = new Lazy<List<ServerItem>>(GetServerList, true);
        }

        private List<ClassAvailabilityItem> GetClassAvailability(Account acc)
        {
            var classes = new string[14]
            {
                "Rogue",
                "Assassin",
                "Huntress",
                "Mystic",
                "Trickster",
                "Sorcerer",
                "Ninja",
                "Archer",
                "Wizard",
                "Priest",
                "Necromancer",
                "Warrior",
                "Knight",
                "Paladin"
            };

            if (acc == null)
            {
                return new List<ClassAvailabilityItem>
                {
                    new ClassAvailabilityItem {Class = "Rogue", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Assassin", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Huntress", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Mystic", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Trickster", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Sorcerer", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Ninja", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Archer", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Wizard", Restricted = "unrestricted"},
                    new ClassAvailabilityItem {Class = "Priest", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Necromancer", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Warrior", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Knight", Restricted = "restricted"},
                    new ClassAvailabilityItem {Class = "Paladin", Restricted = "restricted"},
                };
            }

            var ret = new List<ClassAvailabilityItem>();

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                MySqlCommand cmd = db.CreateQuery();
                cmd.CommandText = "SELECT class, available FROM unlockedclasses WHERE accId=@accId;";
                cmd.Parameters.AddWithValue("@accId", acc.AccountId);
                MySqlDataReader rdr = cmd.ExecuteReader();
                if (!rdr.HasRows)
                {
                    rdr.Close();
                    foreach (string s in classes)
                    {
                        MySqlCommand xcmd = db.CreateQuery();
                        xcmd.CommandText =
                            "INSERT INTO unlockedclasses(accId, class, available) VALUES(@accId, @class, @restricted);";
                        xcmd.Parameters.AddWithValue("@accId", acc.AccountId);
                        xcmd.Parameters.AddWithValue("@class", s);
                        xcmd.Parameters.AddWithValue("@restricted", s == "Wizard" ? "unrestricted" : "restricted");
                        xcmd.ExecuteNonQuery();
                        ret.Add(new ClassAvailabilityItem
                        {
                            Class = s,
                            Restricted = s == "Wizard" ? "unrestricted" : "restricted"
                        });
                    }
                }
                else
                {
                    while (rdr.Read())
                    {
                        ret.Add(new ClassAvailabilityItem
                        {
                            Class = rdr.GetString("class"),
                            Restricted = rdr.GetString("available")
                        });
                    }
                }
            }
            return ret;
        }

        public static bool Connected(string host, int port)
        {
            return true;

            IPAddress[] IPs = Dns.GetHostAddresses(host);

            using (var tcp = new TcpClient())
            {
                tcp.NoDelay = true;

                try
                {
                    tcp.Connect(new IPEndPoint(IPs[0], port));
                }
                catch
                {
                }


                if (tcp.Connected)
                    return true;
                return false;
            }
        }

        public static double CheckUsage(string host, int port)
        {
            return 0;

            IPAddress[] IPs = Dns.GetHostAddresses(host);
            using (var tcp = new TcpClient())
            {
                tcp.NoDelay = true;

                try
                {
                    tcp.Connect(new IPEndPoint(IPs[0], port));
                }
                catch
                {
                }

                if (tcp.Connected)
                    return (tcp.ReceiveTimeout / 10000);
                return 1;
            }
        }

        private List<ServerItem> GetServerList()
        {
            var ret = new List<ServerItem>();
            var num = Program.Settings.GetValue<int>("svrNum");
            for (int i = 0; i < num; i++)
            {
                if (Connected(Program.Settings.GetValue("svr" + i + "Adr"), 2050))
                {
                    ret.Add(new ServerItem
                    {
                        Name = Program.Settings.GetValue("svr" + i + "Name"),
                        Lat = 52.23,
                        Long = 4.55,
                        DNS = Program.Settings.GetValue("svr" + i + "Adr", "127.0.0.1"),
                        Usage = CheckUsage(Program.Settings.GetValue("svr" + i + "Adr"), 2050),
                        AdminOnly = Program.Settings.GetValue<bool>("svr" + i + "Admin", "false")
                    });
                }
            }
            return ret;
        }

        private List<ServerItem> GetServerList(bool isAdmin)
        {
            var ret = new List<ServerItem>();
            var num = Program.Settings.GetValue<int>("svrNum");
            for (int i = 0; i < num; i++)
            {
                if (Connected(Program.Settings.GetValue("svr" + i + "Adr"), 2050))
                {
                    if (isAdmin)
                    {
                        ret.Add(new ServerItem
                        {
                            Name = Program.Settings.GetValue("svr" + i + "Name"),
                            Lat = 52.23,
                            Long = 4.55,
                            DNS = Program.Settings.GetValue("svr" + i + "Adr", "127.0.0.1"),
                            Usage = CheckUsage(Program.Settings.GetValue("svr" + i + "Adr"), 2050),
                            AdminOnly = Program.Settings.GetValue<bool>("svr" + i + "Admin", "false")
                        });
                    }
                    if (!isAdmin && (Program.Settings.GetValue<bool>("svr" + i + "Admin") == false))
                    {
                        ret.Add(new ServerItem
                        {
                            Name = Program.Settings.GetValue("svr" + i + "Name"),
                            Lat = 52.23,
                            Long = 4.55,
                            DNS = Program.Settings.GetValue("svr" + i + "Adr", "127.0.0.1"),
                            Usage = CheckUsage(Program.Settings.GetValue("svr" + i + "Adr"), 2050),
                            AdminOnly = Program.Settings.GetValue<bool>("svr" + i + "Admin", "false")
                        });
                    }
                }
            }
            return ret;
        }

        private List<ServerItem> NoServerList()
        {
            var ret = new List<ServerItem>();
            return ret;
        }

        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            if (query.AllKeys.Length == 0)
            {
                string queryString = string.Empty;
                string currUrl = context.Request.RawUrl;
                int iqs = currUrl.IndexOf('?');
                if (iqs >= 0)
                {
                    query =
                        HttpUtility.ParseQueryString((iqs < currUrl.Length - 1)
                            ? currUrl.Substring(iqs + 1)
                            : String.Empty);
                }
            }

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                bool isGuest = db.Verify(query["guid"], query["password"]) == null ? true : false;
                Account acc = db.Verify(query["guid"], query["password"]);

                var chrs = new Chars
                {
                    Characters = new List<Char>(),
                    NextCharId = 2,
                    MaxNumChars = 1,
                    Account = db.Verify(query["guid"], query["password"]),
                    Servers = isGuest ? NoServerList() : GetServerList(acc.Admin)
                };
                if (chrs.Account != null)
                {
                    if (!chrs.Account.isBanned)
                    {
                        db.GetCharData(chrs.Account, chrs);
                        db.LoadCharacters(chrs.Account, chrs);
                        chrs.News = db.GetNews(Program.GameData, chrs.Account);
                    }
                }
                else
                {
                    chrs.Account = Database.CreateGuestAccount(query["guid"]);
                    chrs.News = db.GetNews(Program.GameData, null);
                }
                chrs.ClassAvailabilityList = GetClassAvailability(chrs.Account);

                var ms = new MemoryStream();
                var serializer = new XmlSerializer(chrs.GetType(),
                    new XmlRootAttribute(chrs.GetType().Name) {Namespace = ""});

                var xws = new XmlWriterSettings();
                xws.OmitXmlDeclaration = true;
                xws.Encoding = Encoding.UTF8;
                XmlWriter xtw = XmlWriter.Create(context.Response.OutputStream, xws);
                serializer.Serialize(xtw, chrs, chrs.Namespaces);
            }
        }
    }
}