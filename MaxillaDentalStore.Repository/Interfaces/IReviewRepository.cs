using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IReviewRepository
    {

        // get all reviews for a specific product related to the product details page
        Task<IEnumerable<Review>> GetPagedReviewsByProductIdAsync(int productId, int pageNumber, int pageSize);
        
        // get review by review id 
        Task<Review?> GetByIdAsync(int reviewId);
        // get the average rating for a specific product related to the product details page
        Task<double> GetAverageRatingAsync(int productId);

        // check if user has already reviewed a product (to prevent duplicates)
        Task<bool> HasUserReviewedProductAsync(int userId, int productId);

        // add a new review to the database related to the product details page
        Task AddAsync(Review review);

        // update an existing review in the database related to the product details page
        void Update(Review review);

        // delete an existing review in the database related to the product details page
        void Delete(Review review);
    }
}
