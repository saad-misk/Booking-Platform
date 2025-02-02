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
            { 402, "Payment Required" },
            { 403, "Forbidden" },
            { 404, "Not Found" },
            { 409, "Conflict" },
            { 422, "Validation Error" },
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
            
            logger.Log(logLevel, exception, 
                "Exception: {Message} | " +
                "Type: {ExceptionType} | " +
                "StatusCode: {StatusCode} | " +
                "Trace: {TraceId} | " +
                "Path: {Path}",
                exception.Message,
                exception.GetType().Name,
                context.Response.StatusCode,
                GetTraceId(context),
                context.Request.Path);
        }

        private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = MapExceptionToProblemDetails(exception);
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Extensions =
                {
                    ["traceId"] = GetTraceId(context),
                    ["timestamp"] = DateTime.UtcNow.ToString("O"),
                    ["errorCode"] = GetErrorCode(exception),
                    ["stackTrace"] = environment.IsDevelopment() ? exception.StackTrace : null
                }
            };

            return problemDetails;
        }

        private (int statusCode, string title, string detail) MapExceptionToProblemDetails(
            Exception exception)
        {
            if (exception is not CustomException customException)
            {
                return (
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    environment.IsDevelopment() 
                        ? exception.Message 
                        : "An unexpected error occurred. Please try again later."
                );
            }

            var (statusCode, title) = customException switch
            {
                UserNotFoundException => (404, "User Not Found"),
                InvalidPasswordException => (400, "Invalid Credentials"),
                DuplicateEmailException => (409, "Email Already Exists"),
                NotFoundException => (404, "Resource Not Found"),
                ConflictException => (409, "Resource Conflict"),
                BadRequestException => (400, "Invalid Request"),
                UnauthorizedException => (401, "Unauthorized Access"),
                PaymentException => (402, "Payment Required"),
                _ => (500, "Internal Server Error")
            };

            // Always show custom exception messages
            return (
                statusCode,
                title,
                customException.Message
            );
        }

        private static string GetErrorCode(Exception exception)
        {
            return exception switch
            {
                UserNotFoundException => "USER_NOT_FOUND",
                InvalidPasswordException => "INVALID_PASSWORD",
                DuplicateEmailException => "DUPLICATE_EMAIL",
                NotFoundException => "RESOURCE_NOT_FOUND",
                ConflictException => "RESOURCE_CONFLICT",
                _ => "UNKNOWN_ERROR"
            };
        }

        private static string GetTraceId(HttpContext context) => 
            Activity.Current?.Id ?? context.TraceIdentifier;
    }
}