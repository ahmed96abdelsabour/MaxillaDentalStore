using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Mapping
{
    public class CartMappingProfile : Profile
    {
        public CartMappingProfile()
        {
            // تحويل الـ Cart
            CreateMap<Cart, CartDTO>().ReverseMap();
            CreateMap<CreateCartDTO, Cart>();

            // تحويل الـ CartItem (مهم جداً لظهور البيانات داخل القائمة)
            CreateMap<CartItem, CartItemDTO>().ReverseMap();
        }
    }
}