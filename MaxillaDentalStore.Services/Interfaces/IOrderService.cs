using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    /// <summary>
    /// Service interface for Order management (Checkout, History, Management)
    /// </summary>
    public interface IOrderService
    {
        // ==================== Checkout Operations ====================

        /// <summary>
        /// Creates an order from the user's active cart.
        /// Transfers cart items to order items and clears the cart.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="createDto">Shipping and notes info</param>
        /// <returns>The created order with full details</returns>
        Task<OrderResponseDto> CreateOrderFromCartAsync(int userId, OrderCreateDto createDto);

        // ==================== Read Operations ====================

        /// <summary>
        /// Get order by ID with full details (Items, Snapshots)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Order Response DTO</returns>
        Task<OrderResponseDto?> GetOrderByIdAsync(int orderId);

        /// <summary>
        /// Get orders for a specific user (History)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <returns>Paginated list of orders</returns>
        Task<PageResult<OrderResponseDto>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize);

        /// <summary>
        /// Get all orders (Admin Dashboard) with optional status filter
        /// </summary>
        /// <param name="pageNumber">Page Number</param>
        /// <param name="pageSize">Page Size</param>
        /// <param name="status">Optional status filter</param>
        /// <returns>Paginated list of orders</returns>
        Task<PageResult<OrderResponseDto>> GetAllOrdersAsync(int pageNumber, int pageSize, OrderStatus? status = null);

        // ==================== Management Operations ====================

        /// <summary>
        /// Cancel an order (User/Admin). 
        /// User can only cancel if status is Pending.
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="userId">User ID requesting cancellation (for validation)</param>
        /// <param name="isAdmin">If true, bypasses ownership check</param>
        Task CancelOrderAsync(int orderId, int userId, bool isAdmin = false);

        /// <summary>
        /// Update order details (Shipping/Notes).
        /// Only allowed if order is Pending.
        /// </summary>
        /// <param name="updateDto">Update DTO</param>
        Task<OrderResponseDto> UpdateOrderAsync(OrderUpdateDto updateDto);

        // ==================== Admin Operations ====================

        /// <summary>
        /// Update order status (Admin only)
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <param name="newStatus">New Status</param>
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
    }
}
