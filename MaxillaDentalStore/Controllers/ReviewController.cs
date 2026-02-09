using Microsoft.AspNetCore.Mvc;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MaxillaDentalStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet("product/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<ReviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductReviews(
            int productId, 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) return BadRequest(new { message = "Page number must be greater than 0" });
            if (pageSize < 1 || pageSize > 100) return BadRequest(new { message = "Page size must be between 1 and 100" });

            var reviews = await _reviewService.GetProductReviewsAsync(productId, pageNumber, pageSize);
            return Ok(reviews);
        }

        [HttpGet("product/{productId}/summary")]
        [ProducesResponseType(typeof(ProductReviewSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductReviewSummary(int productId)
        {
            var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Ok(summary);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
        {
            if (dto == null) return BadRequest(new { message = "Request body cannot be null" });

            if (!dto.ProductId.HasValue && !dto.PackageId.HasValue)
                return BadRequest(new { message = "Either ProductId or PackageId must be provided" });

            if (dto.ReviewRate < 1 || dto.ReviewRate > 5)
                return BadRequest(new { message = "Review rate must be between 1 and 5" });

            // Ensure UserId in DTO matches authenticated user
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int authUserId))
            {
               dto.UserId = authUserId;
            }

            try
            {
                var result = await _reviewService.AddReviewAsync(dto);
                if (!result) return BadRequest(new { message = "Failed to add review" });
                return StatusCode(StatusCodes.Status201Created, new { message = "Review added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteReviewAsync(id);
            if (!result) return NotFound(new { message = $"Review with ID {id} not found" });

            return NoContent();
        }
    }
}
