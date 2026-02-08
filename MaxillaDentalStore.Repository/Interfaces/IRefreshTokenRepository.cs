using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for RefreshToken operations
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// Get refresh token by token string (hashed)
        /// </summary>
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);

        /// <summary>
        /// Get all active refresh tokens for a user
        /// </summary>
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);

        /// <summary>
        /// Add new refresh token
        /// </summary>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// Revoke a specific refresh token
        /// </summary>
        Task RevokeTokenAsync(RefreshToken refreshToken);

        /// <summary>
        /// Revoke all active tokens for a user (useful for logout all devices)
        /// </summary>
        Task RevokeAllUserTokensAsync(int userId);

        /// <summary>
        /// Delete expired tokens (cleanup job)
        /// </summary>
        Task DeleteExpiredTokensAsync();

        /// <summary>
        /// Limit user to maximum number of refresh tokens (e.g., 3 devices)
        /// Deletes oldest tokens if limit exceeded
        /// </summary>
        Task EnforceTokenLimitAsync(int userId, int maxTokens);
    }
}
