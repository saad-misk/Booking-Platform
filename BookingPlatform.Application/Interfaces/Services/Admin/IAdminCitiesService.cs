using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminCitiesService : IAdminService
    {
        Task UpdateCityAsync(UpdateCityRequest request);
        Task<CityResponse> CreateCityAsync(CreateCityRequest request);
        Task DeleteCityAsync(Guid cityId);
        Task<City> GetCityByIdAsync(Guid cityId);
        
    }
}