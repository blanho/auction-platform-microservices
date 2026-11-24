using BuildingBlocks.Application.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace BuildingBlocks.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await ValidateAsync(request, cancellationToken);
        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        return FailureResultFactory<TResponse>.Create(ValidationError.WithErrors(errors));
    }

    private async Task<List<ValidationFailure>> ValidateAsync(TRequest request, CancellationToken cancellationToken)
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in _validators)
        {
            var context = new ValidationContext<TRequest>(request);
            var result = await validator.ValidateAsync(context, cancellationToken);
            failures.AddRange(result.Errors.Where(failure => failure is not null));
        }

        return failures;
    }
}

internal static class FailureResultFactory<TResponse> where TResponse : Result
{
    public static readonly Func<Error, TResponse> Create = BuildFactory();

    private static Func<Error, TResponse> BuildFactory()
    {
        var resultType = typeof(TResponse);
        if (resultType == typeof(Result))
        {
            return error => (TResponse)Result.Failure(error);
        }

        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            return typeof(Result)
                .GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) })!
                .MakeGenericMethod(resultType.GetGenericArguments())
                .CreateDelegate<Func<Error, TResponse>>();
        }

        return _ => throw new InvalidOperationException($"Cannot create failure result for type {resultType}");
    }
}
