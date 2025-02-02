using BookingPlatform.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Persistence;
using System.Linq;
using BookingPlatform.Application.Interfaces.Services;

namespace BookingPlatform.Infrastructure.Services.Images
{
    public class ImageService : IImageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageStorageService _imageStorageService;
        private readonly ILogger<ImageService> _logger;

        public ImageService(
            IUnitOfWork unitOfWork,
            IImageStorageService imageStorageService,
            ILogger<ImageService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Sets the thumbnail for a hotel or city.
        /// </summary>
        public async Task<string> SetThumbnailAsync(Guid entityId, ImageEntityType entityType, IFormFile image)
        {
            string imageUrl = await _imageStorageService.UploadImageAsync(image) ?? string.Empty;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                switch (entityType)
                {
                    case ImageEntityType.Hotel:
                        var hotel = await _unitOfWork.GetRepository<Hotel>().GetByIdAsync(entityId);
                        if (hotel == null) throw new NotFoundException("Hotel not found");

                        await DeleteImageIfExists(hotel.Thumbnail?.Url!);
                        hotel.Thumbnail ??= new Image();
                        hotel.Thumbnail.Url = imageUrl;
                        break;

                    case ImageEntityType.City:
                        var city = await _unitOfWork.GetRepository<City>().GetByIdAsync(entityId);
                        if (city == null) throw new NotFoundException("City not found");

                        await DeleteImageIfExists(city.Thumbnail?.Url!);
                        city.Thumbnail ??= new Image();
                        city.Thumbnail.Url = imageUrl;
                        break;

                    default:
                        throw new ArgumentException("Thumbnails can only be set for hotels or cities.");
                }

                await _unitOfWork.CommitAsync();
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting thumbnail for entity {EntityId} of type {EntityType}.", entityId, entityType);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Adds an image to a hotel, city, or room.
        /// </summary>
        public async Task<string> AddImageAsync(Guid entityId, ImageEntityType entityType, IFormFile image)
        {
            string imageUrl = await _imageStorageService.UploadImageAsync(image) ?? string.Empty;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                switch (entityType)
                {
                    case ImageEntityType.Hotel:
                        var hotel = await _unitOfWork.GetRepository<Hotel>().GetByIdAsync(entityId);
                        if (hotel == null) throw new NotFoundException("Hotel not found");

                        hotel.Gallery ??= new List<Image>();
                        hotel.Gallery.Add(new Image { Url = imageUrl });
                        break;

                    case ImageEntityType.City:
                        var city = await _unitOfWork.GetRepository<City>().GetByIdAsync(entityId);
                        if (city == null) throw new NotFoundException("City not found");

                        city.Images ??= new List<Image>();
                        city.Images.Add(new Image { Url = imageUrl });
                        break;

                    case ImageEntityType.Room:
                        var room = await _unitOfWork.GetRepository<Room>().GetByIdAsync(entityId);
                        if (room == null) throw new NotFoundException("Room not found");

                        room.Images ??= new List<Image>();
                        room.Images.Add(new Image { Url = imageUrl });
                        break;

                    default:
                        throw new ArgumentException("Invalid entity type.");
                }

                await _unitOfWork.CommitAsync();
                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding image for entity {EntityId} of type {EntityType}.", entityId, entityType);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Deletes an image from a hotel, city, or room.
        /// </summary>
        public async Task<bool> DeleteImageAsync(Guid entityId, ImageEntityType entityType, string imageUrl)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                bool deleted = false;

                switch (entityType)
                {
                    case ImageEntityType.Hotel:
                        var hotel = await _unitOfWork.GetRepository<Hotel>().GetByIdAsync(entityId);
                        if (hotel == null) throw new NotFoundException("Hotel not found");

                        var hotelImage = hotel.Gallery?.FirstOrDefault(img => img.Url == imageUrl);
                        if (hotelImage != null)
                        {
                            hotel.Gallery!.Remove(hotelImage);
                            deleted = true;
                        }
                        break;

                    case ImageEntityType.City:
                        var city = await _unitOfWork.GetRepository<City>().GetByIdAsync(entityId);
                        if (city == null) throw new NotFoundException("City not found");

                        var cityImage = city.Images?.FirstOrDefault(img => img.Url == imageUrl);
                        if (cityImage != null)
                        {
                            city.Images!.Remove(cityImage);
                            deleted = true;
                        }
                        break;

                    case ImageEntityType.Room:
                        var room = await _unitOfWork.GetRepository<Room>().GetByIdAsync(entityId);
                        if (room == null) throw new NotFoundException("Room not found");

                        var roomImage = room.Images?.FirstOrDefault(img => img.Url == imageUrl);
                        if (roomImage != null)
                        {
                            room.Images!.Remove(roomImage);
                            deleted = true;
                        }
                        break;

                    default:
                        throw new ArgumentException("Invalid entity type.");
                }

                if (deleted)
                {
                    await _imageStorageService.DeleteImageAsync(imageUrl);
                    await _unitOfWork.CommitAsync();
                }

                return deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image for entity {EntityId} of type {EntityType}.", entityId, entityType);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        private async Task DeleteImageIfExists(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _imageStorageService.DeleteImageAsync(imageUrl);
            }
        }
    }
}