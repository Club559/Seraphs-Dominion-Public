using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.account
{
    internal class getBeginnerPackageTimeLeft : RequestHandler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                Account acc = db.Verify(query["guid"], query["password"]);
                byte[] status;
                if (acc == null)
                {
                    status = Encoding.UTF8.GetBytes("<Error>Account credentials not valid</Error>");
                }
                else
                {
                    MySqlCommand cmd = db.CreateQuery();
                    cmd.CommandText = "SELECT beginnerPackageTimeLeft FROM accounts WHERE uuid=@uuid";
                    cmd.Parameters.AddWithValue("@uuid", query["guid"]);
                    object result = cmd.ExecuteScalar();

                    status = Encoding.UTF8.GetBytes("<BeginnerPackageTimeLeft>" + result + "</BeginnerPackageTimeLeft>");
                }
                context.Response.OutputStream.Write(status, 0, status.Length);
            }
        }
    }
}