using System;

namespace Agora.Domain.Events
{
    public class PaymentFailedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }

        public PaymentFailedEvent(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }
    }
}
