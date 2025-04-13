using BookingPlatform.Application.DTOs.Bookings.Requests;
using BookingPlatform.Application.DTOs.Bookings.Responses;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;

namespace BookingPlatform.Infrastructure.Services.Bookings
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IHotelsRepository _hotelRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookingService(
            IBookingRepository bookingRepository,
            IUnitOfWork unitOfWork,
            IHotelsRepository hotelRepository)
        {
            _bookingRepository = bookingRepository;
            _hotelRepository = hotelRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<BookingResponse> CreateBookingAsync(
            Guid userId, 
            CreateBookingRequest request, 
            CancellationToken cancellationToken = default)
        {
            await _unitOfWork.BeginTransactionAsync();
            
            try
            {
                // Validate hotel and rooms
                var hotel = await _hotelRepository.GetByIdAsync(request.HotelId, cancellationToken);
                if (hotel == null) throw new NotFoundException("Hotel not found");

                var room = hotel.Rooms.FirstOrDefault(r => r.RoomId == request.RoomId);
                if (room == null) throw new NotFoundException("Room not found in the selected hotel");

                // Check room availability
                var conflictingBookings = await _bookingRepository.GetConflictingBookings(
                    request.RoomId, request.CheckInDate, request.CheckOutDate, cancellationToken);

                if (conflictingBookings.Any())
                    throw new BadRequestException("The selected room is not available for the chosen dates");

                // Create booking
                var booking = new Booking
                {
                    BookingId = Guid.NewGuid(),
                    UserId = userId,
                    CheckInDateUtc = request.CheckInDate,
                    CheckOutDateUtc = request.CheckOutDate,
                    Status = BookingStatus.Confirmed,
                    CreatedAtUtc = DateTime.UtcNow,
                    Room = room
                };

                await _bookingRepository.AddAsync(booking, cancellationToken);
                await _unitOfWork.CommitAsync();

                return MapToBookingResponse(booking);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ICollection<BookingResponse>> GetBookings(
            Guid userId, 
            CancellationToken cancellationToken = default)
        {
            var bookings = await _bookingRepository.GetBookings(userId, cancellationToken);
            return bookings.Select(MapToBookingResponse).ToList();
        }

        public async Task<BookingResponse> GetBookingByIdAsync(
            Guid userId, 
            Guid bookingId, 
            CancellationToken cancellationToken = default)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken);
            if (booking == null) throw new NotFoundException("Booking not found");

            return MapToBookingResponse(booking);
        }

        public async Task<bool> DeleteBookingAsync(
            Guid userId, 
            Guid bookingId, 
            CancellationToken cancellationToken = default)
        {
            return await _bookingRepository.DeleteAsync(bookingId, cancellationToken);
        }

        private static BookingResponse MapToBookingResponse(Booking booking)
        {
            return new BookingResponse
            {
                BookingId = booking.BookingId,
                CheckInDate = booking.CheckInDateUtc,
                CheckOutDate = booking.CheckOutDateUtc,
                TotalPrice = booking.Room.PricePerNight * 
                             (decimal)(booking.CheckOutDateUtc - booking.CheckInDateUtc).TotalDays,
                Status = booking.Status.ToString(),
                HotelName = booking.Room.Hotel.Name
            };
        }
    }
}