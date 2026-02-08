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
                .ForMember(dest => dest.IsActive, opt => opt.Ignore()); // Will be computed in service
            
            // Entity -> UserDetailsDto (Summary)
            CreateMap<User, UserDetailsDto>()
                .ForMember(dest => dest.PhoneNumbers, 
                    opt => opt.MapFrom(src => src.UserPhones.Select(p => p.PhoneNumber).ToList()))
                .ForMember(dest => dest.RecentOrders, 
                    opt => opt.MapFrom(src => src.Orders.OrderByDescending(o => o.OrderDate).Take(5)))
                .ForMember(dest => dest.TotalOrders, 
                    opt => opt.MapFrom(src => src.Orders.Count))
                .ForMember(dest => dest.TotalReviews, 
                    opt => opt.MapFrom(src => src.Reviews.Count));
            
            // Entity -> UserFullDetailsDto (Full)
            CreateMap<User, UserFullDetailsDto>();
            
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
            
            // Cart -> CartSummaryDto
            CreateMap<Cart, CartSummaryDto>()
                .ForMember(dest => dest.ItemsCount, 
                    opt => opt.MapFrom(src => src.CartItems.Count))
                .ForMember(dest => dest.TotalPrice, 
                    opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.TotalPrice)));
            
            // Cart -> CartDto
            CreateMap<Cart, CartDto>();
            
            // CartItem -> CartItemDto
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : 
                    src.Package != null ? src.Package.Name : "Unknown"));

            // ==================== Order ====================
            
            // Order -> OrderSummaryDto
            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.ItemsCount, 
                    opt => opt.MapFrom(src => src.OrderItems.Count));
            
            // Order -> OrderFullDto
            CreateMap<Order, OrderFullDto>();
            
            // OrderItem -> OrderItemDto
            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => 
                    src.Product != null ? src.Product.Name : 
                    src.Package != null ? src.Package.Name : "Unknown"));

            // ==================== Review ====================
            
            // Review -> ReviewDto
            CreateMap<Review, ReviewDto>()
                .ForMember(dest => dest.ProductName, 
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.PackageName, 
                    opt => opt.MapFrom(src => src.Package != null ? src.Package.Name : null));
        }
    }
}
