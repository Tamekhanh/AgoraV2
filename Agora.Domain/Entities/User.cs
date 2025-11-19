using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? Username { get; set; }

        // Lưu ý: Trong SQL bạn để BIT (boolean), nhưng Password thường là string. 
        // Tôi để string để hợp lý hóa logic, bạn nên kiểm tra lại kiểu dữ liệu trong SQL.
        public string? Password { get; set; }

        public int? Role { get; set; }
        public int? ImageId { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Navigation Properties
        public ImageFile? ImageFile { get; set; }
        public ICollection<Shop>? Shops { get; set; }
        public ICollection<Cart>? Carts { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
