using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IPackageService
    {
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync();
        Task<PackageDto> GetPackageByIdAsync(int id);
        Task<bool> CreatePackageAsync(CreatePackageDto dto);
        Task<bool> DeletePackageAsync(int id);
    }
}