using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; } // Foreign key to Product
        public string ImageUrl { get; set; } = null!;
        // Navigation Properties :
        public Product Product { get; set; } = null!; // Navigation property to Product as product image is related to one product
    }
}
