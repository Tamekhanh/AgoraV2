using Agora.Application.DTOs;
using System.Threading.Tasks;

namespace Agora.Application.Service
{
    public interface ICartService
    {
        Task<CartDTO> GetCartAsync(int userId);
        Task AddToCartAsync(int userId, AddToCartRequest request);
        Task UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
        Task RemoveFromCartAsync(int userId, int productId);
        Task ClearCartAsync(int userId);
    }
}
