using System;
using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class PaymentFailedEventHandler : IIntegrationEventHandler<PaymentFailedEvent>
    {
        private readonly AgoraDbContext _context;
        private readonly IEventBus _eventBus;
        private readonly ILogger<PaymentFailedEventHandler> _logger;

        public PaymentFailedEventHandler(AgoraDbContext context, IEventBus eventBus, ILogger<PaymentFailedEventHandler> logger)
        {
            _context = context;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(PaymentFailedEvent @event)
        {
            _logger.LogInformation("Handling PaymentFailedEvent for Order {OrderId}", @event.OrderId);

            var order = await _context.Orders.FindAsync(@event.OrderId);
            if (order != null)
            {
                order.PaymentStatus = "Failed";
                order.OrderStatus = -1; // Assuming -1 is Cancelled
                order.Note = $"Payment Failed: {@event.Reason}";
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                await _eventBus.Publish(new OrderCancelledEvent(@event.OrderId, @event.Reason));
            }
        }
    }
}
