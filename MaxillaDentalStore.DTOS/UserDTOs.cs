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
        public CartDto? Cart { get; set; }
        public List<OrderResponseDto> Orders { get; set; } = new List<OrderResponseDto>();
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

}
 