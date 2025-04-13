using BookingPlatform.Domain.Models;

namespace BookingPlatform.Application.DTOs.Auth
{
    public sealed record AuthResponse(
        JwtToken Token,
        DateTime Expiration,
        string Message
    );
}