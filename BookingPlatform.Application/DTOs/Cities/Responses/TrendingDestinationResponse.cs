namespace BookingPlatform.Application.DTOs.Cities.Responses
{
    public class TrendingDestinationResponse
    {
        public Guid CityId { get; set; }
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int BookingsCount { get; set; }
    }
}