namespace BookingPlatform.Domain.Entities
{
    public class Invoice
    {
        public Guid InvoiceId { get; set; }
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; }
        public string FilePath { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}