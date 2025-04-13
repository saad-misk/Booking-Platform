using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Interfaces.Repositories;
using BookingPlatform.Infrastructure.Persistence.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Persistence.Repositories
{
    public class HotelsRepository : Repository<Hotel>, IHotelsRepository
    {
        public HotelsRepository(AppDbContext context, ILogger<HotelsRepository> logger) 
            : base(context, logger) { }

        public async Task<(List<Hotel> Hotels, int TotalCount)> SearchHotelAsync(
            decimal? minPrice,
            decimal? maxPrice,
            int? starRating,
            RoomType? roomType,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(pageNumber, 1); // Ensure pageNumber is at least 1

            var query = _context.Hotels
                .Include(h => h.Gallery)
                .Include(h => h.Rooms)
                .AsQueryable();

            query = ApplyFilters(query, minPrice, maxPrice, starRating, roomType);

            // Get total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting & pagination
            var hotels = await query
                .OrderBy(h => h.Rooms.Min(r => r.PricePerNight))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (hotels, totalCount);
        }

        public async Task<Hotel?> GetHotelDetailsAsync(
            Guid hotelId, 
            CancellationToken cancellationToken = default)
        {
            return await _context.Hotels
                .Include(h => h.Gallery)
                .Include(h => h.Gallery)
                .Include(h => h.Rooms)
                    .ThenInclude(r => r.Images)
                .Include(h => h.Reviews)
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.HotelId == hotelId, cancellationToken);
        }

        public async Task<List<Hotel>> GetFeaturedDealsAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Hotels
                .Include(h => h.Rooms)
                .Include(h => h.City)
                .OrderBy(h => h.Rooms.Min(r => r.PricePerNight))
                .Take(5)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        private static IQueryable<Hotel> ApplyFilters(
            IQueryable<Hotel> query,
            decimal? minPrice,
            decimal? maxPrice,
            int? starRating,
            RoomType? roomType)
        {
            if (starRating.HasValue)
                query = query.Where(h => h.StarRating == starRating);

            if (roomType.HasValue)
                query = query.Where(h => h.Rooms.Any(r => r.RoomClass == roomType));

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                query = query.Where(h => h.Rooms.Any(r =>
                    (!minPrice.HasValue || r.PricePerNight >= minPrice) &&
                    (!maxPrice.HasValue || r.PricePerNight <= maxPrice)));
            }

            return query;
        }
    }
}