using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime? OrderDate { get; set; }
        public int? TotalAmount { get; set; }
        public int? Discount { get; set; }
        public int? Tax { get; set; }
        public int? ShippingFee { get; set; }
        public string? PaymentMethod { get; set; }
        public string? PaymentStatus { get; set; }
        public int? OrderStatus { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Payment>? Payments { get; set; }
    }
}
