using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;        

namespace MaxillaDentalStore.Services.Mapping
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            // من الـ Entity إلى الـ DTO (للعرض)
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));

            // من الـ CreateReviewDto إلى الـ Entity (عند الإضافة)
            CreateMap<CreateReviewDto, Review>();
        }
    }
}