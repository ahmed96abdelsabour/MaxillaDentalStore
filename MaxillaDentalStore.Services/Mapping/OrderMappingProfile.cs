using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Mapping
{
    public class ReviewMappingProfile : Profile
    {
        public ReviewMappingProfile()
        {
            // من Entity لـ DTO (للـ Get)
            CreateMap<Review, ReviewDto>()
                // هنا بنقوله: روح لخاصية User في الـ Entity وخد منها Name حطه في UserName اللي في الـ DTO
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name));

            // من DTO لـ Entity (للـ Create)
            CreateMap<CreateReviewDto, Review>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
