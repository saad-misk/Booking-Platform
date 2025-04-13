using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Cart.Requests
{
    public class AddToCartRequest
    {
        [Required]
        public Guid RoomId { get; set; }
        
        [Required]
        public DateTime CheckInDate { get; set; }
        
        [Required]
        public DateTime CheckOutDate { get; set; }
    }
}