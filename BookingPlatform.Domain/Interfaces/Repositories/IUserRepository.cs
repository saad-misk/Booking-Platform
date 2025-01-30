using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        bool VerifyPassword(User user, string password);
        Task<bool> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    }
}