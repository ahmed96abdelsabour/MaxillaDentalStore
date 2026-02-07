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
                .ForMember(dest => dest.FinalPrice, opt => opt.MapFrom(src => src.FinalPrice))
                .ForMember(dest => dest.ImagesCount, opt => opt.MapFrom(src => src.productImages.Count))
                .ForMember(dest => dest.CategoriesCount, opt => opt.MapFrom(src => src.productCategories.Count))
                .ForMember(dest => dest.ReviewsCount, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? (double?)src.Reviews.Average(r => r.ReviewRate) : null));

            // Entity -> ProductDetailsDto
            CreateMap<Product, ProductDetailsDto>()
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.ReviewSummary, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.productImages))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.productCategories.Select(pc => pc.Category)));

            // Entity -> ProductFullDetailsDto
            CreateMap<Product, ProductFullDetailsDto>()
                .ForMember(dest => dest.Variants, opt => opt.MapFrom(src => src))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.productImages))
                .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.productCategories.Select(pc => pc.Category)))
                .ForMember(dest => dest.PackageItems, opt => opt.MapFrom(src => src.packageItems));


            // ProductCreateDto -> Entity
            CreateMap<ProductCreateDto, Product>()
                .ForMember(dest => dest.productImages, opt => opt.Ignore()) // Handled manually or separate logic
                .ForMember(dest => dest.productCategories, opt => opt.Ignore()); // Handled manually

            // ProductUpdateDto -> Entity (Update only non-null members)
            CreateMap<ProductUpdateDto, Product>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== Product Variants Mappings ====================

            CreateMap<Product, ProductVariantsDto>();

            // ==================== Supporting Entities Mappings ====================

            // ProductImage -> ProductImageDto
            CreateMap<ProductImage, ProductImageDto>();

            // Category -> CategoryDto
            CreateMap<Category, CategoryDto>();

            // Review -> ReviewDto
            CreateMap<Review, ReviewDto>();
            
            // PackageItem -> PackageItemDto
            CreateMap<PackageItem, PackageItemDto>()
                // Assuming PackageItem has navigation to Package. Assuming Package entity has a Name.
                // If PackageItem has PackageName directly or via Package.Name
                // Let's assume typical navigation: PackageItem -> Package -> Name
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package.Name));

            // Product -> ProductReviewSummaryDto
            CreateMap<Product, ProductReviewSummaryDto>()
                .ForMember(dest => dest.TotalReviews, opt => opt.MapFrom(src => src.Reviews.Count))
                .ForMember(dest => dest.AverageRating, opt => opt.MapFrom(src =>
                    src.Reviews.Any() ? (double?)src.Reviews.Average(r => r.ReviewRate) : null))
                .ForMember(dest => dest.RecentReviews, opt => opt.MapFrom(src =>
                    src.Reviews.OrderByDescending(r => r.CreatedAt).Take(5)));
        }
    }
}
