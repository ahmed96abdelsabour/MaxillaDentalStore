using AutoMapper;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;

namespace MaxillaDentalStore.Services.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly MaxillaDentalStore.Common.Abstractions.IDateTimeProvider _dateTimeProvider;

        public PackageService(IUnitOfWork unitOfWork, IMapper mapper, MaxillaDentalStore.Common.Abstractions.IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            
            var packages = await _unitOfWork.Packages.GetAllActivePackagesAsync();
            return _mapper.Map<IEnumerable<PackageDto>>(packages);
        }

        public async Task<PackageDto> GetPackageByIdAsync(int id)
        {
            // get package with details through navigation properties using id
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(id);
            return _mapper.Map<PackageDto>(package);
        }
        
        // create a new package using dto
        public async Task<bool> CreatePackageAsync(CreatePackageDto dto)
        {
            var package = _mapper.Map<Package>(dto);
            package.CreatedAt = _dateTimeProvider.UtcNow;
            package.IsAvilable = true; // Default to available
            
            // Note: dto.ProductIds handling? 
            // The mapping profile doesn't map ProductIds to PackageItems automatically usually unless configured.
            // We should handle the initial products here if provided.
            if (dto.ProductIds != null && dto.ProductIds.Any())
            {
                package.PackageItems = dto.ProductIds.Select(pid => new PackageItem { ProductId = pid }).ToList();
            }

            await _unitOfWork.Packages.AddAsync(package);
            return await _unitOfWork.CommitAsync() > 0;
        }
        // delete a package using id
        public async Task<bool> DeletePackageAsync(int id)
        {
           
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(id);

            if (package == null) return false;

            _unitOfWork.Packages.Delete(package);
            return await _unitOfWork.CommitAsync() > 0;
        }

        // add this method to get products inside a package
        public async Task<IEnumerable<ProductResponseDto>> GetPackageItemsAsync(int packageId)
        {
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(packageId);
            if (package == null) return null;

            // get products inside a package using navigation properties
            var products = package.PackageItems.Select(pi => pi.Product);

            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }
        // add a product to a package using package id and product id
        public async Task<bool> AddProductToPackageAsync(int packageId, int productId)
        {
            // get package with details using package id
            var packageWithItems = await _unitOfWork.Packages.GetPackageWithDetailsAsync(packageId);

            // check if package exists
            if (packageWithItems == null) return false;

            // check if product exists in package
            if (packageWithItems.PackageItems.Any(pi => pi.ProductId == productId))
                return true;
            
            // create a new package item
            var newItem = new PackageItem
            {
                PackageId = packageId,
                ProductId = productId
            };

            // add new item to package items
            packageWithItems.PackageItems.Add(newItem);

            // save changes
            return await _unitOfWork.CommitAsync() > 0;
        }


    }
}