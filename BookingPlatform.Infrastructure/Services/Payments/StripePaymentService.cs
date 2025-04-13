using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Application.Interfaces.HelperServices.Payment;
using BookingPlatform.Domain.Enums;
using Microsoft.Extensions.Logging;
using DomainPaymentMethod = BookingPlatform.Domain.Enums.PaymentMethod;
using Stripe;
using BookingPlatform.Domain.Models;
using Microsoft.Extensions.Options;

namespace BookingPlatform.Infrastructure.Services.Payments
{
    public class StripePaymentService : IPaymentProviderService
    {
        private readonly StripeClient _stripeClient;
        private readonly ILogger<StripePaymentService> _logger;
        private readonly StripeSettings _stripeSettings;

        public StripePaymentService(IOptions<StripeSettings> stripeOptions, ILogger<StripePaymentService> logger)
        {
            _stripeSettings = stripeOptions.Value ?? throw new ArgumentNullException(nameof(stripeOptions));

            if (string.IsNullOrWhiteSpace(_stripeSettings.SecretKey))
            {
                throw new ArgumentException("Stripe Secret Key is not configured properly.");
            }

            _stripeClient = new StripeClient(_stripeSettings.SecretKey);
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            decimal amount,
            DomainPaymentMethod method,
            bool confirmImmediately = true,
            string paymentMethodId = null!)
        {
            try
            {
                var paymentMethodType = GetStripePaymentMethod(method);
                if (string.IsNullOrWhiteSpace(paymentMethodType))
                {
                    return new PaymentResult
                    {
                        Status = PaymentStatus.Failed,
                        ErrorMessage = "Unsupported payment method."
                    };
                }

                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = _stripeSettings.Currency,
                    PaymentMethodTypes = new List<string> { paymentMethodType },
                    Metadata = new Dictionary<string, string>
                    {
                        { "BookingReference", Guid.NewGuid().ToString() }
                    },
                    Confirm = confirmImmediately
                };

                if (confirmImmediately && !string.IsNullOrWhiteSpace(paymentMethodId))
                {
                    options.PaymentMethod = paymentMethodId;
                }

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

        public async Task<PaymentResult> RefundPaymentAsync(string transactionId)
        {
            if (string.IsNullOrWhiteSpace(transactionId))
            {
                return new PaymentResult
                {
                    Status = PaymentStatus.Failed,
                    ErrorMessage = "Transaction ID cannot be null or empty."
                };
            }

            try
            {
                var refundOptions = new RefundCreateOptions
                {
                    PaymentIntent = transactionId
                };

                var refundService = new RefundService(_stripeClient);
                var refund = await refundService.CreateAsync(refundOptions);

                return new PaymentResult
                {
                    TransactionId = refund.Id,
                    Status = refund.Status == "succeeded" ? PaymentStatus.Confirmed : PaymentStatus.Pending
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe refund failed for transaction ID: {TransactionId}", transactionId);
                return new PaymentResult
                {
                    Status = PaymentStatus.Failed,
                    ErrorMessage = ex.Message
                };
            }
        }

        private string GetStripePaymentMethod(DomainPaymentMethod method)
        {
            return method switch
            {
                DomainPaymentMethod.Card => "card",
                DomainPaymentMethod.BankTransfer => "bank_transfer",
                DomainPaymentMethod.PayPal => "paypal",
                _ => null!
            };
        }
    }
}