using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface ICitiesRepository : IRepository<City>
    {
        public Task<List<City>> GetTrendingDestinationsAsync(CancellationToken cancellationToken = default);
    }
}