using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.DTOS
{
    // ==================== Response DTOs ====================

    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public string Type { get; set; } = null!; // Enum as string
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        
        // Order-related properties (for NewOrder, OrderConfirmed notifications)
        public int? OrderId { get; set; }
        public OrderResponseDto? OrderDetails { get; set; }
        
        // Review-related properties (for NewReview notifications)
        public ReviewDetailsForNotificationDto? ReviewDetails { get; set; }
        
        // Common properties
        public string? RelatedCustomerName { get; set; } // Customer name for admin notifications
        public int? RelatedCustomerId { get; set; }
        public bool IsRead { get; set; }
        public bool? IsFirstOrder { get; set; } // Only populated for NewOrder notifications
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Review details for notification display - includes item info and rating
    /// </summary>
    public class ReviewDetailsForNotificationDto
    {
        public int ReviewId { get; set; }
        public int ReviewRate { get; set; }
        public string? ReviewText { get; set; }
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? PackageId { get; set; }
        public string? PackageName { get; set; }
    }

    // ==================== Internal DTOs ====================

    public class NotificationCreateDto
    {
        public int RecipientUserId { get; set; }
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int? OrderId { get; set; }
        public int? RelatedUserId { get; set; }
        public bool? IsFirstOrder { get; set; }
    }

    public class MarkAsReadDto
    {
        public int NotificationId { get; set; }
    }
}
