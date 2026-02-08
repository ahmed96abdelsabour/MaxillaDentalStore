using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IReviewService
    {
      // get all reviews for a product using product id and pagination
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageNumber, int pageSize);
        // get product review summary using product id
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId);
        // add a review using dto
        Task<bool> AddReviewAsync(CreateReviewDto dto);
        // delete a review using review id    
        Task<bool> DeleteReviewAsync(int reviewId);


    }
}
