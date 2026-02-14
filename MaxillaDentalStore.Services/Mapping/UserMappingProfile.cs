using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Mapping
{
    /// <summary>
    /// AutoMapper profile for User-related mappings
    /// </summary>
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // ==================== User Entity <-> DTOs ====================
            
            // Entity -> UserResponseDto
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Will be computed in service
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
            
            // Entity -> UserDetailsDto (Summary)
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.PhoneNumbers, 
                    opt => opt.MapFrom(src => src.UserPhones.Select(p => p.PhoneNumber).ToList()))
                .ForMember(dest => dest.RecentOrders, 
                    opt => opt.MapFrom(src => src.Orders.OrderByDescending(o => o.OrderDate).Take(5)))
                .ForMember(dest => dest.TotalOrders, 
                    opt => opt.MapFrom(src => src.Orders.Count))
                .ForMember(dest => dest.TotalReviews, 
                    opt => opt.MapFrom(src => src.Reviews.Count));
            
            // Entity -> UserFullDetailsDto (Full)
            CreateMap<User, UserFullDetailsDto>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
            

            // UserCreateDto -> Entity
            CreateMap<UserCreateDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => src.Password)) // Will be hashed in service
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Set by service
                .ForMember(dest => dest.UserPhones, opt => opt.Ignore()) // Handled separately
                .ForMember(dest => dest.Cart, opt => opt.Ignore())
                .ForMember(dest => dest.Orders, opt => opt.Ignore())
                .ForMember(dest => dest.Reviews, opt => opt.Ignore());
            
            // UserUpdateDto -> Entity (only update non-null properties)
            CreateMap<UserUpdateDto, User>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // ==================== UserPhone ====================
            
            CreateMap<UserPhone, UserPhoneDto>();

            // ==================== Cart ====================
            
            // Cart -> UserCartSummaryDto
            CreateMap<Cart, UserCartSummaryDto>()
                .ForMember(dest => dest.ItemsCount, 
                    opt => opt.MapFrom(src => src.CartItems.Count))
                .ForMember(dest => dest.TotalPrice, 
                    opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.TotalPrice)));
            
            // Cart -> UserCartDto
            CreateMap<Cart, UserCartDto>()
                .ForMember(dest => dest.Items, 
                    opt => opt.MapFrom(src => src.CartItems));
            

            // CartItem -> UserCartItemDto
            CreateMap<CartItem, UserCartItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : 
                    src.Package != null ? src.Package.Name : "Unknown"))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.productImages.Any() ? src.Product.productImages.First().ImageUrl :
                    src.Package != null ? src.Package.ImageUrl : null));







            // ==================== Order ====================

            // Order -> UserOrderSummaryDto
            CreateMap<Order, UserOrderSummaryDto>()
                .ForMember(dest => dest.ItemsCount, 
                    opt => opt.MapFrom(src => src.OrderItems.Count))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src =>
                    src.Status == OrderStatus.Pending ? "Pending" :
                    src.Status == OrderStatus.Confirmed ? "Confirmed" :
                    src.Status == OrderStatus.Cancelled ? "Cancelled" : "Unknown"));
            


            // Order -> UserOrderDto
            CreateMap<Order, UserOrderDto>()
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src =>
                    src.Status == OrderStatus.Pending ? "Pending" :
                    src.Status == OrderStatus.Confirmed ? "Confirmed" :
                    src.Status == OrderStatus.Cancelled ? "Cancelled" : "Unknown"))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.phoneNumber)) // Fix naming convention case
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems));


            // OrderItem -> UserOrderItemDto
            CreateMap<OrderItem, UserOrderItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : 
                    src.Package != null ? src.Package.Name : "Unknown"))
                .ForMember(dest => dest.ItemImage, opt => opt.MapFrom(src =>
                    src.Product != null && src.Product.productImages.Any() ? src.Product.productImages.First().ImageUrl :
                    src.Package != null ? src.Package.ImageUrl : null));

            



            // ==================== Review ====================

            // Review -> UserReviewDto
            CreateMap<Review, UserReviewDto>()
                .ForMember(dest => dest.ProductName, 
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.PackageName, 
                    opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));
        }
    }
}
