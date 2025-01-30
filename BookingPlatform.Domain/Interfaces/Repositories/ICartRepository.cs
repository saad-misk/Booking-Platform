using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task SaveCartAsync(Cart cart, CancellationToken cancellationToken = default);
        Task RemoveCartItemAsync(Guid cartId, Guid cartItemId, CancellationToken cancellationToken = default);
    }
}