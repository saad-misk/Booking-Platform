using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Rooms.Responses
{
    public class RoomResponse
    {
        public Guid RoomId { get; set; }
        public RoomType RoomType { get; set; }
        public decimal PricePerNight { get; set; }
        public int Capacity { get; set; }
        public RoomStatus Status { get; set; }
    }
}