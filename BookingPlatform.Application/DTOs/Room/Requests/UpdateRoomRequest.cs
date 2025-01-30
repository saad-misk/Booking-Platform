using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Rooms.Requests
{
    public class UpdateRoomRequest
    {
        public Guid RoomId { get; set; }
        public RoomType? RoomType { get; set; }
        public string? Number { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public decimal? PricePerNight { get; set; }
        public RoomStatus? Status { get; set; }
        public ICollection<Image>? Images { get; set; } = new List<Image>();
    }
}