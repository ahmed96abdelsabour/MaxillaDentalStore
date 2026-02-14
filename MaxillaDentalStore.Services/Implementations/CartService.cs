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
            // Use AsNoTracking query for reading to ensure fresh data and performance
            var cart = await _unitOfWork.Carts.GetActiveCartDetailsAsync(userId);
        
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

            decimal unitPrice = 0;

            if (request.ProductId.HasValue)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value);
                if (product == null) throw new KeyNotFoundException($"Product with ID {request.ProductId} not found.");
                if (!product.IsActive) throw new InvalidOperationException($"Product '{product.Name}' is currently unavailable.");

                unitPrice = product.FinalPrice;

                // Use the shared variant validation logic
                var variants = NormalizeAndValidateVariants(product, request.SelectedColor, request.SelectedSize, request.SelectedMaterial);
                request.SelectedColor = variants.Color;
                request.SelectedSize = variants.Size;
                request.SelectedMaterial = variants.Material;
            }
            else if (request.PackageId.HasValue)
            {
                var package = await _unitOfWork.Packages.GetByIdAsync(request.PackageId.Value);
                if (package == null) throw new KeyNotFoundException($"Package with ID {request.PackageId} not found.");
                if (!package.IsAvilable) throw new InvalidOperationException($"Package '{package.Name}' is currently unavailable.");

                unitPrice = package.Price;
                request.SelectedColor = null;
                request.SelectedSize = null;
                request.SelectedMaterial = null;
            }

            // Check for existing item AFTER normalization
            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ((request.ProductId.HasValue && ci.ProductId == request.ProductId) ||
                 (request.PackageId.HasValue && ci.PackageId == request.PackageId)) &&
                ci.SelectedColor == request.SelectedColor &&
                ci.SelectedSize == request.SelectedSize &&
                ci.SelectedMaterial == request.SelectedMaterial);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                _unitOfWork.Carts.Update(cart);
            }
            else
            {
                var newItem = _mapper.Map<CartItem>(request);
                newItem.CartId = cart.CartId;
                newItem.UnitPrice = unitPrice;
                cart.CartItems.Add(newItem);
            }

            await _unitOfWork.CommitAsync();
            return await GetUserCartAsync(userId);
        }

        // edit product in cart 
        // if not found throw not foun 
        // if quantity is 0 or less, remove the item
        public async Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto request)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            if (cart == null) throw new KeyNotFoundException("Cart not found for user.");

            var item = cart.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);
            if (item == null) throw new KeyNotFoundException($"Cart item {cartItemId} not found.");

            // Update quantity
            if (request.Quantity <= 0)
            {
                cart.CartItems.Remove(item);
            }
            else
            {
                item.Quantity = request.Quantity;
                
                // Update notes if provided
                if (request.ItemNotes != null)
                {
                    item.ItemNotes = request.ItemNotes;
                }

                // Update variants if it's a product
                if (item.ProductId.HasValue)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId.Value);
                    if (product != null)
                    {
                        var variants = NormalizeAndValidateVariants(product, request.SelectedColor, request.SelectedSize, request.SelectedMaterial);
                        item.SelectedColor = variants.Color;
                        item.SelectedSize = variants.Size;
                        item.SelectedMaterial = variants.Material;
                    }
                }
            }
            
            await _unitOfWork.CommitAsync();
            return await GetUserCartAsync(userId);
        }

        private (string? Color, string? Size, string? Material) NormalizeAndValidateVariants(Product product, string? selectedColor, string? selectedSize, string? selectedMaterial)
        {
            string? finalColor = null;
            string? finalSize = null;
            string? finalMaterial = null;

            // Color
            if (product.HasColor)
            {
                var allowed = product.Color?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim()).ToList() ?? new List<string>();
                if (string.IsNullOrEmpty(selectedColor) || selectedColor.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    finalColor = allowed.FirstOrDefault();
                }
                else
                {
                    finalColor = allowed.FirstOrDefault(c => c.Equals(selectedColor, StringComparison.OrdinalIgnoreCase));
                    if (finalColor == null)
                        throw new InvalidOperationException($"Selected color '{selectedColor}' is invalid. Available: {product.Color}");
                }
            }

            // Size
            if (product.HasSize)
            {
                var allowed = product.Size?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList() ?? new List<string>();
                if (string.IsNullOrEmpty(selectedSize) || selectedSize.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    finalSize = allowed.FirstOrDefault();
                }
                else
                {
                    finalSize = allowed.FirstOrDefault(s => s.Equals(selectedSize, StringComparison.OrdinalIgnoreCase));
                    if (finalSize == null)
                        throw new InvalidOperationException($"Selected size '{selectedSize}' is invalid. Available: {product.Size}");
                }
            }

            // Material
            if (product.HasMaterial)
            {
                var allowed = product.Material?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).ToList() ?? new List<string>();
                if (string.IsNullOrEmpty(selectedMaterial) || selectedMaterial.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    finalMaterial = allowed.FirstOrDefault();
                }
                else
                {
                    finalMaterial = allowed.FirstOrDefault(m => m.Equals(selectedMaterial, StringComparison.OrdinalIgnoreCase));
                    if (finalMaterial == null)
                        throw new InvalidOperationException($"Selected material '{selectedMaterial}' is invalid. Available: {product.Material}");
                }
            }

            return (finalColor, finalSize, finalMaterial);
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