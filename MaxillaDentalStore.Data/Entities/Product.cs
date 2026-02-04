using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; } 
        public decimal Price { get; set; }
        public string? Company { get; set; }
        public bool HasColor { get; set; } 
        public string? Color { get; set; }
        public bool HasSize { get; set; }
        public string? Size { get; set; }
        public bool HasMaterial { get; set; }
        public string? Material { get; set; }
        public string UnitType { get; set; } = null!;
        public decimal Discount { get; set; }

        [NotMapped]
        public decimal FinalPrice => Price - (Price * Discount / 100);
        public bool IsActive { get; set; } = true;

        // Navigation Properties :
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<ProductImage> productImages { get; set; } = new List<ProductImage>();
        public ICollection<ProductCategory> productCategories { get; set; } = new List<ProductCategory>(); // many to many relationship with category
        public ICollection<PackageItem> packageItems { get; set; } = new List<PackageItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
