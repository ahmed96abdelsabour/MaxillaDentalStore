using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IUserRepository
    {
        // Get user by Id
        Task<User?> GetByIdAsync(int userId);

        // Get user by Email, for login
        Task<User?> GetByEmailAsync(string email);

        // Get all users with Optional Pagination and Active status filter
        Task<PageResult<User>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false);

        // Add user
        Task AddUserAsync(User user);

        // Update user
        Task Update(User user);

        // Delete user
        Task Delete(int userId);

        // Extra: Get user with related data (Cart + Orders + Reviews)
        Task<User?> GetWithDetailsByIdAsync(int userId);

        /// <summary>
        /// Get user profile summary (lightweight) - for UserDetailsDto
        /// Includes UserPhones, Cart (with Items), and top 5 recent Orders
        /// </summary>
        Task<User?> GetSummaryProfileAsync(int userId);
    }
}
