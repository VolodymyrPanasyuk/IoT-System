namespace IoT_System.Application.Models;

/// <summary>
/// Non-generic base class for operation results that encapsulates success status and error information.
/// Use this for operations that don't return data (void operations).
/// </summary>
public class OperationResult
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the OperationResult class with default values.
    /// </summary>
    public OperationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    public OperationResult(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and an exception.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    public OperationResult(bool isSuccess, Exception? exception) : this(isSuccess)
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
    /// Initializes a new instance of the OperationResult class with success status, exception, and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, Exception? exception, IEnumerable<string> errors) : this(isSuccess, errors)
    {
        Exception = exception;
    }

    #endregion

    /// <summary>
    /// Gets or sets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the exception that occurred during the operation. Null if the operation succeeded.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets or sets a collection of custom error messages or validation errors. Empty if no errors occurred.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = [];

    #region Static Factory Methods

    /// <summary>
    /// Creates an OperationResult with the specified success status.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult Result(bool isSuccess)
        => new OperationResult(isSuccess);

    /// <summary>
    /// Creates a successful OperationResult.
    /// </summary>
    /// <returns>A new OperationResult instance with IsSuccess set to true.</returns>
    public static OperationResult Success()
        => new OperationResult(true);

    /// <summary>
    /// Creates a failed OperationResult with optional exception and error messages.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false.</returns>
    public static OperationResult Failure(Exception? exception = null, IEnumerable<string>? errors = null)
        => new OperationResult(false, exception, errors ?? Enumerable.Empty<string>());

    #endregion
}

/// <summary>
/// Generic wrapper for operation results that encapsulates success status, data, and error information.
/// Provides a consistent pattern for handling operation outcomes across repository and service layers.
/// Inherits from OperationResult to provide base functionality.
/// </summary>
/// <typeparam name="T">The type of data returned by the operation.</typeparam>
public class OperationResult<T> : OperationResult
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the OperationResult class with default values.
    /// </summary>
    public OperationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with the specified success status.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    public OperationResult(bool isSuccess) : base(isSuccess)
    {
    }

    /// <summary>
    /// Initializes a new instance of the OperationResult class with success status and data.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    public OperationResult(bool isSuccess, T? data) : this(isSuccess)
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
    /// Initializes a new instance of the OperationResult class with success status and error messages.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, IEnumerable<string> errors) : base(isSuccess, errors)
    {
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
    /// Initializes a new instance of the OperationResult class with all parameters.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <param name="exception">The exception that occurred during the operation.</param>
    /// <param name="errors">A collection of error messages.</param>
    public OperationResult(bool isSuccess, T? data, Exception? exception, IEnumerable<string> errors) : base(isSuccess, exception, errors)
    {
        Data = data;
    }

    #endregion

    /// <summary>
    /// Gets or sets the data returned by the operation. May be null if the operation failed or returned no data.
    /// </summary>
    public T? Data { get; set; }

    #region Static Factory Methods

    /// <summary>
    /// Creates an OperationResult with the specified success status and optional data.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="data">The data returned by the operation.</param>
    /// <returns>A new OperationResult instance.</returns>
    public static OperationResult<T> Result(bool isSuccess, T? data = default)
        => new OperationResult<T>(isSuccess, data);

    /// <summary>
    /// Creates a successful OperationResult with optional data.
    /// </summary>
    /// <param name="data">The data returned by the successful operation.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to true.</returns>
    public new static OperationResult<T> Success(T? data = default)
        => new OperationResult<T>(true, data);

    /// <summary>
    /// Creates a failed OperationResult with optional exception and error messages.
    /// </summary>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false.</returns>
    public new static OperationResult<T> Failure(Exception? exception = null, IEnumerable<string>? errors = null)
        => new OperationResult<T>(false, exception, errors ?? Enumerable.Empty<string>());

    /// <summary>
    /// Creates a failed OperationResult with optional data, exception, and error messages.
    /// Useful when you want to return partial data even on failure.
    /// </summary>
    /// <param name="data">Partial or incomplete data that may still be useful despite the failure.</param>
    /// <param name="exception">The exception that caused the failure.</param>
    /// <param name="errors">A collection of error messages describing the failure.</param>
    /// <returns>A new OperationResult instance with IsSuccess set to false.</returns>
    public static OperationResult<T> Failure(T? data = default, Exception? exception = null, IEnumerable<string>? errors = null)
        => new OperationResult<T>(false, data, exception, errors ?? Enumerable.Empty<string>());

    #endregion
}