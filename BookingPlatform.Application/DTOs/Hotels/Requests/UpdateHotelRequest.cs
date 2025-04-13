using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Hotels.Requests
{
    public class UpdateHotelRequest
    {
        public string? Name { get; set; }
        [Required]
        public Guid HotelId { get; set; }
        public string? Description { get; set; }
        public int StarRating { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string? PhoneNumber { get; set; }
    }
}