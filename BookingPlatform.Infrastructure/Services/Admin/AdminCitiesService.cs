using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;

namespace BookingPlatform.Infrastructure.Services.Admin
{
    public class AdminCitiesService : IAdminCitiesService
    {
        ICitiesRepository _citiesRepository;
        IUnitOfWork _unitOfWork;

        public AdminCitiesService(ICitiesRepository citiesRepository, IUnitOfWork unitOfWork)
        {
            _citiesRepository = citiesRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<City> GetCityByIdAsync(Guid cityId)
        {
            var city = await _citiesRepository.GetByIdAsync(cityId)
                ?? throw new NotFoundException("City not found");
                
            return city;
        }

        public async Task<CityResponse> CreateCityAsync(CreateCityRequest request)
        {
            var city = new City
            {
                CityId = Guid.NewGuid(),
                Name = request.Name,
                Country = request.Country,
                PostOffice = request.PostOffice,
                CityCode = request.CityCode,
            };

            await _citiesRepository.AddAsync(city);
            await _unitOfWork.CommitAsync();

            return MapToCityResponse(city);
        }

        private CityResponse MapToCityResponse(City city)
        {
            return new CityResponse
            {
                CityId = city.CityId,
                Name = city.Name,
                Country = city.Country,
                PostOffice = city.PostOffice,
                CityCode = city.CityCode,
                HotelsCount = city.Hotels.Count,
                BookingsCount = city.BookingsCount,
                Thumbnail = city.Images.FirstOrDefault(i => i.IsThumbnail)
            };
        }

        public async Task UpdateCityAsync(UpdateCityRequest request)
        {
            var city = await _citiesRepository.GetByIdAsync(request.CityId) 
                ?? throw new NotFoundException("City not found");

            UpdateCityProperties(city, request);
            
            _citiesRepository.Update(city);
            await _unitOfWork.CommitAsync();
        }

        private void UpdateCityProperties(City city, UpdateCityRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                city.Name = request.Name.Trim();
            }
            
            if (!string.IsNullOrWhiteSpace(request.Country))
            {
                city.Country = ValidateCountryCode(request.Country.Trim());
            }
            
            if (!string.IsNullOrWhiteSpace(request.PostOffice))
            {
                city.PostOffice = ValidatePostalCode(request.PostOffice.Trim());
            }
            
            if (!string.IsNullOrWhiteSpace(request.CityCode))
            {
                city.CityCode = ValidateCityCodeFormat(request.CityCode.Trim());
            }
        }

        private static string ValidateCountryCode(string countryCode)
        {
            if (countryCode.Length != 2 || !countryCode.All(char.IsLetter))
            {
                throw new BadRequestException("Invalid ISO country code format");
            }
            return countryCode.ToUpper();
        }

        private static string ValidateCityCodeFormat(string cityCode)
        {
            if (cityCode.Length != 3 || !cityCode.All(char.IsLetterOrDigit))
            {
                throw new BadRequestException("City code must be 3 alphanumeric characters");
            }
            return cityCode.ToUpper();
        }

        private static string ValidatePostalCode(string postalCode)
        {
            if (postalCode.Length < 4 || postalCode.Length > 10)
            {
                throw new BadRequestException("Postal code must be between 4-10 characters");
            }
            return postalCode;
        }
        public async Task DeleteCityAsync(Guid cityId)
        {
            var city = await _citiesRepository.GetByIdAsync(cityId);
            if (city == null) throw new NotFoundException("City not found");    
            
            await _citiesRepository.DeleteAsync(cityId);
            await _unitOfWork.CommitAsync();
        }
    }
}