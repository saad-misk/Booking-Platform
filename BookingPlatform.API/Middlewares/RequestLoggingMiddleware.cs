using System.Diagnostics;

namespace BookingPlatform.API.Middlewares
{
    /// <summary>
    /// Middleware for logging incoming HTTP requests and outgoing responses.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger used to log request and response details.</param>
        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to log the request and response details.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Skip health checks
            if (context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            var sw = Stopwatch.StartNew();
            LogRequest(context);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                throw;
            }
            finally
            {
                sw.Stop();
                LogResponse(context, sw.ElapsedMilliseconds);
            }
        }

        /// <summary>
        /// Logs details of the incoming HTTP request.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        private void LogRequest(HttpContext context)
        {
            _logger.LogDebug(
                "Request: {Method} {Path} (Client: {ClientIP}, Agent: {UserAgent})",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.Request.Headers.UserAgent
            );
        }

        /// <summary>
        /// Logs details of the outgoing HTTP response.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
        /// <param name="elapsedMs">The time elapsed in milliseconds for processing the request.</param>
        private void LogResponse(HttpContext context, long elapsedMs)
        {
            _logger.LogDebug(
                "Response: {StatusCode} ({Elapsed}ms)",
                context.Response.StatusCode,
                elapsedMs
            );
        }
    }
}