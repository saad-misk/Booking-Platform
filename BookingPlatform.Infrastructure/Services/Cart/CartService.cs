using BookingPlatform.Application.DTOs.Cart.Requests;
using BookingPlatform.Application.DTOs.Cart.Responses;
using BookingPlatform.Application.Interfaces.Services;
using BookingPlatform.Domain.Entities;
using BookingPlatform.Domain.Exceptions;
using BookingPlatform.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace BookingPlatform.Infrastructure.Services.Cart
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IRoomsRepository _roomRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ICartRepository cartRepository,
            IRoomsRepository roomRepository,
            ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _roomRepository = roomRepository;
            _logger = logger;
        }

        public async Task AddToCartAsync(AddToCartRequest request, Guid cartId)
        {
            var room = await _roomRepository.GetByIdAsync(request.RoomId);
            if (room == null) throw new NotFoundException("Room not found");
            
            var isAvailable = await _roomRepository.IsRoomAvailableAsync(
                request.RoomId, 
                request.CheckInDate, 
                request.CheckOutDate);
                
            if (!isAvailable) throw new ConflictException("Room not available");

            var cart = await _cartRepository.GetByIdAsync(cartId);

            cart!.Items.Add(new CartItem
            {
                CartItemId = Guid.NewGuid(),
                CartId = cartId,
                RoomId = request.RoomId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = room.PricePerNight,
                AddedAtUtc = DateTime.UtcNow
            });

            await _cartRepository.SaveCartAsync(cart);
            _logger.LogInformation("Added room {RoomId} to cart {CartId}", request.RoomId, cart.CartId);
        }

        public async Task<CartResponse> GetCartAsync(Guid cartId)
        {
            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null) return new CartResponse { Items = new List<CartItemResponse>() };

            var roomIds = cart.Items.Select(i => i.RoomId).Distinct();
            var rooms = await _roomRepository.GetRoomsByIdsAsync(roomIds);

            return new CartResponse
            {
                Items = cart.Items.Select(i => new CartItemResponse
                {
                    CartItemId = i.CartItemId,
                    RoomId = i.RoomId,
                    RoomName = rooms.First(r => r.RoomId == i.RoomId).Number,
                    RoomType = rooms.First(r => r.RoomId == i.RoomId).RoomClass.ToString(),
                    CheckInDate = i.CheckInDate,
                    CheckOutDate = i.CheckOutDate,
                    Nights = i.Nights,
                    TotalPrice = i.TotalPrice,
                    ThumbnailUrl = rooms.First(r => r.RoomId == i.RoomId).Images.FirstOrDefault()?.Url ?? string.Empty
                }).ToList()
            };
        }

        public async Task RemoveCartItemAsync(Guid cartItemId, Guid cartId)
        {
            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null) throw new NotFoundException("Cart not found");

            var itemToRemove = cart.Items.FirstOrDefault(i => i.CartItemId == cartItemId);
            if (itemToRemove == null) throw new NotFoundException("Cart item not found");

            cart.Items.Remove(itemToRemove);
            await _cartRepository.SaveCartAsync(cart);
            
            _logger.LogInformation("Removed item {CartItemId} from cart {CartId}", cartItemId, cart.CartId);
        }

        public async Task ClearCartAsync(Guid cartId)
        {
            await _cartRepository.DeleteAsync(cartId);
            _logger.LogInformation("Completely removed cart {CartId}", cartId);
        }
    }
}