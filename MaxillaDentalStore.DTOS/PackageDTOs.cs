namespace MaxillaDentalStore.DTOS
{
    // الداتا اللي بترجع لما نعرض الباقة
    public class PackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }

        // التعديل هنا: استخدم packageitemDto بدل ProductResponseDto
        // عشان الـ AutoMapper يعرف يربطهم آلياً
        public List<packageitemDto> Products { get; set; } = new();
    }

    // الداتا المطلوبة لما نكريت باقة جديدة
    public class CreatePackageDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public List<int> ProductIds { get; set; } 
    }

    public class packageitemDto
    {
        public int ProductId { get; set; }
    
        
        public int packageId { get; set; }
        public int Quantity { get; set; }

        public string productImage { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public string ProductPrice { get; set; }

    }


}
