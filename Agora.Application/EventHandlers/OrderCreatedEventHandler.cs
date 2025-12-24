using System;
using System.Threading.Tasks;
using Agora.Application.Service;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class OrderCreatedEventHandler : IIntegrationEventHandler<OrderCreatedEvent>
    {
        private readonly IProductService _productService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<OrderCreatedEventHandler> _logger;

        public OrderCreatedEventHandler(IProductService productService, IEventBus eventBus, ILogger<OrderCreatedEventHandler> logger)
        {
            _productService = productService;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(OrderCreatedEvent @event)
        {
            _logger.LogInformation("Handling OrderCreatedEvent for Order {OrderId}", @event.OrderId);

            // Stock is already reserved synchronously in OrderService.CheckoutAsync
            // So we just proceed to the next step (Payment) by publishing StockReservedEvent
            
            _logger.LogInformation("Stock already reserved (sync) for Order {OrderId}", @event.OrderId);
            await _eventBus.Publish(new StockReservedEvent(@event.OrderId));
        }
    }
}
