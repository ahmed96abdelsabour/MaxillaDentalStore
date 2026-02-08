using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;        

namespace MaxillaDentalStore.Services.Mapping
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            // mapping from entity to dto (for display)
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));

            // mapping from create review dto to entity (when adding)
            CreateMap<CreateReviewDto, Review>();
        }
    }
}