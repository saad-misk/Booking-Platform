using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Hotels.Requests
{
    public class CreateHotelRequest
    {
        [Required]
        public string Name { get; set; }
        public Guid CityId { get; set; }
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
    }
}