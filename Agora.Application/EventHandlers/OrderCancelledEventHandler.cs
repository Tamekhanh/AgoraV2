using System;
using System.Threading.Tasks;
using Agora.Application.Service;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class OrderCancelledEventHandler : IIntegrationEventHandler<OrderCancelledEvent>
    {
        private readonly IProductService _productService;
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(IProductService productService, ILogger<OrderCancelledEventHandler> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task Handle(OrderCancelledEvent @event)
        {
            _logger.LogInformation("Handling OrderCancelledEvent for Order {OrderId}", @event.OrderId);

            // Release stock (Compensation)
            await _productService.ReleaseStock(@event.OrderId);
        }
    }
}
