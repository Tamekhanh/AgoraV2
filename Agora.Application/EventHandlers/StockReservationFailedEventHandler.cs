using System;
using System.Threading.Tasks;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class StockReservationFailedEventHandler : IIntegrationEventHandler<StockReservationFailedEvent>
    {
        private readonly AgoraDbContext _context;
        private readonly IEventBus _eventBus;
        private readonly ILogger<StockReservationFailedEventHandler> _logger;

        public StockReservationFailedEventHandler(AgoraDbContext context, IEventBus eventBus, ILogger<StockReservationFailedEventHandler> logger)
        {
            _context = context;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(StockReservationFailedEvent @event)
        {
            _logger.LogInformation("Handling StockReservationFailedEvent for Order {OrderId}", @event.OrderId);

            var order = await _context.Orders.FindAsync(@event.OrderId);
            if (order != null)
            {
                order.OrderStatus = -1; // Cancelled
                order.Note = $"Stock Reservation Failed: {@event.Reason}";
                order.UpdatedAt = DateTime.UtcNow;

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // No need to publish OrderCancelledEvent here as stock wasn't reserved, 
                // but if we had other steps before stock, we might need to compensate them.
                // For consistency, we can publish it.
                await _eventBus.Publish(new OrderCancelledEvent(@event.OrderId, @event.Reason));
            }
        }
    }
}
