using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Checkout.Responses
{
    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ConfirmationNumber { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public string? HotelName { get; set; }
        public RoomType? RoomType { get; set; }
        public decimal? TotalPrice { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public string? InvoiceUrl { get; set; }

    }
}