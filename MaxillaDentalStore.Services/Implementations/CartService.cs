using AutoMapper;
using MaxillaDentalStore.Common.Abstractions;
using MaxillaDentalStore.Data.Entities;
using MaxillaDentalStore.DTOS;
using MaxillaDentalStore.Services.Interfaces;
using MaxillaDentalStore.UnitOfWork;

namespace MaxillaDentalStore.Services.Implementations
{
    public class CartService : ICartService
    {

        // we inject the unit of work and automapper to use it in our service
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDateTimeProvider _dateTimeProvider;

        // constructor to initialize the unit of work and automapper
        public CartService(IUnitOfWork unitOfWork, IMapper mapper, IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _dateTimeProvider = dateTimeProvider;
        }

        // call unitofWork to get all user cat if found if not create new one 
        public async Task<CartDto> GetUserCartAsync(int userId)
        {
            
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);

            if (cart == null) return new CartDto { UserId = userId, Items = new List<CartItemDto>() };

            return _mapper.Map<CartDto>(cart);
        }


        // check if product in cart or not first 
        // if found increase quantity 
        // if not add this new item in cart 
        public async Task<CartDto> AddToCartAsync(int userId, AddToCartDto request)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart 
                { 
                    UserId = userId, 
                    IsActive = true, 
                    CreatedAt = _dateTimeProvider.UtcNow 
                };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.CommitAsync();
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                (request.ProductId.HasValue && ci.ProductId == request.ProductId) ||
                (request.PackageId.HasValue && ci.PackageId == request.PackageId));

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                _unitOfWork.Carts.Update(cart);
            }
            else
            {
                var newItem = _mapper.Map<CartItem>(request);
                newItem.CartId = cart.CartId;

                // Validate and set price
                if (request.ProductId.HasValue)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value);
                    if (product == null) throw new KeyNotFoundException($"Product with ID {request.ProductId} not found.");
                    if (!product.IsActive) throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");
                    
                    // Use FinalPrice to account for any discounts
                    newItem.UnitPrice = product.FinalPrice;
                }
                else if (request.PackageId.HasValue)
                {
                    var package = await _unitOfWork.Packages.GetByIdAsync(request.PackageId.Value);
                    if (package == null) throw new KeyNotFoundException($"Package with ID {request.PackageId} not found.");
                    if (!package.IsAvilable) throw new InvalidOperationException($"Package '{package.Name}' is currently unavailable.");

                    newItem.UnitPrice = package.Price;
                }

                cart.CartItems.Add(newItem);
            }

            await _unitOfWork.CommitAsync();
            return await GetUserCartAsync(userId);
        }


        // edit product in cart 
        // if not found throw not foun 
        //if found increase quantity 
        // if quantity is 0 or less, remove the item
        public async Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemDto request)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null) throw new KeyNotFoundException("Cart not found for user.");

            var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == request.CartItemId);

            if (item == null) throw new KeyNotFoundException($"Cart item {request.CartItemId} not found.");

            // If quantity is 0 or less, remove the item
            if (request.Quantity <= 0)
            {
                cart.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = request.Quantity;
            }
            
            await _unitOfWork.CommitAsync();

            return await GetUserCartAsync(userId);
        }


        // remove item and remove it from db  
        public async Task<CartDto> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null) return new CartDto { UserId = userId }; // or throw? Returning empty seems safe.

            var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);

            if (item != null)
            {
                
                cart.CartItems.Remove(item);
                await _unitOfWork.CommitAsync();
            }
            return await GetUserCartAsync(userId);
        }


        // clear cart and remove all items from cart 
        public async Task ClearCartAsync(int userId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                _unitOfWork.Carts.ClearCart(cart);
                await _unitOfWork.CommitAsync();
            }
        }
    }
}