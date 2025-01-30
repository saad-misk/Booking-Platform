using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context, ILogger<BookingRepository> logger)
        : base(context, logger) { }

        public async Task<ICollection<Booking>> GetConflictingBookings(
            Guid roomId, 
            DateTime checkInDate, 
            DateTime checkOutDate, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => b.RoomId == roomId &&
                            b.CheckOutDateUtc > checkInDate &&
                            b.CheckInDateUtc < checkOutDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<ICollection<Booking>> GetBookings(
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(b => b.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}