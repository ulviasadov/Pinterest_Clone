using System.Net;
using System.Net.Mail;

namespace PinterestClone.Services
{
    public class EmailService
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;

        public EmailService()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _smtpHost = config["Smtp:Host"];
            _smtpPort = int.Parse(config["Smtp:Port"]);
            _smtpUser = config["Smtp:Username"];
            _smtpPass = config["Smtp:Password"];
            _fromEmail = config["Smtp:FromEmail"];
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var client = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true
                };
                var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body)
                {
                    IsBodyHtml = true
                };
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                // Log error to console for debugging
                Console.WriteLine($"SMTP Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
