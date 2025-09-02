using System.Net;
using System.Net.Mail;

namespace PinterestClone.Services
{
    public class EmailService
    {
        private readonly string? _smtpHost;
        private readonly int _smtpPort;
        private readonly string? _smtpUser;
        private readonly string? _smtpPass;
        private readonly string? _fromEmail;

        public EmailService()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            _smtpHost = config["Smtp:Host"] ?? throw new Exception("SMTP Host missing in appsettings.json");
            _smtpPort = int.Parse(config["Smtp:Port"] ?? throw new Exception("SMTP Port missing"));
            _smtpUser = config["Smtp:Username"] ?? throw new Exception("SMTP Username missing");
            _smtpPass = config["Smtp:Password"] ?? throw new Exception("SMTP Password missing");
            _fromEmail = config["Smtp:FromEmail"] ?? throw new Exception("FromEmail missing");

        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpHost, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                    client.EnableSsl = true;

                    if (string.IsNullOrEmpty(_fromEmail))
                        throw new Exception("FromEmail is missing in configuration.");

                    using (var mailMessage = new MailMessage(_fromEmail, toEmail, subject, body))
                    {
                        mailMessage.IsBodyHtml = true;
                        client.Send(mailMessage);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Error: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

    }
}
