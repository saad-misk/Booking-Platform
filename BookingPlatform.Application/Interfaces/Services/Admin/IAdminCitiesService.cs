using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Domain.Entities;

namespace BookingPlatform.Application.Interfaces.Services.Admin
{
    public interface IAdminCitiesService : IAdminService
    {
        Task UpdateCityAsync(UpdateCityRequest request, CancellationToken cancellationToken = default);
        Task<CityResponse> CreateCityAsync(CreateCityRequest request, CancellationToken cancellationToken = default);
        Task DeleteCityAsync(Guid cityId, CancellationToken cancellationToken = default);
        Task<List<CityResponse>> GetAllCitiesAsync(CancellationToken cancellationToken = default);
        Task<City> GetCityByIdAsync(Guid cityId, CancellationToken cancellationToken = default);
    }
}