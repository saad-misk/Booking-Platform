using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Entities
{
    public class Payment
    {
        public Guid PaymentId { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}