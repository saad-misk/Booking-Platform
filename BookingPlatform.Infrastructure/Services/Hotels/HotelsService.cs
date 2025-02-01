using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.DTOs.Reviews.Responses;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BookingPlatform.Infrastructure.Services.Hotels
{
    public class HotelsService : IHotelsService
    {
        private readonly IHotelsRepository _hotelRepository;
        private readonly ILogger<HotelsService> _logger;

        public HotelsService(
            IHotelsRepository repository,
            ILogger<HotelsService> logger)
        {
            _hotelRepository = repository;
            _logger = logger;
        }

        public async Task<List<HotelsResponse>> SearchHotelsAsync(
            HotelSearchCriteria request,
            CancellationToken cancellationToken = default)
        {
            const string operationName = nameof(SearchHotelsAsync);
            _logger.LogInformation("Starting {OperationName} operation", operationName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _hotelRepository.SearchHotelAsync(
                    request.MinPrice, 
                    request.MaxPrice,
                    request.StarRating,
                    request.RoomType,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                if (result.Hotels == null || result.Hotels.Count == 0)
                {
                    _logger.LogWarning("No hotels found matching search criteria");
                    return [];
                }

                var response = result.Hotels.Select(MapToResponse).ToList();

                _logger.LogInformation("{OperationName} completed in {ElapsedMilliseconds}ms", 
                                       operationName, stopwatch.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName}", operationName);
                throw;
            }
        }

        public async Task<HotelDetailsResponse> GetHotelDetails(Guid hotelId, 
            CancellationToken cancellationToken = default)
        {
            var hotel = await _hotelRepository.GetByIdAsync(hotelId, cancellationToken);
            if (hotel == null) 
                throw new NotFoundException("Hotel not found");

            return new HotelDetailsResponse
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                StarRating = hotel.StarRating,
                Description = hotel.Description ?? string.Empty,
                Latitude = hotel.Latitude,
                Longitude = hotel.Longitude,
                ReviewsRating = hotel.Reviews.Any() ? 
                    hotel.Reviews.Average(r => r.Rating) : 0,
                GalleryUrls = hotel.Gallery.Select(g => g.Url).ToList(),
                Rooms = hotel.Rooms.Select(r => new RoomResponse
                {
                    RoomId = r.RoomId,
                    RoomType = r.RoomClass,
                    PricePerNight = r.PricePerNight,
                    Capacity = r.Capacity,
                }).ToList(),
                Reviews = hotel.Reviews.Select(r => new ReviewResponse
                {
                    Author = r.Guest.FirstName,
                    Rating = r.Rating,
                    Comment = r.Content,
                    ReviewDate = r.CreatedAtUtc
                }).ToList()
            };
        }

        public async Task<List<FeaturedDealResponse>> GetFeaturedDeals(
            CancellationToken cancellationToken = default)
        {
            const string operationName = nameof(GetFeaturedDeals);
            _logger.LogInformation("Starting {OperationName} operation", operationName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var featuredDeals = await _hotelRepository.GetFeaturedDealsAsync(cancellationToken);

                if (featuredDeals == null || featuredDeals.Count == 0)
                {
                    _logger.LogWarning("No featured deals found");
                    return [];
                }

                var response = featuredDeals.Select(hotel => new FeaturedDealResponse
                {
                    Id = hotel.HotelId,
                    Name = hotel.Name,
                    City = hotel.City.Name,
                    ThumbnailUrl = hotel.Thumbnail?.Url ?? string.Empty,
                    OriginalPrice = hotel.Rooms.FirstOrDefault()?.PricePerNight ?? 0,
                    DiscountedPrice = (hotel.Rooms.FirstOrDefault()?.PricePerNight ?? 0) * 0.9m,
                    Rating = hotel.ReviewsRating
                }).ToList();

                _logger.LogInformation("{OperationName} completed in {ElapsedMilliseconds}ms", 
                                       operationName, stopwatch.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName}", operationName);
                throw;
            }
        }

        private static HotelsResponse MapToResponse(Hotel hotel)
        {
            return new HotelsResponse
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                Description = hotel.Description ?? string.Empty,
                StarRating = hotel.StarRating,
                ThumbnailUrl = hotel.Thumbnail?.Url ?? string.Empty,
                StartingPrice = hotel.Rooms.FirstOrDefault()?.PricePerNight ?? 0,
                AvailableRoomTypes = hotel.Rooms
                    .Select(r => r.RoomClass.ToString())
                    .Distinct()
                    .ToList()
            };
        }
    }
}