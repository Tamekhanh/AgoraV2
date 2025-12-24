using System;

namespace Agora.Domain.Events
{
    public class OrderCancelledEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
        
        public OrderCancelledEvent(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }
    }
}
