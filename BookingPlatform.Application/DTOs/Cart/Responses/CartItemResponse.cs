namespace BookingPlatform.Application.DTOs.Cart.Responses
{
    public class CartItemResponse
    {
        public Guid CartItemId { get; set; }
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Nights { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}