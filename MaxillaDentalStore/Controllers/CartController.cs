using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;

namespace MaxillaDentalStore.API.Controllers
{
    /// <summary>
    /// Cart Controller - Manages shopping cart operations
    /// Follows Clean Architecture: Zero business logic, only delegates to ICartService
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        /// <summary>
        /// Constructor - Injects ICartService dependency
        /// </summary>
        /// <param name="cartService">Cart service for business logic</param>
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Get user's cart by user ID
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User's cart with all items</returns>
        /// <response code="200">Returns the user's cart</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CartDto>> GetUserCart(int userId)
        {
            var cart = await _cartService.GetUserCartAsync(userId);
            return Ok(cart);
        }

        /// <summary>
        /// Add an item to the cart (product or package)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="request">Item details to add</param>
        /// <returns>Updated cart</returns>
        /// <response code="200">Item successfully added to cart</response>
        /// <response code="400">Invalid request data</response>
        [HttpPost]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CartDto>> AddToCart([FromQuery] int userId, [FromBody] AddToCartDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }

            if (!request.ProductId.HasValue && !request.PackageId.HasValue)
            {
                return BadRequest("Either ProductId or PackageId must be provided");
            }

            var cart = await _cartService.AddToCartAsync(userId, request);
            return Ok(cart);
        }

        /// <summary>
        /// Update quantity of an item in the cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cartItemId">Cart item ID to update</param>
        /// <param name="request">Updated quantity details</param>
        /// <returns>Updated cart</returns>
        /// <response code="200">Item quantity successfully updated</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Cart item not found</response>
        [HttpPut("items/{cartItemId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CartDto>> UpdateCartItem([FromQuery] int userId, int cartItemId, [FromBody] UpdateCartItemDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null");
            }

            if (request.CartItemId != cartItemId)
            {
                return BadRequest("CartItemId in URL does not match request body");
            }

            if (request.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than zero");
            }

            try
            {
                var cart = await _cartService.UpdateCartItemAsync(userId, request);
                return Ok(cart);
            }
            catch (Exception ex) when (ex.Message.Contains("not found"))
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Remove a specific item from the cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="cartItemId">Cart item ID to remove</param>
        /// <returns>Updated cart without the removed item</returns>
        /// <response code="200">Item successfully removed from cart</response>
        [HttpDelete("{userId}/items/{cartItemId}")]
        [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CartDto>> RemoveFromCart(int userId, int cartItemId)
        {
            var cart = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            return Ok(cart);
        }

        /// <summary>
        /// Clear all items from the user's cart
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>No content</returns>
        /// <response code="204">Cart successfully cleared</response>
        [HttpDelete("{userId}/clear")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ClearCart(int userId)
        {
            await _cartService.ClearCartAsync(userId);
            return NoContent();
        }
    }
}
