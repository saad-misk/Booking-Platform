namespace BookingPlatform.Application.DTOs.Hotels.Responses
{
    public class HotelsResponse
    {
        public Guid HotelId { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty; 
        public int StarRating { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty; 
        public decimal StartingPrice { get; set; } // Min room price
        public List<string> AvailableRoomTypes { get; set; } = new();
    }
}