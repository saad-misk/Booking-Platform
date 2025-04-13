using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using System.Threading;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface IHotelsService
    {
        Task<List<HotelsResponse>> SearchHotelsAsync(
            HotelSearchCriteria request, 
            CancellationToken cancellationToken = default);

        Task<HotelDetailsResponse> GetHotelDetails(
            Guid hotelId, 
            CancellationToken cancellationToken = default);
    }
}