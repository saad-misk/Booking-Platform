using System.ComponentModel.DataAnnotations;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Checkout
{
    public class PaymentResult
    {
        [Required]
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public string? ErrorMessage { get; set; }
    }
}