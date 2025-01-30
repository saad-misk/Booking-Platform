using BookingPlatform.Application.DTOs.Checkout;
using BookingPlatform.Domain.Enums;

namespace BookingPlatform.Application.Interfaces.HelperServices
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, PaymentMethod method);
        Task RefundPaymentAsync(string TransactionId);
    }
}