using BookingPlatform.Application.DTOs.Checkout.Requests;
using BookingPlatform.Application.DTOs.Checkout.Responses;
using BookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Handles checkout operations for cart items.
    /// </summary>
    [Route("api/checkout")]
    [ApiController]
    public class CheckoutController : ControllerBase
    {
        private readonly ISingleItemCheckoutService _checkoutService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutController"/>.
        /// </summary>
        /// <param name="checkoutService">The checkout service.</param>
        public CheckoutController(ISingleItemCheckoutService checkoutService)
        {
            _checkoutService = checkoutService;
        }

        /// <summary>
        /// Processes checkout for a specific item in the cart.
        /// </summary>
        /// <param name="cartId">The cart identifier.</param>
        /// <param name="cartItemId">The cart item identifier.</param>
        /// <param name="request">Checkout details including payment information.</param>
        /// <returns>Returns a checkout response with transaction details.</returns>
        /// <response code="200">Checkout completed successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="404">Cart or cart item not found.</response>
        [HttpPost("process/{cartId:guid}/{cartItemId:guid}")]
        [ProducesResponseType(typeof(CheckoutResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ProcessCheckout(
            [FromRoute] Guid cartId, 
            [FromRoute] Guid cartItemId, 
            [FromBody] CheckoutRequest request)
        {
            var checkoutResponse = await _checkoutService.ProcessSingleCartItemAsync(cartId, cartItemId, request);
            return Ok(checkoutResponse);
        }
    }
}