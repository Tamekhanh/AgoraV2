using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<UserRegisteredEventHandler> _logger;

        public UserRegisteredEventHandler(IEmailService emailService, ILogger<UserRegisteredEventHandler> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(UserRegisteredEvent @event)
        {
            _logger.LogInformation("Handling UserRegisteredEvent for user {UserId}", @event.UserId);
            await _emailService.SendEmailAsync(@event.Email, "Welcome to Agora", $"Welcome {@event.Name}, thanks for registering!");
        }
    }
}
