using System.Net;

namespace IoT_System.Application.Models;

/// <summary>
/// Non-generic base class for operation results that encapsulates success status, HTTP status code, and error information.
/// Use this for operations that don't return data (void operations).
/// </summary>
public class OperationResult
{
    #region Private Fields

    private bool _isSuccess;
    private HttpStatusCode _statusCode = HttpStatusCode.OK;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the OperationResult class with default values (Success = true, StatusCode = 200 OK).
    /// </summary>
    public OperationResult()
    {
        _isSuccess = true;
        _statusCode = HttpStatusCode.OK;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status.
    /// StatusCode will be automatically set: 200 OK for success, 500 Internal Server Error for failure.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    public OperationResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified HTTP status code.
    /// IsSuccess will be automatically determined: true for 2xx/3xx codes, false for 4xx/5xx codes.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    public OperationResult(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status and status code.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode)
    {
        _isSuccess = isSuccess;
        _statusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and an exception.
    /// StatusCode will be set to 500 Internal Server Error by default.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, Exception? exception) : this(isSuccess)
    {
        Exception = exception;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code and an exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(HttpStatusCode statusCode, Exception? exception) : this(statusCode)
    {
        Exception = exception;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, Exception? exception) : this(isSuccess, statusCode)
    {
        Exception = exception;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, IEnumerable<string> errors) : this(isSuccess)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, IEnumerable<string> errors) : this(statusCode)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, IEnumerable<string> errors) : this(isSuccess, statusCode)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, exception, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, Exception? exception, IEnumerable<string> errors) : this(isSuccess, exception)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code, exception, and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, Exception? exception, IEnumerable<string> errors) : this(statusCode, exception)
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with all parameters.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, Exception? exception, IEnumerable<string> errors) : this(isSuccess, statusCode, exception)
    {
        Errors = errors;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets whether the operation was successful.
    /// Setting this property automatically adjusts the StatusCode:
    /// - If set to true and current StatusCode is 4xx/5xx, StatusCode becomes 200 OK
    /// - If set to false and current StatusCode is 2xx/3xx, StatusCode becomes 500 Internal Server Error
    /// </summary>
    public bool IsSuccess
    {
        get => _isSuccess;
        set
        {
            _isSuccess = value;
            var code = (int)_statusCode;

            if (_isSuccess)
            {
                // If marking as success but we have an error status code, change to OK
                if (code >= 400)
                {
                    _statusCode = HttpStatusCode.OK;
                }
            }
            else
            {
                // If marking as failure but we have a success status code, change to InternalServerError
                if (code < 400)
                {
                    _statusCode = HttpStatusCode.InternalServerError;
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the HTTP status code.
    /// Setting this property automatically adjusts the IsSuccess property:
    /// - Status codes 200-399 (2xx Success, 3xx Redirection) set IsSuccess to true
    /// - Status codes 400-599 (4xx Client Error, 5xx Server Error) set IsSuccess to false
    /// </summary>
    public HttpStatusCode StatusCode
    {
        get => _statusCode;
        set
        {
            _statusCode = value;
            var code = (int)value;
            // 2xx and 3xx are considered success, 4xx and 5xx are failures
            _isSuccess = code is >= 200 and < 400;
        }
    }

    /// <summary>
    /// Gets or sets the exception that occurred during the operation. Null if the operation succeeded.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets a collection of custom error messages or validation errors. Empty if no errors occurred.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = [];

    /// <summary>
    /// Gets whether this operation result has any errors (exception or error messages).
    /// </summary>
    public bool HasErrors => Exception != null || Errors.Any();

    #endregion

    #region Static Factory Methods - Basic

    /// <summary>
    /// Creates an OperationResult with the specified success status.
    /// StatusCode will be 200 OK for success, 500 Internal Server Error for failure.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult Result(bool isSuccess)
        => new(isSuccess);

    /// <summary>
    /// Creates an OperationResult with the specified HTTP status code.
    /// IsSuccess will be determined automatically based on the status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult Result(HttpStatusCode statusCode)
        => new(statusCode);

    /// <summary>
    /// Creates an OperationResult with the specified success status and status code.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult Result(bool isSuccess, HttpStatusCode statusCode)
        => new(isSuccess, statusCode);

    /// <summary>
    /// Creates a successful OperationResult with 200 OK status.
    /// </summary>
    /// <returns>A new OperationResult instance with IsSuccess set to true and StatusCode 200.</returns>
    public static OperationResult Success()
        => new(true, HttpStatusCode.OK);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false and StatusCode 500.</returns>
    public static OperationResult Failure(Exception? exception = null, IEnumerable<string>? errors = null)
        => new(false, HttpStatusCode.InternalServerError, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status and a single error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false and StatusCode 500.</returns>
    public static OperationResult Failure(string error, Exception? exception = null)
        => new(false, HttpStatusCode.InternalServerError, exception, [error]);

    #endregion

    #region Static Factory Methods - 2xx Success Status Codes

    /// <summary>
    /// Creates a successful OperationResult with 200 OK status.
    /// Typically used for successful GET, PUT, or PATCH operations.
    /// </summary>
    /// <returns>A new OperationResult instance with StatusCode 200.</returns>
    public static OperationResult Ok()
        => new(true, HttpStatusCode.OK);

    /// <summary>
    /// Creates a successful OperationResult with 201 Created status.
    /// Typically used after successfully creating a new resource (POST).
    /// </summary>
    /// <returns>A new OperationResult instance with StatusCode 201.</returns>
    public static OperationResult Created()
        => new(true, HttpStatusCode.Created);

    /// <summary>
    /// Creates a successful OperationResult with 202 Accepted status.
    /// Typically used when the request has been accepted for processing but is not yet complete.
    /// </summary>
    /// <returns>A new OperationResult instance with StatusCode 202.</returns>
    public static OperationResult Accepted()
        => new(true, HttpStatusCode.Accepted);

    /// <summary>
    /// Creates a successful OperationResult with 204 No Content status.
    /// Typically used for successful operations that don't return data (e.g., DELETE, UPDATE).
    /// </summary>
    /// <returns>A new OperationResult instance with StatusCode 204.</returns>
    public static OperationResult NoContent()
        => new(true, HttpStatusCode.NoContent);

    #endregion

    #region Static Factory Methods - 4xx Client Error Status Codes

    /// <summary>
    /// Creates a failed OperationResult with 400 Bad Request status.
    /// Typically used for validation errors or malformed requests.
    /// </summary>
    /// <param name="errors">A collection of validation or request error messages.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 400.</returns>
    public static OperationResult BadRequest(IEnumerable<string>? errors = null, Exception? exception = null)
        => new(false, HttpStatusCode.BadRequest, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 400 Bad Request status and a single error message.
    /// </summary>
    /// <param name="error">The validation or request error message.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 400.</returns>
    public static OperationResult BadRequest(string error, Exception? exception = null)
        => new(false, HttpStatusCode.BadRequest, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 401 Unauthorized status.
    /// Typically used when authentication is required but missing or invalid.
    /// </summary>
    /// <param name="error">The authentication error message (default: "Unauthorized access").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 401.</returns>
    public static OperationResult Unauthorized(string error = "Unauthorized access", Exception? exception = null)
        => new(false, HttpStatusCode.Unauthorized, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 403 Forbidden status.
    /// Typically used when the user is authenticated but lacks permissions.
    /// </summary>
    /// <param name="error">The authorization error message (default: "Access forbidden").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 403.</returns>
    public static OperationResult Forbidden(string error = "Access forbidden", Exception? exception = null)
        => new(false, HttpStatusCode.Forbidden, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 404 Not Found status.
    /// Typically used when a requested resource doesn't exist.
    /// </summary>
    /// <param name="error">The not found error message (default: "Resource not found").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 404.</returns>
    public static OperationResult NotFound(string error = "Resource not found", Exception? exception = null)
        => new(false, HttpStatusCode.NotFound, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 409 Conflict status.
    /// Typically used when the request conflicts with the current state (e.g., duplicate entry).
    /// </summary>
    /// <param name="error">The conflict error message (default: "Resource conflict").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 409.</returns>
    public static OperationResult Conflict(string error = "Resource conflict", Exception? exception = null)
        => new(false, HttpStatusCode.Conflict, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 422 Unprocessable Entity status.
    /// Typically used when the request is well-formed but contains semantic errors.
    /// </summary>
    /// <param name="errors">A collection of semantic validation error messages.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 422.</returns>
    public static OperationResult UnprocessableEntity(IEnumerable<string>? errors = null, Exception? exception = null)
        => new(false, HttpStatusCode.UnprocessableEntity, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 422 Unprocessable Entity status and a single error message.
    /// </summary>
    /// <param name="error">The semantic validation error message.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 422.</returns>
    public static OperationResult UnprocessableEntity(string error, Exception? exception = null)
        => new(false, HttpStatusCode.UnprocessableEntity, exception, [error]);

    #endregion

    #region Static Factory Methods - 5xx Server Error Status Codes

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status.
    /// Typically used for unexpected server errors.
    /// </summary>
    /// <param name="exception">The exception that caused the server error.</param>
    /// <param name="errors">A collection of error messages.</param>
    /// <returns>A new OperationResult instance with StatusCode 500.</returns>
    public static OperationResult InternalServerError(Exception? exception = null, IEnumerable<string>? errors = null)
        => new(false, HttpStatusCode.InternalServerError, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status and a single error message.
    /// </summary>
    /// <param name="error">The error message describing the server error.</param>
    /// <param name="exception">Optional exception that caused the server error.</param>
    /// <returns>A new OperationResult instance with StatusCode 500.</returns>
    public static OperationResult InternalServerError(string error, Exception? exception = null)
        => new(false, HttpStatusCode.InternalServerError, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 503 Service Unavailable status.
    /// Typically used when the service is temporarily unavailable.
    /// </summary>
    /// <param name="error">The service unavailable error message (default: "Service temporarily unavailable").</param>
    /// <param name="exception">Optional exception that caused the unavailability.</param>
    /// <returns>A new OperationResult instance with StatusCode 503.</returns>
    public static OperationResult ServiceUnavailable(string error = "Service temporarily unavailable", Exception? exception = null)
        => new(false, HttpStatusCode.ServiceUnavailable, exception, [error]);

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Converts this OperationResult to a non-generic OperationResult (returns self).
    /// </summary>
    /// <returns>The current OperationResult instance.</returns>
    public OperationResult ToOperationResult() => this;

    /// <summary>
    /// Converts this OperationResult to a generic OperationResult with the specified data.
    /// </summary>
    /// <typeparam name="T">The type of data to include.</typeparam>
    /// <param name="data">The data to include in the result.</param>
    /// <returns>A new generic OperationResult instance.</returns>
    public OperationResult<T> ToOperationResult<T>(T? data = default)
        => new(IsSuccess, StatusCode, data, Exception, Errors);

    #endregion
}

/// <summary>
/// Generic wrapper for operation results that encapsulates success status, data, HTTP status code, and error information.
/// Provides a consistent pattern for handling operation outcomes across repository and service layers.
/// Inherits from OperationResult to provide base functionality.
/// </summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
public class OperationResult<T> : OperationResult
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the OperationResult class with default values (Success = true, StatusCode = 200 OK).
    /// </summary>
    public OperationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status.
    /// StatusCode will be automatically set: 200 OK for success, 500 Internal Server Error for failure.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    public OperationResult(bool isSuccess) : base(isSuccess)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified HTTP status code.
    /// IsSuccess will be automatically determined based on the status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    public OperationResult(HttpStatusCode statusCode) : base(statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status and status code.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode) : base(isSuccess, statusCode)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and data.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    public OperationResult(bool isSuccess, T? data) : base(isSuccess)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code and data.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    public OperationResult(HttpStatusCode statusCode, T? data) : base(statusCode)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, and data.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, T? data) : base(isSuccess, statusCode)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, Exception? exception) : base(isSuccess, exception)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code and an exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(HttpStatusCode statusCode, Exception? exception) : base(statusCode, exception)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, Exception? exception) : base(isSuccess, statusCode, exception)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, data, and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, T? data, Exception? exception) : base(isSuccess, exception)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code, data, and an exception.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(HttpStatusCode statusCode, T? data, Exception? exception) : base(statusCode, exception)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, data, and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, T? data, Exception? exception) : base(isSuccess, statusCode, exception)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, IEnumerable<string> errors) : base(isSuccess, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, IEnumerable<string> errors) : base(statusCode, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, IEnumerable<string> errors) : base(isSuccess, statusCode, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, data, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, T? data, IEnumerable<string> errors) : base(isSuccess, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code, data, and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, T? data, IEnumerable<string> errors) : base(statusCode, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, data, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, T? data, IEnumerable<string> errors) : base(isSuccess, statusCode, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, exception, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, Exception? exception, IEnumerable<string> errors) : base(isSuccess, exception, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code, exception, and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, Exception? exception, IEnumerable<string> errors) : base(statusCode, exception, errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, status code, exception, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, Exception? exception, IEnumerable<string> errors) : base(isSuccess, statusCode, exception,
        errors)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status, data, exception, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, T? data, Exception? exception, IEnumerable<string> errors) : base(isSuccess, exception, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with status code, data, exception, and error messages.
    /// </summary>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(HttpStatusCode statusCode, T? data, Exception? exception, IEnumerable<string> errors) : base(statusCode, exception, errors)
    {
        Data = data;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with all parameters.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code representing the operation result.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, HttpStatusCode statusCode, T? data, Exception? exception, IEnumerable<string> errors) : base(isSuccess, statusCode,
        exception, errors)
    {
        Data = data;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data returned by the operation. May be null if the operation failed or returned no data.
    /// </summary>
    public T? Data { get; set; }

    #endregion

    #region Implicit Operators

    /// <summary>
    /// Implicitly converts data of type T to a successful OperationResult with 200 OK status.
    /// Allows you to return data directly from methods without wrapping it in OperationResult.Success().
    /// </summary>
    /// <param name="data">The data to wrap in a successful OperationResult.</param>
    public static implicit operator OperationResult<T>(T? data)
        => Success(data);

    /// <summary>
    /// Implicitly converts OperationResult&lt;T&gt; to T by extracting the Data property.
    /// Useful when you need to get the underlying data value directly.
    /// Warning: Returns null if the operation failed or data is null.
    /// </summary>
    /// <param name="result">The OperationResult to extract data from.</param>
    public static implicit operator T?(OperationResult<T> result)
        => result.Data;

    #endregion

    #region Static Factory Methods - Basic

    /// <summary>
    /// Creates an OperationResult with the specified success status and optional data.
    /// StatusCode will be 200 OK for success, 500 Internal Server Error for failure.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult<T> Result(bool isSuccess, T? data = default)
        => new(isSuccess, data);

    /// <summary>
    /// Creates an OperationResult with the specified HTTP status code and optional data.
    /// IsSuccess will be determined automatically based on the status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult<T> Result(HttpStatusCode statusCode, T? data = default)
        => new(statusCode, data);

    /// <summary>
    /// Creates an OperationResult with the specified success status, status code, and optional data.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult<T> Result(bool isSuccess, HttpStatusCode statusCode, T? data = default)
        => new(isSuccess, statusCode, data);

    /// <summary>
    /// Creates a successful OperationResult with 200 OK status and optional data.
    /// </summary>
    /// <param name="data">The data returned by the successful operation.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to true and StatusCode 200.</returns>
    public static OperationResult<T> Success(T? data = default)
        => new(true, HttpStatusCode.OK, data);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false and StatusCode 500.</returns>
    public new static OperationResult<T> Failure(Exception? exception = null, IEnumerable<string>? errors = null)
        => new(false, HttpStatusCode.InternalServerError, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status and a single error message.
    /// </summary>
    /// <param name="error">The error message describing the failure.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false and StatusCode 500.</returns>
    public new static OperationResult<T> Failure(string error, Exception? exception = null)
        => new(false, HttpStatusCode.InternalServerError, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status and optional partial data.
    /// Useful when you want to return partial data even on failure.
    /// </summary>
    /// <param name="data">Partial or incomplete data that may still be useful despite the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false and StatusCode 500.</returns>
    public static OperationResult<T> Failure(T? data, Exception? exception = null, IEnumerable<string>? errors = null)
        => new(false, HttpStatusCode.InternalServerError, data, exception, errors ?? []);

    #endregion

    #region Static Factory Methods - 2xx Success Status Codes

    /// <summary>
    /// Creates a successful OperationResult with 200 OK status and optional data.
    /// Typically used for successful GET, PUT, or PATCH operations.
    /// </summary>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new OperationResult instance with StatusCode 200.</returns>
    public static OperationResult<T> Ok(T? data = default)
        => new(true, HttpStatusCode.OK, data);

    /// <summary>
    /// Creates a successful OperationResult with 201 Created status and optional data.
    /// Typically used after successfully creating a new resource (POST).
    /// </summary>
    /// <param name="data">The created resource data.</param>
    /// <returns>A new OperationResult instance with StatusCode 201.</returns>
    public static OperationResult<T> Created(T? data = default)
        => new(true, HttpStatusCode.Created, data);

    /// <summary>
    /// Creates a successful OperationResult with 202 Accepted status and optional data.
    /// Typically used when the request has been accepted for processing but is not yet complete.
    /// </summary>
    /// <param name="data">Optional data about the accepted operation.</param>
    /// <returns>A new OperationResult instance with StatusCode 202.</returns>
    public static OperationResult<T> Accepted(T? data = default)
        => new(true, HttpStatusCode.Accepted, data);

    #endregion

    #region Static Factory Methods - 4xx Client Error Status Codes

    /// <summary>
    /// Creates a failed OperationResult with 400 Bad Request status.
    /// Typically used for validation errors or malformed requests.
    /// </summary>
    /// <param name="errors">A collection of validation or request error messages.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 400.</returns>
    public new static OperationResult<T> BadRequest(IEnumerable<string>? errors = null, Exception? exception = null)
        => new(false, HttpStatusCode.BadRequest, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 400 Bad Request status and a single error message.
    /// </summary>
    /// <param name="error">The validation or request error message.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 400.</returns>
    public new static OperationResult<T> BadRequest(string error, Exception? exception = null)
        => new(false, HttpStatusCode.BadRequest, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 400 Bad Request status and optional partial data.
    /// </summary>
    /// <param name="data">Partial data that may be useful despite the validation error.</param>
    /// <param name="errors">A collection of validation error messages.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 400.</returns>
    public static OperationResult<T> BadRequest(T? data, IEnumerable<string>? errors = null, Exception? exception = null)
        => new(false, HttpStatusCode.BadRequest, data, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 401 Unauthorized status.
    /// Typically used when authentication is required but missing or invalid.
    /// </summary>
    /// <param name="error">The authentication error message (default: "Unauthorized access").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 401.</returns>
    public new static OperationResult<T> Unauthorized(string error = "Unauthorized access", Exception? exception = null)
        => new(false, HttpStatusCode.Unauthorized, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 403 Forbidden status.
    /// Typically used when the user is authenticated but lacks permissions.
    /// </summary>
    /// <param name="error">The authorization error message (default: "Access forbidden").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 403.</returns>
    public new static OperationResult<T> Forbidden(string error = "Access forbidden", Exception? exception = null)
        => new(false, HttpStatusCode.Forbidden, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 404 Not Found status.
    /// Typically used when a requested resource doesn't exist.
    /// </summary>
    /// <param name="error">The not found error message (default: "Resource not found").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 404.</returns>
    public new static OperationResult<T> NotFound(string error = "Resource not found", Exception? exception = null)
        => new(false, HttpStatusCode.NotFound, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 409 Conflict status.
    /// Typically used when the request conflicts with the current state (e.g., duplicate entry).
    /// </summary>
    /// <param name="error">The conflict error message (default: "Resource conflict").</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 409.</returns>
    public new static OperationResult<T> Conflict(string error = "Resource conflict", Exception? exception = null)
        => new(false, HttpStatusCode.Conflict, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 422 Unprocessable Entity status.
    /// Typically used when the request is well-formed but contains semantic errors.
    /// </summary>
    /// <param name="errors">A collection of semantic validation error messages.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 422.</returns>
    public new static OperationResult<T> UnprocessableEntity(IEnumerable<string>? errors = null, Exception? exception = null)
        => new(false, HttpStatusCode.UnprocessableEntity, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 422 Unprocessable Entity status and a single error message.
    /// </summary>
    /// <param name="error">The semantic validation error message.</param>
    /// <param name="exception">Optional exception that caused the failure.</param>
    /// <returns>A new OperationResult instance with StatusCode 422.</returns>
    public new static OperationResult<T> UnprocessableEntity(string error, Exception? exception = null)
        => new(false, HttpStatusCode.UnprocessableEntity, exception, [error]);

    #endregion

    #region Static Factory Methods - 5xx Server Error Status Codes

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status.
    /// Typically used for unexpected server errors.
    /// </summary>
    /// <param name="exception">The exception that caused the server error.</param>
    /// <param name="errors">A collection of error messages.</param>
    /// <returns>A new OperationResult instance with StatusCode 500.</returns>
    public new static OperationResult<T> InternalServerError(Exception? exception = null, IEnumerable<string>? errors = null)
        => new(false, HttpStatusCode.InternalServerError, exception, errors ?? []);

    /// <summary>
    /// Creates a failed OperationResult with 500 Internal Server Error status and a single error message.
    /// </summary>
    /// <param name="error">The error message describing the server error.</param>
    /// <param name="exception">Optional exception that caused the server error.</param>
    /// <returns>A new OperationResult instance with StatusCode 500.</returns>
    public new static OperationResult<T> InternalServerError(string error, Exception? exception = null)
        => new(false, HttpStatusCode.InternalServerError, exception, [error]);

    /// <summary>
    /// Creates a failed OperationResult with 503 Service Unavailable status.
    /// Typically used when the service is temporarily unavailable.
    /// </summary>
    /// <param name="error">The service unavailable error message (default: "Service temporarily unavailable").</param>
    /// <param name="exception">Optional exception that caused the unavailability.</param>
    /// <returns>A new OperationResult instance with StatusCode 503.</returns>
    public new static OperationResult<T> ServiceUnavailable(string error = "Service temporarily unavailable", Exception? exception = null)
        => new(false, HttpStatusCode.ServiceUnavailable, exception, [error]);

    #endregion

    #region Conversion Methods

    /// <summary>
    /// Converts this generic OperationResult to a non-generic OperationResult (removes data).
    /// </summary>
    /// <returns>A new non-generic OperationResult instance.</returns>
    public new OperationResult ToOperationResult()
        => new(IsSuccess, StatusCode, Exception, Errors);

    /// <summary>
    /// Converts this OperationResult to a different data type while preserving status and error information.
    /// </summary>
    /// <typeparam name="TOut">The target data type.</typeparam>
    /// <param name="data">The new data to include in the result.</param>
    /// <returns>A new OperationResult with the specified data type.</returns>
    public new OperationResult<TOut> ToOperationResult<TOut>(TOut? data = default)
        => new(IsSuccess, StatusCode, data, Exception, Errors);

    /// <summary>
    /// Converts this OperationResult to a different data type using a transformation function.
    /// </summary>
    /// <typeparam name="TOut">The target data type.</typeparam>
    /// <param name="mapper">Function to transform the current data to the new type.</param>
    /// <returns>A new OperationResult with the transformed data.</returns>
    public OperationResult<TOut> Map<TOut>(Func<T?, TOut?> mapper)
    {
        if (!IsSuccess)
        {
            return new OperationResult<TOut>(IsSuccess, StatusCode, default, Exception, Errors);
        }

        try
        {
            var mappedData = mapper(Data);
            return new OperationResult<TOut>(IsSuccess, StatusCode, mappedData, Exception, Errors);
        }
        catch (Exception ex)
        {
            return new OperationResult<TOut>(false, HttpStatusCode.InternalServerError, default, ex, ["Error mapping data"]);
        }
    }

    #endregion
}