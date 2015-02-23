using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace server.picture
{
    internal class get : RequestHandler
    {
        private readonly byte[] buff = new byte[0x10000];

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

            //warning: maybe has hidden url injection
            string id = query["id"];
            string instance = query["instance"];

            byte[] status = Encoding.UTF8.GetBytes("<Error>Bad Request</Error>");
            ;

            if (instance != "local" || instance != "production" || instance != "testing")
                status = Encoding.UTF8.GetBytes("<Error>Invalid Instance.</Error>");
            else
            {
                foreach (char i in id)
                {
                    if (char.IsLetter(i) || i == '_' || i == '-') continue;

                    status = Encoding.UTF8.GetBytes("<Error>Invalid ID.</Error>");
                    context.Response.OutputStream.Write(status, 0, status.Length);
                    return;
                }
                string path = Path.GetFullPath("texture/_" + id + ".png");
                if (!File.Exists(path))
                {
                    status = Encoding.UTF8.GetBytes("<Error>Invalid ID.</Error>");
                    context.Response.OutputStream.Write(status, 0, status.Length);
                    return;
                }

                context.Response.ContentType = "image/png";
                using (FileStream i = File.OpenRead(path))
                {
                    int c;
                    while ((c = i.Read(buff, 0, buff.Length)) > 0)
                        context.Response.OutputStream.Write(buff, 0, c);
                }
            }
            context.Response.OutputStream.Write(status, 0, status.Length);
        }
    }
}