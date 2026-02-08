using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;

namespace MaxillaDentalStore.API.Controllers
{
    /// <summary>
    /// Category Controller - Manages product categories
    /// Follows Clean Architecture: Zero business logic, only delegates to ICategoryService
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        /// <summary>
        /// Constructor - Injects ICategoryService dependency
        /// </summary>
        /// <param name="categoryService">Category service for business logic</param>
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of all categories</returns>
        /// <response code="200">Returns all categories</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAllCategories()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }

        /// <summary>
        /// Get a specific category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        /// <response code="200">Returns the category</response>
        /// <response code="404">Category not found</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryDTO>> GetCategoryById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            
            if (category == null)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return Ok(category);
        }

        /// <summary>
        /// Get all products belonging to a specific category
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>List of products in the category</returns>
        /// <response code="200">Returns products in the category</response>
        [HttpGet("{id}/products")]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetCategoryProducts(int id)
        {
            var products = await _categoryService.GetProductsByCategoryIdAsync(id);
            return Ok(products);
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="dto">Category creation details</param>
        /// <returns>Created category</returns>
        /// <response code="201">Category successfully created</response>
        /// <response code="400">Invalid request data</response>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CategoryDTO>> CreateCategory([FromBody] CreateCategoryDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            var createdCategory = await _categoryService.CreateAsync(dto);
            
            return CreatedAtAction(
                nameof(GetCategoryById), 
                new { id = createdCategory.CategoryId }, 
                createdCategory);
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="id">Category ID to update</param>
        /// <param name="dto">Updated category data</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Category successfully updated</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="404">Category not found</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateCategoryDTO dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            var result = await _categoryService.UpdateAsync(id, dto);
            
            if (!result)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return NoContent();
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="id">Category ID to delete</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Category successfully deleted</response>
        /// <response code="404">Category not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteAsync(id);
            
            if (!result)
            {
                return NotFound($"Category with ID {id} not found");
            }

            return NoContent();
        }
    }
}
