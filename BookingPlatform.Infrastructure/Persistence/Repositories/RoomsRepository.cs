using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class RoomsRepository : Repository<Room>, IRoomsRepository
    {
        public RoomsRepository(AppDbContext context, ILogger<Repository<Room>> logger)
         : base(context, logger) { }

        public async Task<List<Room>> GetRoomsByIdsAsync(
            IEnumerable<Guid> roomIds, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.Images)
                .Where(r => roomIds.Contains(r.RoomId))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsRoomAvailableAsync(
            Guid roomId, 
            DateTime checkInDate, 
            DateTime checkOutDate, 
            CancellationToken cancellationToken = default)
        {
            var hasConflictingBookings = await _context.Bookings
                .AnyAsync(b => b.RoomId == roomId &&
                               b.CheckInDateUtc < checkOutDate &&
                               b.CheckOutDateUtc > checkInDate, 
                          cancellationToken);

            var room = await _dbSet
                .FirstOrDefaultAsync(r => r.RoomId == roomId, cancellationToken);

            return !hasConflictingBookings && 
                   room != null && 
                   room.Status == RoomStatus.Available;
        }
    }
}