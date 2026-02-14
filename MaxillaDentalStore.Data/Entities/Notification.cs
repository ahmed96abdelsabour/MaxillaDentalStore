using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int RecipientUserId { get; set; } // Who receives the notification (Admin or Customer)
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int? OrderId { get; set; } // Nullable for non-order notifications
        public int? ReviewId { get; set; } // Nullable for non-review notifications
        public int? RelatedUserId { get; set; } // Related customer if notification is for admin
        public bool IsRead { get; set; } = false;
        public bool? IsFirstOrder { get; set; } // Only for NewOrder notifications
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public User RecipientUser { get; set; } = null!; // The user who receives the notification
        public User? RelatedUser { get; set; } // Related customer (for admin notifications)
        public Order? Order { get; set; } // Related order
        public Review? Review { get; set; } // Related review
    }
}
