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

        public async Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageNumber, int pageSize)
        {
            
            var reviews = await _unitOfWork.Reviews.GetPagedReviewsByProductIdAsync(productId, pageNumber, pageSize);
            return _mapper.Map<IEnumerable<ReviewDto>>(reviews);
        }

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

        public async Task<bool> AddReviewAsync(CreateReviewDto dto)
        {
            var review = _mapper.Map<Review>(dto);

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