using Microsoft.AspNetCore.Mvc;
using BookingPlatform.Application.DTOs.Cart.Requests;
using BookingPlatform.Application.DTOs.Cart.Responses;
using BookingPlatform.Application.Interfaces.Services;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Manages the shopping cart for users.
    /// </summary>
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CartController"/>.
        /// </summary>
        /// <param name="cartService">The cart service.</param>
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Adds an item to the cart.
        /// </summary>
        /// <param name="request">The item details to add.</param>
        /// <param name="cartId">The cart identifier (from header).</param>
        /// <returns>Returns `201 Created` if successful.</returns>
        /// <response code="201">Item added successfully.</response>
        /// <response code="400">Invalid request format.</response>
        /// <response code="404">Cart or item not found.</response>
        [HttpPost("items")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AddToCart(
            [FromBody] AddToCartRequest request,
            [FromHeader(Name = "X-Cart-Id")] Guid cartId)
        {
            await _cartService.AddToCartAsync(request, cartId);
            return CreatedAtAction(nameof(GetCart), new { cartId }, null);
        }

        /// <summary>
        /// Retrieves the cart details.
        /// </summary>
        /// <param name="cartId">The cart identifier (from header).</param>
        /// <returns>The cart details.</returns>
        /// <response code="200">Cart retrieved successfully.</response>
        /// <response code="404">Cart not found.</response>
        [HttpGet]
        [ProducesResponseType(typeof(CartResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCart(
            [FromHeader(Name = "X-Cart-Id")] Guid cartId)
        {
            var response = await _cartService.GetCartAsync(cartId);
            return Ok(response);
        }

        /// <summary>
        /// Removes an item from the cart.
        /// </summary>
        /// <param name="cartItemId">The item identifier.</param>
        /// <param name="cartId">The cart identifier (from header).</param>
        /// <returns>Returns `204 No Content` if successful.</returns>
        /// <response code="204">Item removed successfully.</response>
        /// <response code="404">Item not found.</response>
        [HttpDelete("items/{cartItemId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> RemoveCartItem(
            [FromRoute] Guid cartItemId,
            [FromHeader(Name = "X-Cart-Id")] Guid cartId)
        {
            await _cartService.RemoveCartItemAsync(cartItemId, cartId);
            return NoContent();
        }

        /// <summary>
        /// Clears all items from the cart.
        /// </summary>
        /// <param name="cartId">The cart identifier (from header).</param>
        /// <returns>Returns `204 No Content` if successful.</returns>
        /// <response code="204">Cart cleared successfully.</response>
        [HttpDelete]
        [ProducesResponseType(204)]
        public async Task<IActionResult> ClearCart(
            [FromHeader(Name = "X-Cart-Id")] Guid cartId)
        {
            await _cartService.ClearCartAsync(cartId);
            return NoContent();
        }
    }
}