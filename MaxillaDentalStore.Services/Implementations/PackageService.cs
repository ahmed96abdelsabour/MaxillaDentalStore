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

        public PackageService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync()
        {
            
            var packages = await _unitOfWork.Packages.GetAllActivePackagesAsync();
            return _mapper.Map<IEnumerable<PackageDto>>(packages);
        }

        public async Task<PackageDto> GetPackageByIdAsync(int id)
        {
            // تغيير الاسم من GetByIdAsync للاسم المخصص بالتفاصيل
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(id);
            return _mapper.Map<PackageDto>(package);
        }
        public async Task<bool> CreatePackageAsync(CreatePackageDto dto)
        {
            var package = _mapper.Map<Package>(dto);
            await _unitOfWork.Packages.AddAsync(package);
            return await _unitOfWork.CommitAsync() > 0;
        }

        public async Task<bool> DeletePackageAsync(int id)
        {
           
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(id);

            if (package == null) return false;

            _unitOfWork.Packages.Delete(package);
            return await _unitOfWork.CommitAsync() > 0;
        }
    }
}