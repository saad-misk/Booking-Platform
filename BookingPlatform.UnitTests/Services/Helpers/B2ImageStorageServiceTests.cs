using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using BookingPlatform.Infrastructure.Services.Images.B2CloudStorage;
using Microsoft.AspNetCore.Http;

namespace BookingPlatform.UnitTests.Services.Helpers
{
    public class B2ImageStorageServiceTests
    {
        private readonly Mock<IAmazonS3> _mockS3Client;
        private readonly Mock<ILogger<B2ImageStorageService>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly B2ImageStorageService _service;
        private const string BucketName = "test-bucket";

        public B2ImageStorageServiceTests()
        {
            _mockS3Client = new Mock<IAmazonS3>();
            _mockLogger = new Mock<ILogger<B2ImageStorageService>>();
            _mockConfig = new Mock<IConfiguration>();
            
            _mockConfig.Setup(c => c["B2:BucketName"]).Returns(BucketName);

            _service = new B2ImageStorageService(_mockS3Client.Object, _mockConfig.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task UploadImageAsync_ShouldReturnUrl_WhenUploadSucceeds()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var stream = new MemoryStream(new byte[10]);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.Length).Returns(10);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

            _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                        .ReturnsAsync(new PutObjectResponse());

            // Act
            var result = await _service.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.NotNull(result);
            Assert.Contains(BucketName, result);
        }

        [Fact]
        public async Task UploadImageAsync_ShouldReturnNull_WhenFileIsEmpty()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await _service.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task UploadImageAsync_ShouldReturnNull_WhenUploadFails()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var stream = new MemoryStream(new byte[10]);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.Length).Returns(10);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

            _mockS3Client.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
                        .ThrowsAsync(new Exception("Upload failed"));

            // Act
            var result = await _service.UploadImageAsync(mockFile.Object);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldReturnTrue_WhenDeletionSucceeds()
        {
            // Arrange
            _mockS3Client.Setup(s => s.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
                        .ReturnsAsync(new DeleteObjectResponse());

            // Act
            var result = await _service.DeleteImageAsync("valid-key");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldReturnFalse_WhenDeletionFails()
        {
            // Arrange
            _mockS3Client.Setup(s => s.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
                        .ThrowsAsync(new Exception("Deletion failed"));

            // Act
            var result = await _service.DeleteImageAsync("valid-key");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteImageAsync_ShouldReturnFalse_WhenKeyIsEmpty()
        {
            // Act
            var result = await _service.DeleteImageAsync(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ImageExistsAsync_ShouldReturnTrue_WhenImageExists()
        {
            // Arrange
            _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
                        .ReturnsAsync(new GetObjectMetadataResponse());

            // Act
            var result = await _service.ImageExistsAsync("valid-key");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ImageExistsAsync_ShouldReturnFalse_WhenImageDoesNotExist()
        {
            // Arrange
            _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
                        .ThrowsAsync(new AmazonS3Exception("Not Found") { StatusCode = HttpStatusCode.NotFound });

            // Act
            var result = await _service.ImageExistsAsync("invalid-key");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ImageExistsAsync_ShouldReturnFalse_WhenExceptionOccurs()
        {
            // Arrange
            _mockS3Client.Setup(s => s.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), default))
                        .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _service.ImageExistsAsync("valid-key");

            // Assert
            Assert.False(result);
        }
    }
}