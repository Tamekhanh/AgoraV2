using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class Cart
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }
    }
}
