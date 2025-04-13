namespace BookingPlatform.Domain.Models
{
    public record JwtToken(string Token, DateTime Expiration, string Issuer);
}