using System.Diagnostics;
using BookingPlatform.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookingPlatform.API.Middlewares
{
    /// <summary>
    /// A global exception handler middleware that catches unhandled exceptions and returns a structured response.
    /// </summary>
    public class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment) : IExceptionHandler
    {
        private const string FallbackErrorMessage = "An error occurred while processing your request.";
        private static readonly Dictionary<int, string> StatusCodeTitles = new()
        {
            { 400, "Bad Request" },
            { 401, "Unauthorized" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 409, "Conflict" },
            { 500, "Internal Server Error" },
            { 503, "Service Unavailable" }
        };

        /// <summary>
        /// Attempts to handle the exception and generate a ProblemDetails response.
        /// </summary>
        /// <param name="httpContext">The HTTP context where the exception occurred.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of the operation.</returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            LogException(exception, httpContext);

            var problemDetails = CreateProblemDetails(httpContext, exception);
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private void LogException(Exception exception, HttpContext context)
        {
            var logLevel = exception is CustomException ? LogLevel.Warning : LogLevel.Error;
            logger.Log(logLevel, exception, "Exception: {Message} (Trace: {TraceId})", 
                exception.Message, GetTraceId(context));
        }

        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = MapExceptionToProblemDetails(exception);
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Extensions = { ["traceId"] = GetTraceId(context) }
            };
        }

        private (int statusCode, string title, string detail) MapExceptionToProblemDetails(
            Exception exception)
        {
            if (exception is not CustomException customException)
            {
                return (
                    StatusCodes.Status500InternalServerError,
                    StatusCodeTitles[500],
                    environment.IsDevelopment() ? exception.Message : FallbackErrorMessage
                );
            }

            var statusCode = customException switch
            {
                UserNotFoundException => StatusCodes.Status404NotFound,
                InvalidPasswordException => StatusCodes.Status400BadRequest,
                DuplicateEmailException => StatusCodes.Status409Conflict,
                UserRegistrationException => StatusCodes.Status400BadRequest,
                ConflictException => StatusCodes.Status409Conflict,
                BadRequestException => StatusCodes.Status400BadRequest,
                PaymentException => StatusCodes.Status402PaymentRequired,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                CustomException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };

            return (
                statusCode,
                StatusCodeTitles.TryGetValue(statusCode, out var title) ? title : "Error",
                environment.IsDevelopment() ? customException.Message : FallbackErrorMessage
            );
        }

        private static string GetTraceId(HttpContext context) => 
            Activity.Current?.Id ?? context.TraceIdentifier;
    }
}