using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Bookings.Requests
{
    public class CreateBookingRequest
    {
        [Required]
        public Guid HotelId { get; set; }
        
        [Required]
        public Guid RoomId { get; set; } = new();
        
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
    }
}