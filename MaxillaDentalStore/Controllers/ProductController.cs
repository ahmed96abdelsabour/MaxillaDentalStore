using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


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

        /// <summary>
        /// Get all products with pagination and filtering (Category, Name, Price Range).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PageResult<ProductResponseDto>), 200)]
        public async Task<ActionResult<PageResult<ProductResponseDto>>> GetAllProducts([FromQuery] ProductFilterDto filterDto)
        {
            var result = await _productService.GetAllAsync(filterDto);
            return Ok(result);
        }

        /// <summary>
        /// Get full product details including images and reviews.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductFullDetailsDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductFullDetailsDto>> GetProductById(int id)
        {
            var result = await _productService.GetProductFullDetailsAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Create a new product (Admin Only).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct([FromBody] ProductCreateDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            try
            {
                var result = await _productService.CreateProductAsync(createDto);
                return CreatedAtAction(nameof(GetProductById), new { id = result.ProductId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing product (Admin Only).
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]

        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProductResponseDto>> UpdateProduct(int id, [FromBody] ProductUpdateDto updateDto)
        {
            if (id != updateDto.ProductId) return BadRequest("Mismatched Product ID.");
            
            try
            {
                var result = await _productService.UpdateProductAsync(updateDto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a product (Admin Only).
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }
    }
}
