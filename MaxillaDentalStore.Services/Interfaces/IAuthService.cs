using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user, hashes password, and returns a JWT token.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto request);

        /// <summary>
        /// Authenticates a user by email/password and returns a JWT token.
        /// </summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto request, string? deviceId = null);

        /// <summary>
        /// Refreshes access token using refresh token.
        /// Implements token rotation for better security.
        /// </summary>
        Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request);

        /// <summary>
        /// Revokes refresh token (logout).
        /// </summary>
        Task LogoutAsync(string refreshToken);

        /// <summary>
        /// Revokes all user refresh tokens (logout all devices).
        /// </summary>
        Task LogoutAllDevicesAsync(int userId);
    }
}
