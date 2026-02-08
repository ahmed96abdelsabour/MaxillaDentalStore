using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.DTOS
{
    // ==================== Request DTOs ====================

    /// <summary>
    /// Data required for user registration
    /// </summary>
    public class RegisterDto
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
        
        // Optional fields
        public string? PhoneNumber { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
    }

    /// <summary>
    /// Data required for user login
    /// </summary>
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    // ==================== Response DTOs ====================

    /// <summary>
    /// Auth success response containing Token and User Info
    /// Contains both Access Token and Refresh Token
    /// </summary>
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Role { get; set; } = null!; // "Admin" or "Customer"
        public string Token { get; set; } = null!; // Access Token (JWT) - Short-lived (15 min)
        public string RefreshToken { get; set; } = null!; // Refresh Token - Long-lived (30 days)
        public DateTime TokenExpiresAt { get; set; } // Access token expiration
        public DateTime RefreshTokenExpiresAt { get; set; } // Refresh token expiration
    }

    /// <summary>
    /// Request DTO for refreshing access token
    /// </summary>
    public class RefreshTokenRequestDto
    {
        public string RefreshToken { get; set; } = null!;
        public string? DeviceId { get; set; } // Optional: للتحكم في الأجهزة
    }

    /// <summary>
    /// Response DTO after successful token refresh
    /// </summary>
    public class RefreshTokenResponseDto
    {
        public string Token { get; set; } = null!; // New Access Token
        public string RefreshToken { get; set; } = null!; // New Refresh Token (Token Rotation)
        public DateTime TokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}

