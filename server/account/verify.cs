using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using db;

namespace server.account
{
    internal class verify : RequestHandler
    {
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
                Account acc = db.Verify(query["guid"], query["password"]);
                if (acc == null)
                {
                    byte[] status = Encoding.UTF8.GetBytes("<Error>Account credentials not valid</Error>");
                    context.Response.OutputStream.Write(status, 0, status.Length);
                }
                else
                {
                    var serializer = new XmlSerializer(acc.GetType(),
                        new XmlRootAttribute(acc.GetType().Name) {Namespace = ""});

                    var xws = new XmlWriterSettings();
                    xws.OmitXmlDeclaration = true;
                    xws.Encoding = Encoding.UTF8;
                    XmlWriter xtw = XmlWriter.Create(context.Response.OutputStream, xws);
                    serializer.Serialize(xtw, acc, acc.Namespaces);
                }
            }
        }
    }
}