using System;

namespace MaxillaDentalStore.DTOS
{
    public class CreateReviewDto
    {
        public int UserId { get; set; }
        public int? ProductId { get; set; }
        public int? PackageId { get; set; }
        public string? ReviewText { get; set; }
        public int ReviewRate { get; set; }
    }

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

    /// <summary>
    /// Product review summary - includes total count, average rating, and recent reviews
    /// </summary>
    public class ProductReviewSummaryDto
    {
        public int TotalReviews { get; set; }
        public double? AverageRating { get; set; }
    }
}
