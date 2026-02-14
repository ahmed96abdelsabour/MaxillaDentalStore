using AutoMapper;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{
    /// <summary>
    /// Service implementation for User operations
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context; // For IsActive calculation
        private readonly IDateTimeProvider _dateTimeProvider;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        // ==================== Read Operations ====================

        public async Task<UserResponseDto?> GetByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.IsActive = await IsUserActiveAsync(userId);
            
            return userDto;
        }

        public async Task<UserResponseDto?> GetByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            if (user == null)
                return null;

            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.IsActive = await IsUserActiveAsync(user.UserId);
            
            return userDto;
        }

        public async Task<UserResponseDto?> GetByPhoneNumberAsync(string phoneNumber)
        {
            var user = await _unitOfWork.Users.GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
                return null;
            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.IsActive = await IsUserActiveAsync(user.UserId);
            return userDto;
        }
        public async Task<PageResult<UserResponseDto>> GetAllUsersAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            // Get paginated users from repository (already filtered by active status if needed)
            var pagedUsers = await _unitOfWork.Users.GetAllUsersAsync(pageNumber, pageSize, includeInactive);

            // Map to DTOs
            var userDtos = _mapper.Map<List<UserResponseDto>>(pagedUsers.Items);

            // Calculate IsActive for each user (batch operation for performance)
            var userIds = userDtos.Select(u => u.UserId).ToList();
            var activeUserIds = await GetActiveUserIdsAsync(userIds);

            foreach (var userDto in userDtos)
            {
                userDto.IsActive = activeUserIds.Contains(userDto.UserId);
            }

            // Return paginated result
            return new PageResult<UserResponseDto>
            {
                Items = userDtos,
                PageNumber = pagedUsers.PageNumber,
                PageSize = pagedUsers.PageSize,
                TotalItems = pagedUsers.TotalItems
            };
        }

        // ==================== Details Operations ====================

        public async Task<UserDetailsDto?> GetUserDetailsAsync(int userId)
        {
            // Performance optimization: Using Repository's lightweight summary method
            var user = await _unitOfWork.Users.GetSummaryProfileAsync(userId);
            
            if (user == null)
                return null;

            var userDetails = _mapper.Map<UserDetailsDto>(user);
            
            // Map Cart to CartSummaryDto if exists
            if (user.Cart != null)
            {
                userDetails.Cart = _mapper.Map<UserCartSummaryDto>(user.Cart);
            }

            // Get reviews count efficiently (kept here as it's specific aggregation not loaded in entity)
            userDetails.TotalReviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .CountAsync();
            
            // Get total orders count
            userDetails.TotalOrders = await _context.Orders
                .Where(o => o.UserId == userId)
                .CountAsync();

            return userDetails;
        }

        public async Task<UserFullDetailsDto?> GetUserFullDetailsAsync(int userId)
        {
            // Use repository method that loads all related data
            var user = await _unitOfWork.Users.GetWithDetailsByIdAsync(userId);
            
            if (user == null)
                return null;

            return _mapper.Map<UserFullDetailsDto>(user);
        }

        // ==================== Write Operations ====================

        public async Task<UserResponseDto> CreateUserAsync(UserCreateDto createDto)
        {
            // Sanitize inputs
            if (string.IsNullOrWhiteSpace(createDto.ClinicName)) createDto.ClinicName = null;
            if (string.IsNullOrWhiteSpace(createDto.ClinicAddress)) createDto.ClinicAddress = null;

            // Check if email already exists
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(createDto.Email);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {createDto.Email} already exists.");

            // Map DTO to Entity
            var user = _mapper.Map<User>(createDto);
            user.CreatedAt = _dateTimeProvider.UtcNow; // استخدام IDateTimeProvider للـ testability
            // Note: PasswordHash will be set from createDto.Password by mapping
            // In production, this should be hashed in AuthService before being passed here

            // Add user to database
            await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.CommitAsync();

            // Handle phone numbers if provided
            if (createDto.PhoneNumbers != null && createDto.PhoneNumbers.Any())
            {
                foreach (var phoneNumber in createDto.PhoneNumbers)
                {
                    if (string.IsNullOrWhiteSpace(phoneNumber)) continue;

                    var userPhone = new UserPhone
                    {
                        UserId = user.UserId,
                        PhoneNumber = phoneNumber
                    };
                    await _context.UserPhones.AddAsync(userPhone);
                }
                await _unitOfWork.CommitAsync();
            }

            // Return created user as DTO
            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.IsActive = false; // New user has no cart/orders yet
            
            return userDto;
        }

        public async Task<UserResponseDto> UpdateUserAsync(UserUpdateDto updateDto)
        {
            // Get existing user - مش بنستخدم GetByIdAsync عشان محتاجين tracked entity للـ update
            // GetByIdAsync بيستخدم AsNoTracking فمش هينفع
            // لازم نستخدم Find أو نجيب الـ entity بدون AsNoTracking
            var user = await _context.Users.FindAsync(updateDto.UserId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {updateDto.UserId} not found.");

            // Sanitize inputs
            if (updateDto.Name != null && string.IsNullOrWhiteSpace(updateDto.Name)) updateDto.Name = null; // Name required, might fail val
            if (updateDto.Email != null && string.IsNullOrWhiteSpace(updateDto.Email)) updateDto.Email = null; // Email required
            if (updateDto.ClinicName != null && string.IsNullOrWhiteSpace(updateDto.ClinicName)) updateDto.ClinicName = null;
            if (updateDto.ClinicAddress != null && string.IsNullOrWhiteSpace(updateDto.ClinicAddress)) updateDto.ClinicAddress = null;

            // Map changes to entity (only non-null properties will be updated)
            _mapper.Map(updateDto, user);

            // Update in repository
            await _unitOfWork.Users.Update(user);
            await _unitOfWork.CommitAsync();

            // Return updated user
            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.IsActive = await IsUserActiveAsync(user.UserId);
            
            return userDto;
        }

        public async Task DeleteUserAsync(int userId)
        {
            // Repository Delete method will check if user exists
            await _unitOfWork.Users.Delete(userId);
            await _unitOfWork.CommitAsync();
        }

        // ==================== Phone Management ====================

        public async Task AddPhoneNumberAsync(int userId, string phoneNumber)
        {
            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {userId} not found.");

            // Check if phone number already exists for this user
            var existingPhone = await _context.UserPhones
                .AnyAsync(up => up.UserId == userId && up.PhoneNumber == phoneNumber);
            
            if (existingPhone)
                throw new InvalidOperationException($"Phone number {phoneNumber} already exists for this user.");

            // Add phone number
            var userPhone = new UserPhone
            {
                UserId = userId,
                PhoneNumber = phoneNumber
            };

            await _context.UserPhones.AddAsync(userPhone);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdatePhoneNumberAsync(int phoneId, string newPhoneNumber)
        {
            var userPhone = await _context.UserPhones.FindAsync(phoneId);
            if (userPhone == null)
                throw new InvalidOperationException($"Phone with ID {phoneId} not found.");

            // Check if new phone number already exists for this user
            var duplicateExists = await _context.UserPhones
                .AnyAsync(up => up.UserId == userPhone.UserId && 
                               up.PhoneNumber == newPhoneNumber && 
                               up.UserPhoneId != phoneId);
            
            if (duplicateExists)
                throw new InvalidOperationException($"Phone number {newPhoneNumber} already exists for this user.");

            userPhone.PhoneNumber = newPhoneNumber;
            await _unitOfWork.CommitAsync();
        }

        public async Task RemovePhoneNumberAsync(int userId, int phoneId)
        {
            var userPhone = await _context.UserPhones
                .FirstOrDefaultAsync(up => up.UserPhoneId == phoneId && up.UserId == userId);
            
            if (userPhone == null)
                throw new InvalidOperationException($"Phone with ID {phoneId} not found for user {userId}.");

            _context.UserPhones.Remove(userPhone);
            await _unitOfWork.CommitAsync();
        }

        // ==================== Utility Methods ====================

        public async Task<bool> IsUserActiveAsync(int userId)
        {
            // Check if user has any CartItems or OrderItems
            // Using Any() is more efficient than counting
            var hasCartItems = await _context.CartItems
                .AnyAsync(ci => ci.Cart.UserId == userId);

            if (hasCartItems)
                return true;

            var hasOrderItems = await _context.OrderItems
                .AnyAsync(oi => oi.Order.UserId == userId);

            return hasOrderItems;
        }

        // ==================== Private Helper Methods ====================

        /// <summary>
        /// Get list of active user IDs from a collection of user IDs
        /// Optimized for batch operations
        /// استخدمنا HashSet عشان:
        /// 1. Contains() في HashSet هو O(1) بينما في List هو O(n)
        /// 2. UnionWith بتمنع duplicates automatically
        /// 3. Performance أفضل لما نعمل lookup على الـ IDs
        /// </summary>
        private async Task<HashSet<int>> GetActiveUserIdsAsync(List<int> userIds)
        {
            var activeUserIds = new HashSet<int>();

            // Get users with CartItems
            var usersWithCartItems = await _context.CartItems
                .Where(ci => userIds.Contains(ci.Cart.UserId))
                .Select(ci => ci.Cart.UserId)
                .Distinct()
                .ToListAsync();

            activeUserIds.UnionWith(usersWithCartItems);

            // Get users with OrderItems
            var usersWithOrderItems = await _context.OrderItems
                .Where(oi => userIds.Contains(oi.Order.UserId))
                .Select(oi => oi.Order.UserId)
                .Distinct()
                .ToListAsync();

            activeUserIds.UnionWith(usersWithOrderItems);

            return activeUserIds;
        }

 
    }
}
