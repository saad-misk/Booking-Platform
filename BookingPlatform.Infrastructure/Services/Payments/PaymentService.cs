using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Application.Interfaces.HelperServices.Payment;
using BookingPlatform.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentProviderService _paymentProvider;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentProviderService paymentProvider, ILogger<PaymentService> logger)
        {
            _paymentProvider = paymentProvider;
            _logger = logger;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod method)
        {
            _logger.LogInformation("Processing payment: Amount {Amount}, Method {Method}", amount, method);
            var result = await _paymentProvider.ProcessPaymentAsync(amount, method);

            if (result.Status != PaymentStatus.Confirmed)
            {
                _logger.LogWarning("Payment not confirmed. TransactionId: {TransactionId}, Status: {Status}",
                    result.TransactionId, result.Status);
            }

            return result;
        }

        public async Task RefundPaymentAsync(string transactionId)
        {
            _logger.LogInformation("Processing refund for TransactionId: {TransactionId}", transactionId);
            await _paymentProvider.RefundPaymentAsync(transactionId);
        }
    }
}