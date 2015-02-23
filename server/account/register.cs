using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.account
{
    internal class register : RequestHandler
    {
        public static bool IsUsername(string username)
        {
            string pattern;
            // start with a letter, allow letter or number, length between 6 to 12.
            pattern = @"^[a-zA-Z][a-zA-Z]{3,20}$";

            var regex = new Regex(pattern);
            return regex.IsMatch(username);
        }

        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                byte[] status;
                if (!IsUsername(query["newGUID"]))
                    status = Encoding.UTF8.GetBytes("<Error>Invalid username</Error>");
                else
                {
                    if (db.HasUuid(query["guid"]) &&
                        db.Verify(query["guid"], "") != null)
                    {
                        if (db.HasUuid(query["newGUID"]))
                            status = Encoding.UTF8.GetBytes("<Error>Duplicate username</Error>");
                        else if (db.HasEmail(query["email"]))
                            status = Encoding.UTF8.GetBytes("<Error>Duplicate email</Error>");
                        else
                        {
                            MySqlCommand cmd = db.CreateQuery();
                            cmd.CommandText =
                                "UPDATE accounts SET uuid=@newUuid, password=SHA1(@password), email=@email, guest=FALSE WHERE uuid=@uuid;";
                            cmd.Parameters.AddWithValue("@uuid", query["guid"]);
                            cmd.Parameters.AddWithValue("@newUuid", query["newGUID"]);
                            cmd.Parameters.AddWithValue("@password", query["newPassword"]);
                            cmd.Parameters.AddWithValue("@email", query["email"]);
                            if (cmd.ExecuteNonQuery() > 0)
                                status = Encoding.UTF8.GetBytes("<Success />");
                            else
                                status = Encoding.UTF8.GetBytes("<Error>Internal Error</Error>");
                        }
                    }
                    else
                    {
						if (db.HasUuid(query["newGUID"])) {
							status = Encoding.UTF8.GetBytes("<Error>Duplicate username</Error>");
						} else if (db.HasEmail(query["email"])) {
							status = Encoding.UTF8.GetBytes("<Error>Duplicate email</Error>");
						} else {
							if (db.Register(query["newGUID"], query["newPassword"], query["email"], false) != null)
								status = Encoding.UTF8.GetBytes("<Success />");
							else
								status = Encoding.UTF8.GetBytes("<Error>Internal Error</Error>");
						}
                    }
                }
                context.Response.OutputStream.Write(status, 0, status.Length);
            }
        }
    }
}