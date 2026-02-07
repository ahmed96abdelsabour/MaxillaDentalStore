using System;

namespace MaxillaDentalStore.DTOS
{
    /// <summary>
    /// يستخدم لعرض بيانات التقييم في واجهة المستخدم (صفحة تفاصيل المنتج)
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }

        // اسم المستخدم اللي كتب التقييم (بيتم سحبه من User.Name عبر الـ Mapper)
        public string UserName { get; set; } = null!;

        public string? ReviewText { get; set; }

        // التقييم الرقمي (عادة من 1 لـ 5)
        public int ReviewRate { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// يستخدم عند استقبال بيانات تقييم جديد من المستخدم
    /// </summary>
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
    /// يستخدم لإظهار ملخص التقييمات أعلى صفحة المنتج
    /// </summary>
    public class ProductReviewSummaryDto
    {
        // متوسط التقييم (مثلاً 4.7)
        public double AverageRating { get; set; }

        // إجمالي عدد الأشخاص اللي قيموا المنتج
        public int TotalReviews { get; set; }
    }
}