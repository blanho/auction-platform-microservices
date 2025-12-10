using Common.Storage.Abstractions;
using Common.Storage.Clients;
using Common.Storage.Configuration;
using Common.Storage.Enums;
using Common.Storage.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Storage.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<StorageOptions>(options =>
        {
            var section = configuration.GetSection(StorageOptions.SectionName);
            section.Bind(options);
        });

        var storageSection = configuration.GetSection(StorageOptions.SectionName);
        var providerType = storageSection.GetValue<StorageType>("Provider");

        switch (providerType)
        {
            case StorageType.AzureBlob:
                services.AddSingleton<IStorageProvider, AzureBlobStorageProvider>();
                break;
            case StorageType.Cloudinary:
                services.AddSingleton<IStorageProvider, CloudinaryStorageProvider>();
                break;
            case StorageType.Local:
            default:
                services.AddSingleton<IStorageProvider, LocalStorageProvider>();
                break;
        }

        return services;
    }

    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        Action<StorageOptions> configureOptions)
    {
        var options = new StorageOptions();
        configureOptions(options);
        
        services.Configure(configureOptions);

        switch (options.Provider)
        {
            case StorageType.AzureBlob:
                services.AddSingleton<IStorageProvider, AzureBlobStorageProvider>();
                break;
            case StorageType.Cloudinary:
                services.AddSingleton<IStorageProvider, CloudinaryStorageProvider>();
                break;
            case StorageType.Local:
            default:
                services.AddSingleton<IStorageProvider, LocalStorageProvider>();
                break;
        }

        return services;
    }

    public static IServiceCollection AddFileStorageClient(
        this IServiceCollection services,
        string baseUrl)
    {
        services.AddHttpClient<IFileStorageClient, FileStorageClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }

    public static IServiceCollection AddFileStorageClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5005";
        
        services.AddHttpClient<IFileStorageClient, FileStorageClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        });

        return services;
    }

    public static IServiceCollection AddFileStorageGrpcClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GrpcFileStorageOptions>(options =>
        {
            options.ServiceUrl = configuration["FileStorage:GrpcUrl"] ?? "https://localhost:5003";
            if (int.TryParse(configuration["FileStorage:ChunkSize"], out var chunkSize))
            {
                options.ChunkSize = chunkSize;
            }
        });

        services.AddSingleton<IFileStorageGrpcClient, FileStorageGrpcClient>();

        return services;
    }

    public static IServiceCollection AddFileStorageGrpcClient(
        this IServiceCollection services,
        Action<GrpcFileStorageOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<IFileStorageGrpcClient, FileStorageGrpcClient>();

        return services;
    }
}
