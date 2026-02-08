using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaxillaDentalStore.API.Controllers
{
    /// <summary>
    /// User management controller
    /// Provides CRUD operations, phone management, and user details
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ==================== Read Operations ====================

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User basic information</returns>
        [Authorize]
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)] // Cache for 60 seconds
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            // Authorization: User can only access their own data, or Admin can access any
            if (!await IsAuthorizedToAccessUser(id))
            {
                return Forbid();
            }

            var user = await _userService.GetByIdAsync(id);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Get user by email (Admin only)
        /// </summary>
        /// <param name="email">User email</param>
        /// <returns>User information</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var user = await _userService.GetByEmailAsync(email);
            
            if (user == null)
            {
                return NotFound(new { message = $"User with email {email} not found." });
            }

            return Ok(user);
        }

        /// <summary>
        /// Get all users with pagination (Admin only)
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="includeInactive">Include inactive users (default: false)</param>
        /// <returns>Paginated list of users</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("all")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)] // Cache for 30 seconds
        [ProducesResponseType(typeof(PageResult<UserResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeInactive = false)
        {
            if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { message = "Invalid pagination parameters. PageNumber must be >= 1, PageSize must be 1-100." });
            }

            var users = await _userService.GetAllUsersAsync(pageNumber, pageSize, includeInactive);
            return Ok(users);
        }

        // ==================== Details Operations ====================

        /// <summary>
        /// Get user summary details (lightweight - for dashboards)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details with cart summary and recent orders</returns>
        [Authorize]
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(UserDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetails(int id)
        {
            if (!await IsAuthorizedToAccessUser(id))
            {
                return Forbid();
            }

            var userDetails = await _userService.GetUserDetailsAsync(id);
            
            if (userDetails == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(userDetails);
        }

        /// <summary>
        /// Get user full details (includes all related data - heavy operation)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Complete user information with all orders, reviews, cart items</returns>
        [Authorize]
        [HttpGet("{id}/full-details")]
        [ProducesResponseType(typeof(UserFullDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFullDetails(int id)
        {
            if (!await IsAuthorizedToAccessUser(id))
            {
                return Forbid();
            }

            var userFullDetails = await _userService.GetUserFullDetailsAsync(id);
            
            if (userFullDetails == null)
            {
                return NotFound(new { message = $"User with ID {id} not found." });
            }

            return Ok(userFullDetails);
        }

        // ==================== Write Operations (Admin Only) ====================

        /// <summary>
        /// Create a new user (Admin only)
        /// </summary>
        /// <param name="createDto">User creation data</param>
        /// <returns>Created user information</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.UserId }, createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update user information (Admin only)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="updateDto">Updated user data</param>
        /// <returns>Updated user information</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto updateDto)
        {
            if (id != updateDto.UserId)
            {
                return BadRequest(new { message = "ID in URL does not match ID in body." });
            }

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(updateDto);
                return Ok(updatedUser);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete user (Admin only)
        /// </summary>
        /// <param name="id">User ID to delete</param>
        /// <returns>No content</returns>
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== Phone Management ====================

        /// <summary>
        /// Add phone number to user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="phoneNumber">Phone number to add</param>
        /// <returns>No content</returns>
        [Authorize]
        [HttpPost("{userId}/phone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPhoneNumber(int userId, [FromBody] AddPhoneNumberRequest request)
        {
            if (!await IsAuthorizedToAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                await _userService.AddPhoneNumberAsync(userId, request.PhoneNumber);
                return Ok(new { message = "Phone number added successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update phone number
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="phoneId">Phone ID to update</param>
        /// <param name="request">New phone number</param>
        /// <returns>No content</returns>
        [Authorize]
        [HttpPut("{userId}/phone/{phoneId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdatePhoneNumber(int userId, int phoneId, [FromBody] UpdatePhoneNumberRequest request)
        {
            if (!await IsAuthorizedToAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                await _userService.UpdatePhoneNumberAsync(phoneId, request.NewPhoneNumber);
                return Ok(new { message = "Phone number updated successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Remove phone number from user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="phoneId">Phone ID to remove</param>
        /// <returns>No content</returns>
        [Authorize]
        [HttpDelete("{userId}/phone/{phoneId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemovePhoneNumber(int userId, int phoneId)
        {
            if (!await IsAuthorizedToAccessUser(userId))
            {
                return Forbid();
            }

            try
            {
                await _userService.RemovePhoneNumberAsync(userId, phoneId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== Utility ====================

        /// <summary>
        /// Check if user is active (has cart items or orders)
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>Active status</returns>
        [Authorize]
        [HttpGet("{id}/is-active")]
        [ResponseCache(Duration = 30, Location = ResponseCacheLocation.Any)]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> IsActive(int id)
        {
            if (!await IsAuthorizedToAccessUser(id))
            {
                return Forbid();
            }

            var isActive = await _userService.IsUserActiveAsync(id);
            return Ok(new { userId = id, isActive });
        }

        // ==================== Private Helper Methods ====================

        /// <summary>
        /// Check if current user is authorized to access another user's data
        /// Authorization: User can access their own data, or Admin can access any
        /// </summary>
        private async Task<bool> IsAuthorizedToAccessUser(int targetUserId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Admin can access any user
            if (currentUserRole == "Admin")
            {
                return true;
            }

            // User can only access their own data
            if (int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                return currentUserId == targetUserId;
            }

            return false;
        }
    }

    // ==================== Request DTOs for Phone Management ====================

    public class AddPhoneNumberRequest
    {
        public string PhoneNumber { get; set; } = null!;
    }

    public class UpdatePhoneNumberRequest
    {
        public string NewPhoneNumber { get; set; } = null!;
    }
}
