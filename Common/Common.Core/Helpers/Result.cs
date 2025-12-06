namespace Common.Core.Helpers;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException("Successful result cannot have an error.");
        
        if (!isSuccess && error == null)
            throw new InvalidOperationException("Failed result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
    
    /// <summary>
    /// Creates a result from a nullable value - Success if not null, Failure otherwise
    /// </summary>
    public static Result<T> FromValue<T>(T? value, Error error) where T : class
        => value is not null ? Success(value) : Failure<T>(error);

    /// <summary>
    /// Creates a result based on a condition
    /// </summary>
    public static Result Create(bool condition, Error error)
        => condition ? Success() : Failure(error);
}

/// <summary>
/// Represents the result of an operation that returns a value
/// </summary>
/// <typeparam name="T">The type of the value</typeparam>
public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    /// <summary>
    /// Implicitly converts a value to a successful result
    /// </summary>
    public static implicit operator Result<T>(T value) => Success(value);

    /// <summary>
    /// Gets the value or throws if the result is a failure
    /// </summary>
    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException($"Cannot get value from failed result: {Error?.Message}");
        
        return Value!;
    }

    /// <summary>
    /// Gets the value or returns a default value if the result is a failure
    /// </summary>
    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value! : defaultValue;
}

/// <summary>
/// Extension methods for Result types implementing Railway-Oriented Programming
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Chains operations - if the result is successful, executes the next operation
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> next)
    {
        return result.IsFailure 
            ? Result.Failure<TOut>(result.Error!) 
            : next(result.Value!);
    }

    /// <summary>
    /// Async version of Bind
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next)
    {
        return result.IsFailure 
            ? Result.Failure<TOut>(result.Error!) 
            : await next(result.Value!);
    }

    /// <summary>
    /// Async version of Bind for Task results
    /// </summary>
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return result.IsFailure 
            ? Result.Failure<TOut>(result.Error!) 
            : await next(result.Value!);
    }

    /// <summary>
    /// Transforms the value if the result is successful
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsFailure 
            ? Result.Failure<TOut>(result.Error!) 
            : Result.Success(mapper(result.Value!));
    }

    /// <summary>
    /// Async version of Map
    /// </summary>
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.IsFailure 
            ? Result.Failure<TOut>(result.Error!) 
            : Result.Success(mapper(result.Value!));
    }

    /// <summary>
    /// Executes an action if the result is successful, returns the original result
    /// </summary>
    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value!);
        
        return result;
    }

    /// <summary>
    /// Async version of Tap
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value!);
        
        return result;
    }

    /// <summary>
    /// Pattern matching on result
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess 
            ? onSuccess(result.Value!) 
            : onFailure(result.Error!);
    }

    /// <summary>
    /// Ensures a condition is met, or returns a failure
    /// </summary>
    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value!) 
            ? result 
            : Result.Failure<T>(error);
    }

    /// <summary>
    /// Combines multiple results into a single result
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failedResult = results.FirstOrDefault(r => r.IsFailure);
        return failedResult ?? Result.Success();
    }

    /// <summary>
    /// Combines multiple results with values into a single result with all values
    /// </summary>
    public static Result<IEnumerable<T>> Combine<T>(IEnumerable<Result<T>> results)
    {
        var resultList = results.ToList();
        var failedResult = resultList.FirstOrDefault(r => r.IsFailure);
        
        if (failedResult != null)
            return Result.Failure<IEnumerable<T>>(failedResult.Error!);

        return Result.Success(resultList.Select(r => r.Value!));
    }
}
