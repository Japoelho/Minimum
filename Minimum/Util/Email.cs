using System.Net;
using System.Net.Mail;

namespace Minimum.Util
{
    public class Email
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SMTP { get; set; }
        public int Port { get; set; }

        public bool Send(MailMessage email)
        {
            NetworkCredential credentials = new NetworkCredential(Username, Password);
            SmtpClient smtpClient = new SmtpClient(SMTP, Port);

            smtpClient.EnableSsl = false;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = credentials;

            smtpClient.Send(email);

            return true;
        }
    }
}
