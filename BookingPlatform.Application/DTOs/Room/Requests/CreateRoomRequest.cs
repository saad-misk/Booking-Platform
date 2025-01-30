using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Rooms.Requests
{
    public class CreateRoomRequest
    {
        public RoomType RoomType { get; set; }
        public Guid HotelId { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}