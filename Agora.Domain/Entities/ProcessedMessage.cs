using System;

namespace Agora.Domain.Entities
{
    public class ProcessedMessage
    {
        public Guid Id { get; set; }
        public string MessageId { get; set; } = string.Empty;
        public string ConsumerName { get; set; } = string.Empty;
        public DateTime ProcessedOn { get; set; }
    }
}
