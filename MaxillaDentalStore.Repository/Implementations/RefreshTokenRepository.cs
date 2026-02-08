using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    /// <summary>
    /// Repository implementation for RefreshToken operations
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RefreshTokenRepository(AppDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == tokenHash);
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            var now = _dateTimeProvider.UtcNow;

            // استخدام Composite Index: IX_RefreshToken_UserId_RevokedAt_ExpiresAt
            return await _context.RefreshTokens
                .AsNoTracking()
                .Where(rt => rt.UserId == userId 
                    && rt.RevokedAt == null 
                    && rt.ExpiresAt > now)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            await _context.RefreshTokens.AddAsync(refreshToken);
        }

        public Task RevokeTokenAsync(RefreshToken refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            refreshToken.RevokedAt = _dateTimeProvider.UtcNow;
            _context.RefreshTokens.Update(refreshToken);
            
            return Task.CompletedTask;
        }

        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var now = _dateTimeProvider.UtcNow;

            // Get all active tokens for user
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            // Revoke all
            foreach (var token in activeTokens)
            {
                token.RevokedAt = now;
            }

            _context.RefreshTokens.UpdateRange(activeTokens);
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var now = _dateTimeProvider.UtcNow;

            // Find expired OR revoked tokens older than 30 days
            var tokensToDelete = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < now || 
                            (rt.RevokedAt != null && rt.RevokedAt < now.AddDays(-30)))
                .ToListAsync();

            _context.RefreshTokens.RemoveRange(tokensToDelete);
        }

        public async Task EnforceTokenLimitAsync(int userId, int maxTokens)
        {
            // Get all active tokens sorted by creation date (newest first)
            var activeTokens = await GetActiveTokensByUserIdAsync(userId);

            // If user has more than max allowed tokens, delete the oldest
            if (activeTokens.Count >= maxTokens)
            {
                var tokensToRevoke = activeTokens
                    .OrderBy(rt => rt.CreatedAt) // Oldest first
                    .Take(activeTokens.Count - maxTokens + 1) // Keep space for new token
                    .ToList();

                var now = _dateTimeProvider.UtcNow;
                foreach (var token in tokensToRevoke)
                {
                    var tokenToUpdate = await _context.RefreshTokens.FindAsync(token.RefreshTokenId);
                    if (tokenToUpdate != null)
                    {
                        tokenToUpdate.RevokedAt = now;
                    }
                }
            }
        }
    }
}
