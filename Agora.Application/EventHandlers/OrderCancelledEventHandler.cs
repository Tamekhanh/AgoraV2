using System;
using System.Threading.Tasks;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class OrderCancelledEventHandler : IIntegrationEventHandler<OrderCancelledEvent>
    {
        private readonly IProductService _productService;
        private readonly ICartService _cartService;
        private readonly AgoraDbContext _context;
        private readonly ILogger<OrderCancelledEventHandler> _logger;

        public OrderCancelledEventHandler(
            IProductService productService,
            ICartService cartService,
            AgoraDbContext context,
            ILogger<OrderCancelledEventHandler> logger)
        {
            _productService = productService;
            _cartService = cartService;
            _context = context;
            _logger = logger;
        }

        public async Task Handle(OrderCancelledEvent @event)
        {
            _logger.LogInformation("Handling OrderCancelledEvent for Order {OrderId}", @event.OrderId);

            // Release stock (Compensation)
            await _productService.ReleaseStock(@event.OrderId);

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems!)
                    .FirstOrDefaultAsync(o => o.Id == @event.OrderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found when trying to re-add items to cart after cancellation", @event.OrderId);
                    return;
                }

                if (!order.UserId.HasValue)
                {
                    _logger.LogWarning("Order {OrderId} has no associated UserId; cannot re-add items to cart", @event.OrderId);
                    return;
                }

                if (order.OrderItems == null)
                {
                    _logger.LogInformation("Order {OrderId} has no items to re-add to cart", @event.OrderId);
                    return;
                }

                foreach (var item in order.OrderItems)
                {
                    if (!item.ProductId.HasValue || !item.Quantity.HasValue || item.Quantity.Value <= 0)
                    {
                        continue;
                    }

                    var request = new AddToCartRequest
                    {
                        ProductId = item.ProductId.Value,
                        Quantity = item.Quantity.Value
                    };

                    try
                    {
                        await _cartService.AddToCart(order.UserId.Value, request);
                        _logger.LogInformation(
                            "Re-added product {ProductId} (qty {Quantity}) from cancelled order {OrderId} back to cart for user {UserId}",
                            item.ProductId.Value,
                            item.Quantity.Value,
                            @event.OrderId,
                            order.UserId.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to re-add product {ProductId} from cancelled order {OrderId} back to cart for user {UserId}",
                            item.ProductId.Value,
                            @event.OrderId,
                            order.UserId.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error when re-adding cancelled order {OrderId} items back to cart", @event.OrderId);
            }
        }
    }
}
