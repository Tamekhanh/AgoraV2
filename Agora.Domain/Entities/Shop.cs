using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class Shop
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? TaxCode { get; set; }
        public string? Address { get; set; }
        public int? UserId { get; set; }
        public int? ImageId { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation Properties
        public User? User { get; set; }
        public ImageFile? ImageFile { get; set; }
        public ICollection<Product>? Products { get; set; }
    }
}
