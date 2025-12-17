using System.Threading.Tasks;
using Agora.Application.DTOs;

namespace Agora.Application.Service
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    }
}
