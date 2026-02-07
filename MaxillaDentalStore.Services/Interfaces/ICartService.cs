using MaxillaDentalStore.DTOS;
using System.Threading.Tasks;

namespace MaxillaDentalStore.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetCartByUserIdAsync(int userId);
        Task<CartDTO> CreateCartAsync(CreateCartDTO dto);
        Task<bool> DeactivateCartAsync(int cartId);
    }
}