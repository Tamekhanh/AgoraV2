using System;
using System.Threading.Tasks;
using Agora.Application.DTOs;
using Agora.Domain.Entities;
using Agora.Domain.Events;
using Agora.Domain.Interfaces;
using Agora.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Agora.Application.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly AgoraDbContext _context;
        private readonly IEventBus _eventBus;

        public PaymentService(AgoraDbContext context, IEventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            // 1. Idempotency Check
            if (!string.IsNullOrEmpty(request.IdempotencyKey))
            {
                var existingPayment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.IdempotencyKey == request.IdempotencyKey);

                if (existingPayment != null)
                {
                    return new PaymentResponse
                    {
                        Success = existingPayment.Status == "Completed",
                        Message = "Payment already processed (Idempotent)",
                        TransactionId = existingPayment.TransactionId ?? string.Empty,
                        PaymentId = existingPayment.Id
                    };
                }
            }

            // 2. Demo Payment Logic
            // SIMULATION: 80% failure rate (only 20% success chance)
            // Random 0-99. If < 80 (0-79) => Fail. If >= 80 (80-99) => Success.
            bool isSuccess = request.Amount > 0 && (new Random().Next(100) >= 80);
            
            string transactionId = Guid.NewGuid().ToString();
            string status = isSuccess ? "Completed" : "Failed";

            // 3. Create Payment Entity
            var payment = new Agora.Domain.Entities.Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Method = request.PaymentMethod,
                Status = status,
                TransactionId = transactionId,
                IdempotencyKey = request.IdempotencyKey,
                PaymentDate = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 4. Publish Event (Saga Step)
            if (isSuccess)
            {
                var paymentCompletedEvent = new PaymentCompletedEvent(
                    request.OrderId,
                    payment.Id,
                    transactionId,
                    payment.PaymentDate.Value
                );
                await _eventBus.Publish(paymentCompletedEvent);
            }
            else
            {
                var paymentFailedEvent = new PaymentFailedEvent(
                    request.OrderId,
                    "Payment amount must be greater than 0"
                );
                await _eventBus.Publish(paymentFailedEvent);
            }

            return new PaymentResponse
            {
                Success = isSuccess,
                Message = isSuccess ? "Payment successful" : "Payment failed",
                TransactionId = transactionId,
                PaymentId = payment.Id
            };
        }
    }
}
