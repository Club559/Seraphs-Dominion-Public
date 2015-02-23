using System.Collections.Generic;
using System.Net;
using System.Text;
using server.account;
using server.credits;
using server.guild;
using server.picture;
using add = server.clientError.add;
using delete = server.@char.delete;
using list = server.@char.list;

namespace server
{
    internal abstract class RequestHandler
    {
        public abstract void HandleRequest(HttpListenerContext context);

        protected void Write(HttpListenerContext txt, string val)
        {
            byte[] buff = Encoding.UTF8.GetBytes(val);
            txt.Response.OutputStream.Write(buff, 0, buff.Length);
        }
    }

    internal static class RequestHandlers
    {
        public static readonly Dictionary<string, RequestHandler> Handlers = new Dictionary<string, RequestHandler>
        {
            {"/crossdomain.xml", new crossdomain()},
            {"/char/list", new list()},
            {"/char/delete", new delete()},
            {"/char/fame", new @char.fame()},
            {"/clientError/add", new add()},
            {"/account/register", new register()},
            {"/account/verify", new verify()},
            {"/account/forgotPassword", new forgotPassword()},
            {"/account/sendVerifyEmail", new sendVerifyEmail()},
            {"/account/changePassword", new changePassword()},
            {"/account/purchaseCharSlot", new purchaseCharSlot()},
            {"/account/setName", new setName()},
            {"/account/getBeginnerPackageTimeLeft", new getBeginnerPackageTimeLeft()},
            {"/credits/getoffers", new getoffers()},
            {"/credits/add", new credits.add()},
            {"/fame/list", new fame.list()},
            {"/guild/getBoard", new getBoard()},
            {"/guild/setBoard", new setBoard()},
            {"/guild/listMembers", new listMembers()},
            {"/picture/get", new get()},
            {"/picture/list", new picture.list()},
            {"/picture/save", new save()},
            {"/picture/delete", new picture.delete()}
        };
    }
}