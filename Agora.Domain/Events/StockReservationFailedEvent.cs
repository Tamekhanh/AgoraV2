using System;

namespace Agora.Domain.Events
{
    public class StockReservationFailedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public string Reason { get; set; }
        
        public StockReservationFailedEvent(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
        }
    }
}
