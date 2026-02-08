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

        // add this method to get products inside a package
        public async Task<IEnumerable<ProductResponseDto>> GetPackageItemsAsync(int packageId)
        {
            var package = await _unitOfWork.Packages.GetPackageWithDetailsAsync(packageId);
            if (package == null) return null;

            // استخراج المنتجات من داخل كائنات PackageItem
            var products = package.PackageItems.Select(pi => pi.Product);

            return _mapper.Map<IEnumerable<ProductResponseDto>>(products);
        }

        // add this method to add a product to a package

        public async Task<bool> AddProductToPackageAsync(int packageId, int productId)
        {
            // 1. استخدم الميثود اللي إحنا متأكدين إنها موجودة في الـ Repository بتاعك
            // دي هترجع الباقة ومعاها الـ PackageItems عشان نعرف نضيف عليها
            var packageWithItems = await _unitOfWork.Packages.GetPackageWithDetailsAsync(packageId);

            // تأكد من وجود الباقة
            if (packageWithItems == null) return false;

            // 2. التحقق مما إذا كان المنتج موجوداً بالفعل في الباقة
            if (packageWithItems.PackageItems.Any(pi => pi.ProductId == productId))
                return true;
            
            // 3. إنشاء سجل جديد في الجدول الوسيط
            var newItem = new PackageItem
            {
                PackageId = packageId,
                ProductId = productId
            };

            // 4. إضافة السجل الجديد للقائمة
            packageWithItems.PackageItems.Add(newItem);

            // 5. حفظ التغييرات
            return await _unitOfWork.CommitAsync() > 0;
        }


    }
}