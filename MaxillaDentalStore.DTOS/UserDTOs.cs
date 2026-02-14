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
        public string Role { get; set; } = null!;
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
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<string> PhoneNumbers { get; set; } = new List<string>();
        public UserCartSummaryDto? Cart { get; set; }
        public List<UserOrderSummaryDto> RecentOrders { get; set; } = new List<UserOrderSummaryDto>();
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
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<UserPhoneDto> UserPhones { get; set; } = new List<UserPhoneDto>();
        public UserCartDto? Cart { get; set; }
        
        public List<UserOrderDto> Orders { get; set; } = new List<UserOrderDto>();
        public List<UserReviewDto> Reviews { get; set; } = new List<UserReviewDto>();
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

    // ==================== User-Specific Nested DTOs (Decoupled) ====================

    // --- Cart ---
    public class UserCartSummaryDto
    {
        public int CartId { get; set; }
        public int ItemsCount { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UserCartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; }
        public List<UserCartItemDto> Items { get; set; } = new List<UserCartItemDto>();
        public decimal TotalCartPrice => Items.Sum(ci => ci.TotalPrice);
    }

    public class UserCartItemDto
    {
        public int CartItemId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string ItemName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ImageUrl { get; set; }
    }

    // --- Order ---
    public class UserOrderSummaryDto
    {
        public int OrderId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public int ItemsCount { get; set; }
    }

    public class UserOrderDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string OrderStatus { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string? Notes { get; set; }
        public List<UserOrderItemDto> OrderItems { get; set; } = new List<UserOrderItemDto>();
    }

    public class UserOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int? ProductId { get; set; }
        public string? ItemName { get; set; }
        public string? ItemImage { get; set; }
        public int? PackageId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string? SelectedColor { get; set; }
        public string? SelectedSize { get; set; }
        public string? SelectedMaterial { get; set; }
        public string? ItemNotes { get; set; }
    }

    // --- Review ---
    public class UserReviewDto
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
 