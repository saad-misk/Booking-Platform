using BookingPlatform.Application.DTOs.Rooms.Requests;
using BookingPlatform.Application.DTOs.Rooms.Responses;
using BookingPlatform.Application.Interfaces.Services.Admin;
using BookingPlatform.Application.DTOs.Hotels.Requests;
using BookingPlatform.Application.DTOs.Hotels.Responses;
using BookingPlatform.Application.DTOs.Cities.Requests;
using BookingPlatform.Application.DTOs.Cities.Responses;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Administrative endpoints for managing application resources
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Produces(MediaTypeNames.Application.Json)]
    [Consumes(MediaTypeNames.Application.Json)]
    public class AdminController : ControllerBase
    {
        private readonly IAdminHotelsService _adminHotelsService;
        private readonly IAdminCitiesService _adminCitiesService;
        private readonly IAdminSearchService _adminSearchService;
        private readonly IAdminRoomsService _adminRoomsService;

        /// <summary>
        /// Initializes a new instance of the AdminController
        /// </summary>
        /// <param name="adminHotelsService">Service for hotel management operations</param>
        /// <param name="adminCitiesService">Service for city management operations</param>
        /// <param name="adminSearchService">Service for administrative search operations</param>
        /// <param name="adminRoomsService">Service for room management operations</param>
        public AdminController(
            IAdminHotelsService adminHotelsService,
            IAdminCitiesService adminCitiesService,
            IAdminSearchService adminSearchService,
            IAdminRoomsService adminRoomsService)
        {
            _adminHotelsService = adminHotelsService;
            _adminCitiesService = adminCitiesService;
            _adminSearchService = adminSearchService;
            _adminRoomsService = adminRoomsService;
        }

        #region Rooms Endpoints

        /// <summary>
        /// Retrieves all rooms
        /// </summary>
        /// <returns>List of room details</returns>
        /// <response code="200">Successfully retrieved all rooms</response>
        [HttpGet("rooms")]
        [ProducesResponseType(typeof(IEnumerable<RoomResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRooms()
        {
            var rooms = await _adminRoomsService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        /// <summary>
        /// Retrieves a specific room by unique identifier
        /// </summary>
        /// <param name="roomId">Room identifier</param>
        /// <returns>Room details</returns>
        /// <response code="200">Successfully retrieved the room</response>
        /// <response code="404">Room not found</response>
        [HttpGet("rooms/{roomId:guid}")]
        [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoomById(Guid roomId)
        {
            var room = await _adminRoomsService.GetRoomByIdAsync(roomId);
            return room != null ? Ok(room) : NotFound();
        }

        /// <summary>
        /// Creates a new room
        /// </summary>
        /// <param name="request">Room creation details</param>
        /// <returns>Newly created room details</returns>
        /// <response code="201">Successfully created the room</response>
        /// <response code="400">Invalid request parameters</response>
        [HttpPost("rooms")]
        [ProducesResponseType(typeof(RoomResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRoom(
            [FromBody] CreateRoomRequest request)
        {
            var room = await _adminRoomsService.CreateRoomAsync(request);
            return CreatedAtAction(nameof(GetRoomById), new { roomId = room.RoomId }, room);
        }

        /// <summary>
        /// Updates an existing room
        /// </summary>
        /// <param name="roomId">Room identifier</param>
        /// <param name="request">Room update details</param>
        /// <response code="204">Successfully updated the room</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="404">Room not found</response>
        [HttpPut("rooms/{roomId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRoom(
            Guid roomId,
            [FromBody] UpdateRoomRequest request)
        {
            if (roomId != request.RoomId)
            {
                return BadRequest("Route identifier does not match request body identifier");
            }

            await _adminRoomsService.UpdateRoomAsync(request);
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific room
        /// </summary>
        /// <param name="roomId">Room identifier</param>
        /// <response code="204">Successfully deleted the room</response>
        /// <response code="404">Room not found</response>
        [HttpDelete("rooms/{roomId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRoom(Guid roomId)
        {
            await _adminRoomsService.DeleteRoomAsync(roomId);
            return NoContent();
        }

        #endregion

        #region Hotels Endpoints

        /// <summary>
        /// Creates a new hotel
        /// </summary>
        /// <param name="request">Hotel creation details</param>
        /// <returns>Newly created hotel details</returns>
        /// <response code="201">Successfully created the hotel</response>
        /// <response code="400">Invalid request parameters</response>
        [HttpPost("hotels")]
        [ProducesResponseType(typeof(HotelDetailsResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateHotel(
            [FromBody] CreateHotelRequest request)
        {
            var hotel = await _adminHotelsService.CreateHotelAsync(request);
            return CreatedAtAction(nameof(GetHotelById), new { hotelId = hotel.HotelId }, hotel);
        }

        /// <summary>
        /// Retrieves a specific hotel by unique identifier
        /// </summary>
        /// <param name="hotelId">Hotel identifier</param>
        /// <returns>Hotel details</returns>
        /// <response code="200">Successfully retrieved the hotel</response>
        /// <response code="404">Hotel not found</response>
        [HttpGet("hotels/{hotelId:guid}")]
        [ProducesResponseType(typeof(HotelDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHotelById(Guid hotelId)
        {
            var hotel = await _adminHotelsService.GetHotelByIdAsync(hotelId);
            return hotel != null ? Ok(hotel) : NotFound();
        }

        /// <summary>
        /// Updates an existing hotel
        /// </summary>
        /// <param name="hotelId">Hotel identifier</param>
        /// <param name="request">Hotel update details</param>
        /// <response code="204">Successfully updated the hotel</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="404">Hotel not found</response>
        [HttpPut("hotels/{hotelId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateHotel(
            Guid hotelId,
            [FromBody] UpdateHotelRequest request)
        {
            if (hotelId != request.HotelId)
            {
                return BadRequest("Route identifier does not match request body identifier");
            }

            await _adminHotelsService.UpdateHotelAsync(request);
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific hotel
        /// </summary>
        /// <param name="hotelId">Hotel identifier</param>
        /// <response code="204">Successfully deleted the hotel</response>
        /// <response code="404">Hotel not found</response>
        [HttpDelete("hotels/{hotelId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteHotel(Guid hotelId)
        {
            await _adminHotelsService.DeleteHotelAsync(hotelId);
            return NoContent();
        }

        #endregion

        #region Cities Endpoints

        /// <summary>
        /// Creates a new city
        /// </summary>
        /// <param name="request">City creation details</param>
        /// <returns>Newly created city details</returns>
        /// <response code="201">Successfully created the city</response>
        /// <response code="400">Invalid request parameters</response>
        [HttpPost("cities")]
        [ProducesResponseType(typeof(CityResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCity(
            [FromBody] CreateCityRequest request)
        {
            var city = await _adminCitiesService.CreateCityAsync(request);
            return CreatedAtAction(nameof(GetCityById), new { cityId = city.CityId }, city);
        }

        /// <summary>
        /// Retrieves a specific city by unique identifier
        /// </summary>
        /// <param name="cityId">City identifier</param>
        /// <returns>City details</returns>
        /// <response code="200">Successfully retrieved the city</response>
        /// <response code="404">City not found</response>
        [HttpGet("cities/{cityId:guid}")]
        [ProducesResponseType(typeof(CityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCityById(Guid cityId)
        {
            var city = await _adminCitiesService.GetCityByIdAsync(cityId);
            return city != null ? Ok(city) : NotFound();
        }

        /// <summary>
        /// Updates an existing city
        /// </summary>
        /// <param name="cityId">City identifier</param>
        /// <param name="request">City update details</param>
        /// <response code="204">Successfully updated the city</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="404">City not found</response>
        [HttpPut("cities/{cityId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCity(
            Guid cityId,
            [FromBody] UpdateCityRequest request)
        {
            if (cityId != request.CityId)
            {
                return BadRequest("Route identifier does not match request body identifier");
            }

            await _adminCitiesService.UpdateCityAsync(request);
            return NoContent();
        }

        /// <summary>
        /// Deletes a specific city
        /// </summary>
        /// <param name="cityId">City identifier</param>
        /// <response code="204">Successfully deleted the city</response>
        /// <response code="404">City not found</response>
        [HttpDelete("cities/{cityId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCity(Guid cityId)
        {
            await _adminCitiesService.DeleteCityAsync(cityId);
            return NoContent();
        }

        #endregion

        #region Search Endpoints

        /// <summary>
        /// Searches hotels based on specified criteria
        /// </summary>
        /// <param name="request">Search parameters</param>
        /// <returns>List of matching hotels</returns>
        /// <response code="200">Successfully completed the search</response>
        /// <response code="400">Invalid search parameters</response>
        [HttpPost("search/hotels")]
        [ProducesResponseType(typeof(IEnumerable<HotelDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchHotels(
            [FromBody] HotelSearchCriteria request)
        {
            var results = await _adminSearchService.SearchHotelsAsync(request);
            return Ok(results);
        }

        /// <summary>
        /// Searches cities based on specified criteria
        /// </summary>
        /// <param name="request">Search parameters</param>
        /// <returns>List of matching cities</returns>
        /// <response code="200">Successfully completed the search</response>
        /// <response code="400">Invalid search parameters</response>
        [HttpPost("search/cities")]
        [ProducesResponseType(typeof(IEnumerable<CityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SearchCities(
            [FromBody] CitySearchCriteria request)
        {
            var results = await _adminSearchService.SearchCitiesAsync(request);
            return Ok(results);
        }

        #endregion
    }
}