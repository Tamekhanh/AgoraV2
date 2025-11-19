using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }
        public int? CartId { get; set; }
        public int? ProductId { get; set; }
        public int? Quantity { get; set; }
        public int? Status { get; set; }

        // Navigation Properties
        public Cart? Cart { get; set; }
        public Product? Product { get; set; }
    }
}
