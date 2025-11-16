using IoT_System.Application.Models;

namespace IoT_System.Application.Common.Helpers;

/// <summary>
/// Provides utility methods for executing operations with automatic OperationResult wrapping and exception handling.
/// Enables consistent error handling and result patterns across all application layers.
/// </summary>
public static class ExecutionHelper
{
    #region Execute with Result

    /// <summary>
    /// Executes an asynchronous operation that returns a result and wraps it in an OperationResult.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <returns>An OperationResult containing the operation result or exception information.</returns>
    public static async Task<OperationResult<TResult>> ExecuteAsync<TResult>(Func<Task<TResult>> operation)
    {
        try
        {
            var result = await operation();
            return OperationResult<TResult>.Success(result);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes a synchronous operation that returns a result and wraps it in an OperationResult.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The synchronous operation to execute.</param>
    /// <returns>An OperationResult containing the operation result or exception information.</returns>
    public static OperationResult<TResult> Execute<TResult>(Func<TResult> operation)
    {
        try
        {
            var result = operation();
            return OperationResult<TResult>.Success(result);
        }
        catch (Exception ex)
        {
            return OperationResult<TResult>.Failure(ex);
        }
    }

    #endregion

    #region Execute without Result

    /// <summary>
    /// Executes an asynchronous operation without a return value and wraps the success status in an OperationResult.
    /// </summary>
    /// <param name="operation">The asynchronous operation to execute.</param>
    /// <returns>An OperationResult indicating success (true) or failure with exception information.</returns>
    public static async Task<OperationResult<bool>> ExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation();
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(ex);
        }
    }

    /// <summary>
    /// Executes a synchronous operation without a return value and wraps the success status in an OperationResult.
    /// </summary>
    /// <param name="operation">The synchronous operation to execute.</param>
    /// <returns>An OperationResult indicating success (true) or failure with exception information.</returns>
    public static OperationResult<bool> Execute(Action operation)
    {
        try
        {
            operation();
            return OperationResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(ex);
        }
    }

    #endregion

    #region Execute Multiple Operations

    /// <summary>
    /// Executes multiple asynchronous operations in sequence and returns all results.
    /// Stops execution on first failure unless continueOnError is true.
    /// </summary>
    /// <typeparam name="TResult">The type of result returned by each operation.</typeparam>
    /// <param name="operations">Collection of asynchronous operations to execute.</param>
    /// <param name="continueOnError">If true, continues executing remaining operations even after a failure.</param>
    /// <returns>An OperationResult containing all successful results and any exceptions encountered.</returns>
    public static async Task<OperationResult<IEnumerable<TResult>>> ExecuteMultipleAsync<TResult>(IEnumerable<Func<Task<TResult>>> operations,
        bool continueOnError = false)
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
                if (!continueOnError)
                {
                    return OperationResult<IEnumerable<TResult>>.Failure(
                        results,
                        new AggregateException(exceptions),
                        exceptions.Select(e => e.Message)
                    );
                }
            }
        }

        if (exceptions.Any())
        {
            return OperationResult<IEnumerable<TResult>>.Failure(
                results,
                new AggregateException(exceptions),
                exceptions.Select(e => e.Message)
            );
        }

        return OperationResult<IEnumerable<TResult>>.Success(results);
    }

    #endregion
}