using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    /// <summary>
    /// Service interface for Product operations
    /// </summary>
    public interface IProductService
    {
        // ==================== Read Operations ====================

        /// <summary>
        /// Get product by ID - returns basic product information
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>ProductResponseDto or null if not found</returns>
        Task<ProductResponseDto?> GetByIdAsync(int productId);

        /// <summary>
        /// Get all products with pagination and filtering
        /// </summary>
        /// <param name="filterDto">Filtering and pagination parameters</param>
        /// <returns>Paginated list of products</returns>
        Task<PageResult<ProductResponseDto>> GetAllAsync(ProductFilterDto filterDto);

        // ==================== Details Operations ====================

        /// <summary>
        /// Get product details (summary version) - lightweight
        /// Includes images, categories, and review summary
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>ProductDetailsDto or null if not found</returns>
        Task<ProductDetailsDto?> GetProductDetailsAsync(int productId);

        /// <summary>
        /// Get product full details - includes everything (heavy operation)
        /// Includes images, categories, all reviews, and package items
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <returns>ProductFullDetailsDto or null if not found</returns>
        Task<ProductFullDetailsDto?> GetProductFullDetailsAsync(int productId);

        // ==================== Browsing Operations (Public) ====================

        /// <summary>
        /// Get all available (active) products
        /// </summary>
        /// <returns>List of active products</returns>
        Task<List<ProductResponseDto>> GetAvailableProductsAsync();

        /// <summary>
        /// Get products by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of products in the specified category</returns>
        Task<List<ProductResponseDto>> GetProductsByCategoryAsync(int categoryId);

        /// <summary>
        /// Search products by keyword (searches name, description, company)
        /// </summary>
        /// <param name="keyword">Search keyword</param>
        /// <returns>List of matching products</returns>
        Task<List<ProductResponseDto>> SearchProductsAsync(string keyword);

        /// <summary>
        /// Get top rated products based on review ratings
        /// </summary>
        /// <param name="count">Number of products to return</param>
        /// <returns>List of top rated products</returns>
        Task<List<ProductResponseDto>> GetTopRatedProductsAsync(int count);

        // ==================== Write Operations (Admin) ====================

        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="createDto">Product creation data</param>
        /// <returns>Created product</returns>
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto);

        /// <summary>
        /// Update product information
        /// </summary>
        /// <param name="updateDto">Product update data (only non-null fields are updated)</param>
        /// <returns>Updated product</returns>
        Task<ProductResponseDto> UpdateProductAsync(ProductUpdateDto updateDto);

        /// <summary>
        /// Delete product by ID
        /// </summary>
        /// <param name="productId">Product ID</param>
        Task DeleteProductAsync(int productId);

        // ==================== Image Management ====================

        /// <summary>
        /// Add a single image to product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageUrl">Image URL to add</param>
        Task AddProductImageAsync(int productId, string imageUrl);

        /// <summary>
        /// Remove a specific image from product
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageId">Image ID to remove</param>
        Task RemoveProductImageAsync(int productId, int imageId);

        /// <summary>
        /// Update all product images (replaces existing images)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="imageUrls">New list of image URLs</param>
        Task UpdateProductImagesAsync(int productId, List<string> imageUrls);

        // ==================== Category Management ====================

        /// <summary>
        /// Add product to a category
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="categoryId">Category ID</param>
        Task AddProductToCategoryAsync(int productId, int categoryId);

        /// <summary>
        /// Remove product from a category
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="categoryId">Category ID</param>
        Task RemoveProductFromCategoryAsync(int productId, int categoryId);

        /// <summary>
        /// Update all product categories (replaces existing categories)
        /// </summary>
        /// <param name="productId">Product ID</param>
        /// <param name="categoryIds">New list of category IDs</param>
        Task UpdateProductCategoriesAsync(int productId, List<int> categoryIds);
    }
}
