using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; } // calculated at order time
        public string ShippingAddress { get; set; } = null!; // will navigate to clinic Address in future updates
        public string phoneNumber { get; set; } = null!; // will navigate to user phone numbers in future updates


        // Navigation Properties :
        public User User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
