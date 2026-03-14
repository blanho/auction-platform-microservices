using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Extensions.DependencyInjection;

public static class EndpointExtensions
{
    public static RouteHandlerBuilder WithValidation<T>(this RouteHandlerBuilder builder) where T : class
    {
        return builder.AddEndpointFilter<ValidationFilter<T>>();
    }
}

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T>? _validator;
    private readonly ILogger<ValidationFilter<T>> _logger;

    public ValidationFilter(IServiceProvider serviceProvider, ILogger<ValidationFilter<T>> logger)
    {
        _validator = serviceProvider.GetService<IValidator<T>>();
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (_validator == null)
        {
            return await next(context);
        }

        var argToValidate = context.Arguments.OfType<T>().FirstOrDefault();
        if (argToValidate == null)
        {
            return await next(context);
        }

        var validationResult = await _validator.ValidateAsync(argToValidate);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning("Validation failed for {Type}: {Errors}", typeof(T).Name, errors);

            var problemDetails = new ProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Extensions =
                {
                    ["errors"] = errors
                }
            };

            return Results.BadRequest(problemDetails);
        }

        return await next(context);
    }
}
