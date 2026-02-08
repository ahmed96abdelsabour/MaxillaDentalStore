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

    /// <summary>
    /// Review DTO
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string? ProductName { get; set; }
        public string? PackageName { get; set; }
        public string? ReviewText { get; set; }
        public int ReviewRate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
