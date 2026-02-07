using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

public class CartMappingProfile : Profile
{
    public CartMappingProfile()
    {
        // Cart -> CartDto
        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

        // mapping from cartitem to cart itemdto
        CreateMap<CartItem, CartItemDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src =>
                src.Product != null ? src.Product.Name : (src.Package != null ? src.Package.Name : "")))
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                src.Product != null && src.Product.productImages.Any()
                ? src.Product.productImages.FirstOrDefault()!.ImageUrl : null))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice));

        // AddToCartDto -> CartItem
        CreateMap<AddToCartDto, CartItem>();
       
    }
}