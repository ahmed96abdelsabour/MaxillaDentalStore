using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System.Linq;

public class PackageMappingProfile : Profile
{
    public PackageMappingProfile()
    {
        CreateMap<Package, PackageDto>()
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.PackageItems))
            .ReverseMap();

        CreateMap<CreatePackageDto, Package>();

        CreateMap<PackageItem, PackageItemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price)) 
            .ForMember(dest => dest.PackageId, opt => opt.MapFrom(src => src.PackageId))
            .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => 
                src.Product.productImages.Any() ? src.Product.productImages.FirstOrDefault().ImageUrl : null))
            .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1)); // Default quantity 1 for now
    }
}