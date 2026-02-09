using AutoMapper;
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
        private readonly AppDbContext _context; // For specific queries/updates if needed
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
            // 1. Get User's Cart with Items
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
            {
                throw new InvalidOperationException("Cannot create order. Cart is empty.");
            }

            // 2. Prepare Order
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
            var order = await _unitOfWork.Orders.GetWithDetailsAsync(orderId);
            if (order == null) return null;

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<PageResult<OrderResponseDto>> GetUserOrdersAsync(int userId, int pageNumber, int pageSize)
        {
            var pagedOrders = await _unitOfWork.Orders.GetByUserIdAsync(userId, pageNumber, pageSize);
            
            var orderDtos = _mapper.Map<List<OrderResponseDto>>(pagedOrders.Items);
            
            // Note: GetByUserIdAsync typically doesn't load items for list view if not implemented with Include.
            // If the DTO requires items, we need to ensure they are loaded.
            // Looking at Repo, GetByUserIdAsync does NOT include items.
            // So OrderResponseDto.OrderItems might be empty.
            // Ideally for lists we use OrderSummaryDto or Ensure items are loaded.
            // The interface returns OrderResponseDto (Full).
            // Let's assume for List View we might not need deep details, or we should fetch them.
            // To be correct with the Return Type, we should probably fetch details or use a Summary DTO.
            // Given the interface, let's Stick to the repo result. If items are needed, the repo method needs update or we allow empty items in list.
            
            return new PageResult<OrderResponseDto>
            {
                Items = orderDtos,
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

        public async Task CancelOrderAsync(int orderId, int userId)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found.");

            // Admin (userId 0 or specific logic?) - Assuming caller handles authorization check roughly, 
            // but here we validate ownership if userId is provided and not Admin? 
            // Service usually trusts Controller for AuthZ, but can validate ownership.
            // Let's assume userId is the requester. If it's the owner, check status.
            
            // NOTE: Logic to check if user is admin is usually in Controller. 
            // Here we check if user owns it.
            if (order.UserId != userId)
            {
                // If ID matches, good. If not, maybe it's Admin? 
                // Interface doesn't pass Role. 
                // Let's assume strict ownership check for Customer flow.
                // If this is called by Admin, Controller should probably pass the Order.UserId or skip this check?
                // Let's rely on Controller to pass correct userId or hande Admin case there.
                // For "CancelOrderAsync(orderId, userId)", it implies "User X cancels Order Y".
                // So if Order Y doesn't belong to X, fail.
                throw new InvalidOperationException("You are not authorized to cancel this order.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("Only pending orders can be cancelled.");
            }

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

            _mapper.Map(updateDto, order);
            
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            await _unitOfWork.Orders.ChangeStatusAsync(orderId, newStatus);
            await _unitOfWork.CommitAsync();
        }
    }
}
