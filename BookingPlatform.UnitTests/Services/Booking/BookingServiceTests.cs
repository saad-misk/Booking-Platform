using Moq;
using BookingPlatform.Application.DTOs.Bookings.Requests;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Services.Bookings;

namespace BookingPlatform.Infrastructure.Services.Tests.Bookings
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _bookingRepositoryMock;
        private readonly Mock<IHotelsRepository> _hotelRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly BookingService _bookingService;

        public BookingServiceTests()
        {
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _hotelRepositoryMock = new Mock<IHotelsRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _bookingService = new BookingService(
                _bookingRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _hotelRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateBookingAsync_HotelNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                HotelId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(2)
            };

            _hotelRepositoryMock.Setup(r => r.GetByIdAsync(request.HotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Hotel?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(Guid.NewGuid(), request));

            _unitOfWorkMock.Verify(u => u.BeginTransactionAsync(), Times.Once);
            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_RoomNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                HotelId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(2)
            };

            var hotel = new Hotel { HotelId = request.HotelId, Rooms = new List<Room>() };
            _hotelRepositoryMock.Setup(r => r.GetByIdAsync(request.HotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hotel);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.CreateBookingAsync(Guid.NewGuid(), request));

            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_ConflictingBookings_ThrowsBadRequestException()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                HotelId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(2)
            };

            var hotel = new Hotel
            {
                HotelId = request.HotelId,
                Rooms = new List<Room> { new Room { RoomId = request.RoomId } }
            };

            var conflictingBookings = new List<Booking> { new Booking() };

            _hotelRepositoryMock.Setup(r => r.GetByIdAsync(request.HotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hotel);
            _bookingRepositoryMock.Setup(r => r.GetConflictingBookings(
                request.RoomId, request.CheckInDate, request.CheckOutDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(conflictingBookings);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() =>
                _bookingService.CreateBookingAsync(Guid.NewGuid(), request));

            _unitOfWorkMock.Verify(u => u.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateBookingAsync_Success_ReturnsBookingResponse()
        {
            // Arrange
            var request = new CreateBookingRequest
            {
                HotelId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                CheckInDate = DateTime.UtcNow,
                CheckOutDate = DateTime.UtcNow.AddDays(3)
            };

            var hotel = new Hotel
            {
                HotelId = request.HotelId,
                Name = "Test Hotel",
                Rooms = new List<Room> { new Room { RoomId = request.RoomId, PricePerNight = 100m } }
            };
            hotel.Rooms.First().Hotel = hotel;

            _hotelRepositoryMock.Setup(r => r.GetByIdAsync(request.HotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(hotel);
            _bookingRepositoryMock.Setup(r => r.GetConflictingBookings(
                request.RoomId, request.CheckInDate, request.CheckOutDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _bookingService.CreateBookingAsync(Guid.NewGuid(), request);

            // Assert
            Assert.Equal(request.CheckInDate, result.CheckInDate);
            Assert.Equal(300m, result.TotalPrice, 2); // 3 days * 100
            Assert.Equal("Test Hotel", result.HotelName);

            _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GetBookings_Success_ReturnsMappedResponses()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var bookings = new List<Booking>
            {
                new Booking
                {
                    BookingId = Guid.NewGuid(),
                    Room = new Room { PricePerNight = 150m, Hotel = new Hotel { Name = "Hotel 1" } },
                    CheckInDateUtc = DateTime.UtcNow,
                    CheckOutDateUtc = DateTime.UtcNow.AddDays(2)
                }
            };

            _bookingRepositoryMock.Setup(r => r.GetBookings(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetBookings(userId);

            // Assert
            Assert.Single(result);
            Assert.Equal(300m, result.First().TotalPrice, 2);
        }

        [Fact]
        public async Task GetBookingByIdAsync_BookingExists_ReturnsResponse()
        {
            // Arrange
            var booking = new Booking
            {
                BookingId = Guid.NewGuid(),
                Room = new Room { PricePerNight = 100m, Hotel = new Hotel { Name = "Test Hotel" } },
                CheckInDateUtc = DateTime.UtcNow,
                CheckOutDateUtc = DateTime.UtcNow.AddDays(2)
            };

            _bookingRepositoryMock.Setup(r => r.GetByIdAsync(booking.BookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid(), booking.BookingId);

            // Assert
            Assert.Equal(booking.BookingId, result.BookingId);
            Assert.Equal(200m, result.TotalPrice, 2);
        }

        [Fact]
        public async Task GetBookingByIdAsync_BookingNotFound_ThrowsException()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _bookingRepositoryMock.Setup(r => r.GetByIdAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Booking?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _bookingService.GetBookingByIdAsync(Guid.NewGuid(), bookingId));
        }

        [Fact]
        public async Task DeleteBookingAsync_Success_ReturnsTrue()
        {
            // Arrange
            var bookingId = Guid.NewGuid();
            _bookingRepositoryMock.Setup(r => r.DeleteAsync(bookingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _bookingService.DeleteBookingAsync(Guid.NewGuid(), bookingId);

            // Assert
            Assert.True(result);
        }
    }
}