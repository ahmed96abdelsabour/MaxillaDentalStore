using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ==================== Customer Operations ====================

        /// <summary>
        /// Create order from active cart (Checkout)
        /// </summary>
        [Authorize]
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] OrderCreateDto createDto)
        {
            var userId = GetCurrentUserId();
            try
            {
                var order = await _orderService.CreateOrderFromCartAsync(userId, createDto);
                return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get current user's order history
        /// </summary>
        [Authorize]
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(PageResult<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyOrders([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userId = GetCurrentUserId();
            var result = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Get order details
        /// User can see their own, Admin can see any
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found." });
            }

            var userId = GetCurrentUserId();
            // Allow if user owns order OR user is Admin
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return Ok(order);
        }

        /// <summary>
        /// Cancel order (User can cancel if Pending)
        /// </summary>
        [Authorize]
        [HttpPost("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetCurrentUserId();
            try
            {
                bool isAdmin = User.IsInRole("Admin");
                await _orderService.CancelOrderAsync(id, userId, isAdmin);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Admin Operations ====================

        /// <summary>
        /// Get all orders (Admin only)
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpGet("allOrders")]
        [ProducesResponseType(typeof(PageResult<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] OrderStatus? status = null)
        {
            var result = await _orderService.GetAllOrdersAsync(pageNumber, pageSize, status);
            return Ok(result);
        }

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatus newStatus)
        {
            await _orderService.UpdateOrderStatusAsync(id, newStatus);
            return NoContent();
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
