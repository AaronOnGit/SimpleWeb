using SimpleWeb.Models;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Web.Helpers;
using System.Web.Security;

namespace SimpleWeb.Services
{
    public static class AccountServices
    {
        //Login service
        public static bool PasswordSignIn(string email, string password, bool rememberMe)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Email == email && x.IsActivated == true).FirstOrDefault();
                if (ReferenceEquals(user, null))
                {
                    return false;
                }
                var passwordMatch = Crypto.VerifyHashedPassword(user.Password, password);
                if (!passwordMatch)
                {
                    return false;
                }
                FormsAuthentication.SetAuthCookie(user.Email, rememberMe);
                return true;
            }
        }

        //Add a new user service
        public static bool AddUser(string email, string password)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = new User
                {
                    Email = email,
                    Password = Crypto.HashPassword(password),
                    CreatedOn = DateTime.Now
                };
                db.Users.Add(user);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        //Check User's email in DB
        public static bool FindUserByEmail(string email)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Email == email).FirstOrDefault();
                if (ReferenceEquals(user, null))
                {
                    return false;
                }
                return true;
            }
        }

        //Service to check if the account is activated
        public static bool IsEmailConfirmed(string email)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Email == email && x.IsActivated == true).FirstOrDefault();
                if (ReferenceEquals(user, null))
                {
                    return false;
                }
                return true;
            }
        }

        //Token generation service
        public static string GenerateToken(string email)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Email == email).FirstOrDefault();
                //Check if there's a token in database OR has expired, 
                //then generate token else use same token
                if (user.Token == null || user.TokenExpiry <= DateTime.Now)
                {
                    string token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                        .Substring(0, 22)   //Removed padding characters "=="
                        .Replace("/", "_")
                        .Replace("+", "-"); //Replaced + as it is considered as space when in url.
                    user.Token = token;
                    user.TokenExpiry = DateTime.Now.AddHours(24);
                    db.SaveChanges();
                    return token;
                }
                else
                {
                    return user.Token;
                }
            }
        }

        //Token verification service
        public static bool VerifyToken(string token)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Token == token).FirstOrDefault();
                if (ReferenceEquals(user, null) || user.TokenExpiry <= DateTime.Now)
                {
                    return false;
                }
                return true;
            }
        }

        //Password update service
        public static bool UpdatePassword(string email, string password, string token)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Email == email && x.Token == token).FirstOrDefault();
                if (ReferenceEquals(user, null))
                {
                    return false;
                }
                user.Password = Crypto.HashPassword(password);
                user.TokenExpiry = DateTime.Now;
                db.SaveChanges();
                return true;
            }
        }

        //Service to activate user account
        public static bool ActivateUser(string token)
        {
            using (var db = new SimpleWebEntities())
            {
                var user = db.Users.Where(x => x.Token == token).FirstOrDefault();
                if (ReferenceEquals(user, null) || user.TokenExpiry <= DateTime.Now)
                {
                    return false;
                }
                user.IsActivated = true;
                db.SaveChanges();
                return true;
            }
        }

        //Email sending service
        public static void SendEmail(string destination, string subject, string body)
        {
            //string mailFromAddress = "jinqiaojue@gmail.com";
            //string mailFromPassword = "ksfhsnrmG5";
            //string host = "smtp.gmail.com";
            //int port = 587;

            MailMessage msg = new MailMessage();
            //msg.From = new MailAddress(mailFromAddress);
            msg.To.Add(destination);
            msg.Subject = subject;
            msg.SubjectEncoding = System.Text.Encoding.UTF8;
            msg.Body = body;
            msg.BodyEncoding = System.Text.Encoding.UTF8;
            msg.IsBodyHtml = true;

            //SmtpClient Client = new SmtpClient(host, port);
            SmtpClient Client = new SmtpClient();
            //Client.UseDefaultCredentials = false;
            //Client.Credentials = new System.Net.NetworkCredential(mailFromAddress, mailFromPassword);
            //Client.EnableSsl = true;
            //Client.DeliveryMethod = SmtpDeliveryMethod.Network;

            var MailSendingThread = new Thread(() =>
            {
                try
                {
                    Client.Send(msg);
                }
                catch (SmtpFailedRecipientException e)
                {
                    SmtpStatusCode statusCode = e.StatusCode;
                    if (statusCode == SmtpStatusCode.MailboxBusy
                        || statusCode == SmtpStatusCode.MailboxUnavailable
                        || statusCode == SmtpStatusCode.TransactionFailed)
                    {
                        Thread.Sleep(5000);
                        Client.Send(msg);
                    }
                }
                finally
                {
                    msg.Dispose();
                    Client.Dispose();
                }
            });

            MailSendingThread.Start();
        }
    }
}