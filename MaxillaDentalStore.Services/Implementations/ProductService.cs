using AutoMapper;
using MaxillaDentalStore.Common.Helpers;
using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Repositories.Interfaces;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context; // Used for tracked entities & child updates

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
        }

        // ==================== Read Operations ====================

        public async Task<ProductResponseDto?> GetByIdAsync(int productId)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(productId);
            if (product == null)
                return null;

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<PageResult<ProductResponseDto>> GetAllAsync(ProductFilterDto filterDto)
        {
            // Note: Using _context directly here because IProductRepository doesn't support combined filtering + pagination
            // and we are constrained to NOT change the Repository interface.
            var query = _context.Products
                .Include(p => p.productImages)
                .Include(p => p.productCategories)
                .AsNoTracking()
                .AsQueryable();

            if (!filterDto.IncludeInactive)
                query = query.Where(p => p.IsActive);

            if (filterDto.CategoryId.HasValue)
                query = query.Where(p => p.productCategories.Any(pc => pc.CategoryId == filterDto.CategoryId.Value));

            if (!string.IsNullOrWhiteSpace(filterDto.Name))
                query = query.Where(p => p.Name.Contains(filterDto.Name));

            if (filterDto.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filterDto.MinPrice.Value);

            if (filterDto.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filterDto.MaxPrice.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Name)
                .Skip((filterDto.PageNumber - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .ToListAsync();

            return new PageResult<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(items),
                TotalItems = totalItems,
                PageNumber = filterDto.PageNumber,
                PageSize = filterDto.PageSize
            };
        }

        // ==================== Details Operations ====================

        public async Task<ProductDetailsDto?> GetProductDetailsAsync(int productId)
        {
            // Lightweight details: Images + Categories + Review Summary
            // Using Repository's optimized method
            var product = await _unitOfWork.Products.GetSummaryDetailsAsync(productId);

            if (product == null)
                return null;

            var detailsDto = _mapper.Map<ProductDetailsDto>(product);

            // Efficient Review Summary (Count + Avg + Top 5)
            // No need to load all reviews
            var reviewQuery = _context.Reviews.Where(r => r.ProductId == productId);
            
            detailsDto.ReviewSummary = new ProductReviewSummaryWithtop5Dto
            {
                TotalReviews = await reviewQuery.CountAsync(),
                AverageRating = await reviewQuery.AnyAsync() ? await reviewQuery.AverageAsync(r => r.ReviewRate) : null,
                RecentReviews = _mapper.Map<List<ReviewDto>>(await reviewQuery.OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync())
            };

            return detailsDto;
        }

        public async Task<ProductFullDetailsDto?> GetProductFullDetailsAsync(int productId)
        {
            // Use Repository's comprehensive method
            var product = await _unitOfWork.Products.GetWithDetailsByIdAsync(productId);
            if (product == null)
                return null;

            return _mapper.Map<ProductFullDetailsDto>(product);
        }

        // ==================== Browsing Operations (Public) ====================

        public async Task<List<ProductResponseDto>> GetAvailableProductsAsync()
        {
            // Repo method
            var products = await _unitOfWork.Products.GetAvailableAsync();
            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        public async Task<List<ProductResponseDto>> GetProductsByCategoryAsync(int categoryId)
        {
            // Repo method
            var products = await _unitOfWork.Products.GetByCategoryAsync(categoryId);
            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        public async Task<List<ProductResponseDto>> SearchProductsAsync(string keyword)
        {
            // Standardize keyword using Tokenizer/Helper if needed
            // Repo likely expects raw or trimmed string. 
            // StringHelper.Normalize() might be useful but repo handles contains.
            // Let's just trim.
            if (string.IsNullOrWhiteSpace(keyword)) return new List<ProductResponseDto>();
            
            var products = await _unitOfWork.Products.SearchAsync(keyword.Trim());
            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        public async Task<List<ProductResponseDto>> GetTopRatedProductsAsync(int count)
        {
            var products = await _unitOfWork.Products.GetTopRatedAsync(count);
            return _mapper.Map<List<ProductResponseDto>>(products);
        }

        // ==================== Write Operations (Admin) ====================

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto createDto)
        {
            var product = _mapper.Map<Product>(createDto);

            // Add product first to get ID
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.CommitAsync();

            // Handle Images
            if (createDto.ImageUrls != null && createDto.ImageUrls.Any())
            {
                foreach (var url in createDto.ImageUrls)
                {
                    await _context.ProductImages.AddAsync(new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImageUrl = url
                    });
                }
            }

            // Handle Categories
            if (createDto.CategoryIds != null && createDto.CategoryIds.Any())
            {
                // Validate categories exist? Or assume valid IDs?
                // Ideally validate, but for now we trust or let DB FK fail.
                foreach (var catId in createDto.CategoryIds)
                {
                    await _context.ProductCategories.AddAsync(new ProductCategory
                    {
                        ProductId = product.ProductId,
                        CategoryId = catId
                    });
                }
            }

            if ((createDto.ImageUrls != null && createDto.ImageUrls.Any()) || 
                (createDto.CategoryIds != null && createDto.CategoryIds.Any()))
            {
                await _unitOfWork.CommitAsync();
            }

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(ProductUpdateDto updateDto)
        {
            // Fetch tracked entity
            var product = await _context.Products.FindAsync(updateDto.ProductId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {updateDto.ProductId} not found.");

            _mapper.Map(updateDto, product);
            
            // _unitOfWork.Products.Update(product); // Not strictly needed as it's tracked, but good for explicit intent
            // Actually Repos Update calls _context.Update which marks all properties as modified.
            // Since we stuck to tracked entity changes, EF Core detects changes automatically.
            // But to be consistent with pattern:
            await _unitOfWork.Products.Update(product); 
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ProductResponseDto>(product);
        }

        public async Task DeleteProductAsync(int productId)
        {
            await _unitOfWork.Products.Delete(productId);
            await _unitOfWork.CommitAsync();
        }

        // ==================== Image Management ====================

        public async Task AddProductImageAsync(int productId, string imageUrl)
        {
            var exists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!exists)
                throw new InvalidOperationException($"Product with ID {productId} not found.");

            var image = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl
            };

            await _context.ProductImages.AddAsync(image);
            await _unitOfWork.CommitAsync();
        }

        public async Task RemoveProductImageAsync(int productId, int imageId)
        {
            var image = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.ProductImageId == imageId && pi.ProductId == productId);

            if (image == null)
                throw new InvalidOperationException($"Image with ID {imageId} not found for product {productId}.");

            _context.ProductImages.Remove(image);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdateProductImagesAsync(int productId, List<string> imageUrls)
        {
            var exists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!exists)
                throw new InvalidOperationException($"Product with ID {productId} not found.");

            // Standard approach: Remove all existing, Add new
            // Efficiency note: If list is large, might want to diff. For products (usually < 10 images), replace is fine.
            
            var existingImages = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            if (existingImages.Any())
            {
                _context.ProductImages.RemoveRange(existingImages);
            }

            if (imageUrls != null && imageUrls.Any())
            {
                var newImages = imageUrls.Select(url => new ProductImage
                {
                    ProductId = productId,
                    ImageUrl = url
                });
                await _context.ProductImages.AddRangeAsync(newImages);
            }

            await _unitOfWork.CommitAsync();
        }

        // ==================== Category Management ====================

        public async Task AddProductToCategoryAsync(int productId, int categoryId)
        {
            // Check existence
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists) throw new InvalidOperationException($"Product {productId} not found.");

            var categoryExists = await _context.Categories.AnyAsync(c => c.CategoryId == categoryId);
            if (!categoryExists) throw new InvalidOperationException($"Category {categoryId} not found.");

            var exists = await _context.ProductCategories
                .AnyAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);
            
            if (exists) return; // Already exists, idempotent

            await _context.ProductCategories.AddAsync(new ProductCategory
            {
                ProductId = productId,
                CategoryId = categoryId
            });
            await _unitOfWork.CommitAsync();
        }

        public async Task RemoveProductFromCategoryAsync(int productId, int categoryId)
        {
            var link = await _context.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.CategoryId == categoryId);

            if (link == null)
                throw new InvalidOperationException($"Product {productId} is not in category {categoryId}.");

            _context.ProductCategories.Remove(link);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdateProductCategoriesAsync(int productId, List<int> categoryIds)
        {
            var productExists = await _context.Products.AnyAsync(p => p.ProductId == productId);
            if (!productExists) throw new InvalidOperationException($"Product {productId} not found.");

            var existingLinks = await _context.ProductCategories
                .Where(pc => pc.ProductId == productId)
                .ToListAsync();

            _context.ProductCategories.RemoveRange(existingLinks);

            if (categoryIds != null && categoryIds.Any())
            {
                // De-duplicate input IDs
                var distinctIds = categoryIds.Distinct();
                
                var newLinks = distinctIds.Select(cid => new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = cid
                });
                await _context.ProductCategories.AddRangeAsync(newLinks);
            }

            await _unitOfWork.CommitAsync();
        }
    }
}
