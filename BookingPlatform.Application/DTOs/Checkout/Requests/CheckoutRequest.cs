using System.ComponentModel.DataAnnotations;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.DTOs.Checkout.Requests
{
    public class CheckoutRequest
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }
        
        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public PaymentMethod PaymentMethod { get; set; }
        
        public string SpecialRequests { get; set; }
    }

}