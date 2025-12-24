using System;

namespace Agora.Domain.Events
{
    public class OrderConfirmedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        
        public OrderConfirmedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}
