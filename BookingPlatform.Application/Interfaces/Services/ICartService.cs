using BookingPlatform.Application.DTOs.Cart.Requests;
using BookingPlatform.Application.DTOs.Cart.Responses;

namespace BookingPlatform.Application.Interfaces.Services
{
    public interface ICartService
    {
        Task AddToCartAsync(AddToCartRequest request, Guid cartId);
        Task<CartResponse> GetCartAsync(Guid cartId);
        Task RemoveCartItemAsync(Guid cartItemId, Guid cartId);
        Task ClearCartAsync(Guid cartId);
    }
}