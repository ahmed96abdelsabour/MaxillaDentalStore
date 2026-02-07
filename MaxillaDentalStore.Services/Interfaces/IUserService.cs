using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    /// <summary>
    /// Service interface for User operations
    /// </summary>
    public interface IUserService
    {
        // ==================== Read Operations ====================
        
        /// <summary>
        /// Get user by ID - returns basic user information
        /// </summary>
        Task<UserResponseDto?> GetByIdAsync(int userId);

        /// <summary>
        /// Get user by email - used for authentication
        /// </summary>
        Task<UserResponseDto?> GetByEmailAsync(string email);

        /// <summary>
        /// Get all users with pagination and optional inactive filter
        /// </summary>
        Task<PageResult<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false);

        // ==================== Details Operations ====================
        
        /// <summary>
        /// Get user details (summary version) - lightweight, suitable for dashboards
        /// </summary>
        Task<UserDetailsDto?> GetUserDetailsAsync(int userId);

        /// <summary>
        /// Get user full details - includes all collections (Cart, Orders, Reviews, Phones)
        /// Heavy operation - use only when all data is needed
        /// </summary>
        Task<UserFullDetailsDto?> GetUserFullDetailsAsync(int userId);

        // ==================== Write Operations ====================
        
        /// <summary>
        /// Create a new user
        /// </summary>
        Task<UserResponseDto> CreateUserAsync(UserCreateDto createDto);

        /// <summary>
        /// Update user information
        /// </summary>
        Task<UserResponseDto> UpdateUserAsync(UserUpdateDto updateDto);

        /// <summary>
        /// Delete user by ID
        /// </summary>
        Task DeleteUserAsync(int userId);

        // ==================== Phone Management ====================
        
        /// <summary>
        /// Add a phone number to user
        /// </summary>
        Task AddPhoneNumberAsync(int userId, string phoneNumber);

        /// <summary>
        /// Update an existing phone number
        /// </summary>
        Task UpdatePhoneNumberAsync(int phoneId, string newPhoneNumber);

        /// <summary>
        /// Remove a phone number from user
        /// </summary>
        Task RemovePhoneNumberAsync(int userId, int phoneId);

        // ==================== Utility Methods ====================
        
        /// <summary>
        /// Check if user is active (has CartItems or OrderItems)
        /// </summary>
        Task<bool> IsUserActiveAsync(int userId);
    }
}
