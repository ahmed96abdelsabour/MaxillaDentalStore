using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


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

        /// <summary>
        /// Create an order from the current user's active cart.
        /// </summary>
        /// <param name="userId">Temporary userId param (ideally from Claims)</param>
        /// <param name="createDto">Order Creation DTO (Shipping/Notes)</param>
        /// <returns>Created Order Details</returns>
        [HttpPost("checkout/{userId}")]
        [ProducesResponseType(typeof(OrderResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<OrderResponseDto>> Checkout(int userId, [FromBody] OrderCreateDto createDto)
        {
            try
            {
                var result = await _orderService.CreateOrderFromCartAsync(userId, createDto);
                return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Get order details by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Get order history for a specific user.
        /// </summary>
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(PageResult<OrderResponseDto>), 200)]
        public async Task<ActionResult<PageResult<OrderResponseDto>>> GetUserOrders(int userId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _orderService.GetUserOrdersAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Update order status (Admin Only).
        /// </summary>
        [HttpPut("{id}/status")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] OrderStatusUpdateDto updateDto)
        {
            if (id != updateDto.OrderId) return BadRequest("Mismatched Order ID.");
            
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, updateDto.NewStatus);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
