using System.Net;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Models;
using BookingPlatform.Infrastructure.Services.HelperServices;
using Microsoft.Extensions.Options;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BookingPlatform.UnitTests.Services.Helpers
{
    public class EmailServiceTests
    {
        private readonly Mock<ISendGridClient> _sendGridClientMock;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private readonly Booking _booking;

        public EmailServiceTests()
        {
            _emailSettings = new EmailSettings { ApiKey = "dummy_api_key", FromEmail = "test@example.com", FromName = "Test Sender" };
            var emailOptions = Options.Create(_emailSettings);
            _sendGridClientMock = new Mock<ISendGridClient>();
            _emailService = new EmailService(emailOptions, _sendGridClientMock.Object);
            _booking = new Booking { User = new User { FirstName = "John" }, RoomId = new Guid(), CheckInDateUtc = DateTime.UtcNow, CheckOutDateUtc = DateTime.UtcNow.AddDays(1), TotalPrice = 100 };
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_ShouldThrowException_WhenEmailIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _emailService.SendBookingConfirmationAsync("", _booking, "path/to/file.pdf"));
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_ShouldThrowException_WhenBookingIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _emailService.SendBookingConfirmationAsync("test@example.com", null!, "path/to/file.pdf"));
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_ShouldThrowException_WhenFilePathIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _emailService.SendBookingConfirmationAsync("test@example.com", _booking, ""));
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_ShouldThrowException_WhenFileDoesNotExist()
        {
            await Assert.ThrowsAsync<FileNotFoundException>(() => _emailService.SendBookingConfirmationAsync("test@example.com", _booking, "nonexistent.pdf"));
        }

        [Fact]
        public async Task SendBookingConfirmationAsync_ShouldSendEmailSuccessfully()
        {
            // Arrange
            var booking = new Booking { User = new User { FirstName = "John" }, RoomId = new Guid(), CheckInDateUtc = DateTime.UtcNow, CheckOutDateUtc = DateTime.UtcNow.AddDays(1), TotalPrice = 100 };
            var filePath = "test_invoice.pdf";
            File.WriteAllText(filePath, "Invoice content");

            var response = new Response(HttpStatusCode.OK, null, null);
            _sendGridClientMock.Setup(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), default))
                .ReturnsAsync(response);

            // Act
            await _emailService.SendBookingConfirmationAsync("test@example.com", _booking, filePath);

            // Assert
            _sendGridClientMock.Verify(client => client.SendEmailAsync(It.IsAny<SendGridMessage>(), default), Times.Once);

            // Cleanup
            File.Delete(filePath);
        }
    }
}