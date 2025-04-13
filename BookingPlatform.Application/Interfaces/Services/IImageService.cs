using BookingPlatform.Domain.Enums;
using Microsoft.AspNetCore.Http;


namespace BookingPlatform.Application.Interfaces.Services
{
    public interface IImageService
    {
        Task<string> SetThumbnailAsync(Guid entityId, ImageEntityType entityType, IFormFile image);
        Task<string> AddImageAsync(Guid entityId, ImageEntityType entityType, IFormFile image);
        Task<bool> DeleteImageAsync(Guid entityId, ImageEntityType entityType, string imageUrl);
    }
}