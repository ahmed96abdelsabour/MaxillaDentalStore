using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; } // Foreign key to the Cart
        public int? ProductId { get; set; } // in case the cart item is a single product
        public int? PackageId { get; set; } // in case the cart item is a package
        public int Quantity { get; set; } 
        public decimal UnitPrice { get; set; }
        [NotMapped] // This property is not stored in the database, it's calculated on the fly
        public decimal TotalPrice => UnitPrice * Quantity; // Calculated property dynamically
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; } // Notes for specific item (e.g. "Gift wrap", "Check color")
        // Navigation Properties :
        public Cart Cart { get; set; } = null!; // Navigation property to the Cart
        public Product? Product { get; set; } // Navigation property to the Product, it's nullable because a cart item can be either a product or a package
        public Package? Package { get; set; }

    }
}
