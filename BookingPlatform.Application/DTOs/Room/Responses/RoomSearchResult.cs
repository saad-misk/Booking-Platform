namespace BookingPlatform.Application.DTOs.Rooms.Responses
{
    public class RoomSearchResult
    {
        public Guid RoomId { get; set; }
        public string? Number { get; set; }
        public Guid HotelId { get; set; }
        public string? HotelName { get; set; }
        public decimal Price { get; set; }
    }
}