using BookingPlatform.Application.DTOs.Bookings.Requests;
using BookingPlatform.Application.DTOs.Bookings.Responses;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface IBookingService
    {
        Task<BookingResponse> CreateBookingAsync(
            Guid userId, 
            CreateBookingRequest request, 
            CancellationToken cancellationToken = default);

        Task<ICollection<BookingResponse>> GetBookings(
            Guid userId, 
            CancellationToken cancellationToken = default);

        Task<BookingResponse> GetBookingByIdAsync(
            Guid userId, 
            Guid bookingId, 
            CancellationToken cancellationToken = default);

        Task<bool> DeleteBookingAsync(
            Guid userId, 
            Guid bookingId, 
            CancellationToken cancellationToken = default);
    }
}