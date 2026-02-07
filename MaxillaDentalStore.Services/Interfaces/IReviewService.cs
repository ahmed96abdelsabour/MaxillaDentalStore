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
        Task<IEnumerable<ReviewDto>> GetProductReviewsAsync(int productId, int pageNumber, int pageSize);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(int productId);
        Task<bool> AddReviewAsync(CreateReviewDto dto);

       
        Task<bool> DeleteReviewAsync(int reviewId);


    }
}
