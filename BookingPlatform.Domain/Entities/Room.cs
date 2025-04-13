using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities
{
    public class Room
    {
        public Guid RoomId { get; set; }
        public RoomType RoomClass { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public RoomStatus Status { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}