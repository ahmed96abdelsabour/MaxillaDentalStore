using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

public class PackageMappingProfile : Profile
{
    public PackageMappingProfile()
    {
        CreateMap<Package, PackageDto>()
            // مهم جداً: مابينج من PackageItems اللي في الـ Entity لـ Products اللي في الـ DTO
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.PackageItems))
            .ReverseMap();

        CreateMap<CreatePackageDto, Package>();

        CreateMap<PackageItem, packageitemDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price.ToString())) // لأن الـ DTO string
            .ForMember(dest => dest.packageId, opt => opt.MapFrom(src => src.PackageId))
            // التصحيح هنا: نأخذ الـ Url الخاص بأول صورة في القائمة
            .ForMember(dest => dest.productImage, opt => opt.MapFrom(src => src.Product.productImages.FirstOrDefault().ImageUrl))
            .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => 1));
    }
}