namespace BookingPlatform.Application.DTOs.Bookings.Responses
{
    public class BookingResponse
    {
        public Guid BookingId { get; set; }
        public string HotelName { get; set; } = string.Empty; 
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty; 
    }
}