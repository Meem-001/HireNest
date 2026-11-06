using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace InternshipManagementSystem.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");

            using var client = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"]));
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);

            var message = new MailMessage(
                from: new MailAddress(smtpSettings["SenderEmail"], smtpSettings["SenderName"]),
                to: new MailAddress(toEmail)
            );
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            client.Send(message);
        }
    }
}