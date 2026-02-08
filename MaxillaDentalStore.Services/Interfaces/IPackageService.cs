using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface IPackageService
    {

      //  get all packages
        Task<IEnumerable<PackageDto>> GetAllPackagesAsync();
        // get package by id
        Task<PackageDto> GetPackageByIdAsync(int id);
        // create a new package
        Task<bool> CreatePackageAsync(CreatePackageDto dto);
        // delete a package
        Task<bool> DeletePackageAsync(int id);
        // get products inside a package
        Task<IEnumerable<ProductResponseDto>> GetPackageItemsAsync(int packageId);

        // add a product to a package
        Task<bool> AddProductToPackageAsync(int packageId, int productId);
    }
}