using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Hotels.Responses
{
    public class HotelSearchResult
    {
        [Required]
        public Guid HotelId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public Guid CityId { get; set; }
        [Required]
        public string CityName { get; set; }
        [Required]
        public double Rating { get; set; }
    }
}