using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminHotelsService : IAdminService
    {
        Task UpdateHotelAsync(UpdateHotelRequest request);
        Task<Hotel> GetHotelByIdAsync(Guid hotelId);
        Task<HotelDetailsResponse> CreateHotelAsync(CreateHotelRequest request);
        Task DeleteHotelAsync(Guid hotelId);
    }
}