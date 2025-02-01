using System.Security.Claims;
using BookingPlatform.Application.DTOs.Bookings.Requests;
using BookingPlatform.Application.DTOs.Bookings.Responses;
using BookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Handles booking-related operations for authenticated users.
    /// </summary>
    [Authorize(Roles = "NormalUser")]
    [Route("api/user/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IInvoiceService _invoiceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingsController"/> class.
        /// </summary>
        /// <param name="bookingService">Service for managing bookings.</param>
        /// <param name="invoiceService">Service for generating invoices.</param>
        public BookingsController(
            IBookingService bookingService,
            IInvoiceService invoiceService)
        {
            _bookingService = bookingService;
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Creates a new booking for the authenticated user.
        /// </summary>
        /// <param name="request">The booking request details.</param>
        /// <returns>The newly created booking.</returns>
        /// <response code="201">Booking created successfully.</response>
        /// <response code="400">Invalid request parameters.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BookingResponse), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return BadRequest(new { message = "Invalid user ID.", traceId = HttpContext.TraceIdentifier });

            var result = await _bookingService.CreateBookingAsync(userId, request);
            return CreatedAtAction(nameof(GetBookingById), new { id = result.BookingId }, result);
        }

        /// <summary>
        /// Retrieves all bookings of the authenticated user.
        /// </summary>
        /// <returns>A list of the user's bookings.</returns>
        /// <response code="200">Returns the list of bookings.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ICollection<BookingResponse>), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBookings()
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return BadRequest(new { message = "Invalid user ID.", traceId = HttpContext.TraceIdentifier });

            var result = await _bookingService.GetBookings(userId);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a specific booking by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <returns>The booking details.</returns>
        /// <response code="200">Booking retrieved successfully.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookingResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return BadRequest(new { message = "Invalid user ID.", traceId = HttpContext.TraceIdentifier });

            var result = await _bookingService.GetBookingByIdAsync(userId, id);
            if (result == null)
                return NotFound(new { message = "Booking not found.", traceId = HttpContext.TraceIdentifier });

            return Ok(result);
        }

        /// <summary>
        /// Deletes a booking by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <response code="204">Booking deleted successfully.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return BadRequest(new { message = "Invalid user ID.", traceId = HttpContext.TraceIdentifier });

            var deleted = await _bookingService.DeleteBookingAsync(userId, id);
            if (!deleted)
                return NotFound(new { message = "Booking not found.", traceId = HttpContext.TraceIdentifier });

            return NoContent();
        }

        /// <summary>
        /// Generates and downloads the invoice for a booking.
        /// </summary>
        /// <param name="id">The unique identifier of the booking.</param>
        /// <returns>The invoice as a PDF file.</returns>
        /// <response code="200">Returns the invoice in PDF format.</response>
        /// <response code="401">User is not authorized.</response>
        /// <response code="404">Booking not found.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("{id:guid}/invoice")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInvoice(Guid id)
        {
            if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return BadRequest(new { message = "Invalid user ID.", traceId = HttpContext.TraceIdentifier });

            var pdfBytes = await _invoiceService.GenerateInvoiceAsync(userId, id);
            if (pdfBytes == null || pdfBytes.Length == 0)
                return NotFound(new { message = "Invoice not found.", traceId = HttpContext.TraceIdentifier });

            return File(pdfBytes, "application/pdf", $"invoice-{id}.pdf");
        }
    }
}