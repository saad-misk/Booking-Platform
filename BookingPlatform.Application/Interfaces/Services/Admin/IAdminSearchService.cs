
using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.DTOs.Search;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminSearchService : IAdminService
    {
        Task<SearchResult<HotelSearchResult>> SearchHotelsAsync(HotelSearchCriteria criteria, CancellationToken cancellationToken = default);
        Task<SearchResult<CitySearchResult>> SearchCitiesAsync(CitySearchCriteria criteria, CancellationToken cancellationToken = default);
        Task<SearchResult<RoomSearchResult>> SearchRoomsAsync(RoomSearchCriteria criteria, CancellationToken cancellationToken = default);
    }
}