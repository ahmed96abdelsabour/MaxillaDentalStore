using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Mapping
{
    public class NotificationMappingProfile : Profile
    {
        public NotificationMappingProfile()
        {
            // Notification -> NotificationResponseDto
            CreateMap<Notification, NotificationResponseDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.Order))
                .ForMember(dest => dest.ReviewDetails, opt => opt.MapFrom(src => src.Review != null ? new ReviewDetailsForNotificationDto
                {
                    ReviewId = src.Review.ReviewId,
                    ReviewRate = src.Review.ReviewRate,
                    ReviewText = src.Review.ReviewText,
                    ProductId = src.Review.ProductId,
                    ProductName = src.Review.Product != null ? src.Review.Product.Name : null,
                    PackageId = src.Review.PackageId,
                    PackageName = src.Review.Package != null ? src.Review.Package.Name : null
                } : null))
                .ForMember(dest => dest.RelatedCustomerName, opt => opt.MapFrom(src => src.RelatedUser != null ? src.RelatedUser.Name : null))
                .ForMember(dest => dest.RelatedCustomerId, opt => opt.MapFrom(src => src.RelatedUserId));
        }
    }
}
