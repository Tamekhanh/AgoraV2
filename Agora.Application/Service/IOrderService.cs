using System.Threading.Tasks;
using Agora.Application.DTOs;

namespace Agora.Application.Service
{
    public interface IOrderService
    {
        Task<CheckoutResponse> CheckoutAsync(CheckoutRequest request);
    }
}
