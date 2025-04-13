namespace BookingPlatform.Domain.Entities
{
    public class CartItem
    {
        public Guid CartItemId { get; set; }
        public Guid CartId { get; set; }
        public Cart Cart { get; set; }
        
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights => (CheckOutDate - CheckInDate).Days;
        
        public decimal TotalPrice { get; set; }
        
        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    }
}