using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class UserPhone
    {
        public int UserPhoneId { get; set; }
        public int UserId { get; set; } // Foreign key to User
        public string PhoneNumber { get; set; } = null!;
        // Navigation Properties :
        public User User { get; set; } = null!; // Many-to-One relationship with User
    }
}
