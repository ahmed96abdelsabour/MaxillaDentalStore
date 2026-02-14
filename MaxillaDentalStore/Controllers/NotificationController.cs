using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MaxillaDentalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ==================== Admin Endpoints ====================

        /// <summary>
        /// Get all notifications for admin with pagination
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/all")]
        [ProducesResponseType(typeof(PageResult<NotificationResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAdminNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _notificationService.GetAllNotificationsForAdminAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get unread notifications for admin
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/unread")]
        [ProducesResponseType(typeof(PageResult<NotificationResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadAdminNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _notificationService.GetUnreadNotificationsForAdminAsync(pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get unread notification count for admin
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("admin/unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUnreadCountAdmin()
        {
            var count = await _notificationService.GetUnreadCountForAdminAsync();
            return Ok(count);
        }

        // ==================== Customer Endpoints ====================

        /// <summary>
        /// Get current user's notifications
        /// </summary>
        [HttpGet("my-notifications")]
        [ProducesResponseType(typeof(PageResult<NotificationResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var result = await _notificationService.GetNotificationsForUserAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get unread notification count for current user
        /// </summary>
        [HttpGet("my-unread-count")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountForUserAsync(userId);
            return Ok(count);
        }

        // ==================== Common Endpoints ====================

        /// <summary>
        /// Mark a notification as read (user can only mark their own)
        /// </summary>
        [HttpPut("{id}/mark-as-read")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.MarkAsReadAsync(id, userId);
            
            if (!success)
                return NotFound(new { message = "Notification not found or you don't have permission to access it." });

            return NoContent();
        }

        /// <summary>
        /// Mark all notifications as read for current user
        /// </summary>
        [HttpPut("mark-all-as-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.MarkAllAsReadForUserAsync(userId);
            return Ok(new { markedCount = count });
        }

        /// <summary>
        /// Delete a notification (user can only delete their own)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = GetCurrentUserId();
            var success = await _notificationService.DeleteNotificationAsync(id, userId);

            if (!success)
                return NotFound(new { message = "Notification not found or you don't have permission to delete it." });

            return NoContent();
        }

        /// <summary>
        /// Delete all notifications for current user
        /// </summary>
        [HttpDelete("delete-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.DeleteAllNotificationsForUserAsync(userId);
            return Ok(new { deletedCount = count });
        }

        // ==================== Helpers ====================

        private int GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token.");
        }
    }
}
