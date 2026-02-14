using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.DTOS
{
    // ==================== Order Core DTOs ====================

    /// <summary>
    /// DTO for creating a new order from the user's active cart
    /// </summary>
    public class OrderCreateDto
    {
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for order response - full details
    /// </summary>
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; } // Reference to user
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public bool IsFirstOrder { get; set; }

        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    /// <summary>
    /// DTO for order summary (lightweight for lists)
    /// </summary>
    public class OrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemsCount { get; set; }
        public bool IsFirstOrder { get; set; }
    }

    /// <summary>
    /// DTO for updating order details (Shipping/Notes)
    /// Only allowed if order is Pending
    /// </summary>
    public class OrderUpdateDto
    {
        public int OrderId { get; set; }
        public string? ShippingAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Notes { get; set; }
    }

    // ==================== Supporting DTOs ====================

    /// <summary>
    /// DTO for order item details
    /// </summary>
    public class OrderItemDto
    {
        public int OrderItemId { get; set; }
        public int? ProductId { get; set; }
        public string? ItemName { get; set; } // Product or Package name
        public string? ItemImage { get; set; } // Generic image (Product or Package)
        public int? PackageId { get; set; }
        
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        
        // Variant details
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; }
    }

    /// <summary>
    /// DTO for updating order status (Admin)
    /// </summary>
    public class OrderStatusUpdateDto
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
    }
}
