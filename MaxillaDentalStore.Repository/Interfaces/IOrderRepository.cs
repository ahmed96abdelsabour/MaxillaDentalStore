using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        // 🔹 Base
        Task<Order?> GetByIdAsync(int orderId);

        Task<PageResult<Order>> GetAllAsync(
            int pageNumber,
            int pageSize,
            OrderStatus? status = null);

        // 🔹 User-related
        Task<PageResult<Order>> GetByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize);

        // 🔹 Details
        Task<Order?> GetWithDetailsAsync(int orderId);

        // 🔹 Write
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int orderId);
        Task ChangeStatusAsync(int orderId, OrderStatus newStatus);
        Task<OrderStatus?> GetStatusAsync(int orderId);
        Task UpdateNotesAsync(int orderId, string? notes);
        Task UpdateShippingAsync(int orderId, string shippingAddress, string phoneNumber);
        
        // Check if user has purchased a specific product or package (Confirmed order)
        Task<bool> HasUserPurchasedItemAsync(int userId, int? productId, int? packageId);
    }
}
