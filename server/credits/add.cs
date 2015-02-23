using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.credits
{
    internal class add : RequestHandler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            string status;
            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                NameValueCollection query = HttpUtility.ParseQueryString(context.Request.Url.Query);

                MySqlCommand cmd = db.CreateQuery();
                cmd.CommandText = "SELECT id FROM accounts WHERE uuid=@uuid";
                cmd.Parameters.AddWithValue("@uuid", query["guid"]);
                object id = cmd.ExecuteScalar();

                /*if (id != null)
                {
                    int amount = int.Parse(query["jwt"]);
                    if (amount > 0)
                    {
                        cmd.CommandText = "UPDATE stats SET totalCredits = totalCredits + @amount WHERE accId=@accId;";
                        cmd.Parameters.AddWithValue("@accId", (int) id);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.ExecuteNonQuery();
                    }
                    cmd = db.CreateQuery();
                    cmd.CommandText = "UPDATE stats SET credits = credits + @amount WHERE accId=@accId";
                    cmd.Parameters.AddWithValue("@accId", (int) id);
                    cmd.Parameters.AddWithValue("@amount", amount);
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                        status = "Ya done...";
                    else
                        status = "Internal error :(";
                }
                else
                    status = "Account not exists :(";*/
                status = "Feature has been disabled";
            }

            byte[] res = Encoding.UTF8.GetBytes(
                @"<html>
    <head>
        <title>Ya...</title>
    </head>
    <body style='background: #333333'>
        <h1 style='color: #EEEEEE; text-align: center'>
            " + status + @"
        </h1>
    </body>
</html>");
            context.Response.OutputStream.Write(res, 0, res.Length);
        }
    }
}