using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using BookingPlatform.Application.Interfaces.HelperServices;

namespace BookingPlatform.Infrastructure.Services.Images.B2CloudStorage
{
    public class B2ImageStorageService : IImageStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly ILogger<B2ImageStorageService> _logger;

        public B2ImageStorageService(IAmazonS3 s3Client, IConfiguration config, ILogger<B2ImageStorageService> logger)
        {
            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
            _bucketName = config["B2:BucketName"] ?? throw new ArgumentNullException("B2:BucketName is missing in configuration.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads an image to the Backblaze B2 bucket.
        /// </summary>
        public async Task<string?> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Attempted to upload an empty file.");
                return null;
            }

            string key = $"images/{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            try
            {
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType,
                };

                await _s3Client.PutObjectAsync(request);
                 return $"https://f003.backblazeb2.com/file/{_bucketName}/{key}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image: {FileName}", file.FileName);
                return null;
            }
        }

        /// <summary>
        /// Deletes an image from the Backblaze B2 bucket.
        /// </summary>
        public async Task<bool> DeleteImageAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("DeleteImageAsync was called with an empty key.");
                return false;
            }

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.DeleteObjectAsync(request);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image with key: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// Checks if an image exists in the Backblaze B2 bucket.
        /// </summary>
        public async Task<bool> ImageExistsAsync(string key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check if image exists: {Key}", key);
                return false;
            }
        }
    }
}