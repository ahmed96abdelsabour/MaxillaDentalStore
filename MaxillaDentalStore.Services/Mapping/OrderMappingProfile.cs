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
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => 
                    src.Status == OrderStatus.Pending ? "Pending" :
                    src.Status == OrderStatus.Confirmed ? "Confirmed" :
                    src.Status == OrderStatus.Cancelled ? "Cancelled" : "Unknown"))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.phoneNumber)) // Fix naming convention case
                .ForMember(dest => dest.IsFirstOrder, opt => opt.MapFrom(src => !src.User.Orders.Any(o => o.OrderId < src.OrderId)))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));

            // Entity -> OrderSummaryDto (Lightweight)
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => 
                    src.Status == OrderStatus.Pending ? "Pending" :
                    src.Status == OrderStatus.Confirmed ? "Confirmed" :
                    src.Status == OrderStatus.Cancelled ? "Cancelled" : "Unknown"))
                .ForMember(dest => dest.IsFirstOrder, opt => opt.MapFrom(src => !src.User.Orders.Any(o => o.OrderId < src.OrderId)))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.OrderItems.Count));

            // UpdateOrderDto -> Entity (Update Allowed Fields)
            CreateMap<OrderUpdateDto, Order>()
                .ForMember(dest => dest.phoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== OrderItem Mappings ====================

                CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src =>
                    src.Product != null ? src.Product.Name : (src.Package != null ? src.Package.Name : "Unknown Item")))
                .ForMember(dest => dest.ItemImage, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.productImages.Any() ? src.Product.productImages.First().ImageUrl :
                    src.Package != null ? src.Package.ImageUrl : null));
        }
    }
}
