using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class PackageItem
    {
        public int PackageItemId { get; set; }
        public int PackageId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } // Quantity of the product in the package
        // Navigation Properties :
        public Package Package { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}
