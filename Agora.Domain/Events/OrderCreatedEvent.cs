using System;

namespace Agora.Domain.Events
{
    public class OrderCreatedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        
        public OrderCreatedEvent(int orderId, int userId, decimal totalAmount)
        {
            OrderId = orderId;
            UserId = userId;
            TotalAmount = totalAmount;
        }
    }
}
