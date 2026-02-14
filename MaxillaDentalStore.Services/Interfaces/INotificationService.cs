using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface INotificationService
    {
        // Admin operations
        Task<PageResult<NotificationResponseDto>> GetAllNotificationsForAdminAsync(int pageNumber, int pageSize);
        Task<PageResult<NotificationResponseDto>> GetUnreadNotificationsForAdminAsync(int pageNumber, int pageSize);
        Task<int> GetUnreadCountForAdminAsync();

        // Customer operations
        Task<PageResult<NotificationResponseDto>> GetNotificationsForUserAsync(int userId, int pageNumber, int pageSize);
        Task<int> GetUnreadCountForUserAsync(int userId);

        // Common operations
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<int> MarkAllAsReadForUserAsync(int userId);
        Task<bool> DeleteNotificationAsync(int notificationId, int userId);
        Task<int> DeleteAllNotificationsForUserAsync(int userId);

        // Internal notification creation methods
        Task CreateNewOrderNotificationAsync(int orderId, int customerId, bool isFirstOrder);
        Task CreateOrderConfirmedNotificationAsync(int orderId, int customerId);
        Task CreateNewReviewNotificationAsync(int reviewId, int customerId, int? productId, int? packageId);
    }
}
