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

        // Get all users
        Task<IEnumerable<User>> GetAllAsync();

        // Add user
        Task AddAsync(User user);

        // Update user
        void Update(User user); // we don't need async here as changes are tracked by the DbContext not by the repository as we are not saving changes here

        // Delete user
        void Delete(User user); // we don't need async here as changes are tracked by the DbContext not by the repository as we are not saving changes here
                                // we don't use Userid instead of User object to delete
                                //because we may have the user object already fetched from db and we can directly delete it without an extra db call to fetch the user again by id
                                // Update entity tracked by DbContext
                                // Delete entity, supports UnitOfWork pattern

        // Extra: Get user with related data (Cart + Orders + Reviews)
        Task<User?> GetWithDetailsByIdAsync(int userId);
    }
}
