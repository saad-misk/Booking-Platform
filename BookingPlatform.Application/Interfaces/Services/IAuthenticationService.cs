using BookingPlatform.Application.DTOs.Auth;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface IAuthenticationService
    {
        Task<AuthResponse> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
        Task<AuthResponse> RegisterAsync(string firstName, string lastName, string email,
                     string password, CancellationToken cancellationToken = default);
    }
}