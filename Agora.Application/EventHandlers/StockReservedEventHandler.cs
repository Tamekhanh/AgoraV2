using System;
using System.Threading.Tasks;
using Agora.Application.DTOs;
using Agora.Application.Service;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agora.Application.EventHandlers
{
    public class StockReservedEventHandler : IIntegrationEventHandler<StockReservedEvent>
    {
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IEventBus _eventBus;
        private readonly ILogger<StockReservedEventHandler> _logger;

        public StockReservedEventHandler(IPaymentService paymentService, IOrderService orderService, IEventBus eventBus, ILogger<StockReservedEventHandler> logger)
        {
            _paymentService = paymentService;
            _orderService = orderService;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task Handle(StockReservedEvent @event)
        {
            _logger.LogInformation("Handling StockReservedEvent for Order {OrderId}", @event.OrderId);

            // Fetch order details to get amount
            // Assuming GetOrderDetail returns necessary info
            var orderDetail = await _orderService.GetOrderDetail(@event.OrderId);
            if (orderDetail == null || orderDetail.Order == null)
            {
                _logger.LogError("Order {OrderId} not found", @event.OrderId);
                return;
            }

            // Check if order is cancelled or not in pending state
            if (orderDetail.Order.OrderStatus == -1)
            {
                _logger.LogWarning("Order {OrderId} is cancelled. Skipping payment.", @event.OrderId);
                return;
            }

            var paymentRequest = new PaymentRequest
            {
                OrderId = @event.OrderId,
                Amount = orderDetail.Order.TotalAmount ?? 0,
                PaymentMethod = orderDetail.Order.PaymentMethod ?? "CreditCard", // Default or from order
                IdempotencyKey = $"Order-{@event.OrderId}"
            };

            try
            {
                var response = await _paymentService.ProcessPaymentAsync(paymentRequest);
                if (response.Success)
                {
                    _logger.LogInformation("Payment successful for Order {OrderId}", @event.OrderId);
                    await _eventBus.Publish(new PaymentCompletedEvent(@event.OrderId, response.PaymentId, response.TransactionId, DateTime.UtcNow));
                }
                else
                {
                    _logger.LogWarning("Payment failed for Order {OrderId}: {Message}", @event.OrderId, response.Message);
                    await _eventBus.Publish(new PaymentFailedEvent(@event.OrderId, response.Message));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for Order {OrderId}", @event.OrderId);
                await _eventBus.Publish(new PaymentFailedEvent(@event.OrderId, ex.Message));
            }
        }
    }
}
