using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Common.CQRS.Behaviors;
using System.Reflection;

namespace Common.CQRS.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCQRS(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }
}
