namespace BookingPlatform.Application.DTOs.Cities.Responses
{
    public class CitySearchResult
    {
        public Guid CityId { get; set; }
        public string Name { get; set; }
        public int HotelCount { get; set; }
    }
}