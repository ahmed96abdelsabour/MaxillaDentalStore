using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        // Navigation Properties :
        public User User { get; set; } = null!;
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
