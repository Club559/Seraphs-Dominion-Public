﻿using System;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using db;
using MySql.Data.MySqlClient;

namespace server.account
{
    internal class forgotPassword : RequestHandler
    {
        private static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var res = "";
            var rnd = new Random();
            while (0 < length--)
                res += valid[rnd.Next(valid.Length)];
            return res;
        }

        public bool IsValidEmail(string strIn)
        {
            var invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            MatchEvaluator domainMapper = match =>
            {
                // IdnMapping class with default property values.
                var idn = new IdnMapping();

                var domainName = match.Groups[2].Value;
                try
                {
                    domainName = idn.GetAscii(domainName);
                }
                catch (ArgumentException)
                {
                    invalid = true;
                }
                return match.Groups[1].Value + domainName;
            };

            // Use IdnMapping class to convert Unicode domain names. 
            strIn = Regex.Replace(strIn, @"(@)(.+)$", domainMapper);
            if (invalid)
                return false;

            // Return true if strIn is in valid e-mail format. 
            return Regex.IsMatch(strIn,
                      @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                      RegexOptions.IgnoreCase);
        }

        public override void HandleRequest(HttpListenerContext context)
        {
            var rand = new Random();
            NameValueCollection query;
            using (var rdr = new StreamReader(context.Request.InputStream))
                query = HttpUtility.ParseQueryString(rdr.ReadToEnd());

            using (var db = new Database(Program.Settings.GetValue("conn")))
            {
                byte[] status;

                var password = CreatePassword(rand.Next(8, 12));

                if (!db.HasEmail(query["email"]))
                    status = Encoding.UTF8.GetBytes("<Error>Account not found</Error>");
                else if (!IsValidEmail(query["email"]))
                    status = Encoding.UTF8.GetBytes("<Error>Invalid email address</Error>");
                else
                {
                    MySqlCommand cmd = db.CreateQuery();
                    cmd.CommandText = "UPDATE accounts SET password=SHA1(@password) WHERE email=@email;";
                    cmd.Parameters.AddWithValue("@email", query["email"]);
                    cmd.Parameters.AddWithValue("@password", password);

                    if (cmd.ExecuteNonQuery() == 0)
                        status = Encoding.UTF8.GetBytes("<Error>Could not reset account password!</Error>");
                    else
                    {
                        cmd = db.CreateQuery();
                        cmd.CommandText = "SELECT id, uuid, name, email FROM accounts WHERE email=@email";
                        cmd.Parameters.AddWithValue("@email", query["email"]);
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (!rdr.HasRows) return;
                            rdr.Read();

                            Console.WriteLine("Sending Mail to : " + rdr.GetString("email").ToLower());

                            var to = rdr.GetString("email").ToLower();
                            const string @from = "noreply@zerorealms.com";
                            var message = new MailMessage(from, to)
                            {
                                Subject = "Email Verification for : " + rdr.GetString("name")
                            };
                            string htmlBody =
                                @"
<html>
<body style='font-family: 'Segoe UI','Helvetica Neue',Helvetica,Arial,sans-serif;font-size: 13px;line-height: 20px;color: #333;margin:0;padding:0;'>
    <table cellspacing='0' cellpadding='0' border='0' width='100%'>
        <tr style='background: #3f3f3f'>
            <td class='navbar navbar-inverse' align='center'>
                <table width='650px' cellspacing='0' cellpadding='3' class='container' style='width:auto;margin:0;padding:0;'>
                    <tr class='navbar navbar-inverse'>
                        <td><a style='margin:0px -3px;text-decoration:none;display: block;padding: 20px;font-size: 20px;font-weight: 200;color: rgb(241,241,241);text-shadow: 0px -1px 1px rgba(0, 0, 0, 0.55);' class='brand' href='http://zerorealms.com'>Home Page</a></td>
                        <td><a style='margin:0px -3px;text-decoration:none;display: block;padding: 20px;font-size: 20px;font-weight: 200;color: rgb(241,241,241);text-shadow: 0px -1px 1px rgba(0, 0, 0, 0.55);' href='http://forum.zerorealms.com'>Forums</a></li></ul></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr style='background: #d6d6d6;'>
            <td align='center'>
                <table width='650px' cellspacing='0' cellpadding='3' class='container' style='width:960px;margin-top:15px;'>
                    <tr>
                        <th colspan='2'><h1 style='margin: 10px 0px;font-family: inherit;font-weight: bold;line-height: 40px;font-size: 36px;color: inherit;text-rendering: optimizelegibility;margin-bottom: 35px;'>Forgot Password</h1></th>
                    </tr>
                    <tr style='height:40px;'>
                        <td align='right' style='width:42%;'><p style='font-weight:600;color:#474747;'>Account Name : </p></td><td style='width:58%;'><b>" +
                                rdr.GetString("uuid") + @"</b></td>
                    </tr>
                    <tr style='height:40px;'>
                        <td align='right' style='width:42%;'><p style='font-weight:600;color:#474747;'>Character Name : </p></td><td style='width:58%;'><b>" +
                                rdr.GetString("name") + @"</b></td>
                    </tr>
                    <tr style='height:40px;'>
                        <td align='right' style='width:42%;'><p style='font-weight:600;color:#474747;'>Email Address : </p></td><td style='width:58%;'><b style='color:#0063CA;text-decoration:none;'>" +
                                rdr.GetString("email") + @"</b></td>
                    </tr>
					<tr style='height:80px;'>
                        <td colspan='2' align='center' style='width:100%;'><p style='font-weight:600;color:#474747;'>A password reset has been requested for the account described above.<br />We've generated a new password for your account that is active immediately.<br />Please use the password shown below when you login.</p></td>
                    </tr>
                    <tr style='height:40px;'>
                        <td align='right' style='width:42%;'><p style='font-weight:600;color:#474747;'>New Password : </p></td><td style='width:58%;'><b style='color:#0063CA;text-decoration:none;'>" +
                                password + @"</b></td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr style='background: #d6d6d6;'>
            <td align='center'>
                <table width='650px' cellspacing='0' cellpadding='3' class='container' style='width:960px;'>
                    <tr>
                        <td align='center' style='font-size:12pt;'>
                            <hr style='border-right: 0px none;border-left: 0px none;-moz-border-top-colors: none;-moz-border-right-colors: none;-moz-border-bottom-colors: none;-moz-border-left-colors: none;border-image: none;border-width: 1px 0px;border-style: solid none;border-color: #EEE -moz-use-text-color #FFF;'>
                            <p style='height:80px;margin-bottom:35px;'><b>Copyright &copy; 2013-2014 | <a style='color: #0063CA;text-decoration:none;' href='http://zerorealms.com'>ZeroRealms.com</a> - Lucifer</b></p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
                            message.Body = htmlBody;
                            message.IsBodyHtml = true;
                            var client = new SmtpClient
                            {
                                Port = 587,
                                Host = "smtp.mandrillapp.com",
                                Credentials = new NetworkCredential("LuciTheForgotten@gmail.com",
                                    "xI7YXXrmtLC5BdwIEzsG0w")
                            };
                            // Credentials are necessary if the server requires the client  
                            // to authenticate before it will send e-mail on the client's behalf.

                            try
                            {
                                client.Send(message);
                                Console.WriteLine("Sent mail to : " + rdr.GetString("email").ToLower());
                                return;
                            }
                            catch
                            {
                                return;
                            }
                        }
                    }
                }
                context.Response.OutputStream.Write(status, 0, status.Length);
            }
        }
    }
}