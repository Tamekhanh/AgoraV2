using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agora.Application.EventHandlers
{
    public class PaymentCompletedEventHandler : IIntegrationEventHandler<PaymentCompletedEvent>
    {
        private readonly AgoraDbContext _context;
        private readonly IEventBus _eventBus;

        public PaymentCompletedEventHandler(AgoraDbContext context, IEventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public async Task Handle(PaymentCompletedEvent @event)
        {
            var order = await _context.Orders.FindAsync(@event.OrderId);
            if (order != null)
            {
                order.PaymentStatus = "Paid";
                order.UpdatedAt = System.DateTime.UtcNow;
                
                // Update order status if needed, e.g., to "Processing"
                // order.OrderStatus = 1; // Assuming 1 is Processing

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                await _eventBus.Publish(new OrderConfirmedEvent(@event.OrderId));
            }
        }
    }
}
