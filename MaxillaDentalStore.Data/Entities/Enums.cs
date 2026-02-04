using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public enum UserRole
    {
        Admin = 1,
        Customer = 2
    }

    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3
    }
}
