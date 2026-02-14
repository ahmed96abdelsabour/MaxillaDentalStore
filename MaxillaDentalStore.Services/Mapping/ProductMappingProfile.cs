using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System.Linq;

namespace MaxillaDentalStore.Services.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            // ==================== Product Entity <-> DTOs ====================

            // Entity -> ProductResponseDto
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.Price - (src.Price * src.Discount / 100)))
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => 
                    src.productImages.Select(pi => pi.ImageUrl).ToList()))

                .ForMember(dest => dest.ImagesCount, opt => opt.MapFrom(src => src.productImages.Count))
                .ForMember(dest => dest.CategoriesCount, opt => opt.MapFrom(src => src.productCategories.Count))
                .ForMember(dest => dest.ReviewsCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? (double?)src.Reviews.Average(r => r.ReviewRate) : null))
                // Map Variants: string (CSV) -> List<string>
                .ForMember(dest => dest.Colors, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Color) ? src.Color.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()))
                .ForMember(dest => dest.Sizes, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Size) ? src.Size.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()))
                .ForMember(dest => dest.Materials, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Material) ? src.Material.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()));

            // Entity -> ProductDetailsDto
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ReviewSummary, opt => opt.MapFrom(src => src)) // Maps to ProductDetailsReviewSummaryDtoWithTop5
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.productImages))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.productCategories.Select(pc => pc.Category)));

            // Entity -> ProductFullDetailsDto
            CreateMap<Product, ProductFullDetailsDto>()
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.productImages))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.productCategories.Select(pc => pc.Category)))
                .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews))
                .ForMember(dest => dest.PackageItems, opt => opt.MapFrom(src => src.packageItems));


            // ProductCreateDto -> Entity
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.productImages, opt => opt.Ignore()) // Handled manually or separate logic
                .ForMember(dest => dest.productCategories, opt => opt.Ignore()) // Handled manually
                // Map Variants: List<string> -> string (CSV)
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Colors != null && src.Colors.Any() ? string.Join(",", src.Colors) : null))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Sizes != null && src.Sizes.Any() ? string.Join(",", src.Sizes) : null))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Materials != null && src.Materials.Any() ? string.Join(",", src.Materials) : null));

            // ProductUpdateDto -> Entity (Update only non-null members)
            CreateMap<ProductUpdateDto, Product>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Colors != null ? string.Join(",", src.Colors) : null))
                .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Sizes != null ? string.Join(",", src.Sizes) : null))
                .ForMember(dest => dest.Material, opt => opt.MapFrom(src => src.Materials != null ? string.Join(",", src.Materials) : null))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== Product Variants Mappings ====================

            CreateMap<Product, ProductVariantsDto>();

            // ==================== Supporting Entities Mappings ====================

            // ProductImage -> ProductImageDto
            CreateMap<ProductImage, ProductImageDto>();

            // Category -> ProductCategoryDto
            CreateMap<Category, ProductCategoryDto>();

            // Review -> ProductReviewDto
            CreateMap<Review, ProductReviewDto>();
            
            // PackageItem -> ProductPackageItemDto
            CreateMap<PackageItem, ProductPackageItemDto>()
                // Assuming PackageItem has navigation to Package. Assuming Package entity has a Name.
                // If PackageItem has PackageName directly or via Package.Name
                // Let's assume typical navigation: PackageItem -> Package -> Name
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package.Name));

            // Product -> ProductDetailsReviewSummaryDto
            CreateMap<Product, ProductDetailsReviewSummaryDto>()
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? (double?)src.Reviews.Average(r => r.ReviewRate) : null))
                .ForMember(dest => dest.RecentReviews, opt => opt.MapFrom(src =>
                    src.Reviews.OrderByDescending(r => r.CreatedAt).Take(5)));
        }
    }
}
