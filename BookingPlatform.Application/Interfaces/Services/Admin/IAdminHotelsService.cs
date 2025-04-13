using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminHotelsService : IAdminService
    {
        Task UpdateHotelAsync(UpdateHotelRequest request, CancellationToken cancellationToken = default);
        Task<Hotel> GetHotelByIdAsync(Guid hotelId, CancellationToken cancellationToken = default);
        Task<HotelDetailsResponse> CreateHotelAsync(CreateHotelRequest request, CancellationToken cancellationToken = default);
        Task<List<HotelDetailsResponse>> GetAllHotelsAsync(CancellationToken cancellationToken = default);
        Task DeleteHotelAsync(Guid hotelId, CancellationToken cancellationToken = default);
    }
}