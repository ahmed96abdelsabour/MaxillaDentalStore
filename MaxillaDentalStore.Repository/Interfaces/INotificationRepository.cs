using MaxillaDentalStore.Data.Entities;

namespace MaxillaDentalStore.Repository.Interfaces
{
    public interface INotificationRepository
    {
        // Read operations
        Task<IEnumerable<Notification>> GetAllForAdminAsync();
        Task<IEnumerable<Notification>> GetUnreadForAdminAsync();
        Task<IEnumerable<Notification>> GetForUserAsync(int userId);
        Task<int> GetUnreadCountForAdminAsync();
        Task<int> GetUnreadCountForUserAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId);

        // Write operations
        Task AddAsync(Notification notification);
        Task<bool> MarkAsReadAsync(int notificationId, int userId);
        Task<int> MarkAllAsReadForUserAsync(int userId);
        
        // Delete operations
        Task<bool> DeleteAsync(int notificationId, int userId);
        Task<int> DeleteAllForUserAsync(int userId);
    }
}
