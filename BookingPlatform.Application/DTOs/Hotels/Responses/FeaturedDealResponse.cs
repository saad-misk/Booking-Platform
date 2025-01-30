namespace BookingPlatform.Application.DTOs.Hotels.Responses
{
    public class FeaturedDealResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public double Rating { get; set; }
    }
}