using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface IRoomsRepository : IRepository<Room>
    {
        Task<List<Room>> GetRoomsByIdsAsync(IEnumerable<Guid> roomIds, CancellationToken cancellationToken = default);
        Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime checkInDate,
                         DateTime checkOutDate, CancellationToken cancellationToken = default);
    }
}