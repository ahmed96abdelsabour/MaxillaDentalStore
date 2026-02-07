using MaxillaDentalStore.Common.Pagination;
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


        // This method retrieves a user by their unique identifier (userId) from the database.
        public async Task<User?> GetByIdAsync(int userId)
        {
            // we can use FindAsync here because it will check the context first for the entity before querying the database, which can improve performance if the entity is already being tracked by the context.
            // also, FindAsync is more efficient for primary key lookups, as it can take advantage of the database's indexing on the primary key.
            return await _Context.Users
                .AsNoTracking() // we will use AsNoTracking here because we are only reading the data and not modifying it, which can improve performance by avoiding the overhead of tracking changes to the entities in the context.
                .FirstOrDefaultAsync(u => u.UserId == userId); // as we are using AsNoTracking, we cannot use FindAsync here, so we will use FirstOrDefaultAsync instead.
        }


        // This method retrieves a user by their email address from the database.
        public async Task<User?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            return await _Context.Users
                .AsNoTracking() // we will use AsNoTracking here because we are only reading the data and not modifying it, which can improve performance by avoiding the overhead of tracking changes to the entities in the context.
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

        // This method retrieves a paginated list of users from the database.
        public async Task<PageResult<User>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            // get all users, we will use AsNoTracking here because we are only reading the data and not modifying it, which can improve performance by avoiding the overhead of tracking changes to the entities in the context.
            var query = _Context.Users
                               .AsNoTracking()
                               .Where(u => u.Role == UserRole.Customer) // we will only return customers in this method, as admins are not relevant for most user listing scenarios, and this can also improve performance by reducing the number of records returned from the database.
                               .OrderBy(u => u.Name) // we will order the users by name to have a consistent ordering of the results, which is important for pagination to work correctly.
                               .AsQueryable();        // create the queryable to allow further filtering
                                                     
            // If we had an IsActive property, we could filter inactive users here
            if (!includeInactive)
            {
                // نجيب IDs المستخدمين اللي عندهم CartItems أو OrderItems مرة واحدة
                // استخدام EXISTS على جدول CartItems و OrderItems
                query = query.Where(u =>
                    _Context.CartItems.Any(ci => ci.Cart.UserId == u.UserId) ||
                    _Context.OrderItems.Any(oi => oi.Order.UserId == u.UserId)
                );
            }

            // Get total count before applying pagination
            var TotalUsers = await query.CountAsync();
            // Apply pagination
            var items = await query
                                .Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();

            // Return paged result 
            return new PageResult<User>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = TotalUsers
            };
        }

        // This method adds a new user to the database.
        public async Task AddUserAsync(User user)
        {
            var existingUser = await _Context.Users
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"A user with email {user.Email} already exists.");
            else
            await _Context.Users.AddAsync(user);
        }

        // This method deletes a user from the database based on their unique identifier (userId).
        public  Task Delete(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            var existingUser =  _Context.Users
                .Find(userId);
            if (existingUser != null)
              _Context.Users.Remove(existingUser);
            else
              throw new InvalidOperationException($"User with ID {userId} not found.");
            return Task.CompletedTask; // we use return Task.CompletedTask here because the Remove method does not perform any asynchronous operations, it simply marks the entity for deletion in the context. The actual database deletion will occur when SaveChangesAsync is called on the context, which is typically done in a unit of work pattern or at the service layer after all repository operations are completed.

        }

        // This method updates an existing user's information in the database.
        //public Task Update(User user)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException(nameof(user), "User cannot be null.");

        //    // changes will be tracked by the context, so we just need to mark the entity as modified. The actual update will occur when SaveChangesAsync is called on the context.
        //    _Context.Users.Update(user);
        //    return Task.CompletedTask; // we use return Task.CompletedTask here because the Update method does not perform any asynchronous operations, it simply marks the entity as modified in the context. The actual database update will occur when SaveChangesAsync is called on the context, which is typically done in a unit of work pattern or at the service layer after all repository operations are completed.
        //}

        // This method retrieves a user by their ID and includes related entities such as the user's cart, orders, reviews, and phone numbers.
        // The Include and ThenInclude methods are used to specify the related entities to be loaded along with the user.
        // This allows for eager loading of related data, which can improve performance by reducing the number of database queries needed to retrieve the complete user informationc
        // we will use this method when we need to display the user's profile information, including their cart contents, order history, reviews, and contact details, all in one query.
        // as retrieve heavy data, we will use this method only when necessary, such as when displaying the user's profile page or when performing operations that require access to the user's related data.
        // as this huge method can be cutted to many methods, but for simplicity we will keep it as one method for now, but in future we can refactor it to be more modular and maintainable.
        public async Task<User?> GetWithDetailsByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));

            return await _Context.Users
                             .Include(u => u.Cart)
                                .ThenInclude(c => c!.CartItems)
                             .Include(u => u.Orders)
                                .ThenInclude(o => o.OrderItems)
                             .Include(u => u.Reviews)
                             .Include(u => u.UserPhones)
                             .AsNoTracking() // we will use AsNoTracking here because we are only reading the data and not modifying it, which can improve performance by avoiding the overhead of tracking changes to the entities in the context.
                             .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
