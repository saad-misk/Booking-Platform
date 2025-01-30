using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using BookingPlatform.Domain.Interfaces.Repositories;

namespace BookingPlatform.Infrastructure.Services.Admin
{
    public class AdminHotelsService : IAdminHotelsService
    {
        IHotelsRepository _hotelsRepository;
        IUnitOfWork _unitOfWork;
        public AdminHotelsService(IHotelsRepository hotelsRepository, IUnitOfWork unitOfWork)
        {
            _hotelsRepository = hotelsRepository;
            _unitOfWork = unitOfWork;
        } 

        public async Task<Hotel> GetHotelByIdAsync(Guid hotelId)
        {
            var city = await _hotelsRepository.GetByIdAsync(hotelId)
                ?? throw new NotFoundException("Hotel not found");
                
            return city;
        }

        public async Task<HotelDetailsResponse> CreateHotelAsync(CreateHotelRequest request)
        {
            var hotel = new Hotel{
                HotelId = Guid.NewGuid(),
                CityId = request.CityId,
                Name = request.Name,
                Description = request.Description,
                ReviewsRating = 0,
                StarRating = request.StarRating,
            };
            
            await _hotelsRepository.AddAsync(hotel);
            await _unitOfWork.CommitAsync();
            
            return MapToHotelResponse(hotel);
        }

        public async Task UpdateHotelAsync(UpdateHotelRequest request)
        {
            var hotel = await _hotelsRepository.GetByIdAsync(request.HotelId) 
                ?? throw new NotFoundException("Hotel not found");

            UpdateHotelProperties(hotel, request);
            
            _hotelsRepository.Update(hotel);
            await _unitOfWork.CommitAsync();
        }

        public async Task DeleteHotelAsync(Guid hotelId)
        {
            var hotel = await _hotelsRepository.GetByIdAsync(hotelId);
            if (hotel == null) throw new NotFoundException("Hotel not found");
            
            await _hotelsRepository.DeleteAsync(hotelId);
            await _unitOfWork.CommitAsync();
        }

        private void UpdateHotelProperties(Hotel hotel, UpdateHotelRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                hotel.Name = request.Name.Trim();
            }
            
            if (request.Description != null)
            {
                hotel.Description = request.Description;
            }
            
            if (request.StarRating != 0)
            {
                hotel.StarRating = ValidateStarRating(request.StarRating);
            }
            
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                hotel.PhoneNumber = ValidatePhoneNumber(request.PhoneNumber);
            }
        }

        private static int ValidateStarRating(int rating)
        {
            if (rating < 1 || rating > 5)
            {
                throw new BadRequestException("Star rating must be between 1 and 5");
            }
            return rating;
        }

        private static string ValidatePhoneNumber(string phoneNumber)
        {
            var cleanedNumber = new string(phoneNumber.Where(char.IsDigit).ToArray());
            
            if (cleanedNumber.Length < 7 || cleanedNumber.Length > 15)
            {
                throw new BadRequestException("Invalid phone number format");
            }
            
            return cleanedNumber;
        }

        private HotelDetailsResponse MapToHotelResponse(Hotel hotel)
        {
            return new HotelDetailsResponse
            {
                HotelId = hotel.HotelId,
                Name = hotel.Name,
                StarRating = hotel.StarRating,
                Description = hotel.Description ?? string.Empty,
                City = hotel.City.Name,
                ReviewsRating = hotel.ReviewsRating,
            };
        }
    }
}