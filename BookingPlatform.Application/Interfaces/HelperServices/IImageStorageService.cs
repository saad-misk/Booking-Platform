using Microsoft.AspNetCore.Http;

namespace BookingPlatform.Application.Interfaces.HelperServices
{
    public interface IImageStorageService
    {
        Task<string?> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string key);
        Task<bool> ImageExistsAsync(string key);
    }
}