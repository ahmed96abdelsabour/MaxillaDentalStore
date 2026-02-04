using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public UserRole Role { get; set; } 
        public DateTime CreatedAt { get; set; }

        // Navigation Properties : 
        public Cart? Cart { get; set; } // One-to-One relationship with Cart
        public ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>(); // One-to-Many relationship with UserPhone
        public ICollection<Order> Orders { get; set; } = new List<Order>(); // One-to-Many relationship with Order
        public ICollection<Review> Reviews { get; set; } = new List<Review>(); // One-to-Many relationship with Review
    }
}
