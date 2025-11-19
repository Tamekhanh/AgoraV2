using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? UnitPrice { get; set; }
        public int? Discount { get; set; }
        public int? Total { get; set; }

        // Navigation Properties
        public Order? Order { get; set; }
        public Product? Product { get; set; }
    }
}
