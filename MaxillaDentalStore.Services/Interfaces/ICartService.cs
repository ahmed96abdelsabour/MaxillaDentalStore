using MaxillaDentalStore.DTOS;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetUserCartAsync(int userId);
        Task<CartDto> AddToCartAsync(int userId, AddToCartDto request);
        Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemDto request);
        Task<CartDto> RemoveFromCartAsync(int userId, int cartItemId);
        Task ClearCartAsync(int userId);
    }
}