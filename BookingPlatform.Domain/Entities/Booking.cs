using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities
{
    public class Booking
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CheckInDateUtc { get; set; }
        public DateTime CheckOutDateUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string ConfirmationNumber { get; set; }= null!;
        public BookingStatus Status { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}