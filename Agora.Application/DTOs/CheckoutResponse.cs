namespace Agora.Application.DTOs
{
    public class CheckoutResponse
    {
        public int OrderId { get; set; }
        public int TotalAmount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
