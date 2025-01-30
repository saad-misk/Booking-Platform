using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserRepository(
            AppDbContext context,
            ILogger<Repository<User>> logger,
            IPasswordHasher<User> passwordHasher
        ) : base(context, logger)
        {
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }

        public bool VerifyPassword(User user, string password)
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user, 
                user.PasswordHash, 
                password
            );
            return result == PasswordVerificationResult.Success;
        }

        public async Task CreateUserAsync(
            User user, 
            CancellationToken cancellationToken = default
        )
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, user.PasswordHash);
            await AddAsync(user, cancellationToken);
        }
    }
}