using Agora.Application.DTOs;
using System.Threading.Tasks;

namespace Agora.Application.Service
{
    public interface ICartService
    {
        Task<CartDTO> GetCart(int userId);
        Task AddToCart(int userId, AddToCartRequest request);
        Task UpdateCartItem(int userId, UpdateCartItemRequest request);
        Task RemoveFromCart(int userId, int productId);
        Task ClearCart(int userId);
        Task IncreaseCartItem(int userId, int productId);
        Task DecreaseCartItem(int userId, int productId);
    }
}
