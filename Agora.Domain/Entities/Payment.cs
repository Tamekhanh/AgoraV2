using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? Amount { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Navigation Properties
        public Order? Order { get; set; }
    }
}
