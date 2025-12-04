using Agora.Domain.Events;

namespace Agora.Domain.Events
{
    public class UserRegisteredEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

        public UserRegisteredEvent(string userId, string email, string name)
        {
            UserId = userId;
            Email = email;
            Name = name;
        }
    }
}
