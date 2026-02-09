using AutoMapper;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;

namespace MaxillaDentalStore.Services.Implementations
{
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        public ReviewService(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageNumber, int pageSize)
        {
            var reviews = await _unitOfWork.Reviews.GetPagedReviewsByProductIdAsync(productId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId)
        {
            var average = await _unitOfWork.Reviews.GetAverageRatingAsync(productId);
            
            // To get total count efficiently, we might need a dedicated Count method in Repo
            // But reuse PagedReviews with large page size for now as per previous logic (though inefficient)
            // Ideally: Add CountReviewsByProductIdAsync to repo.
            // Sticking to existing logic to minimize repo changes unless requested.
            var allReviews = await _unitOfWork.Reviews.GetPagedReviewsByProductIdAsync(productId, 1, int.MaxValue);

            return new ProductReviewSummaryDto
            {
                AverageRating = Math.Round(average, 1), 
                TotalReviews = allReviews.Count()
            };
        }

        public async Task<bool> AddReviewAsync(CreateReviewDto dto)
        {
            // 1. Check if user purchased this item (Confirmed Order)
            var hasPurchased = await _unitOfWork.Orders.HasUserPurchasedItemAsync(dto.UserId, dto.ProductId, dto.PackageId);
            if (!hasPurchased)
            {
                throw new InvalidOperationException("You can only review items that you have purchased and received (Confirmed).");
            }

            // 2. Check for duplicate review
            if (dto.ProductId.HasValue)
            {
                var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedProductAsync(dto.UserId, dto.ProductId.Value);
                if (hasReviewed)
                {
                    throw new InvalidOperationException($"User {dto.UserId} has already reviewed product {dto.ProductId.Value}");
                }
            }
            else if (dto.PackageId.HasValue)
            {
                var hasReviewed = await _unitOfWork.Reviews.HasUserReviewedPackageAsync(dto.UserId, dto.PackageId.Value);
                if (hasReviewed)
                {
                    throw new InvalidOperationException($"User {dto.UserId} has already reviewed package {dto.PackageId.Value}");
                }
            }

            var review = _mapper.Map<Review>(dto);
            review.CreatedAt = _dateTimeProvider.UtcNow;

            await _unitOfWork.Reviews.AddAsync(review);
            var result = await _unitOfWork.CommitAsync();

            return result > 0;
        }

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