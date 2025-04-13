using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Manages hotel-related operations such as searching for hotels and viewing hotel details.
    /// </summary>
    [ApiController]
    [Route("api/hotels")]
    public class HotelController : ControllerBase
    {
        private readonly IHotelsService _hotelService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HotelController"/> with the given service.
        /// </summary>
        /// <param name="hotelService">The service used to retrieve hotel data.</param>
        public HotelController(IHotelsService hotelService)
        {
            _hotelService = hotelService;
        }

        /// <summary>
        /// Searches for hotels based on specified criteria.
        /// </summary>
        /// <remarks>
        /// Use this endpoint to search for hotels based on different parameters such as city, price, rating, etc.
        /// </remarks>
        /// <param name="request">The search criteria for hotels.</param>
        /// <response code="200">Returns a list of hotels matching the search criteria.</response>
        /// <response code="400">Bad request if the input parameters are invalid.</response>
        [AllowAnonymous]
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<HotelsResponse>), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<List<HotelsResponse>>> SearchHotels(
            [FromQuery] HotelSearchCriteria request)
        {
            var hotels = await _hotelService.SearchHotelsAsync(request);
            return Ok(hotels);
        }

        /// <summary>
        /// Retrieves the details of a specific hotel.
        /// </summary>
        /// <remarks>
        /// This endpoint provides detailed information about a hotel, such as its description, amenities, location, etc.
        /// </remarks>
        /// <param name="hotelId">The unique identifier of the hotel.</param>
        /// <response code="200">Returns detailed information about the specified hotel.</response>
        /// <response code="404">The hotel with the specified ID was not found.</response>
        [HttpGet("{hotelId}")]
        [ProducesResponseType(typeof(HotelDetailsResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetHotelDetails(Guid hotelId)
        {
            var response = await _hotelService.GetHotelDetails(hotelId);
            if (response == null)
            {
                return NotFound();
            }
            return Ok(response);
        }
    }
}