using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task AddUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<PageResult<User>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public async Task<User?> GetByIdAsync(int userId)
        {
            return await _unitOfWork.Users.GetByIdAsync(userId);
        }

        public Task<User?> GetWithDetailsByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
