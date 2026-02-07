using AutoMapper;
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

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtProvider jwtProvider,
            IPasswordHasher passwordHasher,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _jwtProvider = jwtProvider;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto request)
        {
            // 1. Find user by email
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email);
            
            // 2. Validate user existence and password
            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }

            // 3. Generate Token
            var token = _jwtProvider.Generate(user);

            // 4. Return Response
            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresInDays = 30 // Should matches JwtOptions config ideally
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
            
            // 4. Save User
            await _unitOfWork.Users.AddUserAsync(user);
            await _unitOfWork.CommitAsync();

            // 5. Generate Token
            var token = _jwtProvider.Generate(user);

            // 6. Return Response
            return new AuthResponseDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Name = user.Name,
                Role = user.Role.ToString(),
                Token = token,
                ExpiresInDays = 30
            };
        }
    }
}
