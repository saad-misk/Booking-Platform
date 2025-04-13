using BookingPlatform.Application.DTOs.Checkout.Requests;
using BookingPlatform.Application.DTOs.Checkout.Responses;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface ISingleItemCheckoutService
    {
        Task<CheckoutResponse> ProcessSingleCartItemAsync(
                                Guid cartId,
                                Guid cartItemId, 
                                CheckoutRequest request);
    }
}