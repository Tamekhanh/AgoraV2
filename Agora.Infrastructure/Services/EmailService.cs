using System;
using System.Threading.Tasks;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agora.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body)
        {
            // In a real application, you would use an SMTP client or an email provider API (SendGrid, Mailgun, etc.)
            // For testing purposes, we will just log the email details.
            
            _logger.LogInformation("Sending email to {To}", to);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", body);

            // Simulate async work
            return Task.CompletedTask;
        }
    }
}
