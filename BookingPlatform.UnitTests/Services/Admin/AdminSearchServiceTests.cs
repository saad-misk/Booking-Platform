// using BookingPlatform.Application.DTOs.Cities.Requests;
// using BookingPlatform.Application.DTOs.Cities.Responses;
// using BookingPlatform.Application.DTOs.Hotels.Requests;
// using BookingPlatform.Application.DTOs.Hotels.Responses;
// using BookingPlatform.Application.DTOs.Rooms.Requests;
// using BookingPlatform.Application.DTOs.Rooms.Responses;
// using BookingPlatform.Application.DTOs.Search;
// using BookingPlatform.Application.Interfaces.Services.Admin;
// using BookingPlatform.Domain.Entities;
// using BookingPlatform.Domain.Interfaces.Repositories;
// using Moq;
// using System.Linq.Expressions;
// using Xunit;

// namespace BookingPlatform.Infrastructure.Services.Admin.Tests
// {
//     public class AdminSearchServiceTests
//     {
//         private readonly Mock<IRepository<Hotel>> _mockHotelsRepo;
//         private readonly Mock<IRepository<City>> _mockCitiesRepo;
//         private readonly Mock<IRepository<Room>> _mockRoomsRepo;
//         private readonly AdminSearchService _service;

//         public AdminSearchServiceTests()
//         {
//             _mockHotelsRepo = new Mock<IRepository<Hotel>>();
//             _mockCitiesRepo = new Mock<IRepository<City>>();
//             _mockRoomsRepo = new Mock<IRepository<Room>>();
//             _service = new AdminSearchService(
//                 _mockHotelsRepo.Object,
//                 _mockCitiesRepo.Object,
//                 _mockRoomsRepo.Object
//             );
//         }

//             [Fact]
//         public async Task SearchHotelsAsync_ReturnsPagedHotels()
//         {
//             // Arrange
//             var criteria = new HotelSearchCriteria { Name = "Luxury", PageNumber = 1, PageSize = 2 };

//             var hotels = new List<Hotel>
//             {
//                 new Hotel { HotelId = new Guid(), Name = "Luxury Hotel", CityId = new Guid(), City = new City { Name = "Paris" }, ReviewsRating = 4.5 },
//                 new Hotel { HotelId = new Guid(), Name = "Luxury Resort", CityId = new Guid(), City = new City { Name = "London" }, ReviewsRating = 5.0 }
//             };

//             _mockHotelsRepo
//                 .Setup(r => r.CountAsync(It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(hotels.Count);

//             _mockHotelsRepo
//                 .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Hotel, bool>>>(),
//                                     null,
//                                     It.IsAny<IEnumerable<Expression<Func<Hotel, object>>>>(),
//                                     It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(hotels);

//             // Act
//             var result = await _service.SearchHotelsAsync(criteria);

//             // Assert
//             Assert.Equal(2, result.Items.Count());
//             Assert.Equal("Luxury Hotel", result.Items[0].Name);
//             Assert.Equal("Paris", result.Items[0].CityName);
//             Assert.Equal("Luxury Resort", result.Items[1].Name);
//             Assert.Equal("London", result.Items[1].CityName);

//             // Verify repository calls
//             _mockHotelsRepo.Verify(r => r.CountAsync(It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
//             _mockHotelsRepo.Verify(r => r.GetAsync(It.IsAny<Expression<Func<Hotel, bool>>>(),
//                                                 null,
//                                                 It.IsAny<IEnumerable<Expression<Func<Hotel, object>>>>(),
//                                                 0, 2, It.IsAny<CancellationToken>()), Times.Once);
//         }


//         [Fact]
//         public async Task SearchHotelsAsync_IncludesCityInQuery()
//         {
//             // Arrange
//             var criteria = new HotelSearchCriteria();
//             IEnumerable<Expression<Func<Hotel, object>>> capturedIncludes = null!;

//             _mockHotelsRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<Hotel, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<Hotel, object>>>>(),
//                 It.IsAny<int?>(),
//                 It.IsAny<int?>(),
//                 It.IsAny<CancellationToken>()))
//                 .Callback<Expression<Func<Hotel, bool>>, IEnumerable<Expression<Func<Hotel, object>>>, int, int, CancellationToken>(
//                 (filter, includes, skip, take, ct) => capturedIncludes = includes)
//                 .ReturnsAsync(new List<Hotel>());

//             // Act
//             await _service.SearchHotelsAsync(criteria);

//             // Assert
//             Assert.Single(capturedIncludes!);
//             Assert.Contains("h => h.City", capturedIncludes?.First().ToString());
//         }

//         [Fact]
//         public async Task SearchHotelsAsync_ReturnsCorrectMapping()
//         {
//             // Arrange
//             var city = new City { Name = "Test City" };
//             var hotel = new Hotel
//             {
//                 HotelId = Guid.NewGuid(),
//                 Name = "Test Hotel",
//                 City = city,
//                 ReviewsRating = 4.5
//             };

//             _mockHotelsRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<Hotel, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<Hotel, object>>>>(),
//                 It.IsAny<int?>(),
//                 It.IsAny<int?>(),
//                 It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(new List<Hotel> { hotel });

//             // Act
//             var result = await _service.SearchHotelsAsync(new HotelSearchCriteria());

//             // Assert
//             var dto = result?.Items?.Single();
//             Assert.Equal(hotel.HotelId, dto!.HotelId);
//             Assert.Equal(hotel.Name, dto.Name);
//             Assert.Equal(city.Name, dto.CityName);
//             Assert.Equal(hotel.ReviewsRating, dto.Rating);
//         }

//         [Fact]
//         public async Task SearchCitiesAsync_AppliesNameFilter()
//         {
//             // Arrange
//             var criteria = new CitySearchCriteria { Name = "test" };
//             Expression<Func<City, bool>> capturedFilter = null!;

//             _mockCitiesRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<City, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<City, object>>>>(),
//                 It.IsAny<int>(),
//                 It.IsAny<int>(),
//                 It.IsAny<CancellationToken>()))
//                 .Callback<Expression<Func<City, bool>>, IEnumerable<Expression<Func<City, object>>>, int, int, CancellationToken>(
//                 (filter, includes, skip, take, ct) => capturedFilter = filter)
//                 .ReturnsAsync(new List<City>());

//             // Act
//             await _service.SearchCitiesAsync(criteria);

//             // Assert
//             var testCity = new City { Name = "Test City" };
//             var compiledFilter = capturedFilter!.Compile();
//             Assert.True(compiledFilter(testCity));
//         }

//         [Fact]
//         public async Task SearchCitiesAsync_IncludesHotelsCount()
//         {
//             // Arrange
//             var city = new City
//             {
//                 Hotels = new List<Hotel> { new Hotel(), new Hotel() }
//             };

//             _mockCitiesRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<City, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<City, object>>>>(),
//                 It.IsAny<int>(),
//                 It.IsAny<int>(),
//                 It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(new List<City> { city });

//             // Act
//             var result = await _service.SearchCitiesAsync(new CitySearchCriteria());

//             // Assert
//             var dto = result?.Items?.Single();
//             Assert.Equal(2, dto?.HotelCount);
//         }

//         [Fact]
//         public async Task SearchRoomsAsync_FiltersByPriceRange()
//         {
//             // Arrange
//             var criteria = new RoomSearchCriteria
//             {
//                 MinPrice = 50,
//                 MaxPrice = 100
//             };

//             Expression<Func<Room, bool>> capturedFilter = null!;
//             _mockRoomsRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<Room, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<Room, object>>>>(),
//                 It.IsAny<int>(),
//                 It.IsAny<int>(),
//                 It.IsAny<CancellationToken>()))
//                 .Callback<Expression<Func<Room, bool>>, IEnumerable<Expression<Func<Room, object>>>, int, int, CancellationToken>(
//                 (filter, includes, skip, take, ct) => capturedFilter = filter)
//                 .ReturnsAsync(new List<Room>());

//             // Act
//             await _service.SearchRoomsAsync(criteria);

//             // Assert
//             var testRoom = new Room { PricePerNight = 75 };
//             var invalidRoom = new Room { PricePerNight = 120 };
//             var compiledFilter = capturedFilter!.Compile();
            
//             Assert.True(compiledFilter(testRoom));
//             Assert.False(compiledFilter(invalidRoom));
//         }

//         [Fact]
//         public async Task SearchRoomsAsync_IncludesHotelInformation()
//         {
//             // Arrange
//             var hotel = new Hotel { Name = "Test Hotel" };
//             var room = new Room { Hotel = hotel };

//             _mockRoomsRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<Room, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<Room, object>>>>(),
//                 It.IsAny<int>(),
//                 It.IsAny<int>(),
//                 It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(new List<Room> { room });

//             // Act
//             var result = await _service.SearchRoomsAsync(new RoomSearchCriteria());

//             // Assert
//             var dto = result?.Items?.Single();
//             Assert.Equal(hotel.Name, dto?.HotelName);
//         }

//         [Fact]
//         public async Task QueryWithPaging_AppliesPaginationCorrectly()
//         {
//             // Arrange
//             var testData = Enumerable.Range(1, 100)
//                 .Select(i => new Hotel { HotelId = Guid.NewGuid() })
//                 .ToList();

//             _mockHotelsRepo.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Hotel, bool>>>(), It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(testData.Count);

//             _mockHotelsRepo.Setup(r => r.GetAsync(
//                 It.IsAny<Expression<Func<Hotel, bool>>>(),
//                 null,
//                 It.IsAny<IEnumerable<Expression<Func<Hotel, object>>>>(),
//                 It.IsAny<int>(),
//                 It.IsAny<int>(),
//                 It.IsAny<CancellationToken>()))
//                 .ReturnsAsync(testData.Skip(10).Take(10).ToList());

//             var criteria = new HotelSearchCriteria
//             {
//                 PageNumber = 2,
//                 PageSize = 10
//             };

//             // Act
//             var result = await _service.SearchHotelsAsync(criteria);

//             // Assert
//             Assert.Equal(10, result?.Items?.Count());
//             Assert.Equal(100, result?.TotalCount);
//         }
//     }
// }