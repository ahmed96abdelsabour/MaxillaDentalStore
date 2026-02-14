using AutoMapper;
using AutoMapper.QueryableExtensions;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace MaxillaDentalStore.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        // ==================== Admin Operations ====================

        public async Task<PageResult<NotificationResponseDto>> GetAllNotificationsForAdminAsync(int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                    .ThenInclude(o => o.OrderItems)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Product)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Package)
                .Include(n => n.RelatedUser)
                .Where(n => n.RecipientUser.Role == UserRole.Admin)
                .OrderByDescending(n => n.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<NotificationResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PageResult<NotificationResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PageResult<NotificationResponseDto>> GetUnreadNotificationsForAdminAsync(int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                    .ThenInclude(o => o.OrderItems)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Product)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Package)
                .Include(n => n.RelatedUser)
                .Where(n => n.RecipientUser.Role == UserRole.Admin && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<NotificationResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PageResult<NotificationResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountForAdminAsync()
        {
            return await _unitOfWork.Notifications.GetUnreadCountForAdminAsync();
        }

        // ==================== Customer Operations ====================

        public async Task<PageResult<NotificationResponseDto>> GetNotificationsForUserAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Include(n => n.Order)
                    .ThenInclude(o => o.OrderItems)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Product)
                .Include(n => n.Review)
                    .ThenInclude(r => r.Package)
                .Where(n => n.RecipientUserId == userId)
                .OrderByDescending(n => n.CreatedAt);

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<NotificationResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PageResult<NotificationResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<int> GetUnreadCountForUserAsync(int userId)
        {
            return await _unitOfWork.Notifications.GetUnreadCountForUserAsync(userId);
        }

        // ==================== Common Operations ====================

        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var success = await _unitOfWork.Notifications.MarkAsReadAsync(notificationId, userId);
            if (success)
            {
                await _unitOfWork.CommitAsync();
            }
            return success;
        }

        public async Task<int> MarkAllAsReadForUserAsync(int userId)
        {
            var count = await _unitOfWork.Notifications.MarkAllAsReadForUserAsync(userId);
            await _unitOfWork.CommitAsync();
            return count;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationId, int userId)
        {
            // Direct delete, no commit needed as ExecuteDeleteAsync happens on DB immediately in repo
            // But we should follow UoW pattern if possible. 
            // In this specific repo implementation we used ExecuteDeleteAsync which bypasses tracking
            // So just calling it is enough.
            return await _unitOfWork.Notifications.DeleteAsync(notificationId, userId);
        }

        public async Task<int> DeleteAllNotificationsForUserAsync(int userId)
        {
            return await _unitOfWork.Notifications.DeleteAllForUserAsync(userId);
        }

        // ==================== Internal Notification Creation ====================

        public async Task CreateNewOrderNotificationAsync(int orderId, int customerId, bool isFirstOrder)
        {
            // Get admin user ID (assuming first admin in system)
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

            if (adminUser == null)
                return; // No admin to notify

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return;

            var title = isFirstOrder ? "New Order - First Time Customer!" : "New Order";
            var message = isFirstOrder
                ? $"New order #{orderId} from {order.User.Name} (First Order!) - Total: ${order.TotalPrice:F2}"
                : $"New order #{orderId} from {order.User.Name} - Total: ${order.TotalPrice:F2}";

            var notification = new Notification
            {
                RecipientUserId = adminUser.UserId,
                Type = NotificationType.NewOrder,
                Title = title,
                Message = message,
                OrderId = orderId,
                RelatedUserId = customerId,
                IsFirstOrder = isFirstOrder,
                CreatedAt = _dateTimeProvider.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CommitAsync();
        }

        public async Task CreateOrderConfirmedNotificationAsync(int orderId, int customerId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return;

            var title = "Order Confirmed";
            var message = $"Your order #{orderId} has been confirmed and is being processed. Total: ${order.TotalPrice:F2}";

            var notification = new Notification
            {
                RecipientUserId = customerId,
                Type = NotificationType.OrderConfirmed,
                Title = title,
                Message = message,
                OrderId = orderId,
                RelatedUserId = null, // Customer notifications don't have related users
                IsFirstOrder = null,
                CreatedAt = _dateTimeProvider.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CommitAsync();
        }

        public async Task CreateNewReviewNotificationAsync(int reviewId, int customerId, int? productId, int? packageId)
        {
            // Get admin user
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == UserRole.Admin);

            if (adminUser == null)
                return; // No admin to notify

            // Get customer info
            var customer = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == customerId);

            if (customer == null)
                return;

            // Get review to extract rating
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);

            if (review == null)
                return;

            // Build message based on product or package
            string itemName = "";
            if (productId.HasValue)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId.Value);
                itemName = product?.Name ?? "Product";
            }
            else if (packageId.HasValue)
            {
                var package = await _context.Packages
                    .FirstOrDefaultAsync(p => p.PackageId == packageId.Value);
                itemName = package?.Name ?? "Package";
            }

            var title = "New Review Submitted";
            var message = $"{customer.Name} left a {review.ReviewRate}⭐ review for {itemName}";

            var notification = new Notification
            {
                RecipientUserId = adminUser.UserId,
                Type = NotificationType.NewReview,
                Title = title,
                Message = message,
                OrderId = null,
                ReviewId = reviewId,
                RelatedUserId = customerId,
                IsFirstOrder = null,
                CreatedAt = _dateTimeProvider.UtcNow,
                IsRead = false
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.CommitAsync();
        }
    }
}
