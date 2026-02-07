using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System.Linq;

namespace MaxillaDentalStore.Services.Mapping
{
    public class OrderMappingProfile : Profile
    {
        public OrderMappingProfile()
        {
            // ==================== Order Mappings ====================

            // Entity -> OrderResponseDto (Full Details)
            CreateMap<Order, OrderResponseDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.phoneNumber)) // Fix naming convention case
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            // Entity -> OrderSummaryDto (Lightweight)
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.OrderItems.Count));

            // UpdateOrderDto -> Entity (Update Allowed Fields)
            CreateMap<OrderUpdateDto, Order>()
                .ForMember(dest => dest.phoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== OrderItem Mappings ====================

            // Entity -> OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : "Product Unavailable"))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => 
                    src.Product != null && src.Product.productImages.Any() 
                        ? src.Product.productImages.First().ImageUrl 
                        : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src => 
                    src.Package != null ? src.Package.Name : null));
        }
    }
}
