namespace Agora.Application.DTOs
{
    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public int PaymentId { get; set; }
    }
}
