using System.Net;
using IoT_System.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Infrastructure.Extensions;

/// <summary>
/// Extension methods for converting OperationResult to ASP.NET Core IActionResult.
/// Provides seamless integration between service layer results and API responses.
/// </summary>
public static class OperationResultExtensions
{
    #region ToActionResult - Non-Generic OperationResult

    /// <summary>
    /// Converts a non-generic OperationResult to an IActionResult.
    /// Maps status codes and errors appropriately.
    /// </summary>
    /// <param name="result">The operation result to convert.</param>
    /// <returns>An IActionResult with appropriate status code and error information.</returns>
    public static IActionResult ToActionResult(this OperationResult result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkResult(),
                HttpStatusCode.Created => new StatusCodeResult((int)HttpStatusCode.Created),
                HttpStatusCode.Accepted => new AcceptedResult(),
                HttpStatusCode.NoContent => new NoContentResult(),
                _ => new StatusCodeResult((int)result.StatusCode)
            };
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    /// <summary>
    /// Converts a non-generic OperationResult to an IActionResult with a custom success response object.
    /// Useful when you want to return data on success even though the OperationResult doesn't contain data.
    /// </summary>
    /// <typeparam name="T">The type of data to return on success.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="successData">The data to return if the operation was successful.</param>
    /// <returns>An IActionResult with appropriate status code and data or error information.</returns>
    public static IActionResult ToActionResult<T>(this OperationResult result, T successData)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(successData),
                HttpStatusCode.Created => new CreatedResult(string.Empty, successData),
                HttpStatusCode.Accepted => new AcceptedResult(string.Empty, successData),
                HttpStatusCode.NoContent => new NoContentResult(),
                _ => new ObjectResult(successData) { StatusCode = (int)result.StatusCode }
            };
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    /// <summary>
    /// Converts a non-generic OperationResult to an IActionResult with a custom success response factory.
    /// The factory is only invoked if the operation was successful.
    /// </summary>
    /// <typeparam name="T">The type of data to return on success.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="successDataFactory">A factory function to create the success response data.</param>
    /// <returns>An IActionResult with appropriate status code and data or error information.</returns>
    public static IActionResult ToActionResult<T>(this OperationResult result, Func<T> successDataFactory)
    {
        if (!result.IsSuccess)
        {
            return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
        }

        var successData = successDataFactory();
        return result.StatusCode switch
        {
            HttpStatusCode.OK => new OkObjectResult(successData),
            HttpStatusCode.Created => new CreatedResult(string.Empty, successData),
            HttpStatusCode.Accepted => new AcceptedResult(string.Empty, successData),
            HttpStatusCode.NoContent => new NoContentResult(),
            _ => new ObjectResult(successData) { StatusCode = (int)result.StatusCode }
        };
    }

    #endregion

    #region ToActionResult - Generic OperationResult<T>

    /// <summary>
    /// Converts a generic OperationResult&lt;T&gt; to an IActionResult.
    /// Maps status codes, data, and errors appropriately.
    /// </summary>
    /// <typeparam name="T">The type of data in the operation result.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <returns>An IActionResult with appropriate status code and data or error information.</returns>
    public static IActionResult ToActionResult<T>(this OperationResult<T> result)
    {
        if (result.IsSuccess)
        {
            // Handle NoContent status code specially - don't return data
            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return new NoContentResult();
            }

            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(result.Data),
                HttpStatusCode.Created => new CreatedResult(string.Empty, result.Data),
                HttpStatusCode.Accepted => new AcceptedResult(string.Empty, result.Data),
                _ => new ObjectResult(result.Data) { StatusCode = (int)result.StatusCode }
            };
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    /// <summary>
    /// Converts a generic OperationResult&lt;T&gt; to an IActionResult with data transformation.
    /// Useful for mapping domain models to DTOs before returning.
    /// </summary>
    /// <typeparam name="TSource">The type of data in the operation result.</typeparam>
    /// <typeparam name="TDestination">The type of data to return in the response.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="mapper">Function to transform the source data to destination data.</param>
    /// <returns>An IActionResult with transformed data or error information.</returns>
    public static IActionResult ToActionResult<TSource, TDestination>(
        this OperationResult<TSource> result,
        Func<TSource?, TDestination> mapper)
    {
        if (result.IsSuccess)
        {
            var mappedData = mapper(result.Data);

            // Handle NoContent status code specially
            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return new NoContentResult();
            }

            return result.StatusCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(mappedData),
                HttpStatusCode.Created => new CreatedResult(string.Empty, mappedData),
                HttpStatusCode.Accepted => new AcceptedResult(string.Empty, mappedData),
                _ => new ObjectResult(mappedData) { StatusCode = (int)result.StatusCode }
            };
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    #endregion

    #region ToCreatedActionResult - Special handling for Created responses

    /// <summary>
    /// Converts an OperationResult to a CreatedAtAction result.
    /// </summary>
    /// <typeparam name="T">The type of data in the operation result.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="actionName">The name of the action to link to.</param>
    /// <param name="routeValues">Route values for generating the URI.</param>
    /// <returns>A CreatedAtActionResult on success, or error result on failure.</returns>
    public static IActionResult ToCreatedAtActionResult<T>(
        this OperationResult<T> result,
        string actionName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return new CreatedAtActionResult(actionName, null, routeValues, result.Data);
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    /// <summary>
    /// Converts an OperationResult to a CreatedAtRoute result.
    /// </summary>
    /// <typeparam name="T">The type of data in the operation result.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="routeName">The name of the route to link to.</param>
    /// <param name="routeValues">Route values for generating the URI.</param>
    /// <returns>A CreatedAtRouteResult on success, or error result on failure.</returns>
    public static IActionResult ToCreatedAtRouteResult<T>(
        this OperationResult<T> result,
        string routeName,
        object? routeValues = null)
    {
        if (result.IsSuccess)
        {
            return new CreatedAtRouteResult(routeName, routeValues, result.Data);
        }

        return CreateErrorResult(result.StatusCode, result.Errors, result.Exception);
    }

    #endregion

    #region ToActionResult with ActionResult<T>

    /// <summary>
    /// Converts a generic OperationResult&lt;T&gt; to an ActionResult&lt;T&gt;.
    /// This is useful for strongly-typed controller actions.
    /// </summary>
    /// <typeparam name="T">The type of data in the operation result.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <returns>An ActionResult&lt;T&gt; with appropriate status code and data or error information.</returns>
    public static ActionResult<T> ToActionResultOfT<T>(this OperationResult<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return new ActionResult<T>(new NoContentResult());
            }

            return result.StatusCode switch
            {
                HttpStatusCode.OK => result.Data!,
                HttpStatusCode.Created => new ActionResult<T>(new CreatedResult(string.Empty, result.Data)),
                HttpStatusCode.Accepted => new ActionResult<T>(new AcceptedResult(string.Empty, result.Data)),
                _ => new ActionResult<T>(new ObjectResult(result.Data) { StatusCode = (int)result.StatusCode })
            };
        }

        return new ActionResult<T>((ActionResult)CreateErrorResult(result.StatusCode, result.Errors, result.Exception));
    }

    /// <summary>
    /// Converts a generic OperationResult&lt;T&gt; to an ActionResult&lt;TDestination&gt; with data transformation.
    /// </summary>
    /// <typeparam name="TSource">The type of data in the operation result.</typeparam>
    /// <typeparam name="TDestination">The type of data to return in the response.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="mapper">Function to transform the source data to destination data.</param>
    /// <returns>An ActionResult&lt;TDestination&gt; with transformed data or error information.</returns>
    public static ActionResult<TDestination> ToActionResultOfT<TSource, TDestination>(
        this OperationResult<TSource> result,
        Func<TSource?, TDestination> mapper)
    {
        if (result.IsSuccess)
        {
            var mappedData = mapper(result.Data);

            if (result.StatusCode == HttpStatusCode.NoContent)
            {
                return new ActionResult<TDestination>(new NoContentResult());
            }

            return result.StatusCode switch
            {
                HttpStatusCode.OK => mappedData!,
                HttpStatusCode.Created => new ActionResult<TDestination>(new CreatedResult(string.Empty, mappedData)),
                HttpStatusCode.Accepted => new ActionResult<TDestination>(new AcceptedResult(string.Empty, mappedData)),
                _ => new ActionResult<TDestination>(new ObjectResult(mappedData) { StatusCode = (int)result.StatusCode })
            };
        }

        return new ActionResult<TDestination>((ActionResult)CreateErrorResult(result.StatusCode, result.Errors, result.Exception));
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an appropriate error result based on the status code and error information.
    /// </summary>
    private static IActionResult CreateErrorResult(
        HttpStatusCode statusCode,
        IEnumerable<string> errors,
        Exception? exception)
    {
        var errorMessage = string.Join(Environment.NewLine, errors);

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = exception is not null ? exception.Message : GetErrorMessageByStatusCode(statusCode);
        }

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)statusCode,
            Message = errorMessage,
            Errors = errors.ToList(),
            Timestamp = DateTime.UtcNow
        };

        return statusCode switch
        {
            HttpStatusCode.BadRequest => new BadRequestObjectResult(errorResponse),
            HttpStatusCode.Unauthorized => new UnauthorizedObjectResult(errorResponse),
            HttpStatusCode.Forbidden => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status403Forbidden },
            HttpStatusCode.NotFound => new NotFoundObjectResult(errorResponse),
            HttpStatusCode.Conflict => new ConflictObjectResult(errorResponse),
            HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(errorResponse),
            HttpStatusCode.InternalServerError => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status500InternalServerError },
            HttpStatusCode.ServiceUnavailable => new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status503ServiceUnavailable },
            _ => new ObjectResult(errorResponse) { StatusCode = (int)statusCode }
        };
    }

    /// <summary>
    /// Gets a user-friendly error message based on the status code.
    /// </summary>
    private static string GetErrorMessageByStatusCode(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.BadRequest => "The request was invalid or cannot be served.",
            HttpStatusCode.Unauthorized => "Authentication is required and has failed or has not been provided.",
            HttpStatusCode.Forbidden => "You do not have permission to access this resource.",
            HttpStatusCode.NotFound => "The requested resource was not found.",
            HttpStatusCode.Conflict => "The request conflicts with the current state of the server.",
            HttpStatusCode.UnprocessableEntity => "The request was well-formed but contains semantic errors.",
            HttpStatusCode.InternalServerError => "An internal server error occurred.",
            HttpStatusCode.ServiceUnavailable => "The service is temporarily unavailable.",
            _ => "An error occurred while processing your request."
        };
    }

    #endregion
}

/// <summary>
/// Standard error response model for API errors.
/// Provides consistent error structure across all API endpoints.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// User-friendly error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Collection of specific error messages or validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = [];

    /// <summary>
    /// Timestamp when the error occurred (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Indicates whether there are any errors.
    /// </summary>
    public bool HasErrors => Errors.Any();
}