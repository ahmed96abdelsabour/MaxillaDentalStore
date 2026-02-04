using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; } // in case the order item is a package
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; } // i need to store this data at order time and to keep historical price record    
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; }
        // Navigation Properties :
        public Order Order { get; set; } = null!;
        public Product? Product { get; set; } 
        public Package? Package { get; set; } 
    }
}
