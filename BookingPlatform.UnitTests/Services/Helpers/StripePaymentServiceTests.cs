using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Domain.Enums;
using BookingPlatform.Domain.Models;
using BookingPlatform.Infrastructure.Services.Payments;
using DomainPaymentMethod = BookingPlatform.Domain.Enums.PaymentMethod;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Stripe;

namespace BookingPlatform.Infrastructure.Tests.Services.Payments
{
    public class StripePaymentServiceTests
    {
        private readonly Mock<IOptions<StripeSettings>> _stripeOptionsMock;
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;

        public StripePaymentServiceTests()
        {
            _stripeOptionsMock = new Mock<IOptions<StripeSettings>>();
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
        }

        [Fact]
        public void Constructor_WhenSecretKeyMissing_ThrowsArgumentException()
        {
            // Arrange
            var stripeSettings = new StripeSettings { SecretKey = null };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(
                () => new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object));
            Assert.Contains("Stripe Secret Key", ex.Message);
        }

        [Fact]
        public async Task ProcessPaymentAsync_UnsupportedPaymentMethod_ReturnsFailedResult()
        {
            // Arrange: valid settings are provided.
            var stripeSettings = new StripeSettings { SecretKey = "sk_test_valid", Currency = "usd" };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);
            var service = new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object);

            // Use an unsupported payment method by passing an enum value not handled by our switch.
            DomainPaymentMethod unsupportedMethod = (DomainPaymentMethod)999;

            // Act
            PaymentResult result = await service.ProcessPaymentAsync(100m, unsupportedMethod);

            // Assert
            Assert.Equal(PaymentStatus.Failed, result.Status);
            Assert.Equal("Unsupported payment method.", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessPaymentAsync_WhenStripeException_ReturnsFailedResult()
        {
            // Arrange: using an invalid key forces Stripe to throw.
            var stripeSettings = new StripeSettings { SecretKey = "invalid_key", Currency = "usd" };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);
            var service = new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object);

            // Act
            PaymentResult result = await service.ProcessPaymentAsync(100m, DomainPaymentMethod.Card);

            // Assert: result should indicate failure.
            Assert.Equal(PaymentStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);

            // Verify that a log entry was made containing "Stripe payment failed".
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stripe payment failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RefundPaymentAsync_InvalidTransactionId_ReturnsFailedResult(string transactionId)
        {
            // Arrange: using valid settings.
            var stripeSettings = new StripeSettings { SecretKey = "sk_test_valid", Currency = "usd" };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);
            var service = new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object);

            // Act
            PaymentResult result = await service.RefundPaymentAsync(transactionId);

            // Assert: expect a failure result with the appropriate message.
            Assert.Equal(PaymentStatus.Failed, result.Status);
            Assert.Equal("Transaction ID cannot be null or empty.", result.ErrorMessage);
        }

        [Fact]
        public async Task RefundPaymentAsync_WhenStripeException_ReturnsFailedResult()
        {
            // Arrange: using an invalid key to force a Stripe exception.
            var stripeSettings = new StripeSettings { SecretKey = "invalid_key", Currency = "usd" };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);
            var service = new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object);
            string transactionId = "invalid_tx_123";

            // Act
            PaymentResult result = await service.RefundPaymentAsync(transactionId);

            // Assert: expect a failure result.
            Assert.Equal(PaymentStatus.Failed, result.Status);
            Assert.NotNull(result.ErrorMessage);

            // Verify that a log entry was made containing the transaction id.
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(transactionId)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()!),
                Times.Once);
        }

        // NOTE: This is an integration test that requires a valid test key and a proper test payment method (e.g. "pm_card_visa").
        // Without supplying a payment method to the PaymentIntent (which is required when confirming immediately),
        // Stripe will not create a PaymentIntent with a valid Transaction ID.
        // For now, we mark this test to be skipped.
        [Fact(Skip = "Integration test: requires valid test payment method id and proper Stripe configuration")]
        public async Task ProcessPaymentAsync_ReturnsConfirmedPaymentResult_OnSucceededPaymentIntent()
        {
            // Arrange:
            var stripeSettings = new StripeSettings 
            { 
                SecretKey = "sk_test_...", 
                Currency = "usd" 
            };
            _stripeOptionsMock.Setup(x => x.Value).Returns(stripeSettings);
            var service = new StripePaymentService(_stripeOptionsMock.Object, _loggerMock.Object);
            decimal amount = 10.00m;

            // Act:
            PaymentResult result = await service.ProcessPaymentAsync(amount, DomainPaymentMethod.Card);

            // Assert:
            Assert.NotNull(result);
            // The transaction ID should not be empty on a successful PaymentIntent.
            Assert.False(string.IsNullOrWhiteSpace(result.TransactionId));
            Assert.Equal(PaymentStatus.Confirmed, result.Status);
        }
    }
}