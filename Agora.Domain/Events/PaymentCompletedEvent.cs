using System;

namespace Agora.Domain.Events
{
    public class PaymentCompletedEvent : IntegrationEvent
    {
        public int OrderId { get; set; }
        public int PaymentId { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }

        public PaymentCompletedEvent(int orderId, int paymentId, string transactionId, DateTime paymentDate)
        {
            OrderId = orderId;
            PaymentId = paymentId;
            TransactionId = transactionId;
            PaymentDate = paymentDate;
        }
    }
}
