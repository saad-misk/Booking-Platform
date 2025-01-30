
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.DTOs.Cities.Requests
{
    public class UpdateCityRequest
    {
        public Guid CityId { get; set; }
        public Image? Thumbnail { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostOffice { get; set; } = string.Empty;
        public string CityCode { get; set; } = string.Empty;
    }
}