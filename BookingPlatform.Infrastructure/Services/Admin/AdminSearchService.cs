using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.DTOs.Search;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace BookingPlatform.Infrastructure.Services.Admin
{
    public class AdminSearchService : IAdminSearchService
    {
        private readonly IRepository<Hotel> _hotelsRepo;
        private readonly IRepository<City> _citiesRepo;
        private readonly IRepository<Room> _roomsRepo;

        public AdminSearchService(
            IRepository<Hotel> hotelsRepo,
            IRepository<City> citiesRepo,
            IRepository<Room> roomsRepo)
        {
            _hotelsRepo = hotelsRepo;
            _citiesRepo = citiesRepo;
            _roomsRepo = roomsRepo;
        }

        public async Task<SearchResult<HotelSearchResult>> SearchHotelsAsync(HotelSearchCriteria criteria,
                    CancellationToken cancellationToken = default)
        {
            Expression<Func<Hotel, bool>> filter = h =>
                (string.IsNullOrEmpty(criteria.Name) || h.Name.Contains(criteria.Name)) &&
                (!criteria.CityId.HasValue || h.CityId == criteria.CityId) &&
                (!criteria.Rating.HasValue || h.ReviewsRating == criteria.Rating);

            var result = await QueryWithPaging(
                _hotelsRepo,
                filter,
                criteria.PageNumber,
                criteria.PageSize,
                h => new HotelSearchResult
                {
                    HotelId = h.HotelId,
                    Name = h.Name,
                    CityId = h.CityId,
                    CityName = h.City.Name,
                    Rating = h.ReviewsRating
                },
                includes: [h => h.City ],
                cancellationToken);

            return result;
        }

        public async Task<SearchResult<CitySearchResult>> SearchCitiesAsync(CitySearchCriteria criteria,
                    CancellationToken cancellationToken = default)
        {
            Expression<Func<City, bool>> filter = c =>
                string.IsNullOrEmpty(criteria.Name) || c.Name.Contains(criteria.Name);

            var result = await QueryWithPaging(
                _citiesRepo,
                filter,
                criteria.PageNumber,
                criteria.PageSize,
                c => new CitySearchResult
                {
                    CityId = c.CityId,
                    Name = c.Name,
                    HotelCount = c.Hotels.Count
                },
                includes: [c => c.Hotels],
                cancellationToken);

            return result;
        }

        public async Task<SearchResult<RoomSearchResult>> SearchRoomsAsync(RoomSearchCriteria criteria,
                    CancellationToken cancellationToken = default)
        {
            Expression<Func<Room, bool>> filter = r =>
                (!criteria.HotelId.HasValue || r.HotelId == criteria.HotelId) &&
                (!criteria.MinPrice.HasValue || r.PricePerNight >= criteria.MinPrice) &&
                (!criteria.MaxPrice.HasValue || r.PricePerNight <= criteria.MaxPrice);

            var result = await QueryWithPaging(
                _roomsRepo,
                filter,
                criteria.PageNumber,
                criteria.PageSize,
                r => new RoomSearchResult
                {
                    RoomId = r.RoomId,
                    Number = r.Number,
                    HotelId = r.HotelId,
                    HotelName = r.Hotel.Name,
                    Price = r.PricePerNight
                },
                includes: [r => r.Hotel],
                cancellationToken); 

            return result;
        }

        private async Task<SearchResult<TResponse>> QueryWithPaging<TEntity, TResponse>(
            IRepository<TEntity> repo,
            Expression<Func<TEntity, bool>> filter,
            int pageNumber,
            int pageSize,
            Func<TEntity, TResponse> selector,
            IEnumerable<Expression<Func<TEntity, object>>>? includes = null,
            CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var totalCount = await repo.CountAsync(filter, cancellationToken);
            var items = await repo.GetAsync(
                filter: filter,
                includes: includes,
                skip: (pageNumber - 1) * pageSize,
                take: pageSize,
                cancellationToken: cancellationToken);

            return new SearchResult<TResponse>
            {
                Items = items.Select(selector).ToList(),
                TotalCount = totalCount
            };
        }
    }
}