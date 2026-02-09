using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.DTOS
{
    // ==================== Core Product DTOs ====================

    /// <summary>
    /// DTO for product response - basic product information
    /// </summary>
    public class ProductResponseDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; } // Computed: Price - (Price * Discount / 100)
        public string? Company { get; set; }
        public string UnitType { get; set; } = null!;
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }

        // Variant properties
        public bool HasColor { get; set; }
        public string? Color { get; set; }
        public bool HasSize { get; set; }
        public string? Size { get; set; }
        public bool HasMaterial { get; set; }
        public string? Material { get; set; }

        // Summary counts
        public int ImagesCount { get; set; }
        public int CategoriesCount { get; set; }
        public int ReviewsCount { get; set; }
        public double? AverageRating { get; set; } // Computed from reviews
    }

    /// <summary>
    /// DTO for creating a new product
    /// </summary>
    public class ProductCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; } = 0;
        public string? Company { get; set; }
        public string UnitType { get; set; } = null!;

        // Variant properties
        public bool HasColor { get; set; }
        public string? Color { get; set; }
        public bool HasSize { get; set; }
        public string? Size { get; set; }
        public bool HasMaterial { get; set; }
        public string? Material { get; set; }

        public bool IsActive { get; set; } = true;
        
        // Related data
        public List<string>? ImageUrls { get; set; } // Product images to add
        public List<int>? CategoryIds { get; set; } // Categories to assign
    }

    /// <summary>
    /// DTO for updating product information
    /// </summary>
    public class ProductUpdateDto
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public string? Company { get; set; }
        public string? UnitType { get; set; }

        // Variant properties
        public bool? HasColor { get; set; }
        public string? Color { get; set; }
        public bool? HasSize { get; set; }
        public string? Size { get; set; }
        public bool? HasMaterial { get; set; }
        public string? Material { get; set; }

        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// DTO for product details - Summary version (lightweight)
    /// Includes images, categories summary, and review summary
    /// </summary>
    public class ProductDetailsDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Company { get; set; }
        public string UnitType { get; set; } = null!;
        public bool IsActive { get; set; }

        // Variant properties
        public ProductVariantsDto Variants { get; set; } = null!;

        // Related data summaries
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public ProductReviewSummaryWithtop5Dto ReviewSummary { get; set; } = null!;
    }

    /// <summary>
    /// DTO for product full details - includes everything (heavy operation)
    /// </summary>
    public class ProductFullDetailsDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string? Company { get; set; }
        public string UnitType { get; set; } = null!;
        public bool IsActive { get; set; }

        // Variant properties
        public ProductVariantsDto Variants { get; set; } = null!;

        // Related data - FULL
        public List<ProductImageDto> Images { get; set; } = new List<ProductImageDto>();
        public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
        public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public List<PackageProductItemDto> PackageItems { get; set; } = new List<PackageProductItemDto>();
    }

    /// <summary>
    /// DTO for product filtering
    /// </summary>
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Name { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool IncludeInactive { get; set; } = false;
    }

    // ==================== Supporting Product DTOs ====================

    /// <summary>
    /// Product variant properties (Color, Size, Material)
    /// </summary>
    public class ProductVariantsDto
    {
        public bool HasColor { get; set; }
        public string? Color { get; set; }
        public bool HasSize { get; set; }
        public string? Size { get; set; }
        public bool HasMaterial { get; set; }
        public string? Material { get; set; }
    }

    /// <summary>
    /// Product image DTO
    /// </summary>
    public class ProductImageDto
    {
        public int ProductImageId { get; set; }
        public string ImageUrl { get; set; } = null!;
    }

    /// <summary>
    /// Category DTO
    /// </summary>
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }



    /// <summary>
    /// Package item DTO - represents a product included in a package
    /// </summary>
    public class PackageProductItemDto
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Product review summary - includes total count, average rating, and recent reviews
    /// </summary>
    public class ProductReviewSummaryWithtop5Dto
    {
        public int TotalReviews { get; set; }
        public double? AverageRating { get; set; }
        public List<ReviewDto> RecentReviews { get; set; } = new List<ReviewDto>(); // Top 5 recent
    }
}
