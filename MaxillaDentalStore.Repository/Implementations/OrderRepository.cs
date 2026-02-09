using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        // add new order to database
        public async Task AddAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            await _context.Orders.AddAsync(order);
        }

        
        public Task UpdateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        // This method deletes an order from the database based on the provided order ID.
        // but logic say no need to delete order just change status to cancel
        public async Task DeleteAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.", nameof(orderId));

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            if (order.Status == OrderStatus.Confirmed)
                throw new InvalidOperationException("Cannot delete a confirmed order. Consider changing its status to cancelled instead.");
            _context.Orders.Remove(order);
        }

        // get all orders with pagination and optional status filter
        public async Task<PageResult<Order>> GetAllAsync(int pageNumber, int pageSize, OrderStatus? status = null)
        {
            var query = _context.Orders
                .AsNoTracking()
                .AsQueryable();
            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);
            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PageResult<Order>
            {
                Items = orders,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        // get order by id without details, use this method when you just need basic info of order and want to avoid heavy loading of related entities
        public async Task<Order?> GetByIdAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.", nameof(orderId));
            return await _context.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        // get orders by user id with pagination, this method is useful for user order history page where you want to show list of orders without details and allow user to navigate through pages of orders
        public async Task<PageResult<Order>> GetByUserIdAsync(int userId, int pageNumber, int pageSize)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.", nameof(userId));
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId);
            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return new PageResult<Order>
            {
                Items = orders,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

        }

        // This method retrieves an order along with its related order items, products, and packages based on the provided order ID.
        // it is heavy method so use it only when you need details of order
        // it uses eager loading to include related entities in a single query, improving performance by reducing the number of database calls.
        public Task<Order?> GetWithDetailsAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.", nameof(orderId));
            return _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.productImages)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Package)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task ChangeStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.Status = newStatus;
        }

        public async Task<OrderStatus?> GetStatusAsync(int orderId)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.", nameof(orderId));
            return await _context.Orders.FindAsync(orderId) is Order order ? order.Status : null;
        }

        // update notes of order, this method is useful when user want to add special instructions or comments to order after placing it, you can add validation to check if order is still in pending status before allowing update
        public async Task UpdateNotesAsync(int orderId, string? notes)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.", nameof(orderId));
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");
            if (order.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot update notes for confirmed or cancelled orders.");
            order.Notes = notes;
        }

        // update shipping address and phone number of order, this method is useful when user want to change shipping info after placing order but before order is shipped, you can add validation to check if order is still in pending status before allowing update
        public async Task UpdateShippingAsync(int orderId, string shippingAddress, string phoneNumber)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order!.Status == OrderStatus.Confirmed || order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException("Cannot update shipping info for confirmed or cancelled orders.");
            if (order == null)
                throw new InvalidOperationException("Order not found");
            if (string.IsNullOrWhiteSpace(shippingAddress))
                throw new ArgumentException("Shipping address cannot be empty.", nameof(shippingAddress));
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));
            order.ShippingAddress = shippingAddress;
            order.phoneNumber = phoneNumber;
            order.ShippingAddress = shippingAddress;
            order.phoneNumber = phoneNumber;
        }

        public async Task<bool> HasUserPurchasedItemAsync(int userId, int? productId, int? packageId)
        {
            if (userId <= 0) return false;
            
            // Query for orders that are Confirmed (completed purchase)
            // and contain the items
            var query = _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == userId && o.Status == OrderStatus.Confirmed);

            if (productId.HasValue)
            {
                // Check if any order item has this product
                return await query.AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == productId.Value));
            }
            else if (packageId.HasValue)
            {
                 // Check if any order item has this package
                return await query.AnyAsync(o => o.OrderItems.Any(oi => oi.PackageId == packageId.Value));
            }

            return false;
        }
    }
}
