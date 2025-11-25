using System.Net;
using System.Text.Json;
using IoT_System.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IoT_System.Infrastructure.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = MapExceptionToStatusCode(ex);

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = message,
            Errors = new List<string> { ex.Message },
            Timestamp = DateTime.UtcNow
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsJsonAsync(errorResponse, options);
    }

    private static (HttpStatusCode statusCode, string message) MapExceptionToStatusCode(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Authentication is required and has failed or has not been provided."
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "The requested resource was not found."
            ),
            ArgumentNullException => (
                HttpStatusCode.BadRequest,
                "Required argument was null."
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                "Invalid argument provided."
            ),
            InvalidOperationException => (
                HttpStatusCode.BadRequest,
                "The operation is not valid in the current state."
            ),
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                "The operation timed out."
            ),
            NotImplementedException => (
                HttpStatusCode.NotImplemented,
                "This functionality is not yet implemented."
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An internal server error occurred. Please try again later."
            )
        };
    }
}