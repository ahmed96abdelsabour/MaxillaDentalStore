using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;

namespace MaxillaDentalStore.API.Controllers
{
    /// <summary>
    /// Review Controller - Manages product and package reviews
    /// Follows Clean Architecture: Zero business logic, only delegates to IReviewService
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        /// <summary>
        /// Constructor - Injects IReviewService dependency
        /// </summary>
        /// <param name="reviewService">Review service for business logic</param>
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Get all reviews for a specific product with pagination
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <returns>Paginated list of reviews</returns>
        /// <response code="200">Returns reviews for the product</response>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(
            int productId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("Page size must be between 1 and 100");
            }

            var reviews = await _reviewService.GetProductReviewsAsync(productId, pageNumber, pageSize);
            return Ok(reviews);
        }

        /// <summary>
        /// Get review summary for a product (average rating and total count)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>Review summary</returns>
        /// <response code="200">Returns review summary</response>
        [HttpGet("product/{productId}/summary")]
        [ProducesResponseType(typeof(ProductReviewSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviewSummary(int productId)
        {
            var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Ok(summary);
        }

        /// <summary>
        /// Add a new review for a product or package
        /// Note: Service layer validates that user has purchased the item
        /// </summary>
        /// <param name="dto">Review details</param>
        /// <returns>Success status</returns>
        /// <response code="201">Review successfully created</response>
        /// <response code="400">Invalid request data</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Request body cannot be null");
            }

            if (!dto.ProductId.HasValue && !dto.PackageId.HasValue)
            {
                return BadRequest("Either ProductId or PackageId must be provided");
            }

            if (dto.ReviewRate < 1 || dto.ReviewRate > 5)
            {
                return BadRequest("Review rate must be between 1 and 5");
            }

            try
            {
                var result = await _reviewService.AddReviewAsync(dto);
                
                if (!result)
                {
                    return BadRequest("Failed to add review");
                }

                return StatusCode(StatusCodes.Status201Created, "Review added successfully");
            }
            catch (Exception ex) when (ex.Message.Contains("not found") || ex.Message.Contains("does not exist"))
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) when (ex.Message.Contains("already reviewed"))
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Delete a review
        /// </summary>
        /// <param name="id">Review ID</param>
        /// <returns>No content on success</returns>
        /// <response code="204">Review successfully deleted</response>
        /// <response code="404">Review not found</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            
            if (!result)
            {
                return NotFound($"Review with ID {id} not found");
            }

            return NoContent();
        }
    }
}
