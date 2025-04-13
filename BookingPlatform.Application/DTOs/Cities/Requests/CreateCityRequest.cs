using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Cities.Requests
{
    public class CreateCityRequest
    {
        [Required][StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required][StringLength(50)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string PostOffice { get; set; } = string.Empty;

        [Required][StringLength(10)]
        public string CityCode { get; set; } = string.Empty;
    }
}