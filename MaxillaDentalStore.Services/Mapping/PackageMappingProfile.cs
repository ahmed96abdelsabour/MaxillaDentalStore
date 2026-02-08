using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Mapping
{
    public class PackageMappingProfile : Profile
    {
        public PackageMappingProfile()
        {
            CreateMap<Package, PackageDto>().ReverseMap();
            CreateMap<CreatePackageDto, Package>();
        }
    }
}