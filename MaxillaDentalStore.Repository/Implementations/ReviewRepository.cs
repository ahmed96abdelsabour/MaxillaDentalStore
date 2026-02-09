using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        // 1. Get paged reviews for a product with user details
        // We use Include(u => u.User) to show who wrote the review
        public async Task<IEnumerable<Review>> GetPagedReviewsByProductIdAsync(int productId, int pageNumber, int pageSize)
        {
            return await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId)
                .Include(r => r.User) // Important to show the reviewer's name
                .OrderByDescending(r => r.CreatedAt) // Latest reviews first
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // 2. Calculate average rating on the Database level (High Performance)
        // This is much faster than fetching all reviews to calculate their average in C#
        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = _context.Reviews.Where(r => r.ProductId == productId);

            if (!await reviews.AnyAsync())
                return 0;

            return await reviews.AverageAsync(r => r.ReviewRate);
        }

        // Check if a user has already reviewed a specific product (duplicate prevention)
        public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
        }

        public async Task<bool> HasUserReviewedPackageAsync(int userId, int packageId)
        {
            return await _context.Reviews
                .AsNoTracking()
                .AnyAsync(r => r.UserId == userId && r.PackageId == packageId);
        }

        // 3. Add a new review
        public async Task AddAsync(Review review)
        {
            // Optional: You might want to set the CreatedAt date here if not handled by DB
            review.CreatedAt = DateTime.UtcNow;
            await _context.Reviews.AddAsync(review);
        }

        // 4. Update an existing review
        public void Update(Review review)
        {
            _context.Reviews.Update(review);
        }

        // 5. Delete a review
        public void Delete(Review review)
        {
            _context.Reviews.Remove(review);
        }

        public async Task<Review?> GetByIdAsync(int reviewId)
        {
            return await _context.Reviews.FindAsync(reviewId);
        }
    }
}
