using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.DTOS
{
    // ==================== User DTOs ====================
    
    /// <summary>
    /// DTO for user response - used for general user data retrieval
    /// </summary>
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } // Computed from CartItems/OrderItems
    }

    /// <summary>
    /// DTO for creating a new user
    /// SECURITY: Role defaults to Customer. Controller must explicitly set Admin role for admin creation.
    /// </summary>
    public class UserCreateDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public UserRole Role { get; set; } = UserRole.Customer; // Default: Customer (Security)
        public List<string>? PhoneNumbers { get; set; }
    }

    /// <summary>
    /// DTO for updating user information
    /// </summary>
    public class UserUpdateDto
    {
        public int UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
    }

    /// <summary>
    /// DTO for user details - Summary version (lightweight)
    /// </summary>
    public class UserDetailsDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public CartSummaryDto? Cart { get; set; }
        public List<OrderSummaryDto> RecentOrders { get; set; } = new List<OrderSummaryDto>();
        public int TotalOrders { get; set; }
        public int TotalReviews { get; set; }
    }

    /// <summary>
    /// DTO for user full details - includes all collections (heavy operation)
    /// </summary>
    public class UserFullDetailsDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserPhoneDto> UserPhones { get; set; } = new List<UserPhoneDto>();
        public CartFullDto? Cart { get; set; }
        public List<OrderFullDto> Orders { get; set; } = new List<OrderFullDto>();
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }

    // ==================== UserPhone DTOs ====================

    /// <summary>
    /// DTO for user phone number
    /// </summary>
    public class UserPhoneDto
    {
        public int UserPhoneId { get; set; }
        public string PhoneNumber { get; set; } = null!;
    }

    // ==================== Cart DTOs ====================

    /// <summary>
    /// Cart Summary - lightweight for user details
    /// </summary>
    public class CartSummaryDto
    {
        public int CartId { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    /// <summary>
    /// Cart Full - includes all cart items
    /// </summary>
    public class CartFullDto
    {
        public int CartId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new List<CartItemDto>();
        public decimal TotalPrice => CartItems.Sum(ci => ci.TotalPrice);
    }

    /// <summary>
    /// Individual cart item
    /// </summary>
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string ItemName { get; set; } = null!; // Product or Package name
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
    }

    // ==================== Order DTOs ====================


    /// <summary>
    /// Order Full - includes all order items
    /// </summary>
    public class OrderFullDto
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public string? Notes { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    }

    // ==================== Review DTOs ====================

    /// <summary>
    /// Review DTO
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string? ProductName { get; set; }
        public string? PackageName { get; set; }
        public string? ReviewText { get; set; }
        public int ReviewRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
 