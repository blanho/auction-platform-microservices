using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Application.Behaviors;
using System.Reflection;

namespace BuildingBlocks.Application.Extensions;

public static class CqrsExtensions
{
    public static IServiceCollection AddCQRS(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(assemblies);

        return services;
    }

    public static IServiceCollection AddDomainEvents(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(assemblies);
        });

        return services;
    }
}
