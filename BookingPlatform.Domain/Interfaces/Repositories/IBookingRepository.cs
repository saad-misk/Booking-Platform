using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<ICollection<Booking>> GetConflictingBookings(Guid RoomId, DateTime CheckInDate,
                         DateTime CheckOutDate, CancellationToken cancellationToken = default);
        Task<ICollection<Booking>> GetBookings(Guid userId, CancellationToken cancellationToken = default);
    }
}