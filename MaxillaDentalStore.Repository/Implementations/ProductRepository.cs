using MaxillaDentalStore.Common.Pagination;
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
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _Context;
        public ProductRepository(AppDbContext context)
        {
            _Context = context;
        }

        // Retrieve a product from the database by its unique identifier (productId)
        public async Task<Product?> GetByIdAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productId));
            }

            return await _Context.Products
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        // Add a new product to the database
        public async Task AddAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product), "Product cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name cannot be null or empty.", nameof(product));
            }
            if (product.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative.", nameof(product));
            }
            if (string.IsNullOrWhiteSpace(product.UnitType))
            {
                throw new ArgumentException("Product unit type cannot be null or empty.", nameof(product));
            }
            if (product.Discount < 0 || product.Discount > 100)
            {
                throw new ArgumentException("Product discount must be between 0 and 100.", nameof(product));
            }
            await _Context.Products.AddAsync(product);
        }

        // Delete a product from the database by its unique identifier (productId)
        public Task Delete(int productId)
        {
            var product = _Context.Products.Find(productId);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {productId} not found.");
            }
            _Context.Products.Remove(product);
            return Task.CompletedTask;         // async placeholder
        }

        // Update the details of an existing product in the database
        public Task Update(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new ArgumentException("Product name cannot be null or empty.", nameof(product));
            }
            if (product.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative.", nameof(product));
            }
            if (string.IsNullOrWhiteSpace(product.UnitType))
            {
                throw new ArgumentException("Product unit type cannot be null or empty.", nameof(product));
            }
            if (product.Discount < 0 || product.Discount > 100)
            {
                throw new ArgumentException("Product discount must be between 0 and 100.", nameof(product));
            }
            if (product == null)
            {
                throw new ArgumentNullException("Product Not Found" , nameof(product));
            }
            _Context.Products.Update(product);
            return Task.CompletedTask;
        }

        // Retrieve a paginated list of products, with optional filtering to include inactive products
        public async Task<PageResult<Product>> GetAllAsync(int pageNumber, int pageSize, bool includeInactive = false)
        {
            // get all products, order by name, and filter by active status if includeInactive is false
            var query = _Context.Products
                                    .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                                    .OrderBy(p => p.Name)
                                    .AsQueryable(); // Make sure to use AsQueryable to allow for further filtering
            // If includeInactive is false, filter out inactive products
            if (!includeInactive)
            {
                query = query.Where(p => p.IsActive);
            }
            var totalItems = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * pageSize)
                                .Take(pageSize)
                                .ToListAsync();
            return new PageResult<Product>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // Retrieve a list of all available products (active products)
        public async Task<IEnumerable<Product>> GetAvailableAsync()
        {
            if (!_Context.Products.Any(p => p.IsActive))
            {
                throw new InvalidOperationException("No active products found.");
            }
            return await _Context.Products
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .Where(p => p.IsActive)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        // Retrieve a list of products that belong to a specific category, identified by categoryId
        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            if (!_Context.Categories.Any(c => c.CategoryId == categoryId))
            {
                throw new InvalidOperationException($"Category with ID {categoryId} not found.");
            }
            return await _Context.ProductCategories
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .Where(pc => pc.CategoryId == categoryId)
                .Select(pc => pc.Product)
                .ToListAsync();
        }

        // Retrieve a list of top-rated products, limited to a specified count
        public async Task<IEnumerable<Product>> GetTopRatedAsync(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException("Count must be greater than zero.", nameof(count));
            }
             return await _Context.Products
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .Where(p => p.Reviews.Any()) // Ensure the product has reviews to calculate average rating
                .OrderByDescending(p => p.Reviews.Average(r => r.ReviewRate)) // Order by average rating in descending order
                .Take(count)
                .ToListAsync();
        }

        // Retrieve a product along with its associated categories, identified by productId
        public async Task<Product?> GetWithCategoriesAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productId));
            }
            return await _Context.Products
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .Include(p => p.productCategories)
                    .ThenInclude(pc => pc.Category) // Include the related categories through the join entity
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        // Retrieve a product along with its associated details (e.g., categories, images, reviews), identified by productId
        public async Task<Product?> GetWithDetailsByIdAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productId));
            }
            var product = await _Context.Products
                                .Include(p => p.productImages)
                                .Include(p => p.productCategories)
                                    .ThenInclude(pc => pc.Category)
                                .Include(p => p.packageItems)
                                    .ThenInclude(pi => pi.Package)
                                .Include(p => p.Reviews)
                                .AsNoTracking() 
                                .FirstOrDefaultAsync(p => p.ProductId == productId);
            return product;
        }

        // Retrieve a product along with its associated images, identified by productId
        public async Task<Product?> GetWithImagesAsync(int productId)
        {
            if (productId <= 0)
            {
                throw new ArgumentException("Invalid product ID.", nameof(productId));
            }
            return await _Context.Products
                .Include(p => p.productImages)
                .AsNoTracking() // Use AsNoTracking for read-only queries to improve performance
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        // Search for products in the database that match a given keyword, which can be used to find products based on their name, description, or other relevant fields
        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return new List<Product>();

            keyword = keyword.ToLower();
            return await _Context.Products
                                 .AsNoTracking()
                                 .Where(p => p.Name.ToLower().Contains(keyword) ||
                                             (p.Description != null && p.Description.ToLower().Contains(keyword)) ||
                                             (p.Company != null && p.Company.ToLower().Contains(keyword)))
                                 .OrderBy(p => p.Name)
                                 .ToListAsync();
        }
    }
}
