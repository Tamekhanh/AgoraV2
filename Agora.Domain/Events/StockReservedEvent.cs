using System;

namespace Agora.Domain.Events
{
    public class StockReservedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        
        public StockReservedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}
