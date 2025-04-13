using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Domain.Interfaces.Repositories
{
    public interface IHotelsRepository : IRepository<Hotel>
    {
        public Task<(List<Hotel> Hotels, int TotalCount)> SearchHotelAsync(
            decimal? MinPrice,
            decimal? MaxPrice,
            int? StarRating,
            RoomType? RoomTypes,
            int PageNumber,
            int PageSize,
            CancellationToken cancellationToken = default);
        
        Task<List<Hotel>> GetFeaturedDealsAsync(CancellationToken cancellationToken = default);
    }
}