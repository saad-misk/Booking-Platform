using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BookingPlatform.Infrastructure.Services.Cities
{
    public class CitiesService : ICitiesService
    {
        private readonly ICitiesRepository _citiesRepository;
        private readonly ILogger<CitiesService> _logger;

        public CitiesService(ICitiesRepository citiesRepository, ILogger<CitiesService> logger)
        {
            _citiesRepository = citiesRepository;
            _logger = logger;
        }

        public async Task<List<TrendingDestinationResponse>> GetTrendingDestinations(
            CancellationToken cancellationToken = default)
        {
            const string operationName = nameof(GetTrendingDestinations);
            _logger.LogInformation("Starting {OperationName} operation", operationName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var trendingDestinations = await _citiesRepository.GetTrendingDestinationsAsync(cancellationToken);

                _logger.LogDebug("Retrieved {Count} trending destinations", trendingDestinations?.Count ?? 0);

                if (trendingDestinations == null || trendingDestinations.Count == 0)
                {
                    _logger.LogWarning("No trending destinations found");
                    return [];
                }

                var response = trendingDestinations.Select(c => new TrendingDestinationResponse
                {
                    CityId = c.CityId,
                    CityName = c.Name,
                    Country = c.Country,
                    BookingsCount = c.BookingsCount,
                }).ToList();

                _logger.LogInformation("{OperationName} completed in {ElapsedMilliseconds}ms", 
                                       operationName, stopwatch.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in {OperationName}", operationName);
                throw; // Preserve original stack trace
            }
        }
    }
}