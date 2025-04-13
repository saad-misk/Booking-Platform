namespace BookingPlatform.Domain.Entities
{
    public class Image
    {
        public Guid ImageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}