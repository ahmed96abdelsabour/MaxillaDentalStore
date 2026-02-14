using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MaxillaDentalStore.Repository.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        // ==================== Read Operations ====================

        public async Task<IEnumerable<Notification>> GetAllForAdminAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                .Include(n => n.RelatedUser)
                .Where(n => n.RecipientUser.Role == UserRole.Admin)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetUnreadForAdminAsync()
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                .Include(n => n.RelatedUser)
                .Where(n => n.RecipientUser.Role == UserRole.Admin && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetForUserAsync(int userId)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountForAdminAsync()
        {
            return await _context.Notifications
                .Where(n => n.RecipientUser.Role == UserRole.Admin && !n.IsRead)
                .CountAsync();
        }

        public async Task<int> GetUnreadCountForUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .Include(n => n.Order)
                .Include(n => n.RelatedUser)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
        }

        // ==================== Write Operations ====================

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientUserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            return true;
        }

        public async Task<int> MarkAllAsReadForUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
        }

        public async Task<bool> DeleteAsync(int notificationId, int userId)
        {
            // We use ExecuteDelete for performance, but we need to check if it exists and belongs to user first
            // Or just try to delete where Id == id AND Recipient == userId
            
            var rowsAffected = await _context.Notifications
                .Where(n => n.NotificationId == notificationId && n.RecipientUserId == userId)
                .ExecuteDeleteAsync();
                
            return rowsAffected > 0;
        }

        public async Task<int> DeleteAllForUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientUserId == userId)
                .ExecuteDeleteAsync();
        }
    }
}
