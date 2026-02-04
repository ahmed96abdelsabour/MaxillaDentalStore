using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        // Navigation Properties :
        public ICollection<ProductCategory> productCategories { get; set; } = new List<ProductCategory>(); // many to many relationship with product
    }
}
