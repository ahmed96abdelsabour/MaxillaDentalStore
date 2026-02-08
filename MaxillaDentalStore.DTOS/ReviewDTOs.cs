using System;

namespace MaxillaDentalStore.DTOS
{
    public class CreateReviewDto
    {
        public int UserId { get; set; }

        // التقييم ممكن يكون لمنتج أو لباكيدج
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }

        public string? ReviewText { get; set; }

        // القيمة الرقمية للتقييم
        public int ReviewRate { get; set; }
    }

    
}