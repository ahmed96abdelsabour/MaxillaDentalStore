using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Data.Entities
{
    /// <summary>
    /// Refresh Token entity for managing user sessions
    /// Allows token revocation and better security
    /// </summary>
    public class RefreshToken
    {
        public int RefreshTokenId { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = null!; // Hashed token for security
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; } // Null = Active, Not-Null = Revoked
        public string? DeviceId { get; set; } // للتحكم في الأجهزة المختلفة

        // Navigation Property
        public User User { get; set; } = null!;

        // Computed property
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}
