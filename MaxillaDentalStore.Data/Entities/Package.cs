using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Package
    {
        public int PackageId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } 
        public decimal Price { get; set; }
        public bool IsAvilable { get; set; } = true;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation Properties :
        public ICollection<PackageItem> PackageItems { get; set; } = new List<PackageItem>();
        public ICollection<OrderItem> orderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> cartItems { get; set; } = new List<CartItem>();
    }
}
