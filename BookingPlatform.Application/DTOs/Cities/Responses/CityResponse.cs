using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.DTOs.Cities.Responses
{
    public class CityResponse
    {
        public Guid CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostOffice { get; set; } = string.Empty;
        public string CityCode { get; set; } = string.Empty;
        public int HotelsCount { get; set; }
        public int BookingsCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public Image? Thumbnail { get; set; }
    }
}