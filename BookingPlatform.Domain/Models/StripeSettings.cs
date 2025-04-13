namespace BookingPlatform.Domain.Models
{
    public class StripeSettings
    {
        public string? SecretKey { get; set; }
        public string Currency { get; set; } = "usd"; 
    }
}