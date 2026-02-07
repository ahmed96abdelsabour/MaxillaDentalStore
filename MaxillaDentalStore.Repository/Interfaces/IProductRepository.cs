using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IProductRepository
    {
        // 🔹 Base
        Task<Product?> GetByIdAsync(int productId);
        // Get all products with optional pagination and active filter
        Task<PageResult<Product>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false);
        // Extra: Get product with related data (images, categories, package items, reviews)
        Task<Product?> GetWithDetailsByIdAsync(int productId);
        // 🔹 Browsing
        Task<IEnumerable<Product>> GetAvailableAsync();
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string keyword);
        Task<IEnumerable<Product>> GetTopRatedAsync(int count);

        // 🔹 Details (Use-case based)
        Task<Product?> GetWithImagesAsync(int productId);
        Task<Product?> GetWithCategoriesAsync(int productId);

        /// <summary>
        /// Get product details summary (lightweight) - for ProductDetailsDto
        /// Includes Images, Categories summary
        /// </summary>
        Task<Product?> GetSummaryDetailsAsync(int productId);

        // 🔹 Admin
        Task AddAsync(Product product);
        Task Update(Product product);
        Task Delete(int productId);
    }
}
