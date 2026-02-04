using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class ProductCategory
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        // Navigation Properties :
        public Product Product { get; set; } = null!; // many to many relationship between product and category
        public Category Category { get; set; } = null!; // many to many relationship between product and category


    }
}
