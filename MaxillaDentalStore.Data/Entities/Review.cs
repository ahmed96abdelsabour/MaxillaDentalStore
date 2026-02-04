using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int? ProductId { get; set; } // in case the review is for a product
        public int? PackageId { get; set; } // in case the review is for a package
        public string? ReviewText { get; set; }
        public int ReviewRate { get; set; }
        public DateTime CreatedAt { get; set; }
        // Navigation Properties :
        public User User { get; set; } = null!;
        public Product? Product { get; set; } 
        public Package? Package { get; set; }
    }
}
