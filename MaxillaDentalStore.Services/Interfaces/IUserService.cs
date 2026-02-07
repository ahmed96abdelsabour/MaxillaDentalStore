using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IUserService
    {
        // Get user by Id
        Task<User?> GetByIdAsync(int userId);

        // Get user by Email, for login
        Task<User?> GetByEmailAsync(string email);

        // Get all users with pagination and optional active filter
        Task<PageResult<User>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false);

        // Add a new user
        Task AddUserAsync(User user);

        // Update user
        Task UpdateUserAsync(User user);

        // Delete user
        Task DeleteUserAsync(int userId);

        // Get user with details (Cart, Orders, Reviews, Phones)
        Task<User?> GetWithDetailsByIdAsync(int userId);
        
       
    }
}
