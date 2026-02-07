using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user, hashes password, and returns a JWT token.
        /// </summary>
        Task<AuthResponseDto> RegisterAsync(RegisterDto request);

        /// <summary>
        /// Authenticates a user by email/password and returns a JWT token.
        /// </summary>
        Task<AuthResponseDto?> LoginAsync(LoginDto request);
    }
}
