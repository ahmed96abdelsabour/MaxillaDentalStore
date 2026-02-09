using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MaxillaDentalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserCart(int userId)
        {
            // Optional: Check if userId matches authenticated user
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddToCart([FromQuery] int userId, [FromBody] AddToCartDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null" });
            if (!request.ProductId.HasValue && !request.PackageId.HasValue)
                return BadRequest(new { message = "Either ProductId or PackageId must be provided" });

            try
            {
                var cart = await _cartService.AddToCartAsync(userId, request);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); // e.g. Product unavailable
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("items/{cartItemId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCartItem([FromQuery] int userId, int cartItemId, [FromBody] UpdateCartItemDto request)
        {
            if (request == null) return BadRequest(new { message = "Request body cannot be null" });
            if (request.CartItemId != cartItemId) return BadRequest(new { message = "CartItemId in URL does not match request body" });
            // Quantity check allowed <= 0 to remove item? Service logic says "if 0 or less, remove item".
            // So we don't strictly block it, but usually Update is for changing quantity. 
            // If client wants to remove, they should use Delete. But keeping logic flexible.
            
            try
            {
                var cart = await _cartService.UpdateCartItemAsync(userId, request);
                return Ok(cart);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{userId}/items/{cartItemId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveFromCart(int userId, int cartItemId)
        {
            var cart = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            return Ok(cart);
        }

        [HttpDelete("{userId}/clear")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearCart(int userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
