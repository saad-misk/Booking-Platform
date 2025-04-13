using BookingPlatform.Application.DTOs.Cities.Responses;
using System.Threading;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface ICitiesService
    {
        Task<List<TrendingDestinationResponse>> GetTrendingDestinations(
            CancellationToken cancellationToken = default);
    }
}