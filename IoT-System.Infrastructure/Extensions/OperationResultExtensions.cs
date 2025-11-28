using System.Net;
using IoT_System.Application.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IoT_System.Infrastructure.Extensions;

/// <summary>
/// Extension methods for converting OperationResult to ASP.NET Core action results.
/// Supports IActionResult, ActionResult, and ActionResult&lt;T&gt;.
/// </summary>
public static class OperationResultExtensions
{
    #region OperationResult (non-generic) → ActionResult

    /// <summary>
    /// Converts OperationResult to ActionResult.
    /// </summary>
    public static ActionResult ToResult(this OperationResult result)
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
    /// Converts OperationResult to ActionResult with custom success data.
    /// </summary>
    public static ActionResult ToResult<T>(this OperationResult result, T successData)
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
    /// Converts OperationResult to ActionResult with a factory function.
    /// </summary>
    public static ActionResult ToResult<T>(this OperationResult result, Func<T> successDataFactory)
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

    #region OperationResult<T> → ActionResult<T>

    /// <summary>
    /// Converts OperationResult&lt;T&gt; to ActionResult&lt;T&gt; with automatic type inference.
    /// </summary>
    public static ActionResult<T> ToResult<T>(this OperationResult<T> result)
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

        return new ActionResult<T>(CreateErrorResult(result.StatusCode, result.Errors, result.Exception));
    }

    /// <summary>
    /// Converts OperationResult&lt;TSource&gt; to ActionResult&lt;TDestination&gt; with data transformation.
    /// </summary>
    public static ActionResult<TDestination> ToResult<TSource, TDestination>(
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

        return new ActionResult<TDestination>(CreateErrorResult(result.StatusCode, result.Errors, result.Exception));
    }

    #endregion

    #region OperationResult<T> → ActionResult

    /// <summary>
    /// Converts OperationResult&lt;T&gt; to ActionResult (non-generic).
    /// Use this when controller method returns ActionResult.
    /// </summary>
    public static ActionResult ToResultUntyped<T>(this OperationResult<T> result)
    {
        if (result.IsSuccess)
        {
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

    #endregion

    #region Helper Methods

    private static ActionResult CreateErrorResult(
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
/// </summary>
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = [];
    public DateTime Timestamp { get; set; }
    public bool HasErrors => Errors.Any();
}