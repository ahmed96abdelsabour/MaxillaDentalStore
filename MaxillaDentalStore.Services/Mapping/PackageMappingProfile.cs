using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

public class PackageMappingProfile : Profile
{
    public PackageMappingProfile()
    {
        CreateMap<Package, PackageDto>()
            // mapping from package items in entity to products in dto 
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.PackageItems))
            .ReverseMap();

       // mapping from create package dto to package entity
        CreateMap<CreatePackageDto, Package>();

        // mapping from package item entity to package item dto
        CreateMap<PackageItem, packageitemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price.ToString())) // cause dto price is string
            .ForMember(dest => dest.packageId, opt => opt.MapFrom(src => src.PackageId))
            // the correction here: take the Url of the first image in the list
            .ForMember(dest => dest.productImage, opt => opt.MapFrom(src => src.Product.productImages.FirstOrDefault().ImageUrl))
            .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1));
    }
}