using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Models;

namespace BookingPlatform.Domain.Interfaces.JWT
{
    public interface IJwtTokenGenerator
    {
        JwtToken GenerateToken(User user);
    }
}