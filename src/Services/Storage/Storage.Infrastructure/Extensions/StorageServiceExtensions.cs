using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Application.Interfaces;
using Storage.Infrastructure.Messaging;
using Storage.Infrastructure.Persistence;
using Storage.Infrastructure.Persistence.Repositories;
using StorageUnitOfWork = Storage.Application.Interfaces.IUnitOfWork;

namespace Storage.Infrastructure.Extensions;

public static class StorageServiceExtensions
{
    public static IServiceCollection AddStorageInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<StorageUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddStorageMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.ConfigureStorageMessaging(configuration);
    }
}
