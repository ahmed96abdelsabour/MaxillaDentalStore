namespace MaxillaDentalStore.DTOS
{
    public class PackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        public List<PackageItemDto> Products { get; set; } = new();
    }

    public class CreatePackageDto
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> ProductIds { get; set; } = new();
    }

    public class PackageItemDto
    {
        public int ProductId { get; set; }
        public int PackageId { get; set; }
        public int Quantity { get; set; }
        public string? ProductImage { get; set; }
        public string ProductName { get; set; } = null!;
        public string? ProductDescription { get; set; }
        public decimal ProductPrice { get; set; }
    }
}
