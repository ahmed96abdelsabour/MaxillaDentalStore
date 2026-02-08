namespace MaxillaDentalStore.DTOS
{
    // الداتا اللي بترجع لما نعرض الباقة
    public class PackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }
        public string ImageUrl { get; set; }
        // لو حابب تعرض المنتجات اللي جوا الباقة
        public List<ProductResponseDto> Products { get; set; } = new();
    }

    // الداتا المطلوبة لما نكريت باقة جديدة
    public class CreatePackageDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> ProductIds { get; set; } // أيدي المنتجات اللي هتكون في الباقة
    }
}
