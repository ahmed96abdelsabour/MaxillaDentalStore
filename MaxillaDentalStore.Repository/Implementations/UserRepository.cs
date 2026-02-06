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
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _Context;
        public UserRepository(AppDbContext context)
        {
            _Context = context;
        }
        public async Task<User?> GetByIdAsync(int userId)
        {
            // we can use FindAsync here because it will check the context first for the entity before querying the database, which can improve performance if the entity is already being tracked by the context.
            // also, FindAsync is more efficient for primary key lookups, as it can take advantage of the database's indexing on the primary key.
            return await _Context.Users.FindAsync(userId);
        }
        public async Task AddAsync(User user)
        {
            await _Context.Users.AddAsync(user);
        }

        public void  Delete(User user)
        {
             _Context.Users.Remove(user);
        }
        public void Update(User user)
        {
             _Context.Users.Update(user);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _Context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _Context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        // This method updates an existing user's information in the database.
        public Task Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "User cannot be null.");

            // changes will be tracked by the context, so we just need to mark the entity as modified. The actual update will occur when SaveChangesAsync is called on the context.
            _Context.Users.Update(user);
            return Task.CompletedTask; // we use return Task.CompletedTask here because the Update method does not perform any asynchronous operations, it simply marks the entity as modified in the context. The actual database update will occur when SaveChangesAsync is called on the context, which is typically done in a unit of work pattern or at the service layer after all repository operations are completed.
        }

        // This method retrieves a user by their ID and includes related entities such as the user's cart, orders, reviews, and phone numbers.
        // The Include and ThenInclude methods are used to specify the related entities to be loaded along with the user.
        // This allows for eager loading of related data, which can improve performance by reducing the number of database queries needed to retrieve the complete user informationc
        // we will use this method when we need to display the user's profile information, including their cart contents, order history, reviews, and contact details, all in one query.
        // as retrieve heavy data, we will use this method only when necessary, such as when displaying the user's profile page or when performing operations that require access to the user's related data.
        // as this huge method can be cutted to many methods, but for simplicity we will keep it as one method for now, but in future we can refactor it to be more modular and maintainable.
        public async Task<User?> GetWithDetailsByIdAsync(int userId)
        {
             return await _Context.Users
                             .Include(u => u.Cart)
                                .ThenInclude(c => c!.CartItems)
                             .Include(u => u.Orders)
                                .ThenInclude(o => o.OrderItems)
                             .Include(u => u.Reviews)
                             .Include(u => u.UserPhones)
                             .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
