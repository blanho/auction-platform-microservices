using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StorageService.Application.Configuration;
using StorageService.Application.Interfaces;
using StorageService.Application.Services;
using StorageService.Infrastructure.Data;
using StorageService.Infrastructure.Repositories;
using StorageService.Infrastructure.Storage;

namespace StorageService.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<StorageDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });

        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IFileStorageService, FileStorageService>();

        var storageType = configuration.GetValue<string>("Storage:Type") ?? "Local";
        
        switch (storageType.ToLowerInvariant())
        {
            case "local":
            default:
                var basePath = configuration.GetValue<string>("Storage:LocalBasePath") ?? "uploads";
                services.AddSingleton<IStorageProvider>(sp =>
                {
                    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<StorageOptions>>();
                    return new LocalStorageProvider(options, basePath);
                });
                break;
        }

        return services;
    }
}
