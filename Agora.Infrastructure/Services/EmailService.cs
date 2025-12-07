using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Agora.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            
            // Check if real email sending is enabled
            if (!bool.TryParse(emailSettings["EnableRealEmail"], out bool enableRealEmail) || !enableRealEmail)
            {
                _logger.LogInformation("[TEST MODE] Simulated sending email to {To}", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                return;
            }

            var host = emailSettings["Host"];
            var port = int.Parse(emailSettings["Port"] ?? "587");
            var fromEmail = emailSettings["FromEmail"];
            var password = emailSettings["Password"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Email settings are missing. Email to {To} was not sent.", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                return;
            }

            try
            {
                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(fromEmail, password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(fromEmail),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {To}", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }
    }
}
