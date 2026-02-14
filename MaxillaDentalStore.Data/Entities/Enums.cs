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

    public enum NotificationType
    {
        NewOrder = 1,           // For Admin - when a new order is created
        OrderConfirmed = 2,     // For Customer - when admin confirms the order
        OrderCancelled = 3,     // For Customer - when order is cancelled
        NewReview = 4           // For Admin - when a new review is added
    }
}
