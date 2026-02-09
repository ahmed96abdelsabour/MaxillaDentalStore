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
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ==================== Read Operations ====================

        /// <summary>
        /// Get all products with filtering and pagination
        /// </summary>
        [HttpGet("all")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "pageNumber", "pageSize", "categoryId", "name", "minPrice", "maxPrice", "includeInactive" })]
        [ProducesResponseType(typeof(PageResult<ProductResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] ProductFilterDto filter)
        {
            // Security: Only Admin can see inactive products
            if (filter.IncludeInactive && !User.IsInRole("Admin"))
            {
                filter.IncludeInactive = false;
            }

            var result = await _productService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            // Visibility Logic: If not active, only Admin can see it
            if (!product.IsActive && !User.IsInRole("Admin"))
            {
                return NotFound(new { message = "This product is not currently available." });
            }

            return Ok(product);
        }

        /// <summary>
        /// Get simplified product details (for listing/cards)
        /// </summary>
        [HttpGet("{id}/details")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        [ProducesResponseType(typeof(ProductDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetails(int id)
        {
            var product = await _productService.GetProductDetailsAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            if (!product.IsActive && !User.IsInRole("Admin"))
            {
                return NotFound(new { message = "This product is not currently available." });
            }

            return Ok(product);
        }

        /// <summary>
        /// Get full product details (for single product page)
        /// </summary>
        [HttpGet("{id}/full-details")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id" })]
        [ProducesResponseType(typeof(ProductFullDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetFullDetails(int id)
        {
            var product = await _productService.GetProductFullDetailsAsync(id);

            if (product == null)
            {
                return NotFound(new { message = $"Product with ID {id} not found." });
            }

            if (!product.IsActive && !User.IsInRole("Admin"))
            {
                return NotFound(new { message = "This product is not currently available." });
            }

            return Ok(product);
        }

        // ==================== Specific Queries ====================

        [HttpGet("top-rated")]
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // Cache for 5 mins
        public async Task<IActionResult> GetTopRated([FromQuery] int count = 5)
        {
            var products = await _productService.GetTopRatedProductsAsync(count);
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "categoryId" })]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var products = await _productService.SearchProductsAsync(q);
            return Ok(products);
        }

        // ==================== Write Operations (Admin Only) ====================

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto createDto)
        {
            try
            {
                var createdProduct = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetById), new { id = createdProduct.ProductId }, createdProduct);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            if (id != updateDto.ProductId)
            {
                return BadRequest(new { message = "ID mismatch." });
            }

            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(updateDto);
                return Ok(updatedProduct);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Assuming Delete throws if not found or DB error
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== Image Management (Admin Only) ====================

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromBody] string imageUrl)
        {
            try
            {
                await _productService.AddProductImageAsync(id, imageUrl);
                return Ok(new { message = "Image added successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}/images/{imageId}")]
        public async Task<IActionResult> RemoveImage(int id, int imageId)
        {
            try
            {
                await _productService.RemoveProductImageAsync(id, imageId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ==================== Category Management (Admin Only) ====================

        [Authorize(Policy = "AdminOnly")]
        [HttpPost("{id}/categories/{categoryId}")]
        public async Task<IActionResult> AddCategory(int id, int categoryId)
        {
            try
            {
                await _productService.AddProductToCategoryAsync(id, categoryId);
                return Ok(new { message = "Product added to category successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}/categories/{categoryId}")]
        public async Task<IActionResult> RemoveCategory(int id, int categoryId)
        {
            try
            {
                await _productService.RemoveProductFromCategoryAsync(id, categoryId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
