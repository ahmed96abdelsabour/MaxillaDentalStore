using AutoMapper;
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

        // constructor to initialize the unit of work and automapper
        public CartService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                cart = new Cart { UserId = userId, IsActive = true, CreatedAt = DateTime.Now };
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

           

                // check of product value and add it to product in cart 
                if (request.ProductId.HasValue)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value);
                    newItem.UnitPrice = product?.Price ?? 0;
                }

                cart.CartItems.Add(newItem);
            }

            await _unitOfWork.CommitAsync();
            return await GetUserCartAsync(userId);
        }


        // edit product in cart 
        // if not found throw not foun 
        //if found increase quantity 
        public async Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemDto request)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            var item = cart?.CartItems.FirstOrDefault(ci => ci.CartItemId == request.CartItemId);

            if (item == null) throw new Exception("Item not found");

            item.Quantity = request.Quantity;
            await _unitOfWork.CommitAsync();

            return await GetUserCartAsync(userId);
        }


        // remove item and remove it from db  
        public async Task<CartDto> RemoveFromCartAsync(int userId, int cartItemId)
        {
            var cart = await _unitOfWork.Carts.GetCartByUserIdAsync(userId);
            var item = cart?.CartItems.FirstOrDefault(ci => ci.CartItemId == cartItemId);

            if (item != null)
            {
                
                cart!.CartItems.Remove(item);
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