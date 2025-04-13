using System.ComponentModel.DataAnnotations;

namespace BookingPlatform.Application.DTOs.Auth
{
    public sealed record RegisterRequest(
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2)]
        string FirstName,

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2)]
        string LastName,

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        string Email,

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8)]
        string Password
    );
}