using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class CitiesRepository : Repository<City>, ICitiesRepository
    {
        private const int TopVisitedCitiesCount = 5; 

        public CitiesRepository(AppDbContext context, ILogger<Repository<City>> logger) 
        : base(context, logger) { }

        public async Task<List<City>> GetTrendingDestinationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .OrderByDescending(c => c.BookingsCount)
                .ThenBy(c => c.Name)
                .Take(TopVisitedCitiesCount)
                .ToListAsync(cancellationToken);
        }
    }
}