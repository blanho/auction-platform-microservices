namespace BuildingBlocks.Application.Abstractions;

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
    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);

    public static Result<T> FromValue<T>(T? value, Error error) where T : class
        => value is not null ? Success(value) : Failure<T>(error);

    public static Result Create(bool condition, Error error)
        => condition ? Success() : Failure(error);
}

public class Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, Error? error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);

    public T GetValueOrThrow()
    {
        if (IsFailure)
            throw new InvalidOperationException($"Cannot get value from failed result: {Error?.Message}");

        return Value!;
    }

    public T GetValueOrDefault(T defaultValue) => IsSuccess ? Value! : defaultValue;
}

public static class ResultExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> next)
    {
        return result.IsFailure
            ? Result.Failure<TOut>(result.Error!)
            : next(result.Value!);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next)
    {
        return result.IsFailure
            ? Result.Failure<TOut>(result.Error!)
            : await next(result.Value!);
    }

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> next)
    {
        var result = await resultTask;
        return result.IsFailure
            ? Result.Failure<TOut>(result.Error!)
            : await next(result.Value!);
    }

    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mapper)
    {
        return result.IsFailure
            ? Result.Failure<TOut>(result.Error!)
            : Result.Success(mapper(result.Value!));
    }

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(
        this Task<Result<TIn>> resultTask,
        Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        return result.IsFailure
            ? Result.Failure<TOut>(result.Error!)
            : Result.Success(mapper(result.Value!));
    }

    public static Result<T> Tap<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value!);

        return result;
    }

    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> action)
    {
        if (result.IsSuccess)
            await action(result.Value!);

        return result;
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value!)
            : onFailure(result.Error!);
    }

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

    public static Result Combine(params Result[] results)
    {
        var failedResult = results.FirstOrDefault(r => r.IsFailure);
        return failedResult ?? Result.Success();
    }

    public static Result<IEnumerable<T>> Combine<T>(IEnumerable<Result<T>> results)
    {
        var resultList = results.ToList();
        var failedResult = resultList.FirstOrDefault(r => r.IsFailure);

        if (failedResult != null)
            return Result.Failure<IEnumerable<T>>(failedResult.Error!);

        return Result.Success(resultList.Select(r => r.Value!));
    }
}
