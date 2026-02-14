using MaxillaDentalStore.DTOS;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface ICartService
    {

        //  get user cart using user id
        Task<CartDto> GetUserCartAsync(int userId);
        // add item to cart using user id and item id
        Task<CartDto> AddToCartAsync(int userId, AddToCartDto request);
        // update item in cart using user id and item id
        Task<CartDto> UpdateCartItemAsync(int userId, int cartItemId, UpdateCartItemDto request);
        // remove item from cart using user id and item id
        Task<CartDto> RemoveFromCartAsync(int userId, int cartItemId);
        // clear cart using user id
        Task ClearCartAsync(int userId);
    }
}