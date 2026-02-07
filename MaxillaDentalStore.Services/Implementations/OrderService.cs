using AutoMapper;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Common.Pagination;
using MaxillaDentalStore.Data;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Repositories.Interfaces;
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
        private readonly AppDbContext _context; // For Cart clearing if needed, though repo should handle it ideally
        private readonly IDateTimeProvider _dateTimeProvider;

        public OrderService(IUnitOfWork unitOfWork, IMapper mapper, AppDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        // ==================== Checkout Operations ====================

        public async Task<OrderResponseDto> CreateOrderFromCartAsync(int userId, OrderCreateDto createDto)
        {
            // 1. Get User's Cart with Items using Repository
            var cart = await _unitOfWork.Carts.GetActiveCartDetailsAsync(userId);

            if (cart == null || !cart.CartItems.Any())
                throw new InvalidOperationException("Cannot create order. Cart is empty or not found.");

            // 2. Create Order Entity
            var order = new Order
            {
                UserId = userId,
                Status = OrderStatus.Pending,
                OrderDate = _dateTimeProvider.UtcNow, 
                ShippingAddress = createDto.ShippingAddress,
                phoneNumber = createDto.PhoneNumber,
                Notes = createDto.Notes,
                OrderItems = new List<OrderItem>()
            };

            decimal totalOrderPrice = 0;

            // 3. Process Items & Validate Availability
            foreach (var cartItem in cart.CartItems)
            {
                decimal unitPrice = 0;
                decimal itemTotalPrice = 0;

                // A. Product Item
                if (cartItem.ProductId.HasValue)
                {
                    var product = cartItem.Product;
                    if (product == null || !product.IsActive)
                        throw new InvalidOperationException($"Product '{product?.Name ?? "Unknown"}' is no longer available.");

                    unitPrice = product.FinalPrice; // Snapshot Price
                    itemTotalPrice = unitPrice * cartItem.Quantity;
                }
                // B. Package Item (If applicable)
                else if (cartItem.PackageId.HasValue)
                {
                    var package = cartItem.Package;
                    // Check package availability logic if needed (Assuming packages are active if exist)
                    unitPrice = package?.Price ?? 0;
                    itemTotalPrice = unitPrice * cartItem.Quantity;
                }

                totalOrderPrice += itemTotalPrice;

                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    PackageId = cartItem.PackageId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotalPrice,
                    SelectedColor = cartItem.SelectedColor,
                    SelectedSize = cartItem.SelectedSize,
                    SelectedMaterial = cartItem.SelectedMaterial,
                    ItemNotes = cartItem.ItemNotes
                };

                order.OrderItems.Add(orderItem);
            }

            order.TotalPrice = totalOrderPrice;

            // 4. Save Order
            await _unitOfWork.Orders.AddAsync(order);

            // 5. Clear Cart (Remove items)
            _unitOfWork.Carts.ClearCart(cart);

            // 6. Commit Transaction
            await _unitOfWork.CommitAsync();

            // 7. Return Response
            return _mapper.Map<OrderResponseDto>(order);
        }

        // ==================== Read Operations ====================

        public async Task<OrderResponseDto?> GetOrderByIdAsync(int orderId)
        {
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order == null) return null;

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<PageResult<OrderResponseDto>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)
        {
            var pagedOrders = await _unitOfWork.Orders.GetByUserIdAsync(userId, pageNumber, pageSize);
            
            return new PageResult<OrderResponseDto>
            {
                Items = _mapper.Map<List<OrderResponseDto>>(pagedOrders.Items),
                TotalItems = pagedOrders.TotalItems,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }

        public async Task<PageResult<OrderResponseDto>> GetAllOrdersAsync(int pageNumber, int pageSize, OrderStatus? status = null)
        {
            var pagedOrders = await _unitOfWork.Orders.GetAllAsync(pageNumber, pageSize, status);

            return new PageResult<OrderResponseDto>
            {
                Items = _mapper.Map<List<OrderResponseDto>>(pagedOrders.Items),
                TotalItems = pagedOrders.TotalItems,
                PageNumber = pagedOrders.PageNumber,
                PageSize = pagedOrders.PageSize
            };
        }

        // ==================== Management Operations ====================
        // for admin to cancel order and user to cancel order
        public async Task CancelOrderAsync(int orderId, int userId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            // Check authorization (if userId provided)
            if (userId != 0 && order.UserId != userId) // userId 0 means Admin/System override
                throw new UnauthorizedAccessException("You are not authorized to cancel this order.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Only pending orders can be cancelled.");

            await _unitOfWork.Orders.ChangeStatusAsync(orderId, OrderStatus.Cancelled);
            await _unitOfWork.CommitAsync();
        }

        // for admin to update order
        public async Task<OrderResponseDto> UpdateOrderAsync(OrderUpdateDto updateDto)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(updateDto.OrderId);
            if (order == null)
                throw new InvalidOperationException($"Order {updateDto.OrderId} not found.");

            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Cannot update details of a processed order.");

            // Map updates
            _mapper.Map(updateDto, order);

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            await _unitOfWork.Orders.ChangeStatusAsync(orderId, newStatus);
            await _unitOfWork.CommitAsync();
        }
    }
}
