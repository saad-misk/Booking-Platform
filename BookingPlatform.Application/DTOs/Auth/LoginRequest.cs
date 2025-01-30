using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Auth
{
    public sealed record LoginRequest(
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        string Email,

        [Required(ErrorMessage = "Password is required")]
        string Password
    );
}