using System.Net;
using IoT_System.Application.Models;

namespace IoT_System.Application.Common.Helpers;

/// <summary>
/// Provides utility methods for executing operations with automatic OperationResult wrapping and exception handling.
/// Enables consistent error handling and result patterns across all application layers.
/// </summary>
public static class ExecutionHelper
{
    #region Execute with Result (Single Operation)

    /// <summary>
    /// Executes an asynchronous operation that returns a result and wraps it in an OperationResult.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or exception information.</returns>
    public static async Task<OperationResult<TResult>> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            var result = await operation();
            return new OperationResult<TResult>(true, statusCode, result);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex, [ex.Message]);
        }
    }

    /// <summary>
    /// Executes a synchronous operation that returns a result and wraps it in an OperationResult.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The synchronous operation to execute.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or exception information.</returns>
    public static OperationResult<TResult> Execute<TResult>(
        Func<TResult> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            var result = operation();
            return new OperationResult<TResult>(true, statusCode, result);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex, [ex.Message]);
        }
    }

    #endregion

    #region Execute without Result (Single Operation)

    /// <summary>
    /// Executes an asynchronous operation without a return value and wraps the success status in an OperationResult.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult indicating success or failure with exception information.</returns>
    public static async Task<OperationResult> ExecuteAsync(
        Func<Task> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            await operation();
            return new OperationResult(true, statusCode);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex, [ex.Message]);
        }
    }

    /// <summary>
    /// Executes a synchronous operation without a return value and wraps the success status in an OperationResult.
    /// </summary>
    /// <param name="operation">The synchronous operation to execute.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult indicating success or failure with exception information.</returns>
    public static OperationResult Execute(
        Action operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            operation();
            return new OperationResult(true, statusCode);
        }
        catch (Exception ex)
        {
            return OperationResult.Failure(ex, [ex.Message]);
        }
    }

    #endregion

    #region Execute with Custom Error Handler

    /// <summary>
    /// Executes an asynchronous operation with a custom error handler.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="errorHandler">Custom function to handle exceptions and return appropriate OperationResult.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or custom error response.</returns>
    public static async Task<OperationResult<TResult>> ExecuteAsync<TResult>(
        Func<Task<TResult>> operation,
        Func<Exception, OperationResult<TResult>> errorHandler,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            var result = await operation();
            return new OperationResult<TResult>(true, statusCode, result);
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous operation without a return value with a custom error handler.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <param name="errorHandler">Custom function to handle exceptions and return appropriate OperationResult.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult indicating success or custom error response.</returns>
    public static async Task<OperationResult> ExecuteAsync(
        Func<Task> operation,
        Func<Exception, OperationResult> errorHandler,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        try
        {
            await operation();
            return new OperationResult(true, statusCode);
        }
        catch (Exception ex)
        {
            return errorHandler(ex);
        }
    }

    #endregion

    #region Execute with Validation

    /// <summary>
    /// Executes an asynchronous operation with pre-execution validation.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="validator">Validation function that returns null if valid, or error message if invalid.</param>
    /// <param name="operation">The asynchronous operation to execute if validation passes.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or validation/execution errors.</returns>
    public static async Task<OperationResult<TResult>> ExecuteWithValidationAsync<TResult>(
        Func<string?> validator,
        Func<Task<TResult>> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var validationError = validator();
        if (validationError != null)
        {
            return OperationResult<TResult>.BadRequest(validationError);
        }

        return await ExecuteAsync(operation, statusCode);
    }

    /// <summary>
    /// Executes an asynchronous operation with pre-execution validation that can return multiple errors.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="validator">Validation function that returns empty list if valid, or error messages if invalid.</param>
    /// <param name="operation">The asynchronous operation to execute if validation passes.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or validation/execution errors.</returns>
    public static async Task<OperationResult<TResult>> ExecuteWithValidationAsync<TResult>(
        Func<IEnumerable<string>> validator,
        Func<Task<TResult>> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var validationErrors = validator().ToList();
        if (validationErrors.Any())
        {
            return OperationResult<TResult>.BadRequest(validationErrors);
        }

        return await ExecuteAsync(operation, statusCode);
    }

    /// <summary>
    /// Executes an asynchronous operation with async pre-execution validation.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="validator">Async validation function that returns null if valid, or error message if invalid.</param>
    /// <param name="operation">The asynchronous operation to execute if validation passes.</param>
    /// <param name="statusCode">Optional status code to use on success (default: 200 OK).</param>
    /// <returns>An OperationResult containing the operation result or validation/execution errors.</returns>
    public static async Task<OperationResult<TResult>> ExecuteWithValidationAsync<TResult>(
        Func<Task<string?>> validator,
        Func<Task<TResult>> operation,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var validationError = await validator();
        if (validationError != null)
        {
            return OperationResult<TResult>.BadRequest(validationError);
        }

        return await ExecuteAsync(operation, statusCode);
    }

    #endregion

    #region Execute Multiple Operations with Result

    /// <summary>
    /// Executes multiple asynchronous operations and returns all results.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by each operation.</typeparam>
    /// <param name="operations">Collection of asynchronous operations to execute.</param>
    /// <param name="options">Options controlling execution behavior.</param>
    /// <returns>An OperationResult containing all successful results and any exceptions encountered.</returns>
    public static async Task<OperationResult<IEnumerable<TResult?>>> ExecuteMultipleAsync<TResult>(
        IEnumerable<Func<Task<TResult>>> operations,
        BulkExecutionOptions? options = null)
    {
        options ??= new BulkExecutionOptions();

        if (options.ExecuteInParallel)
        {
            return await ExecuteInParallelAsync(operations, options);
        }

        return await ExecuteSequentiallyAsync(operations, options);
    }

    /// <summary>
    /// Executes multiple synchronous operations and returns all results.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by each operation.</typeparam>
    /// <param name="operations">Collection of synchronous operations to execute.</param>
    /// <param name="options">Options controlling execution behavior.</param>
    /// <returns>An OperationResult containing all successful results and any exceptions encountered.</returns>
    public static OperationResult<IEnumerable<TResult?>> ExecuteMultiple<TResult>(
        IEnumerable<Func<TResult>> operations,
        BulkExecutionOptions? options = null)
    {
        options ??= new BulkExecutionOptions();

        var results = new List<TResult>();
        var exceptions = new List<Exception>();

        foreach (var operation in operations)
        {
            try
            {
                var result = operation();
                results.Add(result);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (!options.ContinueOnError)
                {
                    break;
                }
            }
        }

        return ProcessBulkResults(results, exceptions, options);
    }

    #endregion

    #region Execute Multiple Operations without Result

    /// <summary>
    /// Executes multiple asynchronous operations without return values.
    /// </summary>
    /// <param name="operations">Collection of asynchronous operations to execute.</param>
    /// <param name="options">Options controlling execution behavior.</param>
    /// <returns>An OperationResult indicating the number of successful operations or failure information.</returns>
    public static async Task<OperationResult<int>> ExecuteMultipleAsync(
        IEnumerable<Func<Task>> operations,
        BulkExecutionOptions? options = null)
    {
        options ??= new BulkExecutionOptions();

        if (options.ExecuteInParallel)
        {
            return await ExecuteInParallelAsync(operations, options);
        }

        return await ExecuteSequentiallyAsync(operations, options);
    }

    /// <summary>
    /// Executes multiple synchronous operations without return values.
    /// </summary>
    /// <param name="operations">Collection of synchronous operations to execute.</param>
    /// <param name="options">Options controlling execution behavior.</param>
    /// <returns>An OperationResult indicating the number of successful operations or failure information.</returns>
    public static OperationResult<int> ExecuteMultiple(
        IEnumerable<Action> operations,
        BulkExecutionOptions? options = null)
    {
        options ??= new BulkExecutionOptions();

        var successCount = 0;
        var exceptions = new List<Exception>();

        foreach (var operation in operations)
        {
            try
            {
                operation();
                successCount++;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (!options.ContinueOnError)
                {
                    break;
                }
            }
        }

        return ProcessBulkResults(successCount, exceptions, options);
    }

    #endregion

    #region Private Helper Methods

    private static async Task<OperationResult<IEnumerable<TResult?>>> ExecuteInParallelAsync<TResult>(
        IEnumerable<Func<Task<TResult>>> operations,
        BulkExecutionOptions options)
    {
        var tasks = operations.Select(async operation =>
        {
            try
            {
                var result = await operation();
                return OperationResult<TResult>.Success(result);
            }
            catch (Exception ex)
            {
                return OperationResult<TResult>.Failure(default, ex, [ex.Message]);
            }
        }).ToList();

        var completedTasks = await Task.WhenAll(tasks);

        var results = completedTasks
            .Where(t => t.IsSuccess)
            .Select(t => t.Data)
            .ToList();

        var exceptions = completedTasks
            .Where(t => !t.IsSuccess)
            .Select(t => t.Exception!)
            .ToList();

        return ProcessBulkResults(results, exceptions, options);
    }

    private static async Task<OperationResult<IEnumerable<TResult?>>> ExecuteSequentiallyAsync<TResult>(
        IEnumerable<Func<Task<TResult>>> operations,
        BulkExecutionOptions options)
    {
        var results = new List<TResult>();
        var exceptions = new List<Exception>();

        foreach (var operation in operations)
        {
            try
            {
                var result = await operation();
                results.Add(result);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (!options.ContinueOnError)
                {
                    break;
                }
            }
        }

        return ProcessBulkResults(results, exceptions, options);
    }

    private static async Task<OperationResult<int>> ExecuteInParallelAsync(
        IEnumerable<Func<Task>> operations,
        BulkExecutionOptions options)
    {
        var tasks = operations.Select(async operation =>
        {
            try
            {
                await operation();
                return OperationResult.Success();
            }
            catch (Exception ex)
            {
                return OperationResult.Failure(ex, [ex.Message]);
            }
        }).ToList();

        var completedTasks = await Task.WhenAll(tasks);

        var successCount = completedTasks.Count(t => t.IsSuccess);
        var exceptions = completedTasks
            .Where(t => !t.IsSuccess)
            .Select(t => t.Exception!)
            .ToList();

        return ProcessBulkResults(successCount, exceptions, options);
    }

    private static async Task<OperationResult<int>> ExecuteSequentiallyAsync(
        IEnumerable<Func<Task>> operations,
        BulkExecutionOptions options)
    {
        var successCount = 0;
        var exceptions = new List<Exception>();

        foreach (var operation in operations)
        {
            try
            {
                await operation();
                successCount++;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (!options.ContinueOnError)
                {
                    break;
                }
            }
        }

        return ProcessBulkResults(successCount, exceptions, options);
    }

    private static OperationResult<IEnumerable<TResult?>> ProcessBulkResults<TResult>(
        IEnumerable<TResult?> results,
        List<Exception> exceptions,
        BulkExecutionOptions options)
    {
        var resultsList = results.ToList();
        var hasAnySuccess = resultsList.Any();
        var hasAnyFailure = exceptions.Any();
        var allFailed = !hasAnySuccess && hasAnyFailure;

        // If all operations failed, always return failure
        if (allFailed)
        {
            return OperationResult<IEnumerable<TResult?>>.Failure(
                resultsList,
                new AggregateException(exceptions),
                exceptions.Select(e => e.Message)
            );
        }

        // If some failed, but we have successes and SucceedWithPartialFailures is true
        if (hasAnyFailure && options.SucceedWithPartialFailures)
        {
            return new OperationResult<IEnumerable<TResult?>>(
                isSuccess: true,
                statusCode: HttpStatusCode.OK,
                data: resultsList,
                exception: new AggregateException(exceptions),
                errors: exceptions.Select(e => e.Message)
            );
        }

        // If some failed and SucceedWithPartialFailures is false
        if (hasAnyFailure)
        {
            return OperationResult<IEnumerable<TResult?>>.Failure(
                resultsList,
                new AggregateException(exceptions),
                exceptions.Select(e => e.Message)
            );
        }

        // All succeeded
        return OperationResult<IEnumerable<TResult?>>.Success(resultsList);
    }

    private static OperationResult<int> ProcessBulkResults(
        int successCount,
        List<Exception> exceptions,
        BulkExecutionOptions options)
    {
        var hasAnySuccess = successCount > 0;
        var hasAnyFailure = exceptions.Any();
        var allFailed = !hasAnySuccess && hasAnyFailure;

        // If all operations failed, always return failure
        if (allFailed)
        {
            return OperationResult<int>.Failure(
                successCount,
                new AggregateException(exceptions),
                exceptions.Select(e => e.Message)
            );
        }

        // If some failed, but we have successes and SucceedWithPartialFailures is true
        if (hasAnyFailure && options.SucceedWithPartialFailures)
        {
            return new OperationResult<int>(
                isSuccess: true,
                statusCode: HttpStatusCode.OK,
                data: successCount,
                exception: new AggregateException(exceptions),
                errors: exceptions.Select(e => e.Message)
            );
        }

        // If some failed and SucceedWithPartialFailures is false
        if (hasAnyFailure)
        {
            return OperationResult<int>.Failure(
                successCount,
                new AggregateException(exceptions),
                exceptions.Select(e => e.Message)
            );
        }

        // All succeeded
        return OperationResult<int>.Success(successCount);
    }

    #endregion
}

/// <summary>
/// Options for configuring bulk operation execution behavior.
/// </summary>
public class BulkExecutionOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to execute operations in parallel (async only).
    /// Default is false (sequential execution).
    /// </summary>
    public bool ExecuteInParallel { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to continue executing remaining operations after an error occurs.
    /// If false, execution stops immediately on the first error (sequential mode only).
    /// If true, all operations will be attempted regardless of failures.
    /// Default is true.
    /// Note: In parallel mode, all operations are always attempted.
    /// </summary>
    public bool ContinueOnError { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to return success even when some operations fail,
    /// as long as at least one operation succeeds. Exceptions and errors will still be included.
    /// If all operations fail, the result will always be marked as failure.
    /// Default is false.
    /// </summary>
    public bool SucceedWithPartialFailures { get; set; } = false;
}