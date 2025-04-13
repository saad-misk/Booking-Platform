namespace BookingPlatform.Domain.Entities
{
    public class Review
    {
        public Guid ReviewId { get; set; }
        public Guid GuestId { get; set; }
        public User Guest { get; set; }
        public Guid HotelId { get; set; }
        public Hotel Hotel { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}