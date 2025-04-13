using System.ComponentModel.DataAnnotations;
using BookingPlatform.Application.DTOs.Reviews.Responses;
using BookingPlatform.Application.DTOs.Rooms.Responses;

namespace BookingPlatform.Application.DTOs.Hotels.Responses
{
    public class HotelDetailsResponse
    {
        [Required]
        public Guid HotelId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string City { get; set; }
        public string? Description { get; set; }
        public double ReviewsRating { get; set; }
        public int StarRating { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> GalleryUrls { get; set; } = new();
        public List<RoomResponse> Rooms { get; set; } = new();
        public List<ReviewResponse> Reviews { get; set; } = new();
    }
}