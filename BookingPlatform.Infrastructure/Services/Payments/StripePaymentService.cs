using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Application.Interfaces.HelperServices;
using BookingPlatform.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DomainPaymentMethod = BookingPlatform.Domain.Enums.PaymentMethod;
using Stripe;

namespace BookingPlatform.Infrastructure.Services.Payments
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeClient _stripeClient;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration config, ILogger<StripePaymentService> logger)
        {
            var secretKey = config["Stripe:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                throw new ArgumentException("Stripe Secret Key is not configured in app settings.", nameof(config));
            }

            _stripeClient = new StripeClient(secretKey);
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, DomainPaymentMethod method)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents for Stripe
                    Currency = "usd",
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingReference", Guid.NewGuid().ToString() }
                    }
                };

                var paymentIntentService = new PaymentIntentService(_stripeClient);
                var paymentIntent = await paymentIntentService.CreateAsync(options);

                return new PaymentResult
                {
                    TransactionId = paymentIntent.Id,
                    Status = paymentIntent.Status == "succeeded"
                        ? PaymentStatus.Confirmed
                        : PaymentStatus.Pending
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe payment failed");
                return new PaymentResult
                {
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task RefundPaymentAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));
            }

            try
            {
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = transactionId
                };

                var refundService = new RefundService(_stripeClient);
                await refundService.CreateAsync(refundOptions);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe refund failed for transaction ID: {TransactionId}", transactionId);
                throw new Exception($"Failed to refund transaction: {transactionId}", ex);
            }
        }
    }
}