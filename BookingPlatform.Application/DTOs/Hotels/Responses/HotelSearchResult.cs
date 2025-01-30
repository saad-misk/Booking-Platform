namespace BookingPlatform.Application.DTOs.Hotels.Responses
{
    public class HotelSearchResult
    {
        public Guid HotelId { get; set; }
        public string Name { get; set; }
        public Guid CityId { get; set; }
        public string CityName { get; set; }
        public double Rating { get; set; }
    }
}