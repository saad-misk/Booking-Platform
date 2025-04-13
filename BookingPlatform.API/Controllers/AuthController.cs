using BookingPlatform.Application.DTOs.Auth;
using BookingPlatform.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Controllers
{
    /// <summary>
    /// Handles user authentication and registration operations
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/>
        /// </summary>
        /// <param name="authenticationService">Authentication service handling business logic</param>
        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Registers a new user account
        /// </summary>
        /// <param name="request">User registration details</param>
        /// <param name="cancellationToken">Cancellation token for aborting the request</param>
        /// <returns>Authentication result with JWT token</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid request parameters</response>
        /// <response code="409">Email address already registered</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _authenticationService.RegisterAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Authenticates an existing user
        /// </summary>
        /// <param name="request">User login credentials</param>
        /// <param name="cancellationToken">Cancellation token for aborting the request</param>
        /// <returns>Authentication result with JWT token</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request format</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _authenticationService.LoginAsync(
                request.Email,
                request.Password,
                cancellationToken);

            return Ok(result);
        }
    }
}