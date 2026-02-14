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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly INotificationService _notificationService;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context, IDateTimeProvider dateTimeProvider, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _notificationService = notificationService;
        }

        // ==================== Checkout Operations ====================

        public async Task<OrderResponseDto> CreateOrderFromCartAsync(int userId, OrderCreateDto createDto)
        {
            // Sanitize inputs
            if (string.IsNullOrWhiteSpace(createDto.Notes)) createDto.Notes = null;
            // ShippingAddress and PhoneNumber are required/non-nullable in DTO usually, so we trust validation or let it fail DB constraints if empty
            // But we can trim or check

            // 1. Get User with Phones for defaults
            var user = await _context.Users
                .Include(u => u.UserPhones)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            
            if (user == null) throw new KeyNotFoundException("User not found.");

            // 2. Resolve Shipping Address and Phone Number
            var finalShippingAddress = createDto.ShippingAddress;
            if (string.IsNullOrWhiteSpace(finalShippingAddress) || finalShippingAddress.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                finalShippingAddress = user.ClinicAddress;
                if (string.IsNullOrWhiteSpace(finalShippingAddress))
                    throw new InvalidOperationException("Shipping address is required, and no clinic address is set in your profile.");
            }

            var finalPhoneNumber = createDto.PhoneNumber;
            if (string.IsNullOrWhiteSpace(finalPhoneNumber) || finalPhoneNumber.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                finalPhoneNumber = string.Join(", ", user.UserPhones.Select(p => p.PhoneNumber));
                if (string.IsNullOrWhiteSpace(finalPhoneNumber))
                    throw new InvalidOperationException("Phone number is required, and no phone numbers are set in your profile.");
            }

            // 3. Get User's Cart with Items
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cannot create order. Cart is empty.");
            }

            // 4. Prepare Order
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = _dateTimeProvider.UtcNow,
                ShippingAddress = finalShippingAddress,
                phoneNumber = finalPhoneNumber,
                Notes = createDto.Notes,
                OrderItems = new List<OrderItem>()
            };

            decimal grandTotal = 0;

            // 3. Process Cart Items to Order Items
            foreach (var cartItem in cart.CartItems)
            {
                var orderItem = new OrderItem
                {
                    Quantity = cartItem.Quantity,
                    SelectedColor = cartItem.SelectedColor,
                    SelectedSize = cartItem.SelectedSize,
                    SelectedMaterial = cartItem.SelectedMaterial,
                    ItemNotes = cartItem.ItemNotes
                };

                decimal unitPrice = 0;

                // Handle Product
                if (cartItem.ProductId.HasValue)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(cartItem.ProductId.Value);
                    if (product == null || !product.IsActive)
                    {
                        throw new InvalidOperationException($"Product with ID {cartItem.ProductId} is no longer available.");
                    }
                    
                    // Use FinalPrice (Price - Discount)
                    unitPrice = product.FinalPrice;
                    
                    orderItem.ProductId = product.ProductId;
                    // Note: Product navigation property will be set by EF when saving if tracked, 
                    // but we just set ID here.
                }
                // Handle Package
                else if (cartItem.PackageId.HasValue)
                {
                    var package = await _unitOfWork.Packages.GetByIdAsync(cartItem.PackageId.Value);
                    if (package == null || !package.IsAvilable)
                    {
                        throw new InvalidOperationException($"Package with ID {cartItem.PackageId} is no longer available.");
                    }

                    unitPrice = package.Price;
                    orderItem.PackageId = package.PackageId;
                }
                else
                {
                    continue; // Skip invalid items
                }

                // Set Prices
                orderItem.UnitPrice = unitPrice;
                orderItem.TotalPrice = unitPrice * cartItem.Quantity;

                grandTotal += orderItem.TotalPrice;
                order.OrderItems.Add(orderItem);
            }

            order.TotalPrice = grandTotal;

            // 4. Save Order
            await _unitOfWork.Orders.AddAsync(order);
            
            // 5. Clear Cart (or remove items)
            _unitOfWork.Carts.ClearCart(cart);

            await _unitOfWork.CommitAsync();

            // Check if this is the customer's first order
            bool isFirstOrder = await _context.Orders
                .CountAsync(o => o.UserId == userId) == 1;

            // Create notification for admin about new order
            await _notificationService.CreateNewOrderNotificationAsync(order.OrderId, userId, isFirstOrder);

            // 6. Return Response using GetOrderById to ensure full mapping (images etc)
            // or Map directly if we load necessary props.
            // Since we just created it, we have the entities in memory but maybe not images for mapping?
            // Safer to re-fetch or map manually if performant. 
            // Let's re-fetch to be safe about includes for mapping.
            return await GetOrderByIdAsync(order.OrderId) ?? throw new Exception("Failed to retrieve created order.");
        }

        // ==================== Read Operations ====================

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Where(o => o.OrderId == orderId)
                .AsNoTracking()
                .ProjectTo<OrderResponseDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<PageResult<OrderResponseDto>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)
        {
            var query = _context.Orders
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate); // Specific user orders usually newest first

            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<OrderResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new PageResult<OrderResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PageResult<OrderResponseDto>> GetAllOrdersAsync(int pageNumber, int pageSize, OrderStatus? status = null)
        {
            var query = _context.Orders.AsNoTracking();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }
            
            query = query.OrderByDescending(o => o.OrderDate); // Newest orders first for Admin dashboard

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<OrderResponseDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            
            return new PageResult<OrderResponseDto>
            {
                Items = items,
                TotalItems = totalItems,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // ==================== Management Operations ====================

        public async Task CancelOrderAsync(int orderId, int userId, bool isAdmin = false)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            // If not Admin, enforce ownership check
            if (!isAdmin && order.UserId != userId)
            {
                throw new InvalidOperationException("You are not authorized to cancel this order.");
            }

            // If Admin, they can cancel any order? Usually yes.
            // But status check applies to everyone? 
            // Usually Admin can cancel "Confirmed" orders too if needed (refunds etc), but here let's stick to Pending for safety 
            // or allow Admin to cancel any non-completed/shipped status?
            // "Only pending orders can be cancelled" rule might be business logic.
            // If User -> Only Pending. If Admin -> Maybe any active status?
            // For now, let's keep it consistent: Cancellation is for "stopping a process". 
            // If checking "Pending", assume simple flow. Admin might need to cancel Confirmed too.
            // Let's allow Admin to cancel "Confirmed" too, but not "Shipped/Completed" (if they existed).
            // DTO/Enum has Pending, Confirmed, Cancelled. 
            // So if Admin, allow Pending OR Confirmed. User only Pending.
            
            if (order.Status == OrderStatus.Cancelled)
                 throw new InvalidOperationException("Order is already cancelled.");

            if (!isAdmin && order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled by user.");
            }
            
            // If Admin, they can cancel Confirmed too. The only state they shouldn't cancel is maybe Completed (if it existed) or match rule.
            // Since we only have Pending/Confirmed/Cancelled/Unknown (from Mapping), let's assume Admin implies power.
            
            await _unitOfWork.Orders.ChangeStatusAsync(orderId, OrderStatus.Cancelled);
            await _unitOfWork.CommitAsync();
        }

        public async Task<OrderResponseDto> UpdateOrderAsync(OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(updateDto.OrderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot update non-pending order.");

            // Sanitize inputs
            if (updateDto.Notes != null && string.IsNullOrWhiteSpace(updateDto.Notes)) updateDto.Notes = null;
            if (updateDto.ShippingAddress != null && string.IsNullOrWhiteSpace(updateDto.ShippingAddress)) updateDto.ShippingAddress = null;
            if (updateDto.PhoneNumber != null && string.IsNullOrWhiteSpace(updateDto.PhoneNumber)) updateDto.PhoneNumber = null;

            _mapper.Map(updateDto, order);
            
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            var oldStatus = order.Status;

            await _unitOfWork.Orders.ChangeStatusAsync(orderId, newStatus);
            await _unitOfWork.CommitAsync();

            // Send notification to customer when order is confirmed
            if (newStatus == OrderStatus.Confirmed && oldStatus != OrderStatus.Confirmed)
            {
                await _notificationService.CreateOrderConfirmedNotificationAsync(orderId, order.UserId);
            }
        }
    }
}
