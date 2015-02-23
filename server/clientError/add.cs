using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using db;

namespace server.clientError
{
    internal class add : RequestHandler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                string username = query["guid"];
                string error = query["text"];

                string errors = @"errors";
                if (!Directory.Exists(errors))
                {
                    Directory.CreateDirectory(errors);
                }
                using (var writer = new StreamWriter(errors + @"\clientError", true))
                {
                    writer.WriteLine(username + " Sent Error : \n\r" + error);
                }

                byte[] status = Encoding.UTF8.GetBytes("<Success/>");
                context.Response.OutputStream.Write(status, 0, status.Length);
            }
        }
    }
}