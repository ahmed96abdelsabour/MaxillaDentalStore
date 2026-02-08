using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;

namespace MaxillaDentalStore.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        // declartion of unit of work and automaper
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        // constructor to intaliaze service 
        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // get all reviews for a product using product id and pagination
        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageNumber, int pageSize)
        {
            
            var reviews = await _unitOfWork.Reviews.GetPagedReviewsByProductIdAsync(productId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        // get product review summary using product id
        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId)
        {
            var average = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);

          
            var allReviews = await _unitOfWork.Reviews.GetPagedReviewsByProductIdAsync(productId, 1, int.MaxValue);

            return new ProductReviewSummaryDto
            {
                AverageRating = Math.Round(average, 1), 
                TotalReviews = allReviews.Count()
            };
        }

        // add a review using dto
        public async Task<bool> AddReviewAsync(CreateReviewDto dto)
        {
            // Check for duplicate review - prevent same user from reviewing same product twice
            if (dto.ProductId.HasValue)
            {
                var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedProductAsync(dto.UserId, dto.ProductId.Value);
                if (hasReviewed)
                {
                    throw new InvalidOperationException($"User {dto.UserId} has already reviewed product {dto.ProductId.Value}");
                }
            }

            var review = _mapper.Map<Review>(dto);

            await _unitOfWork.Reviews.AddAsync(review);
            var result = await _unitOfWork.CommitAsync();

            return result > 0;
        }

        // delete a review using review id    
        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
            if (review == null) return false; 

            _unitOfWork.Reviews.Delete(review);
            var result = await _unitOfWork.CommitAsync();
            return result > 0;
        }
    }
}