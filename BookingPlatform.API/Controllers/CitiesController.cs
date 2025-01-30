using BookingPlatform.Application.DTOs.Cities.Responses;
using BookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Manages city-related operations such as retrieving trending destinations.
    /// </summary>
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICitiesService _citiesService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CitiesController"/>.
        /// </summary>
        /// <param name="citiesService">The service used to retrieve city data.</param>
        public CitiesController(ICitiesService citiesService)
        {
            _citiesService = citiesService;
        }

        /// <summary>
        /// Retrieves the top 5 most visited trending destinations.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a list of the top trending cities based on most visits. 
        /// It can be used to display popular travel destinations on the platform.
        /// </remarks>
        /// <response code="200">Successfully returns a list of trending destinations.</response>
        /// <response code="500">Internal server error if unable to retrieve the data.</response>
        [HttpGet("trending")]
        [ProducesResponseType(typeof(List<TrendingDestinationResponse>), 200)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetTrendingDestinations()
        {
            var trendingDestinations = await _citiesService.GetTrendingDestinations();
            return Ok(trendingDestinations);
        }
    }
}