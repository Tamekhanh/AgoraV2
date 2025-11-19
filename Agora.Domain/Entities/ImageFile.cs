using System;
using System.Collections.Generic;
using System.Text;

namespace Agora.Domain.Entities
{
    public class ImageFile
    {
        public int Id { get; set; }
        public byte[]? Data { get; set; } // Mapping cột [imageFile] binary

        // Navigation Properties (Optional: Reverse mapping)
        public ICollection<Product>? Products { get; set; }
        public ICollection<Shop>? Shops { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
