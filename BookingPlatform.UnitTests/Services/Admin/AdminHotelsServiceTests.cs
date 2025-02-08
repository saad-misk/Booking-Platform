using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Infrastructure.Services.Admin;
using Moq;

namespace BookingPlatform.UnitTests.Services.Admin
{
    public class AdminHotelsServiceTests
    {
        private readonly Mock<IHotelsRepository> _mockHotelsRepo;
        private readonly Mock<ICitiesRepository> _mockCitiesRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly AdminHotelsService _service;

        public AdminHotelsServiceTests()
        {
            _mockHotelsRepo = new Mock<IHotelsRepository>();
            _mockCitiesRepo = new Mock<ICitiesRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new AdminHotelsService(
                _mockUnitOfWork.Object,
                _mockHotelsRepo.Object,
                _mockCitiesRepo.Object
            );
        }

        [Fact]
        public async Task GetHotelByIdAsync_WhenHotelExists_ReturnsHotel()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var expectedHotel = new Hotel { HotelId = hotelId };
            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedHotel);

            // Act
            var result = await _service.GetHotelByIdAsync(hotelId);

            // Assert
            Assert.Equal(expectedHotel, result);
        }

        [Fact]
        public async Task GetHotelByIdAsync_WhenHotelDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Hotel?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetHotelByIdAsync(hotelId));
        }

        [Fact]
        public async Task CreateHotelAsync_WhenCityExists_CreatesHotelAndCommits()
        {
            // Arrange
            var request = new CreateHotelRequest
            {
                CityId = Guid.NewGuid(),
                Name = "New Hotel",
                Description = "Test Description",
                StarRating = 4
            };
            var city = new City { CityId = request.CityId };
            _mockCitiesRepo.Setup(r => r.GetByIdAsync(request.CityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(city);
            Hotel addedHotel = null!;
            _mockHotelsRepo.Setup(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()))
                .Callback<Hotel, CancellationToken>((h, _) => addedHotel = h);

            // Act
            var result = await _service.CreateHotelAsync(request);

            // Assert
            _mockHotelsRepo.Verify(r => r.AddAsync(It.IsAny<Hotel>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(city, addedHotel?.City);
        }

        [Fact]
        public async Task CreateHotelAsync_WhenCityDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var request = new CreateHotelRequest { CityId = Guid.NewGuid() };
            _mockCitiesRepo.Setup(r => r.GetByIdAsync(request.CityId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((City?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateHotelAsync(request));
        }

        [Fact]
        public async Task UpdateHotelAsync_UpdatesPropertiesCorrectly()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var existingHotel = new Hotel
            {
                HotelId = hotelId,
                Name = "Old Name",
                Description = "Old Description",
                StarRating = 3,
                PhoneNumber = "1234567"
            };
            var request = new UpdateHotelRequest
            {
                HotelId = hotelId,
                Name = "New Name",
                Description = "New Description",
                StarRating = 4,
                PhoneNumber = "+1 (234) 567-890"
            };

            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingHotel);

            // Act
            await _service.UpdateHotelAsync(request);

            // Assert
            Assert.Equal("New Name", existingHotel.Name);
            Assert.Equal("New Description", existingHotel.Description);
            Assert.Equal(4, existingHotel.StarRating);
            Assert.Equal("1234567890", existingHotel.PhoneNumber);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateHotelAsync_WhenHotelNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var request = new UpdateHotelRequest { HotelId = Guid.NewGuid() };
            _mockHotelsRepo.Setup(r => r.GetByIdAsync(request.HotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Hotel?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateHotelAsync(request));
        }

        [Fact]
        public async Task UpdateHotelAsync_WithInvalidStarRating_ThrowsBadRequest()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var existingHotel = new Hotel { HotelId = hotelId };
            var request = new UpdateHotelRequest
            {
                HotelId = hotelId,
                StarRating = 6 // Invalid rating
            };

            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingHotel);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateHotelAsync(request));
        }

        [Fact]
        public async Task UpdateHotelAsync_WithInvalidPhoneNumber_ThrowsBadRequest()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var existingHotel = new Hotel { HotelId = hotelId };
            var request = new UpdateHotelRequest
            {
                HotelId = hotelId,
                PhoneNumber = "123" // Too short
            };

            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingHotel);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.UpdateHotelAsync(request));
        }

        [Fact]
        public async Task DeleteHotelAsync_DeletesHotelAndCommits()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            var existingHotel = new Hotel { HotelId = hotelId };
            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingHotel);

            // Act
            await _service.DeleteHotelAsync(hotelId);

            // Assert
            _mockHotelsRepo.Verify(r => r.DeleteAsync(hotelId, It.IsAny<CancellationToken>()), Times.Once);
            _mockUnitOfWork.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteHotelAsync_WhenHotelNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var hotelId = Guid.NewGuid();
            _mockHotelsRepo.Setup(r => r.GetByIdAsync(hotelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Hotel?)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteHotelAsync(hotelId));
        }
    }
}