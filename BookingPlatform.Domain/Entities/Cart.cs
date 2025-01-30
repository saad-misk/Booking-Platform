namespace BookingPlatform.Domain.Entities
{
    public class Cart
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } // Cart expiration time
    }
}