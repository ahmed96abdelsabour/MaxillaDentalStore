using AutoMapper;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Authentication;
using MaxillaDentalStore.Common.Helpers;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtProvider _jwtProvider;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;
        private const int MAX_REFRESH_TOKENS_PER_USER = 3; // حد أقصى 3 أجهزة

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher,
            IMapper mapper,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto request, string? deviceId = null)
        {
            // 1. Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            
            // 2. Validate user existence and password
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            // 3. Generate Access Token
            var accessToken = _jwtProvider.Generate(user);
            var accessTokenExpiresAt = _jwtProvider.GetExpirationTime();

            // 4. Generate Refresh Token
            var (refreshToken, refreshTokenHash) = GenerateRefreshToken();
            var refreshTokenExpiresAt = _dateTimeProvider.UtcNow.AddDays(30);

            // 5. Enforce token limit (max 3 devices)
            await _unitOfWork.RefreshTokens.EnforceTokenLimitAsync(user.UserId, MAX_REFRESH_TOKENS_PER_USER);

            // 6. Save Refresh Token to DB
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshTokenHash, // Store hashed token
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = _dateTimeProvider.UtcNow,
                DeviceId = deviceId
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CommitAsync();

            // 7. Return Response
            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Token = accessToken,
                RefreshToken = refreshToken, // Return plain token to user
                TokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto request)
        {
            // 1. Check if email already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            // 2. Map DTO to User Entity
            var user = _mapper.Map<User>(request);

            // 3. Hash Password
            user.PasswordHash = _passwordHasher.Hash(request.Password);
            user.CreatedAt = _dateTimeProvider.UtcNow;
            
            // Handle Phone Number (Manually for clarity)
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.UserPhones.Add(new UserPhone 
                { 
                    PhoneNumber = request.PhoneNumber
                });
            }
            
            // 4. Save User
            await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.CommitAsync();

            // 5. Generate Tokens
            var accessToken = _jwtProvider.Generate(user);
            var accessTokenExpiresAt = _jwtProvider.GetExpirationTime();

            var (refreshToken, refreshTokenHash) = GenerateRefreshToken();
            var refreshTokenExpiresAt = _dateTimeProvider.UtcNow.AddDays(30);

            // 6. Save Refresh Token
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = refreshTokenHash,
                ExpiresAt = refreshTokenExpiresAt,
                CreatedAt = _dateTimeProvider.UtcNow
            };

            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CommitAsync();

            // 7. Return Response
            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Token = accessToken,
                RefreshToken = refreshToken,
                TokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt
            };
        }

        public async Task<RefreshTokenResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1. Hash the incoming refresh token
            var tokenHash = HashToken(request.RefreshToken);

            // 2. Find refresh token in DB
            var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(tokenHash);

            // 3. Validate refresh token
            if (refreshToken == null || !refreshToken.IsActive)
            {
                return null; // Invalid or expired token
            }

            // 4. Get user
            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
            if (user == null)
            {
                return null;
            }

            // 5. Revoke old refresh token (Token Rotation for security)
            await _unitOfWork.RefreshTokens.RevokeTokenAsync(refreshToken);

            // 6. Generate new Access Token
            var newAccessToken = _jwtProvider.Generate(user);
            var newAccessTokenExpiresAt = _jwtProvider.GetExpirationTime();

            // 7. Generate new Refresh Token (Token Rotation)
            var (newRefreshToken, newRefreshTokenHash) = GenerateRefreshToken();
            var newRefreshTokenExpiresAt = _dateTimeProvider.UtcNow.AddDays(30);

            // 8. Save new Refresh Token
            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = user.UserId,
                Token = newRefreshTokenHash,
                ExpiresAt = newRefreshTokenExpiresAt,
                CreatedAt = _dateTimeProvider.UtcNow,
                DeviceId = request.DeviceId ?? refreshToken.DeviceId
            };

            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            await _unitOfWork.CommitAsync();

            // 9. Return new tokens
            return new RefreshTokenResponseDto
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                TokenExpiresAt = newAccessTokenExpiresAt,
                RefreshTokenExpiresAt = newRefreshTokenExpiresAt
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var tokenHash = HashToken(refreshToken);
            var token = await _unitOfWork.RefreshTokens.GetByTokenAsync(tokenHash);

            if (token != null && token.IsActive)
            {
                await _unitOfWork.RefreshTokens.RevokeTokenAsync(token);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task LogoutAllDevicesAsync(int userId)
        {
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(userId);
            await _unitOfWork.CommitAsync();
        }

        // ==================== Private Helper Methods ====================

        /// <summary>
        /// Generate cryptographically secure refresh token
        /// Returns: (Plain token for user, Hashed token for DB)
        /// </summary>
        private (string plainToken, string hashedToken) GenerateRefreshToken()
        {
            // Generate 64 bytes random token
            var randomBytes = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var plainToken = Convert.ToBase64String(randomBytes);
            var hashedToken = HashToken(plainToken);

            return (plainToken, hashedToken);
        }

        /// <summary>
        /// Hash refresh token using SHA256
        /// لأن refresh tokens طويلة ومخزنة في DB، نحتاج hash للـ security
        /// </summary>
        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
