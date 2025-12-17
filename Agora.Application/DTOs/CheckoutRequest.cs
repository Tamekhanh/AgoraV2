namespace Agora.Application.DTOs
{
    public class CheckoutRequest
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
