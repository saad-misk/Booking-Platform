using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.Interfaces.HelperServices.Payment
{
    public interface IPaymentProviderService
    {
        Task<PaymentResult> ProcessPaymentAsync(
            decimal amount,
            PaymentMethod method,
            bool confirmImmediately = true,
            string paymentMethodId = null!);
        Task<PaymentResult> RefundPaymentAsync(string transactionId);
    }
}