using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context, ILogger<CartRepository> logger)
            : base(context, logger) { }

        public async Task SaveCartAsync(Cart cart, CancellationToken cancellationToken = default)
        {
            cart.ExpiresAt = DateTime.UtcNow.AddDays(1);

            var existingCart = await _dbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cart.CartId, cancellationToken);

            if (existingCart == null)
            {
                if (cart.CartId == Guid.Empty)
                    cart.CartId = Guid.NewGuid();

                await AddAsync(cart, cancellationToken);
            }
            else
            {
                existingCart.UserId = cart.UserId;
                existingCart.ExpiresAt = cart.ExpiresAt;

                foreach (var item in cart.Items)
                {
                    if (item.CartItemId == Guid.Empty)
                        item.CartItemId = Guid.NewGuid();

                    existingCart.Items.Add(item);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveCartItemAsync(Guid cartId, Guid cartItemId, CancellationToken cancellationToken = default)
        {
            var cart = await _dbSet.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CartId == cartId, cancellationToken);

            if (cart == null)
            {
                throw new NotFoundException($"Cart with ID {cartId} not found.");
            }

            var cartItem = cart.Items.FirstOrDefault(item => item.CartItemId == cartItemId);

            if (cartItem == null)
            {
                throw new NotFoundException($"Cart item with ID {cartItemId} not found in cart {cartId}.");
            }

            cart.Items.Remove(cartItem);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}