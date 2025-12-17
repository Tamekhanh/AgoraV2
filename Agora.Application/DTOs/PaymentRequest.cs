namespace Agora.Application.DTOs
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public int Amount { get; set; }
        public string PaymentMethod { get; set; } = "Demo";
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
